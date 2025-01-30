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

    private Baggage currentBaggage = null;
    private uint requestOwnerId;
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
        currentBaggage = null;
    }

    private async Task<Baggage> RequestGrabAsync(uint owner_id, IPduManager pduManager)
    {
        if (currentBaggage)
        {
            return currentBaggage;
        }

        // Find Baggage
        currentBaggage = this.magnet.FindNearestBaggage();
        if (currentBaggage == null)
        {
            return null;
        }
        requestOwnerId = owner_id;
        requestStartTime = Time.time;

        INamedPdu npdu = pduManager.CreateNamedPdu(ShareSimServer.robotName, ShareSimServer.pduRequest);
        if (npdu == null)
        {
            throw new System.Exception($"Can not find npdu: {ShareSimServer.robotName} / {ShareSimServer.pduRequest}");
        }

        ShareObjectOwnerRequest req = new ShareObjectOwnerRequest(npdu.Pdu);
        req.object_name = currentBaggage.name;
        req.request_type = (uint)ShareObjectOwnerRequestType.Acquire;
        req.new_owner_id = owner_id;
        req.request_time = (uint)(requestStartTime * 1000);

        pduManager.WriteNamedPdu(npdu);
        bool ret = await pduManager.FlushNamedPdu(npdu);
        if (!ret)
        {
            Debug.LogError($"FlushNamedPdu failed for {ShareSimServer.robotName} / {ShareSimServer.pduRequest}");
        }
        return currentBaggage;
    }

    private uint GetCurrentOwner(Baggage baggage)
    {
        return ShareSimClient.Instance.GetOwnerId(baggage.name);
    }

    private bool Grab()
    {
        if (currentBaggage == null)
        {
            Debug.LogError("Attempted to grab with no requesting baggage.");
            return false;
        }

        bool ret = this.magnet.TurnOn(currentBaggage);
        if (!ret)
        {
            Debug.LogError("Failed to grab baggage.");
            return false;
        }

        currentBaggage = null;
        return true;
    }

    private bool IsTimeout()
    {
        if (currentBaggage == null)
        {
            return false;
        }
        return (Time.time - requestStartTime) > timeoutDuration;
    }

    public async Task<GrabResult> RequestGrab(uint owner_id, IPduManager pduManager)
    {
        // すでにリクエスト中なら即座に `AlreadyRequesting` を返す
        if (currentBaggage != null)
        {
            Debug.LogWarning("Already requesting baggage. Ignoring new request.");
            return GrabResult.AlreadyRequesting;
        }

        Debug.Log("Starting new grab request...");
        currentBaggage = await RequestGrabAsync(owner_id, pduManager);
        if (currentBaggage == null)
        {
            Debug.LogWarning("No baggage found to grab.");
            return GrabResult.NoBaggage;
        }

        // 所有権取得を待つ
        while (currentBaggage != null)
        {
            if (IsTimeout())
            {
                Debug.LogWarning("Ownership request timed out!");
                currentBaggage = null;
                return GrabResult.Timeout;
            }

            uint owner_id_now = GetCurrentOwner(currentBaggage);
            if (owner_id_now == uint.MaxValue)
            {
                Debug.LogWarning("Failed to retrieve owner information.");
                throw new System.Exception("Invalid share object: " + currentBaggage.name);
            }

            if (owner_id_now == requestOwnerId)
            {
                break; // 所有権獲得
            }
            else if (owner_id_now != ShareSimServer.owner_id)
            {
                Debug.LogWarning("Another robot took ownership!");
                currentBaggage = null;
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
        if (currentBaggage == null)
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
        req.object_name = currentBaggage.name;
        req.request_type = (uint)ShareObjectOwnerRequestType.Release;
        req.new_owner_id = requestOwnerId;
        req.request_time = (uint)(Time.time * 1000); // 正しいリクエスト時間を設定

        pduManager.WriteNamedPdu(npdu);
        bool ret = await pduManager.FlushNamedPdu(npdu);

        if (!ret)
        {
            Debug.LogError($"FlushNamedPdu failed for {ShareSimServer.robotName} / {ShareSimServer.pduRequest}");
            return ReleaseResult.FlushFailed;
        }

        Debug.Log($"Successfully released {currentBaggage.name}");
        currentBaggage = null;
        this.magnet.TurnOff();

        return ReleaseResult.Success;
    }

}
