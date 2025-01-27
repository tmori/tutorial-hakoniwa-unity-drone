using System.Threading.Tasks;
using hakoniwa.pdu.interfaces;
using hakoniwa.pdu.msgs.geometry_msgs;
using UnityEngine;

namespace hakoniwa.ar.bridge
{
    public class HakoniwaDeviceAvatar : MonoBehaviour, IHakoniwaArObject
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
            var ret = await pdu_manager.DeclarePduForRead(robotName, pdu_name);
            Debug.Log($"declare for read pdu_name: {robotName}/{pdu_name} ret = {ret}");
        }
        void FixedUpdate()
        {
            var pdu_manager = ARBridge.Instance.Get();
            if (pdu_manager == null)
            {
                return;
            }

            /*
             * Position
             */
            IPdu pdu_pos = pdu_manager.ReadPdu(robotName, pdu_name);
            if (pdu_pos == null)
            {
                Debug.Log("Can not get pdu of pos");
            }
            else
            {
                Twist pos = new Twist(pdu_pos);
                //Debug.Log($"Twist ({pos.linear.x} {pos.linear.y} {pos.linear.z})");
                UpdatePosition(pos);
            }
        }

        private void UpdatePosition(Twist pos)
        {
            UnityEngine.Vector3 unity_pos = new UnityEngine.Vector3();
            unity_pos.z = (float)pos.linear.x;
            unity_pos.x = -(float)pos.linear.y;
            unity_pos.y = (float)pos.linear.z;
            body.transform.position = unity_pos;
            //Debug.Log("pos: " + body.transform.position);

            float rollDegrees = Mathf.Rad2Deg * (float)pos.angular.x;
            float pitchDegrees = Mathf.Rad2Deg * (float)pos.angular.y;
            float yawDegrees = Mathf.Rad2Deg * (float)pos.angular.z;

            UnityEngine.Quaternion rotation = UnityEngine.Quaternion.Euler(pitchDegrees, -yawDegrees, -rollDegrees);
            body.transform.rotation = rotation;
        }

    }
}
