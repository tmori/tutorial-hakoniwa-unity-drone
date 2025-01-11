using System;
using hakoniwa.pdu.interfaces;
using hakoniwa.pdu.msgs.geometry_msgs;
using hakoniwa.pdu.msgs.hako_mavlink_msgs;
using hakoniwa.sim;
using hakoniwa.sim.core;
using UnityEngine;

public class DroneAvatar : MonoBehaviour, IHakoObject
{
    IHakoPdu hakoPdu;
    public string robotName = "DroneTransporter";
    public string pdu_name_propeller = "drone_motor";
    public string pdu_name_pos = "drone_pos";
    public GameObject body;
    private DronePropeller drone_propeller;

    public void EventInitialize()
    {
        Debug.Log("Event Initialize");
        if (body == null)
        {
            throw new Exception("Body is not assigned");
        }
        drone_propeller = this.GetComponentInChildren<DronePropeller>();
        if (drone_propeller == null)
        {
            throw new Exception("Can not found drone propeller");
        }


        hakoPdu = HakoAsset.GetHakoPdu();
        /*
         * Position
         */
        var ret = hakoPdu.DeclarePduForRead(robotName, pdu_name_pos);
        if (ret == false)
        {
            throw new ArgumentException($"Can not declare pdu for read: {robotName} {pdu_name_pos}");
        }
        /*
         * Propeller
         */
        ret = hakoPdu.DeclarePduForRead(robotName, pdu_name_propeller);
        if (ret == false)
        {
            throw new ArgumentException($"Can not declare pdu for read: {robotName} {pdu_name_propeller}");
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
        IPdu pdu_pos = pduManager.ReadPdu(robotName, pdu_name_pos);
        Twist pos = new Twist(pdu_pos);
        UpdatePosition(pos);

        /*
         * Propeller
         */
        IPdu pdu_propeller = pduManager.ReadPdu(robotName, pdu_name_propeller);
        HakoHilActuatorControls propeller = new HakoHilActuatorControls(pdu_propeller);
        //Debug.Log("c1: " + propeller.controls[0]);
        drone_propeller.Rotate((float)propeller.controls[0], (float)propeller.controls[1], (float)propeller.controls[2], (float)propeller.controls[3]);
    }

    private void UpdatePosition(Twist pos)
    {
        UnityEngine.Vector3 unity_pos = new UnityEngine.Vector3();
        unity_pos.z = (float)pos.linear.x;
        unity_pos.x = -(float)pos.linear.y;
        unity_pos.y = (float)pos.linear.z;
        body.transform.position = unity_pos;

        float rollDegrees = Mathf.Rad2Deg * (float)pos.angular.x;
        float pitchDegrees = Mathf.Rad2Deg * (float)pos.angular.y;
        float yawDegrees = Mathf.Rad2Deg * (float)pos.angular.z;

        UnityEngine.Quaternion rotation = UnityEngine.Quaternion.Euler(pitchDegrees, -yawDegrees, -rollDegrees);
        body.transform.rotation = rotation;
    }
}
