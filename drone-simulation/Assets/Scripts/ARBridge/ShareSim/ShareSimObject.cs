using hakoniwa.pdu.interfaces;
using hakoniwa.sim;
using hakoniwa.sim.core;
using UnityEngine;

namespace hakoniwa.ar.bridge.sharesim
{
    public class ShareSimObject : MonoBehaviour
    {
        public GameObject target_object;
        private uint target_owner_id = 0;
        private uint my_owner_id = 0;
        private IShareSimPhysics physics;
        private IShareSimAvatar avatar;

        public string GetName()
        {
            return target_object.name;
        }

        public void SetTargetOwnerId(uint owner_id)
        {
            target_owner_id = owner_id;
        }
        public uint GetTargetOwnerId()
        {
            return target_owner_id;
        }
        public void SetOwnerId(uint owner_id)
        {
            my_owner_id = owner_id;
        }
        public uint GetOwnerId()
        {
            return my_owner_id;
        }
        public bool IsOwner(uint owner_id)
        {
            return (owner_id == target_owner_id);
        }
        public void DoInitialize()
        {
            if (target_object == null)
            {
                throw new System.Exception("Can not find target_object");
            }

            physics = target_object.GetComponentInChildren<IShareSimPhysics>();
            if (physics == null)
            {
                throw new System.Exception("Can not find IShareSimPhysics on " + target_object.name);
            }
            avatar = target_object.GetComponentInChildren<IShareSimAvatar>();
            if (avatar == null)
            {
                throw new System.Exception("Can not find IShareSimAvatar on " + target_object.name);
            }
        }

        public void DoStart()
        {
            if (IsOwner(my_owner_id))
            {
                physics.StartPhysics();
            }
            else
            {
                avatar.StartAvatarProc();
            }
        }
        public void DoUpdate(IPduManager pduManager)
        {
            if (IsOwner(my_owner_id))
            {
                //TODO PDU Write for physics
                physics.UpdatePosition(pduManager);
            }
            else
            {
                //TODO PDU Read and update pos and rot of target 
                avatar.UpdatePosition(pduManager);
            }
        }

        public void DoStop()
        {
            if (IsOwner(my_owner_id))
            {
                physics.StopPhysics();
            }
            else
            {
                avatar.StopAvatarProc();
            }
        }

    }
}
