using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hakoniwa.DroneService;
using System;
using hakoniwa.ar.bridge;

public class Drone : MonoBehaviour
{
    public GameObject body;
    public GameObject propeller1; // プロペラ1
    public GameObject propeller2; // プロペラ2
    public GameObject propeller3; // プロペラ3
    public GameObject propeller4; // プロペラ4
    public GameObject propeller5;
    public GameObject propeller6;
    public float maxRotationSpeed = 1f; // 最大回転速度（度/秒）
    public bool xr;
    private DroneCollision my_collision;

    public int radio_control_timeout = 50;
    public double stick_strength = 0.1;
    public double stick_yaw_strength = 1.0;

    private HakoDroneInputManager drone_input;
    private HakoDroneXrInputManager xr_drone_input;
    private IHakoniwaArBridge ibridge;

    void Start()
    {
        ibridge = HakoniwaArBridgeDevice.Instance;
        if (xr)
        {
            xr_drone_input = HakoDroneXrInputManager.Instance;
        }
        else
        {
            drone_input = HakoDroneInputManager.Instance;
        }
        my_collision = this.GetComponentInChildren<DroneCollision>();
        if (my_collision == null) {
            throw new Exception("Can not found collision");
        }
        my_collision.SetIndex(0);
        // Resourcesから設定ファイルをロード
        string droneConfigText = LoadTextFromResources("config/drone/rc/drone_config_0");
        string controllerConfigText = LoadTextFromResources("config/controller/param-api-mixer");

        // 必要なファイルがロードできなければ例外をスロー
        if (string.IsNullOrEmpty(droneConfigText))
        {
            throw new Exception("Failed to load droneConfigText from Resources.");
        }

        if (string.IsNullOrEmpty(controllerConfigText))
        {
            throw new Exception("Failed to load controllerConfigText from Resources.");
        }

        // DroneServiceRC.InitSingleの呼び出し
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

    /// <summary>
    /// Resourcesフォルダから指定したパスのテキストファイルをロードします。
    /// 拡張子は不要です（例: "config/drone/rc/drone_config_0"）。
    /// </summary>
    /// <param name="resourcePath">Resourcesフォルダ内の相対パス（拡張子なし）</param>
    /// <returns>ロードしたテキストデータ</returns>
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
        if (xr)
        {
            HandleXrInput();
        }
        else
        {
            HandleGamePadInput();
        }
    }
    private void HandleXrInput()
    {
        // 左スティックの入力取得 (OVRInput.RawAxis2D.LThumbstick)
        Vector2 leftStick = xr_drone_input.GetLeftStickInput();//OVRInput.Get(OVRInput.RawAxis2D.LThumbstick);
        // 右スティックの入力取得 (OVRInput.RawAxis2D.RThumbstick)
        Vector2 rightStick = xr_drone_input.GetRightStickInput();//OVRInput.Get(OVRInput.RawAxis2D.RThumbstick);
        float horizontal = rightStick.x; // 左右移動
        float forward = rightStick.y;   // 前後移動

        float yaw = leftStick.x;      // ヨー回転
        float pitch = leftStick.y;    // 垂直移動

        // Debug表示 (必要に応じて)
        if (leftStick != Vector2.zero)
            Debug.Log($"左スティック: X={leftStick.x}, Y={leftStick.y}");
        if (rightStick != Vector2.zero)
            Debug.Log($"右スティック: X={rightStick.x}, Y={rightStick.y}");

        // ボタンAの入力取得
        //if (OVRInput.GetDown(OVRInput.RawButton.A))
        if (xr_drone_input.IsXButtonPressed())
        {
            DroneServiceRC.PutRadioControlButton(0, 1); // ボタンONを送信
            Debug.Log("AボタンDOWN");
        }
        //else if (OVRInput.GetUp(OVRInput.RawButton.A))
        else if (xr_drone_input.IsXButtonReleased())
        {
            DroneServiceRC.PutRadioControlButton(0, 0); // ボタンOFFを送信
            Debug.Log("AボタンUP");
        }

        // ドローンサービスに入力を送る
        DroneServiceRC.PutHorizontal(0, horizontal * stick_strength);  // 横移動
        DroneServiceRC.PutForward(0, -forward * stick_strength);       // 前後移動
        DroneServiceRC.PutHeading(0, yaw * stick_yaw_strength);        // 水平回転 (ヨー)
        DroneServiceRC.PutVertical(0, -pitch * stick_strength);        // 垂直移動 (ピッチ)
    }
    private void HandleGamePadInput()
    {
        Vector2 rotateInput = drone_input.GetLeftStickInput(); //inputActions.Gameplay.LeftStick.ReadValue<Vector2>();
        Vector2 moveInput = drone_input.GetRightStickInput(); //inputActions.Gameplay.RightStick.ReadValue<Vector2>();
        float horizontal = moveInput.x; // 左右移動
        float forward = moveInput.y;   // 前後移動

        float yaw = rotateInput.x;    // ヨー回転
        float pitch = rotateInput.y;  // 垂直移動
        //Debug.Log($"Left Stick: X = {moveInput.x}, Y = {moveInput.y}");
        //Debug.Log($"Right Stick: X = {rotateInput.x}, Y = {rotateInput.y}");
        // ボタン A (例: ラジオコントロール)
        //if (inputActions.Gameplay.Xbuttonn.WasPressedThisFrame())
        if (drone_input.IsXButtonPressed())
        {
            DroneServiceRC.PutRadioControlButton(0, 1);
            Debug.Log("GamePad: Aボタン on");
        }
        //else if (inputActions.Gameplay.Xbuttonn.WasReleasedThisFrame())
        else if (drone_input.IsXButtonReleased())
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
            RotatePropeller(propeller2, -(float)c2);
            RotatePropeller(propeller3, (float)c3);
            RotatePropeller(propeller4, -(float)c4);
            if (propeller5)
            {
                RotatePropeller(propeller5, (float)c1);
            }
            if (propeller6)
            {
                RotatePropeller(propeller6, (float)c2);
            }
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

    private void OnApplicationQuit()
    {
        int ret = DroneServiceRC.Stop();
        Debug.Log("Stop: ret = " + ret);
    }
}
