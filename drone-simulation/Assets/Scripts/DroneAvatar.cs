using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hakoniwa.DroneService;


public class DroneAvatar : MonoBehaviour
{
    public string droneConfigDirPath;
    private string customJsonPath = null;
    public bool isKeyboardControl = false;
    public GameObject body;
    public GameObject propeller1; // プロペラ1
    public GameObject propeller2; // プロペラ2
    public GameObject propeller3; // プロペラ3
    public GameObject propeller4; // プロペラ4
    public float maxRotationSpeed = 1f; // 最大回転速度（度/秒）
    private int radio_control_count = 0;
    public int radio_control_timeout = 50;
    public double stick_strength = 0.1;
    public double stick_yaw_strength = 1.0;
    private DroneInputActions inputActions;
    private void Awake()
    {
        // Input Actions のインスタンスを初期化
        inputActions = new DroneInputActions();
    }
    private void OnEnable()
    {
        Debug.Log("Enabled");
        // Input Actions を有効化
        inputActions.Gameplay.Enable();
    }
    private void OnDisable()
    {
        Debug.Log("Disabled");
        // Input Actions を無効化
        inputActions.Gameplay.Disable();
    }
    // Start is called before the first frame update
    void Start()
    {
        int ret = DroneServiceRC.Init(droneConfigDirPath, customJsonPath, isKeyboardControl);
        Debug.Log("Init: ret = " + ret);
        if (ret != 0)
        {
            throw new System.Exception("Can not Initialize DroneService Rc");
        }
        ret = DroneServiceRC.Start();
        if (ret != 0)
        {
            throw new System.Exception("Can not Start DroneService Rc");
        }
        Debug.Log("Start: ret = " + ret);
    }

    // Update is called once per frame
    void Update()
    {
        if (isKeyboardControl)
        {
            HandleKeyboardInput();
        }
        else
        {
            HandleGamePadInput();
        }
        DroneServiceRC.Run();
    }
    private void HandleGamePadInput()
    {
        Vector2 rotateInput = inputActions.Gameplay.LeftStick.ReadValue<Vector2>();
        Vector2 moveInput = inputActions.Gameplay.RightStick.ReadValue<Vector2>();
        float horizontal = moveInput.x; // 左右移動
        float forward = moveInput.y;   // 前後移動

        float yaw = rotateInput.x;    // ヨー回転
        float pitch = rotateInput.y;  // 垂直移動
        //Debug.Log($"Left Stick: X = {moveInput.x}, Y = {moveInput.y}");
        //Debug.Log($"Right Stick: X = {rotateInput.x}, Y = {rotateInput.y}");
        // ボタン A (例: ラジオコントロール)
        if (inputActions.Gameplay.Xbuttonn.WasPressedThisFrame())
        {
            DroneServiceRC.PutRadioControlButton(0, 1);
            Debug.Log("GamePad: Aボタン on");
        }
        else if (inputActions.Gameplay.Xbuttonn.WasReleasedThisFrame())
        {
            DroneServiceRC.PutRadioControlButton(0, 0 );
            Debug.Log("GamePad: Aボタン off");
        }

        // ドローンサービスに入力を送る
        DroneServiceRC.PutHorizontal(0, horizontal * stick_strength); // 横移動
        DroneServiceRC.PutForward(0, -forward * stick_strength);     // 前後移動
        DroneServiceRC.PutHeading(0, yaw * stick_yaw_strength);      // 水平回転 (ヨー)
        DroneServiceRC.PutVertical(0, -pitch * stick_strength);      // 垂直移動 (ピッチ)
    }

    private void FixedUpdate()
    {
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
            // オイラー角をラジアンから度に変換
            float rollDegrees = Mathf.Rad2Deg * (float)roll;
            float pitchDegrees = Mathf.Rad2Deg * (float)pitch;
            float yawDegrees = Mathf.Rad2Deg * (float)yaw;

            // Unityの回転に適用（Quaternionを使用）
            Quaternion rotation = Quaternion.Euler(pitchDegrees, -yawDegrees, -rollDegrees);
            body.transform.rotation = rotation;
        }
        // ドローンサービスからコントロールデータを取得
        double c1, c2, c3, c4, c5, c6, c7, c8;
        ret = DroneServiceRC.GetControls(0, out c1, out c2, out c3, out c4, out c5, out c6, out c7, out c8);

        if (ret == 0)
        {
            // デューティレート（c1 ～ c4）に応じてプロペラを回転
            RotatePropeller(propeller1, (float)c1);
            RotatePropeller(propeller2, (float)c2);
            RotatePropeller(propeller3, (float)c3);
            RotatePropeller(propeller4, (float)c4);
        }
    }
    private void RotatePropeller(GameObject propeller, float dutyRate)
    {
        if (propeller == null) return;

        // デューティレートを回転速度に変換
        float rotationSpeed = maxRotationSpeed * dutyRate;

        // プロペラをY軸回転（必要に応じて軸を変更）
        propeller.transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);
    }
    private void HandleKeyboardInput()
    {
        // Stick operations
        if (Input.GetKey(KeyCode.W))
            DroneServiceRC.PutVertical(0, -stick_strength);
        if (Input.GetKey(KeyCode.S))
            DroneServiceRC.PutVertical(0, stick_strength);
        if (Input.GetKey(KeyCode.A))
            DroneServiceRC.PutHeading(0, -stick_yaw_strength);
        if (Input.GetKey(KeyCode.D))
            DroneServiceRC.PutHeading(0, stick_yaw_strength);
        if (Input.GetKey(KeyCode.I))
            DroneServiceRC.PutForward(0, -stick_strength);
        if (Input.GetKey(KeyCode.K))
            DroneServiceRC.PutForward(0, stick_strength);
        if (Input.GetKey(KeyCode.J))
            DroneServiceRC.PutHorizontal(0, -stick_strength);
        if (Input.GetKey(KeyCode.L))
            DroneServiceRC.PutHorizontal(0, stick_strength);

        // Button operations
        if (Input.GetKeyDown(KeyCode.X))
        {
            DroneServiceRC.PutRadioControlButton(0, 1);
            Debug.Log("Input X on");
            radio_control_count = radio_control_timeout;
        }

        // Get position
        if (Input.GetKeyDown(KeyCode.P))
        {
            double x, y, z;
            DroneServiceRC.GetPosition(0, out x, out y, out z);
            Debug.Log($"Position: x={x:F1}, y={y:F1}, z={z:F1}");
        }

        // Get attitude
        if (Input.GetKeyDown(KeyCode.R))
        {
            double roll, pitch, yaw;
            DroneServiceRC.GetAttitude(0, out roll, out pitch, out yaw);
            Debug.Log($"Attitude: roll={roll:F1}, pitch={pitch:F1}, yaw={yaw:F1}");
        }

        // Get simulation time
        if (Input.GetKeyDown(KeyCode.T))
        {
            ulong simTime = DroneServiceRC.GetTimeUsec(0);
            Debug.Log($"Simulation Time (usec): {simTime}");
        }

    }
    private void OnApplicationQuit()
    {
        int ret = DroneServiceRC.Stop();
        Debug.Log("Stop: ret = " + ret);
    }
}
