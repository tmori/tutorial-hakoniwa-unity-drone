using hakoniwa.pdu.interfaces;
using hakoniwa.pdu.msgs.hako_msgs;
using hakoniwa.sim;
using hakoniwa.sim.core;
using UnityEngine;

namespace hakoniwa.ar.bridge.sharesim
{
    public class ShareSimObject : MonoBehaviour
    {
        public GameObject target_object;
        public uint target_owner_id = 0;
        public uint my_owner_id = 0;
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
            physics.Initialize(target_object);
            avatar.Initialize(target_object);
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
        public async void DoUpdate(IPduManager pduManager, ulong sim_time)
        {
            IPdu pdu = pduManager.ReadPdu(target_object.name, ShareSimServer.pduOwner);
            if (pdu == null)
            {
                Debug.Log("Can not get pdu of pos on " + target_object.name);
                return;
            }
            ShareObjectOwner owner = new ShareObjectOwner(pdu);
            if (IsOwner(my_owner_id))
            {
                owner.object_name = target_object.name;
                owner.owner_id = my_owner_id;
                owner.last_update = (ulong)sim_time;
                physics.UpdatePosition(owner);
                var key = pduManager.WritePdu(target_object.name, pdu);
                _ = await pduManager.FlushPdu(key);
            }
            else
            {
                avatar.UpdatePosition(owner);
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
