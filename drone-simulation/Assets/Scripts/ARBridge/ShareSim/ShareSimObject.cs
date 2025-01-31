using System;
using System.Threading.Tasks;
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
        /// <summary>
        /// 現在このオブジェクトを所有しているデバイスのID
        /// </summary>
        public uint current_owner_id = 0;
        /// <summary>
        /// 自分のデバイス（固定ID）
        /// </summary>
        public uint device_owner_id = 0;
        private IShareSimPhysics physics;
        private IShareSimAvatar avatar;
        private ulong updateTime = 0;
        public string GetName()
        {
            return target_object.name;
        }

        public void SetCurrentOwnerId(uint owner_id)
        {
            current_owner_id = owner_id;
            updateTime = 0;
        }
        public uint GetCurrentOwnerId()
        {
            return current_owner_id;
        }
        public void SetDeviceOwnerId(uint owner_id)
        {
            device_owner_id = owner_id;
        }
        public uint GetDeviceOwnerId()
        {
            return device_owner_id;
        }
        public bool IsOwner(uint owner_id)
        {
            return (owner_id == current_owner_id);
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
            if (IsOwner(device_owner_id))
            {
                physics.StartPhysics();
            }
            else
            {
                avatar.StartAvatarProc();
            }
        }
        public async Task<uint> DoUpdate(IPduManager pduManager, ulong sim_time)
        {
            IPdu pdu = pduManager.ReadPdu(target_object.name, ShareSimServer.pduOwner);
            if (pdu == null)
            {
                Debug.Log("Can not get pdu of owner on " + target_object.name);
                return uint.MaxValue;
            }
            ShareObjectOwner owner = new ShareObjectOwner(pdu);
            if (IsOwner(device_owner_id))
            {
                owner.object_name = target_object.name;
                owner.owner_id = device_owner_id;
                owner.last_update = (ulong)sim_time;
                physics.UpdatePosition(owner);
                var key = pduManager.WritePdu(target_object.name, pdu);
                _ = await pduManager.FlushPdu(key);
            }
            else
            {
                this.updateTime = owner.last_update;
                Debug.Log("update time: " + updateTime);
                avatar.UpdatePosition(owner);
            }
            return owner.owner_id;
        }
        public ulong GetUpdateTime()
        {
            return this.updateTime;
        }
        public async Task DoFlushAsync(IPduManager pduManager)
        {
            IPdu pdu = pduManager.ReadPdu(target_object.name, ShareSimServer.pduOwner);
            if (pdu == null)
            {
                throw new Exception("Can not get pdu of owner on " + target_object.name);
            }
            ShareObjectOwner owner = new ShareObjectOwner(pdu);
            owner.owner_id = current_owner_id;
            var key = pduManager.WritePdu(owner.object_name, pdu);
            _ = await pduManager.FlushPdu(key);
            return;
        }

        public void DoStop()
        {
            if (IsOwner(device_owner_id))
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
