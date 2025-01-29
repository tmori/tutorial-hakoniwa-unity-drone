using UnityEngine;
using System.Collections;
using hakoniwa.sim;
using System;
using hakoniwa.sim.core;
using hakoniwa.pdu.interfaces;
using hakoniwa.pdu.msgs.geometry_msgs;
using System.Threading.Tasks;

namespace hakoniwa.ar.bridge
{
    public class HakoniwaPlayer : MonoBehaviour, IHakoObject
    {
        IHakoPdu hakoPdu;
        public string robotName = "Baggage";
        public string pdu_name = "pos";
        public GameObject body;

        public void EventInitialize()
        {
            Debug.Log("Event Initialize");
            if (body == null)
            {
                throw new Exception("Body is not assigned");
            }
            hakoPdu = HakoAsset.GetHakoPdu();
            /*
             * Position
             */
            var ret = hakoPdu.DeclarePduForWrite(robotName, pdu_name);
            if (ret == false)
            {
                throw new ArgumentException($"Can not declare pdu for write: {robotName} {pdu_name}");
            }
        }

        public void EventReset()
        {
            //nothing to do
        }

        public void EventStart()
        {
            //nothing to do
        }

        public void EventStop()
        {
            //nothing to do
        }
        private async Task EventTickAsync()
        {
            var pdu_manager = hakoPdu.GetPduManager();
            if (pdu_manager == null)
            {
                Debug.LogWarning("PDU Manager is null. Skipping EventTick.");
                return;
            }

            INamedPdu npdu = pdu_manager.CreateNamedPdu(robotName, pdu_name);
            if (npdu == null)
            {
                throw new System.Exception($"Can not find npdu: {robotName} / {pdu_name}");
            }

            Twist pdu = new Twist(npdu.Pdu);
            SetPosition(pdu, body.transform.position, body.transform.localEulerAngles);
            pdu_manager.WriteNamedPdu(npdu);

            bool ret = await pdu_manager.FlushNamedPdu(npdu);
            if (!ret)
            {
                Debug.LogError($"FlushNamedPdu failed for {robotName} / {pdu_name}");
            }
        }

        /// <summary>
        /// `FixedUpdate()` から呼び出される。  
        /// 内部で非同期処理を実行するが、`FixedUpdate()` 内で確実に完了するように `GetAwaiter().GetResult()` で同期的に実行する。  
        /// </summary>
        public void EventTick()
        {
            try
            {
                EventTickAsync().GetAwaiter().GetResult(); // 非同期を同期処理として完了させる
            }
            catch (Exception ex)
            {
                Debug.LogError($"EventTickAsync() failed: {ex}");
            }
        }
        private void SetPosition(Twist pos, UnityEngine.Vector3 unity_pos, UnityEngine.Vector3 unity_rot)
        {
            pos.linear.x = unity_pos.z;
            pos.linear.y = -unity_pos.x;
            pos.linear.z = unity_pos.y;

            pos.angular.x = -Mathf.Deg2Rad * unity_rot.z;
            pos.angular.y = Mathf.Deg2Rad * unity_rot.x;
            pos.angular.z = -Mathf.Deg2Rad * unity_rot.y;
        }

    }
}