using hakoniwa.ar.bridge.sharesim;
using hakoniwa.objects.core;
using hakoniwa.pdu.interfaces;
using hakoniwa.pdu.msgs.hako_msgs;
using System.Threading.Tasks;
using UnityEngine;

public class BaggageGrabber : MonoBehaviour
{
    public enum GrabResult
    {
        Success,          // 取得成功
        AlreadyRequesting, // すでにリクエスト中
        Timeout,          // タイムアウト
        OwnershipLost,    // 他のロボットに取られた
        NoBaggage,        // 近くに荷物がなかった
        FailedToGrab      // 掴むのに失敗
    }
    public enum ReleaseResult
    {
        Success,         // リリース成功
        NoBaggage,       // リリースする荷物がない
        FlushFailed      // FlushNamedPdu 失敗
    }

    private Baggage requestingBaggage = null;
    private uint requesting_owner_id;
    private Magnet magnet;
    private float requestStartTime;
    public float timeoutDuration = 5.0f; // タイムアウト時間（秒）

    private void Start()
    {
        magnet = this.GetComponentInChildren<Magnet>();
        if (magnet == null)
        {
            throw new System.Exception("Can not find magnet on " + this.transform.name);
        }
        requestingBaggage = null;
    }

    private async Task<Baggage> RequestGrabAsync(uint owner_id, IPduManager pduManager)
    {
        if (requestingBaggage)
        {
            return requestingBaggage;
        }

        // Find Baggage
        var baggage = this.magnet.FindNearestBaggage();
        if (baggage == null)
        {
            return null;
        }
        requestingBaggage = baggage;
        requesting_owner_id = owner_id;
        requestStartTime = Time.time;

        INamedPdu npdu = pduManager.CreateNamedPdu(ShareSimServer.robotName, ShareSimServer.pduRequest);
        if (npdu == null)
        {
            throw new System.Exception($"Can not find npdu: {ShareSimServer.robotName} / {ShareSimServer.pduRequest}");
        }

        ShareObjectOwnerRequest req = new ShareObjectOwnerRequest(npdu.Pdu);
        req.object_name = baggage.name;
        req.request_type = (uint)ShareObjectOwnerRequestType.Acquire;
        req.new_owner_id = owner_id;
        req.request_time = (uint)(requestStartTime * 1000);

        pduManager.WriteNamedPdu(npdu);
        bool ret = await pduManager.FlushNamedPdu(npdu);
        if (!ret)
        {
            Debug.LogError($"FlushNamedPdu failed for {ShareSimServer.robotName} / {ShareSimServer.pduRequest}");
        }
        return requestingBaggage;
    }

    private uint GetCurrentOwner(Baggage baggage, IPduManager pduManager)
    {
        IPdu pdu = pduManager.ReadPdu(baggage.name, ShareSimServer.pduOwner);
        if (pdu == null)
        {
            Debug.LogWarning($"Can not get pdu of owner on {baggage.name}");
            return uint.MaxValue; // 修正: PDU取得失敗を明確化
        }
        ShareObjectOwner obj = new ShareObjectOwner(pdu);
        return obj.owner_id;
    }

    private bool Grab()
    {
        if (requestingBaggage == null)
        {
            Debug.LogError("Attempted to grab with no requesting baggage.");
            return false;
        }

        bool ret = this.magnet.TurnOn(requestingBaggage);
        if (!ret)
        {
            Debug.LogError("Failed to grab baggage.");
            return false;
        }

        requestingBaggage = null;
        return true;
    }

    private bool IsTimeout()
    {
        if (requestingBaggage == null)
        {
            return false;
        }
        return (Time.time - requestStartTime) > timeoutDuration;
    }

    public async Task<GrabResult> RequestGrab(uint owner_id, IPduManager pduManager)
    {
        // すでにリクエスト中なら即座に `AlreadyRequesting` を返す
        if (requestingBaggage != null)
        {
            Debug.LogWarning("Already requesting baggage. Ignoring new request.");
            return GrabResult.AlreadyRequesting;
        }

        Debug.Log("Starting new grab request...");
        requestingBaggage = await RequestGrabAsync(owner_id, pduManager);
        if (requestingBaggage == null)
        {
            Debug.LogWarning("No baggage found to grab.");
            return GrabResult.NoBaggage;
        }

        // 所有権取得を待つ
        while (requestingBaggage != null)
        {
            if (IsTimeout())
            {
                Debug.LogWarning("Ownership request timed out!");
                requestingBaggage = null;
                return GrabResult.Timeout;
            }

            uint owner_id_now = GetCurrentOwner(requestingBaggage, pduManager);
            if (owner_id_now == uint.MaxValue)
            {
                Debug.LogWarning("Failed to retrieve owner information.");
                return GrabResult.Timeout;
            }

            if (owner_id_now == requesting_owner_id)
            {
                break; // 所有権獲得
            }
            else if (owner_id_now != ShareSimServer.owner_id)
            {
                Debug.LogWarning("Another robot took ownership!");
                requestingBaggage = null;
                return GrabResult.OwnershipLost;
            }

            await Task.Delay(100); // 次のチェックまで少し待機
        }

        // 所有権を獲得したので Grab を試行
        bool success = Grab();
        if (success)
        {
            Debug.Log("Successfully grabbed baggage!");
            return GrabResult.Success;
        }
        else
        {
            return GrabResult.FailedToGrab; // 失敗時の結果を明確化
        }
    }
    public async Task<ReleaseResult> RequestRelease(uint owner_id, IPduManager pduManager)
    {
        if (requestingBaggage == null)
        {
            Debug.LogWarning("No baggage to release.");
            return ReleaseResult.NoBaggage;
        }

        INamedPdu npdu = pduManager.CreateNamedPdu(ShareSimServer.robotName, ShareSimServer.pduRequest);
        if (npdu == null)
        {
            throw new System.Exception($"Can not find npdu: {ShareSimServer.robotName} / {ShareSimServer.pduRequest}");
        }

        ShareObjectOwnerRequest req = new ShareObjectOwnerRequest(npdu.Pdu);
        req.object_name = requestingBaggage.name;
        req.request_type = (uint)ShareObjectOwnerRequestType.Release;
        req.new_owner_id = requesting_owner_id;
        req.request_time = (uint)(Time.time * 1000); // 正しいリクエスト時間を設定

        pduManager.WriteNamedPdu(npdu);
        bool ret = await pduManager.FlushNamedPdu(npdu);

        if (!ret)
        {
            Debug.LogError($"FlushNamedPdu failed for {ShareSimServer.robotName} / {ShareSimServer.pduRequest}");
            return ReleaseResult.FlushFailed;
        }

        Debug.Log($"Successfully released {requestingBaggage.name}");
        requestingBaggage = null;
        this.magnet.TurnOff();

        return ReleaseResult.Success;
    }

}
