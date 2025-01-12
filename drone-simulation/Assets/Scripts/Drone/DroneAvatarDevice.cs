using System;
using hakoniwa.pdu.interfaces;
using hakoniwa.pdu.msgs.geometry_msgs;
using hakoniwa.pdu.msgs.hako_mavlink_msgs;
using UnityEngine;

public class DroneAvatarDevice : MonoBehaviour
{
    public string robotName = "DroneTransporter";
    public string pdu_name_propeller = "drone_motor";
    public string pdu_name_pos = "drone_pos";
    public GameObject body;
    private DronePropeller drone_propeller;

    void Start()
    {
        if (body == null)
        {
            throw new Exception("Body is not assigned");
        }
        drone_propeller = this.GetComponentInChildren<DronePropeller>();
        if (drone_propeller == null)
        {
            throw new Exception("Can not found drone propeller");
        }
    }
    void FixedUpdate()
    {
        var pduManager = ARBridge.Instance.Get();
        if (pduManager == null)
        {
            return;
        }

        /*
         * Position
         */
        IPdu pdu_pos = pduManager.ReadPdu(robotName, pdu_name_pos);
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

        /*
         * Propeller
         */
        IPdu pdu_propeller = pduManager.ReadPdu(robotName, pdu_name_propeller);
        if (pdu_propeller == null)
        {
            Debug.Log("Can not get pdu of propeller");
        }
        else
        {
            HakoHilActuatorControls propeller = new HakoHilActuatorControls(pdu_propeller);
            //Debug.Log("c1: " + propeller.controls[0]);
            drone_propeller.Rotate((float)propeller.controls[0], (float)propeller.controls[1], (float)propeller.controls[2], (float)propeller.controls[3]);
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
