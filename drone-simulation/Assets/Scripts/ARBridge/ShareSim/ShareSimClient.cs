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
        public uint GetOwnerId()
        {
            return my_owner_id;
        }
        private static ShareSimClient _instance;

        /// <summary>
        /// シングルトンのインスタンス取得
        /// </summary>
        public static ShareSimClient Instance
        {
            get
            {
                if (_instance == null)
                {
                    Debug.LogError("ShareSimClient instance is not initialized.");
                }
                return _instance;
            }
        }

        private void Awake()
        {
            // すでにインスタンスが存在する場合は、重複を防ぐ
            if (_instance != null && _instance != this)
            {
                Debug.LogWarning("Multiple ShareSimClient instances detected. Destroying duplicate.");
                Destroy(this.gameObject);
                return;
            }

            _instance = this;
        }

        private void OnDestroy()
        {
            // インスタンスが削除された場合、参照をクリア
            if (_instance == this)
            {
                _instance = null;
            }
        }

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
                owner.SetOwnerId(my_owner_id);
                owner.SetTargetOwnerId(ShareSimServer.owner_id);
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
        async void FixedUpdate()
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
                    var owner_id = await owner.DoUpdate(pduManager, sim_time);
                    Debug.Log($"update owner_id= {owner_id} targetOwnerId: {owner.GetTargetOwnerId()}");
                    if (owner_id == uint.MaxValue)
                    {
                        //nothing to do
                    }
                    else if (owner_id != owner.GetTargetOwnerId())
                    {
                        Debug.Log($"update target owner id: new_owner_id= {owner_id} targetOwnerId: {owner.GetTargetOwnerId()}");
                        owner.DoStop();
                        owner.SetTargetOwnerId(owner_id);
                        owner.DoStart();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"DoRequestAsync() failed: {ex}");
            }
        }
        public uint GetTargetOwnerId(string object_name)
        {
            foreach (var owner in owners)
            {
                if (owner.GetName() == object_name)
                {
                    return owner.GetTargetOwnerId();
                }
            }
            return uint.MaxValue;
        }
        public bool SetTargetOwnerId(string object_name, uint target_owner_id)
        {
            foreach (var owner in owners)
            {
                if (owner.GetName() == object_name)
                {
                    owner.SetTargetOwnerId(target_owner_id);
                    return true;
                }
            }
            return false;
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

