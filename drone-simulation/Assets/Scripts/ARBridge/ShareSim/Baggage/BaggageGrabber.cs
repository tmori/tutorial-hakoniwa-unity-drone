using hakoniwa.ar.bridge.sharesim;
using hakoniwa.objects.core;
using hakoniwa.pdu.interfaces;
using hakoniwa.pdu.msgs.hako_msgs;
using System.Threading.Tasks;
using UnityEngine;

public class BaggageGrabber : MonoBehaviour
{
    public Baggage requestingBaggage = null;
    private Magnet magnet;

    private void Start()
    {
        magnet = this.GetComponentInChildren<Magnet>();
        if (magnet == null)
        {
            throw new System.Exception("Can not find magnet on " + this.transform.name);
        }
        requestingBaggage = null;
    }
    public async Task<Baggage> RequestGrabAsync(uint owner_id, IPduManager pduManager)
    {
        if (requestingBaggage)
        {
            return requestingBaggage;
        }
        //Find Baggage
        var baggage = this.magnet.FindNearestBaggage();
        if (baggage == null)
        {
            return null;
        }
        requestingBaggage = baggage;
        INamedPdu npdu = pduManager.CreateNamedPdu(ShareSimServer.robotName, ShareSimServer.pduRequest);
        if (npdu == null)
        {
            throw new System.Exception($"Can not find npdu: {ShareSimServer.robotName} / {ShareSimServer.pduRequest}");
        }
        ShareObjectOwnerRequest req = new ShareObjectOwnerRequest(npdu.Pdu);
        req.object_name = baggage.name;
        req.request_type = (uint)ShareObjectOwnerRequestType.Acquire;
        req.new_owner_id = owner_id;
        req.request_time = 0; //TODO
        pduManager.WriteNamedPdu(npdu);

        bool ret = await pduManager.FlushNamedPdu(npdu);
        if (!ret)
        {
            Debug.LogError($"FlushNamedPdu failed for {ShareSimServer.robotName} / {ShareSimServer.pduRequest}");
        }
        return requestingBaggage;
    }
    public uint GetCurrentOwner(IPduManager pduManager)
    {
        if (requestingBaggage == null)
        {
            return ShareSimServer.owner_id;
        }
        IPdu pdu = pduManager.ReadPdu(requestingBaggage.name, ShareSimServer.pduOwner);
        if (pdu == null)
        {
            Debug.Log("Can not get pdu of pos on " + requestingBaggage.name);
            return ShareSimServer.owner_id;
        }
        ShareObjectOwner obj = new ShareObjectOwner(pdu);
        return obj.owner_id;
    }
    public bool Grab()
    {
        if (requestingBaggage == null)
        {
            return false;
        }
        bool ret = this.magnet.TurnOn(requestingBaggage);
        if (ret)
        {
            requestingBaggage = null;
        }
        return ret;
    }
}
