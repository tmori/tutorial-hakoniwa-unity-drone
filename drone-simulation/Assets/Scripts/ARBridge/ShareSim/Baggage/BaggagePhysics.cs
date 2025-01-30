using hakoniwa.ar.bridge.sharesim;
using hakoniwa.objects.core;
using hakoniwa.pdu.msgs.hako_msgs;
using UnityEngine;

public class BaggagePhysics : MonoBehaviour, IShareSimPhysics
{
    private Baggage baggage;

    public void Initialize(GameObject target)
    {
        baggage = target.GetComponentInChildren<Baggage>();
        if (baggage == null)
        {
            throw new System.Exception("Can not find baggage on " + this.transform.name);
        }
    }

    public void StartPhysics()
    {
        //nothing to do
    }

    public void StopPhysics()
    {
        //nothing to do
    }

    public void UpdatePosition(ShareObjectOwner owner)
    {
        //nothing to do
    }

}
