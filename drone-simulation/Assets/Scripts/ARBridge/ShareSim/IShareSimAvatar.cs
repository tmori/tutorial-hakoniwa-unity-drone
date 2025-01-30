using hakoniwa.pdu.interfaces;
using UnityEngine;

namespace hakoniwa.ar.bridge.sharesim
{
    public interface IShareSimAvatar
    {
        void StartAvatarProc();
        void StopAvatarProc();
        void UpdatePosition(IPduManager pduManager);
    }
}
