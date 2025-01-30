using hakoniwa.ar.bridge.sharesim;
using hakoniwa.pdu.interfaces;
using hakoniwa.pdu.msgs.geometry_msgs;
using hakoniwa.pdu.msgs.hako_msgs;
using UnityEngine;

namespace hakoniwa.ar.bridge.sharesim
{
    public class DefaultShareSimAvatar : MonoBehaviour, IShareSimAvatar
    {
        private GameObject body;
        private Rigidbody rd;

        public void Initialize(GameObject target)
        {
            this.body = target;
            this.rd = this.body.GetComponentInChildren<Rigidbody>();
        }

        public void StartAvatarProc()
        {
            if (this.rd != null)
            {
                this.rd.isKinematic = true;
            }
        }

        public void StopAvatarProc()
        {
            if (this.rd != null)
            {
                this.rd.isKinematic = false;
            }
        }

        public void UpdatePosition(ShareObjectOwner owner)
        {
            UpdatePosition(owner.pos);
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
