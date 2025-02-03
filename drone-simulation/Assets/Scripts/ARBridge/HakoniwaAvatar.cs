using UnityEngine;
using System.Collections;
using hakoniwa.sim;
using System;
using hakoniwa.sim.core;
using hakoniwa.pdu.interfaces;
using hakoniwa.pdu.msgs.geometry_msgs;

namespace hakoniwa.ar.bridge
{
    public class HakoniwaAvatar : MonoBehaviour, IHakoObject
    {
        IHakoPdu hakoPdu;
        public string robotName = "Player1";
        public string pdu_name = "head";
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
            var ret = hakoPdu.DeclarePduForRead(robotName, pdu_name);
            if (ret == false)
            {
                throw new ArgumentException($"Can not declare pdu for read: {robotName} {pdu_name}");
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

        public void EventTick()
        {
            var pduManager = hakoPdu.GetPduManager();
            if (pduManager == null)
            {
                return;
            }

            /*
             * Position
             */
            IPdu pdu_pos = pduManager.ReadPdu(robotName, pdu_name);
            if (pdu_pos == null)
            {
               // Debug.Log("Can not get pdu of pos");
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