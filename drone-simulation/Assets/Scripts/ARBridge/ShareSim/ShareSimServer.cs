using UnityEngine;
using System.Collections;
using hakoniwa.sim;
using System.Collections.Generic;
using System;
using hakoniwa.sim.core;
using hakoniwa.pdu.interfaces;
using hakoniwa.pdu.msgs.hako_msgs;
using System.Threading.Tasks;

namespace hakoniwa.ar.bridge.sharesim
{
    public enum ShareObjectOwnerRequestType
    {
        None = 0,
        Acquire = 1,
        Release = 2
    }
    public class ShareSimServer : MonoBehaviour, IHakoObject
    {
        IHakoPdu hakoPdu;
        public const uint owner_id = 0;
        public const string robotName = "ShareSim";
        public const string pduRequest = "req";
        public const string pduTime = "core_time";
        public const string pduOwner = "owner";
        public List<ShareSimObject> owners;
        public ulong timeoutDuration = 5000000; // usec

        public void EventInitialize()
        {
            hakoPdu = HakoAsset.GetHakoPdu();
            /*
             * req
             */
            var ret = hakoPdu.DeclarePduForRead(robotName, pduRequest);
            if (ret == false)
            {
                throw new ArgumentException($"Can not declare pdu for read: {robotName} {pduRequest}");
            }
            ret = hakoPdu.DeclarePduForWrite(robotName, pduRequest);
            if (ret == false)
            {
                throw new ArgumentException($"Can not declare pdu for write: {robotName} {pduRequest}");
            }
            /*
             * hako core time
             */
            ret = hakoPdu.DeclarePduForWrite(robotName, pduTime);
            if (ret == false)
            {
                throw new ArgumentException($"Can not declare pdu for read: {robotName} {pduRequest}");
            }
            /*
             * share object pdu
             */
            foreach (var owner in owners)
            {
                ret = hakoPdu.DeclarePduForRead(owner.GetName(), pduOwner);
                if (ret == false)
                {
                    throw new ArgumentException($"Can not declare pdu for read: {owner.GetName()} {pduOwner}");
                }
                ret = hakoPdu.DeclarePduForWrite(owner.GetName(), pduOwner);
                if (ret == false)
                {
                    throw new ArgumentException($"Can not declare pdu for write: {owner.GetName()} {pduOwner}");
                }
                owner.SetDeviceOwnerId(owner_id);
                owner.DoInitialize();
                owner.DoStart();
            }
        }
        private async Task DoRequestAsync(IPduManager pduManager)
        {
            IPdu pdu = pduManager.ReadPdu(robotName, pduRequest);
            if (pdu == null)
            {
                Debug.Log("Can not get pdu of req");
                return;
            }
            ShareObjectOwnerRequest req = new ShareObjectOwnerRequest(pdu);
            if (req.request_type == (uint)ShareObjectOwnerRequestType.None)
            {
                //Debug.Log("no request");
                return;
            }
            Debug.Log("request target name: " + req.object_name);
            // find object
            ShareSimObject target = null;
            foreach (var obj in owners)
            {
                if (obj.GetName() == req.object_name)
                {
                    target = obj;
                    break;
                }
            }
            if (target == null)
            {
                throw new Exception("Invalid target name: " + req.object_name);
            }
            if ((req.request_type == (uint)ShareObjectOwnerRequestType.Acquire) && (target.GetCurrentOwnerId() != owner_id))
            {
                Debug.Log($"BUSY: can not acquire owner because is target{target.GetName()} is not released. current owner is {target.GetCurrentOwnerId()}");
            }
            else
            {
                //update target owner
                target.DoStop();
                if (req.request_type == (uint)ShareObjectOwnerRequestType.Acquire) {
                    target.SetCurrentOwnerId(req.new_owner_id);
                }
                else
                {
                    target.SetCurrentOwnerId(owner_id);
                }
                target.DoFlushAsync(pduManager).GetAwaiter().GetResult();
                target.DoStart();
                Debug.Log("Updated owner: " + req.new_owner_id);
            }
            // done
            INamedPdu npdu = pduManager.CreateNamedPdu(robotName, pduRequest);
            if (npdu == null)
            {
                throw new Exception("Can not find npdu of " + pduRequest);
            }
            var wreq = new ShareObjectOwnerRequest(npdu.Pdu);
            wreq.object_name = req.object_name;
            wreq.new_owner_id = req.new_owner_id;
            wreq.request_time = req.request_time;
            wreq.request_type = (uint)ShareObjectOwnerRequestType.None; //no request
            pduManager.WriteNamedPdu(npdu);
            _ = await pduManager.FlushNamedPdu(npdu);
        }
        private async Task UpdateHakoTimAsync(IPduManager pduManager)
        {
            INamedPdu npdu = pduManager.CreateNamedPdu(robotName, pduTime);
            if (npdu == null)
            {
                throw new System.Exception($"Can not find npdu: {robotName} / {pduTime}");
            }
            SimTime sim_time = new SimTime(npdu.Pdu);
            sim_time.time_usec = (ulong)HakoAsset.GetHakoControl().GetWorldTime();
            pduManager.WriteNamedPdu(npdu);

            bool ret = await pduManager.FlushNamedPdu(npdu);
            if (!ret)
            {
                Debug.LogError($"FlushNamedPdu failed for {robotName} / {pduTime}");
            }

        }
        private bool IsTimeout(ulong lastUpdateTime)
        {
            if (lastUpdateTime == 0)
            {
                return false;
            }
            ulong sim_time = (ulong)HakoAsset.GetHakoControl().GetWorldTime();
            //Debug.Log($"sim_time = {sim_time} updateTime = {lastUpdateTime}");
            return (sim_time - lastUpdateTime) > timeoutDuration;
        }

        public async void EventTick()
        {
            var pduManager = hakoPdu.GetPduManager();
            if (pduManager == null)
            {
                return;
            }
            // request check
            // update hako time
            try
            {
                DoRequestAsync(pduManager).GetAwaiter().GetResult();
                UpdateHakoTimAsync(pduManager).GetAwaiter().GetResult();
                // avatar, physics controls
                foreach (var owner in owners)
                {
                    ulong sim_time = (ulong)HakoAsset.GetHakoControl().GetWorldTime();
                    await owner.DoUpdate(pduManager, sim_time);

                    if (!owner.IsOwner(owner_id) && IsTimeout(owner.GetUpdateTime())) {
                        Debug.LogError("ShareSim object is timeout : " + owner.GetName());
                        owner.DoStop();
                        owner.SetCurrentOwnerId(owner_id);
                        owner.DoFlushAsync(pduManager).GetAwaiter().GetResult();
                        owner.DoStart();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"DoRequestAsync() failed: {ex}");
            }
        }

        public void EventReset()
        {
        }

        public void EventStart()
        {
        }

        public void EventStop()
        {
        }

    }
}

