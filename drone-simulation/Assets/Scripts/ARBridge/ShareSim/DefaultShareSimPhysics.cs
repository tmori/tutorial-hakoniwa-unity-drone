using hakoniwa.ar.bridge.sharesim;
using hakoniwa.pdu.interfaces;
using hakoniwa.pdu.msgs.geometry_msgs;
using hakoniwa.pdu.msgs.hako_msgs;
using UnityEngine;

namespace hakoniwa.ar.bridge.sharesim
{
    public class DefaultShareSimPhysics : MonoBehaviour, IShareSimPhysics
    {
        private GameObject body;
        private Rigidbody rd;
        public void Initialize(GameObject target)
        {
            this.body = target;
            this.rd = this.body.GetComponentInChildren<Rigidbody>();
            if (rd == null)
            {
                throw new System.Exception("Can not find rigidbody on " + target.name);
            }
        }

        public void StartPhysics()
        {
            this.rd.isKinematic = false;
        }

        public void StopPhysics()
        {
            this.rd.isKinematic = true;
        }

        public void UpdatePosition(ShareObjectOwner owner)
        {
            SetPosition(owner.pos, body.transform.position, body.transform.eulerAngles);
        }
        public static void SetPosition(Twist pos, UnityEngine.Vector3 unity_pos, UnityEngine.Vector3 unity_rot)
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