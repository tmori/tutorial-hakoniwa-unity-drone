using hakoniwa.pdu.interfaces;
using hakoniwa.pdu.msgs.hako_msgs;
using UnityEngine;

namespace hakoniwa.ar.bridge.sharesim
{
    public interface IShareSimPhysics
    {
        void Initialize(GameObject target);
        void StartPhysics();
        void StopPhysics();
        void UpdatePosition(ShareObjectOwner owner);
    }
}
