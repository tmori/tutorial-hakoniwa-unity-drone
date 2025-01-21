using UnityEngine;
using Hakoniwa.DroneService;
using System;
using hakoniwa.pdu.interfaces;
using hakoniwa.pdu.msgs.geometry_msgs;
using hakoniwa.pdu.msgs.hako_mavlink_msgs;
using hakoniwa.ar.bridge;
using System.Threading.Tasks;

public class DronePlayer : MonoBehaviour, IHakoniwaArObject
{
    public GameObject body;
    private DroneCollision my_collision;
    private DroneControl drone_control;
    private DronePropeller drone_propeller;
    private IHakoniwaArBridge ibridge;

    public string debug_logpath = null;

    public string robotName = "DroneTransporter";
    public string pdu_name_propeller = "drone_motor";
    public string pdu_name_pos = "drone_pos";
    private void SetPosition(Twist pos, UnityEngine.Vector3 unity_pos)
    {
        pos.linear.x = unity_pos.z;
        pos.linear.y = -unity_pos.x;
        pos.linear.z = unity_pos.y;

        UnityEngine.Vector3 unity_rot = body.transform.localEulerAngles;
        pos.angular.x = -Mathf.Deg2Rad * unity_rot.z;
        pos.angular.y = Mathf.Deg2Rad * unity_rot.x;
        pos.angular.z = -Mathf.Deg2Rad * unity_rot.y;
    }

    private async void FlushPduPos(UnityEngine.Vector3 unity_pos)
    {
        var pdu_manager = ARBridge.Instance.Get();
        if (pdu_manager == null)
        {
            return;
        }
        /*
         * Position
         */
        INamedPdu npdu_pos = pdu_manager.CreateNamedPdu(robotName, pdu_name_pos);
        if (npdu_pos == null || npdu_pos.Pdu == null)
        {
            throw new Exception($"Can not find npdu: {robotName} {pdu_name_pos}");
        }
        Twist pos = new Twist(npdu_pos.Pdu);
        SetPosition(pos, unity_pos);
        pdu_manager.WriteNamedPdu(npdu_pos);
        var ret = await pdu_manager.FlushNamedPdu(npdu_pos);
        //Debug.Log("Flush result: " + ret);
    }
    private async void FlushPduPropeller(float c1, float c2, float c3, float c4)
    {
        var pdu_manager = ARBridge.Instance.Get();
        if (pdu_manager == null)
        {
            return;
        }

        /*
         * Propeller
         */
        INamedPdu npdu = pdu_manager.CreateNamedPdu(robotName, pdu_name_propeller);
        if (npdu == null || npdu.Pdu == null)
        {
            throw new Exception($"Can not find npdu: {robotName} {pdu_name_propeller}");
        }

        HakoHilActuatorControls actuator = new HakoHilActuatorControls(npdu.Pdu);
        float[] controls = new float[16];
        controls[0] = c1;
        controls[1] = c2;
        controls[2] = c3;
        controls[3] = c4;
        actuator.controls = controls;
        //float[] temp = npdu.Pdu.GetDataArray<float>("controls");
        //Debug.Log("size: " + temp.Length);
        //Debug.Log("controls[0]: " + temp[0]);

        pdu_manager.WriteNamedPdu(npdu);
        var ret = await pdu_manager.FlushNamedPdu(npdu);
        //Debug.Log("Flush result: " + ret);
    }
    public async Task DeclarePduAsync(string type_name, string robot_name)
    {
        var pdu_manager = ARBridge.Instance.Get();
        if (pdu_manager == null)
        {
            throw new Exception("Can not get Pdu Manager");
        }
        this.robotName = robot_name;
        var ret = await pdu_manager.DeclarePduForWrite(robotName, pdu_name_pos);
        Debug.Log("declare pdu pos: " + ret);
        ret = await pdu_manager.DeclarePduForWrite(robotName, pdu_name_propeller);
        Debug.Log("declare pdu propeller: " + ret);
    }

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

        int ret = DroneServiceRC.InitSingle(droneConfigText, controllerConfigText, loggerEnable: false, debug_logpath);
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
            UnityEngine.Vector3 unity_pos = new UnityEngine.Vector3();
            unity_pos.z = (float)x;
            unity_pos.x = -(float)y;
            unity_pos.y = (float)z;
            body.transform.position = unity_pos;
            FlushPduPos(unity_pos);
        }
        double roll, pitch, yaw;
        ret = DroneServiceRC.GetAttitude(0, out roll, out pitch, out yaw);
        if (ret == 0)
        {
            float rollDegrees = Mathf.Rad2Deg * (float)roll;
            float pitchDegrees = Mathf.Rad2Deg * (float)pitch;
            float yawDegrees = Mathf.Rad2Deg * (float)yaw;

            UnityEngine.Quaternion rotation = UnityEngine.Quaternion.Euler(pitchDegrees, -yawDegrees, -rollDegrees);
            body.transform.rotation = rotation;
        }

        double c1, c2, c3, c4, c5, c6, c7, c8;
        ret = DroneServiceRC.GetControls(0, out c1, out c2, out c3, out c4, out c5, out c6, out c7, out c8);
        if (ret == 0)
        {
            drone_propeller.Rotate((float)c1, (float)c2, (float)c3, (float)c4);
            FlushPduPropeller((float)c1, (float)c2, (float)c3, (float)c4);
        }
    }

    private void OnApplicationQuit()
    {
        int ret = DroneServiceRC.Stop();
        Debug.Log("Stop: ret = " + ret);
    }

}
