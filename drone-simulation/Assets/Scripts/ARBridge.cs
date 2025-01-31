using System.Collections.Generic;
using System.Threading.Tasks;
using hakoniwa.ar.bridge;
using hakoniwa.environment.impl;
using hakoniwa.environment.interfaces;
using hakoniwa.pdu.core;
using hakoniwa.pdu.interfaces;
using UnityEngine;

public class ARBridge : MonoBehaviour, IHakoniwaArBridgePlayer, IHakoPduInstance
{
    public static IHakoPduInstance Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // シーンをまたいで保持
        }
        else if (!ReferenceEquals(Instance, this)) // Unity 特有の比較の警告を避ける
        {
            Destroy(gameObject);
        }
    }

    private  IHakoniwaArBridge bridge;
    public GameObject base_object;
    private IDroneInput drone_input;
    private Vector3 base_pos;
    private Vector3 base_rot;

    public GameObject player_obj;
    public List<GameObject> avatar_objs;
    private IHakoniwaArObject ar_player;
    private List<IHakoniwaArObject> ar_avatars;
    private IPduManager mgr = null;
    private IEnvironmentService service;

    public bool xr = false;
    public float moveSpeed =  0.1f;
    public float rotationSpeed = 1.0f;

    public IPduManager Get()
    {
        if (mgr == null)
        {
            return null;
        }
        if (mgr.IsServiceEnabled() == false)
        {
            Debug.Log("SERVER IS NOT ENABLED");
            return null;
        }
        return mgr;
    }

    public void setPositioningSpeed(float rotation, float move)
    {
        moveSpeed = move;
        rotationSpeed = rotation;
    }

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

    public Task<bool> StartService(string serverUri)
    {
        service = EnvironmentServiceFactory.Create("websocket_dotnet", "unity", ".");
        mgr = new PduManager(service, ".");
        Debug.Log("Start Service!! " + serverUri);
        var ret = mgr.StartService(serverUri);
        //bool ret = true;
        Debug.Log("Start Service!! " + serverUri + " ret: " + ret);
        return ret;
        //return Task<bool>.FromResult(ret);
    }

    public bool StopService()
    {
        if (mgr != null)
        {
            mgr.StopService();
            mgr = null;
        }
        return true;
    }

    public void UpdatePosition(HakoVector3 position, HakoVector3 rotation)
    {
        Vector2 left_value;
        Vector2 right_value;
        left_value = drone_input.GetLeftStickInput();
        right_value = drone_input.GetRightStickInput();

        float deltaTime = Time.fixedDeltaTime;

        // 移動計算（速度を考慮）
        base_pos.x += right_value.x * moveSpeed * deltaTime;
        base_pos.y += left_value.y * moveSpeed * deltaTime;
        base_pos.z += right_value.y * moveSpeed * deltaTime;

        // 回転計算（速度を考慮）
        base_rot.y += left_value.x * rotationSpeed * deltaTime;
        base_object.transform.position = base_pos;
        base_object.transform.eulerAngles = base_rot;

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ar_player = player_obj.GetComponentInChildren<IHakoniwaArObject>();
        if (ar_player == null)
        {
            throw new System.Exception("Can not find Player ar obj");
        }
        ar_avatars = new List<IHakoniwaArObject>();
        foreach (var entry in avatar_objs)
        {
            var e = entry.GetComponentInChildren<IHakoniwaArObject>();
            if (e == null)
            {
                throw new System.Exception("Can not find Avatar ar obj");
            }
            ar_avatars.Add(e);
        }
        if (xr)
        {
            drone_input = HakoDroneXrInputManager.Instance;
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
        o_button_off = drone_input.IsXButtonPressed();
        r_button_off = drone_input.IsYButtonPressed();
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

    public async Task InitializeAsync(PlayerData player, List<AvatarData> avatars)
    {
        //Debug.Log("Player: " + player.Name);
        await ar_player.DeclarePduAsync(null, null);
        foreach (var avatar in ar_avatars)
        {
            //Debug.Log("avatar: " + avatar.Name);
            await avatar.DeclarePduAsync(null, null);
        }
    }
}
