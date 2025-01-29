using System.Threading.Tasks;
using hakoniwa.pdu.interfaces;
using hakoniwa.pdu.msgs.geometry_msgs;
using UnityEngine;

namespace hakoniwa.ar.bridge
{
    public class HakoniwaDevicePlayer : MonoBehaviour, IHakoniwaArObject
    {
        public string robotName = "Player1";
        public string pdu_name = "head";
        public GameObject body;

        void Start()
        {
            if (body == null)
            {
                throw new System.Exception("Body is not assigned");
            }
        }
        public async Task DeclarePduAsync(string type_name, string robot_name)
        {
            var pdu_manager = ARBridge.Instance.Get();
            if (pdu_manager == null)
            {
                throw new System.Exception("Can not get Pdu Manager");
            }
            this.robotName = robot_name;
            var ret = await pdu_manager.DeclarePduForWrite(robotName, pdu_name);
            Debug.Log($"declare for write pdu_name: {robotName}/{pdu_name} ret = {ret}");
        }
        async void FixedUpdate()
        {
            var pdu_manager = ARBridge.Instance.Get();
            if (pdu_manager == null)
            {
                return;
            }
            INamedPdu npdu = pdu_manager.CreateNamedPdu(robotName, pdu_name);
            if (npdu == null)
            {
                throw new System.Exception($"Can not find npud: {robotName} / {pdu_name}");
            }
            Twist pdu = new Twist(npdu.Pdu);
            SetPosition(pdu, body.transform.position, body.transform.eulerAngles);
            pdu_manager.WriteNamedPdu(npdu);
            var ret = await pdu_manager.FlushNamedPdu(npdu);
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
