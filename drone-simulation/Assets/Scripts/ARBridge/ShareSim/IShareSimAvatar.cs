using hakoniwa.pdu.interfaces;
using hakoniwa.pdu.msgs.hako_msgs;
using UnityEngine;

namespace hakoniwa.ar.bridge.sharesim
{
    public interface IShareSimAvatar
    {
        void Initialize(GameObject target);
        void StartAvatarProc();
        void StopAvatarProc();
        void UpdatePosition(ShareObjectOwner owner);
    }
}
