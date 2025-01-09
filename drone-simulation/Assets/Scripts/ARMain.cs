using System.Threading.Tasks;
using hakoniwa.ar.bridge;
using UnityEngine;

public class ARMain : MonoBehaviour, IHakoniwaArBridgePlayer
{
    private HakoniwaArBridgeDevice bridge;
    public GameObject base_object;
    private HakoDroneInputManager drone_input;
    private HakoDroneXrInputManager xr_drone_input;
    private Vector3 base_pos;
    private Vector3 base_rot;
    public bool xr = false;
    // 移動速度（m/s）
    public float moveSpeed =  0.1f;
    // 回転速度（度/s）
    public float rotationSpeed = 1.0f;

    public void GetBasePosition(out HakoVector3 position, out HakoVector3 rotation)
    {
        position = new HakoVector3(
            base_pos.x,
            base_pos.y,
            base_pos.z
            );
        rotation = new HakoVector3(
            base_rot.x,
            base_rot.y,
            base_rot.z
            );
    }

    public void ResetPostion()
    {
        //TODO
    }

    public Task<bool> StartService(string serverUri)
    {
        //TODO
        return Task.FromResult<bool>(true);
    }

    public bool StopService()
    {
        //TODO Websocket
        return true;
    }

    public void UpdateAvatars()
    {
        //TODO
    }

    public void UpdatePosition(HakoVector3 position, HakoVector3 rotation)
    {
        Vector2 left_value;
        Vector2 right_value;
        if (xr)
        {
            left_value = xr_drone_input.GetLeftStickInput();
            right_value = xr_drone_input.GetRightStickInput();
        }
        else
        {
            left_value = drone_input.GetLeftStickInput();
            right_value = drone_input.GetRightStickInput();
        }
        //Debug.Log("left_value: " + left_value);
        //Debug.Log("Right_value: " + right_value);
        // 時間を考慮して位置と回転を更新
        float deltaTime = Time.fixedDeltaTime;

        // 移動計算（速度を考慮）
        base_pos.x += right_value.x * moveSpeed * deltaTime;
        base_pos.y += left_value.y * moveSpeed * deltaTime;
        base_pos.z += right_value.y * moveSpeed * deltaTime;

        // 回転計算（速度を考慮）
        base_rot.y += left_value.x * rotationSpeed * deltaTime;
        base_object.transform.position = base_pos;
        base_object.transform.localEulerAngles = base_rot;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (xr)
        {
            xr_drone_input = HakoDroneXrInputManager.Instance;
        }
        else
        {
            drone_input = HakoDroneInputManager.Instance;
        }
        base_pos = new Vector3();
        base_rot = new Vector3();
        bridge = HakoniwaArBridgeDevice.Instance;
        bridge.Register(this);
        bridge.Start();
    }
    void Update()
    {
        bool o_button_off = false;
        bool r_button_off = false;
        if (xr)
        {
            o_button_off = xr_drone_input.IsXButtonPressed();
            //TODO
        }
        else
        {
            o_button_off = drone_input.IsOButtonReleased();
            r_button_off = drone_input.IsRButtonReleased();
        }
        if (bridge.GetState() == BridgeState.POSITIONING && o_button_off)
        {
            Debug.Log("o_button_off: " + o_button_off);
            bridge.DevicePlayStartEvent();
        }
        else if (bridge.GetState() == BridgeState.PLAYING && r_button_off)
        {
            Debug.Log("r_button_off: " + o_button_off);
            bridge.DeviceResetEvent();
        }
    }


    void FixedUpdate()
    {
        //Debug.Log("STATE: " + bridge.GetState());
        bridge.Run();
    }
    void OnApplicationQuit()
    {
        bridge.Stop();
    }

    public void SetBasePosition(HakoVector3 position, HakoVector3 rotation)
    {
        base_pos.x = position.X;
        base_pos.y = position.Y;
        base_pos.z = position.Z;
        base_rot.x = rotation.X;
        base_rot.y = rotation.Y;
        base_rot.z = rotation.Z;
    }
}
