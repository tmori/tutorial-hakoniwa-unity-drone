using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hakoniwa.DroneService;
using System;
using hakoniwa.ar.bridge;

public class DronePlayer : MonoBehaviour
{
    public GameObject body;
    private DroneCollision my_collision;
    private DroneControl drone_control;
    private DronePropeller drone_propeller;
    private IHakoniwaArBridge ibridge;
    public int radio_control_timeout = 50;

    void Start()
    {
        ibridge = HakoniwaArBridgeDevice.Instance;
        my_collision = this.GetComponentInChildren<DroneCollision>();
        if (my_collision == null) {
            throw new Exception("Can not found collision");
        }
        drone_control = this.GetComponentInChildren<DroneControl>();
        if (drone_control == null)
        {
            throw new Exception("Can not found drone control");
        }
        drone_propeller = this.GetComponentInChildren<DronePropeller>();
        if (drone_propeller == null)
        {
            throw new Exception("Can not found drone propeller");
        }
        my_collision.SetIndex(0);

        string droneConfigText = LoadTextFromResources("config/drone/rc/drone_config_0");
        string controllerConfigText = LoadTextFromResources("config/controller/param-api-mixer");

        if (string.IsNullOrEmpty(droneConfigText))
        {
            throw new Exception("Failed to load droneConfigText from Resources.");
        }

        if (string.IsNullOrEmpty(controllerConfigText))
        {
            throw new Exception("Failed to load controllerConfigText from Resources.");
        }

        int ret = DroneServiceRC.InitSingle(droneConfigText, controllerConfigText, loggerEnable: false);
        Debug.Log("InitSingle: ret = " + ret);

        if (ret != 0)
        {
            throw new Exception("Can not Initialize DroneService RC with InitSingle");
        }

        // DroneServiceRC.Startの呼び出し
        ret = DroneServiceRC.Start();
        Debug.Log("Start: ret = " + ret);

        if (ret != 0)
        {
            throw new Exception("Can not Start DroneService RC");
        }
    }

    private string LoadTextFromResources(string resourcePath)
    {
        TextAsset textAsset = Resources.Load<TextAsset>(resourcePath);
        return textAsset != null ? textAsset.text : null;
    }

    // Update is called once per frame
    void Update()
    {
        if (ibridge.GetState() == BridgeState.POSITIONING)
        {
            return;
        }
        drone_control.HandleInput();
    }

    private void FixedUpdate()
    {
        // 現在位置を記録
        for (int i = 0; i < 20; i++)
        {
            DroneServiceRC.Run();
        }

        double x, y, z;
        int ret = DroneServiceRC.GetPosition(0, out x, out y, out z);
        if (ret == 0)
        {
            Vector3 unity_pos = new Vector3();
            unity_pos.z = (float)x;
            unity_pos.x = -(float)y;
            unity_pos.y = (float)z;
            body.transform.position = unity_pos;

        }
        double roll, pitch, yaw;
        ret = DroneServiceRC.GetAttitude(0, out roll, out pitch, out yaw);
        if (ret == 0)
        {
            float rollDegrees = Mathf.Rad2Deg * (float)roll;
            float pitchDegrees = Mathf.Rad2Deg * (float)pitch;
            float yawDegrees = Mathf.Rad2Deg * (float)yaw;

            Quaternion rotation = Quaternion.Euler(pitchDegrees, -yawDegrees, -rollDegrees);
            body.transform.rotation = rotation;
        }

        double c1, c2, c3, c4, c5, c6, c7, c8;
        ret = DroneServiceRC.GetControls(0, out c1, out c2, out c3, out c4, out c5, out c6, out c7, out c8);
        if (ret == 0)
        {
            drone_propeller.Rotate((float)c1, (float)c2, (float)c3, (float)c4);
        }
    }

    private void OnApplicationQuit()
    {
        int ret = DroneServiceRC.Stop();
        Debug.Log("Stop: ret = " + ret);
    }
}
