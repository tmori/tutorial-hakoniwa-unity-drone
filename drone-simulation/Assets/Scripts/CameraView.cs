using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraView : MonoBehaviour
{
    [SerializeField]
    private Transform target;

    private Vector3 positionOffset;

    private Quaternion rotationOffset;

    public enum OperateMode
    {
        Mouse,
        Auto,
    }

    public OperateMode currentMode = OperateMode.Mouse;

    [SerializeField, Range(0.1f, 10f)]
    private float wheelSpeed = 8f;

    [SerializeField, Range(0.1f, 10f)]
    private float moveSpeed = 8.0f;

    [SerializeField, Range(0.1f, 10f)]
    private float rotateSpeed = 8.0f;

    [SerializeField]
    private bool useGimbal = true;

    [SerializeField, Range(0.1f, 5f)]
    private float gimbalSmoothSpeed = 2.0f;

    [SerializeField]
    private Vector3 fixedDistance = new Vector3(0, 2, -7);
    [SerializeField]
    private Vector3 fixedAngle = new Vector3(0, 0, 0); // カメラの固定角度

    private Vector3 preMousePos;

    void Start()
    {
        if (target != null)
        {
            positionOffset = fixedDistance;
            rotationOffset = Quaternion.identity;
        }

        // 初期値を保存
        prevFixedDistance = fixedDistance;
        prevFixedAngle = fixedAngle;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            if (currentMode == OperateMode.Auto)
            {
                currentMode = OperateMode.Mouse;
            }
            else
            {
                currentMode = OperateMode.Auto;
            }
        }

        switch (currentMode)
        {
            case OperateMode.Mouse:
                MouseUpdate();
                // fixedDistance と fixedAngle を元にカメラの位置と回転を更新
                UpdateCameraPositionAndRotation();
                break;
            case OperateMode.Auto:
                MouseUpdate(); // マウス操作を実行
                AutoUpdate();  // その後にAutoモードの処理を実行
                break;
        }
        // 現在の値を保存して次回の差分計算に使用
        prevFixedDistance = fixedDistance;
        prevFixedAngle = fixedAngle;

    }

    private void MouseUpdate()
    {
        float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
        if (scrollWheel != 0.0f)
            MouseWheel(scrollWheel);

        if (Input.GetMouseButtonDown(0) ||
           Input.GetMouseButtonDown(1) ||
           Input.GetMouseButtonDown(2))
            preMousePos = Input.mousePosition;

        MouseDrag(Input.mousePosition);

    }

    private void MouseWheel(float delta)
    {
        if (currentMode == OperateMode.Mouse)
        {
            // fixedDistance を前後移動させる
            fixedDistance += transform.forward * delta * wheelSpeed;
        }
        else
        {
            // カメラとターゲットの相対距離を計算し、前後移動
            Vector3 directionToCamera = (transform.position - target.position).normalized;
            fixedDistance.z += directionToCamera.y * delta * wheelSpeed;
        }
    }

    private void MouseDrag(Vector3 mousePos)
    {
        Vector3 diff = mousePos - preMousePos;

        if (diff.magnitude < Vector3.kEpsilon)
            return;

        if (Input.GetMouseButton(2))
        {
            if (currentMode == OperateMode.Mouse)
            {
                // 中ボタンでX、Y軸方向の距離を変更
                fixedDistance.x -= diff.x * Time.deltaTime * moveSpeed;
                fixedDistance.y -= diff.y * Time.deltaTime * moveSpeed;
            }
            else
            {
                // ターゲットとの相対的な位置に基づいてX、Y軸方向の距離を変更
                Vector3 directionToCamera = (transform.position - target.position).normalized;
                fixedDistance.x -= directionToCamera.y * diff.x * Time.deltaTime * moveSpeed;
                fixedDistance.y -= directionToCamera.y * diff.y * Time.deltaTime * moveSpeed;
            }
        }
        else if (Input.GetMouseButton(1))
        {
            // 右ボタンで角度のX,Y方向を変更
            fixedAngle.y += diff.x * Time.deltaTime * rotateSpeed;
            fixedAngle.x -= diff.y * Time.deltaTime * rotateSpeed;
        }

        preMousePos = mousePos;
    }

    private Vector3 prevFixedDistance;
    private Vector3 prevFixedAngle;
    // カメラの位置と回転を fixedDistance と fixedAngle に基づいて更新
    private void UpdateCameraPositionAndRotation()
    {
        // 距離の差分を計算
        Vector3 distanceDiff = fixedDistance - prevFixedDistance;
        transform.position += distanceDiff;

        // 回転の差分を計算
        Vector3 angleDiff = fixedAngle - prevFixedAngle;

        // 差分を使ってカメラを回転
        transform.RotateAround(transform.position, transform.right, angleDiff.x);
        transform.RotateAround(transform.position, Vector3.up, angleDiff.y);
    }
    private void AutoUpdate()
    {
        if (target == null) return;

        // ターゲットの位置を基準にカメラの追従を行う
        Vector3 targetPosition = target.position;
        Quaternion targetRotation = target.rotation;


        // ターゲットの回転に応じたカメラの追従を適用
        Vector3 directionToCamera = (transform.position - target.position).normalized;
        Vector3 desiredPosition = targetPosition +  transform.rotation * fixedDistance;


        // ターゲットの回転に合わせてカメラも回転させる
        Quaternion desiredRotation = Quaternion.Euler(fixedAngle.x, targetRotation.eulerAngles.y + fixedAngle.y, fixedAngle.z);

        if (useGimbal)
        {
            // スムーズにカメラを追従
            transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * gimbalSmoothSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, Time.deltaTime * gimbalSmoothSpeed);
        }
        else
        {
            // 即時的に位置と回転を反映
            transform.position = desiredPosition;
            transform.rotation = desiredRotation;
        }

    }


}
