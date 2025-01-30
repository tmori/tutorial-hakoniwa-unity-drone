using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using hakoniwa.pdu.interfaces;
using hakoniwa.pdu.msgs.hako_msgs;

namespace hakoniwa.ar.bridge.sharesim
{
    public class ShareSimClient : MonoBehaviour, IHakoniwaArObject
    {
        public uint my_owner_id = 1; //1: Quest1, 2: Quest2
        public List<ShareSimObject> owners;
        public async Task DeclarePduAsync(string type_name, string robot_name)
        {
            var pdu_manager = ARBridge.Instance.Get();
            if (pdu_manager == null)
            {
                throw new System.Exception("Can not get Pdu Manager");
            }
            /*
             * Req
             */
            var ret = await pdu_manager.DeclarePduForWrite(ShareSimServer.robotName, ShareSimServer.pduRequest);
            if (ret == false)
            {
                throw new ArgumentException($"Can not declare pdu for write: {ShareSimServer.robotName} {ShareSimServer.pduRequest}");
            }
            /*
             * hako core time
             */
            ret = await pdu_manager.DeclarePduForRead(ShareSimServer.robotName, ShareSimServer.pduTime);
            if (ret == false)
            {
                throw new ArgumentException($"Can not declare pdu for read: {ShareSimServer.robotName} {ShareSimServer.pduRequest}");
            }
            /*
             * share object pdu
             */
            foreach (var owner in owners)
            {
                ret = await pdu_manager.DeclarePduForRead(owner.GetName(), ShareSimServer.pduOwner);
                if (ret == false)
                {
                    throw new ArgumentException($"Can not declare pdu for read: {owner.GetName()} {ShareSimServer.pduOwner}");
                }
                ret = await pdu_manager.DeclarePduForWrite(owner.GetName(), ShareSimServer.pduOwner);
                if (ret == false)
                {
                    throw new ArgumentException($"Can not declare pdu for write: {owner.GetName()} {ShareSimServer.pduOwner}");
                }
                owner.SetOwnerId(ShareSimServer.owner_id);
                owner.DoInitialize();
                owner.DoStart();
            }
        }
        private ulong hako_time;
        public ulong GetHakoTime()
        {
            return hako_time;
        }
        void Start()
        {
            hako_time = 0;
        }
        void FixedUpdate()
        {
            var pduManager = ARBridge.Instance.Get();
            if (pduManager == null)
            {
                return;
            }
            try
            {
                //hako time
                var pdu = pduManager.ReadPdu(ShareSimServer.robotName, ShareSimServer.pduTime);
                if (pdu != null)
                {
                    var sim_time = new SimTime(pdu);
                    this.hako_time = sim_time.time_usec;
                }
                // avatar, physics controls
                foreach (var owner in owners)
                {
                    ulong sim_time = 0; //TODO
                    owner.DoUpdate(pduManager, sim_time);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"DoRequestAsync() failed: {ex}");
            }
        }

        public async Task<bool> RequestOwnerAsync(ShareSimObject obj, ShareObjectOwnerRequestType req_type)
        {
            var pduManager = ARBridge.Instance.Get();
            if (pduManager == null)
            {
                return false;
            }
            INamedPdu npdu = pduManager.CreateNamedPdu(ShareSimServer.robotName, ShareSimServer.pduRequest);
            if (npdu == null)
            {
                Debug.Log("Can not find pdu for sharesim pdu request");
                return false;
            }
            ShareObjectOwnerRequest req = new ShareObjectOwnerRequest(npdu.Pdu);
            req.object_name = obj.GetName();
            req.request_type = (uint)req_type;
            req.request_time = 0; //TODO
            req.new_owner_id = my_owner_id;
            pduManager.WriteNamedPdu(npdu);
            var ret = await pduManager.FlushNamedPdu(npdu);
            return ret;
        }
    }
}

