using hakoniwa.pdu.interfaces;
using UnityEngine;

namespace hakoniwa.ar.bridge.sharesim
{
    public interface IShareSimPhysics
    {
        void StartPhysics();
        void StopPhysics();
        void UpdatePosition(IPduManager pduManager);
    }
}
