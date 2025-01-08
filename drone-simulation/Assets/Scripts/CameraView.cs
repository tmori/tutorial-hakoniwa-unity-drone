using UnityEngine;
using UnityEngine.InputSystem;

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

    [SerializeField, Range(0.001f, 10f)]
    private float wheelSpeed = 8f;

    [SerializeField, Range(0.001f, 10f)]
    private float moveSpeed = 8.0f;

    [SerializeField, Range(0.001f, 10f)]
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
    private Vector3 prevFixedDistance;
    private Vector3 prevFixedAngle;

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
        if (Keyboard.current.aKey.wasPressedThisFrame)
        {
            currentMode = currentMode == OperateMode.Auto ? OperateMode.Mouse : OperateMode.Auto;
        }

        switch (currentMode)
        {
            case OperateMode.Mouse:
                MouseUpdate();
                UpdateCameraPositionAndRotation();
                break;
            case OperateMode.Auto:
                MouseUpdate();
                AutoUpdate();
                break;
        }

        prevFixedDistance = fixedDistance;
        prevFixedAngle = fixedAngle;
    }

    private void MouseUpdate()
    {
        float scrollWheel = Mouse.current.scroll.y.ReadValue();
        if (scrollWheel != 0.0f)
        {
            MouseWheel(scrollWheel);
        }

        if (Mouse.current.leftButton.wasPressedThisFrame ||
            Mouse.current.middleButton.wasPressedThisFrame ||
            Mouse.current.rightButton.wasPressedThisFrame)
        {
            preMousePos = Mouse.current.position.ReadValue();
        }

        MouseDrag(Mouse.current.position.ReadValue());
    }

    private void MouseWheel(float delta)
    {
        if (currentMode == OperateMode.Mouse)
        {
            fixedDistance += transform.forward * delta * wheelSpeed;
        }
        else
        {
            Vector3 directionToCamera = (transform.position - target.position).normalized;
            fixedDistance.z += directionToCamera.y * delta * wheelSpeed;
        }
    }

    private void MouseDrag(Vector3 mousePos)
    {
        Vector3 diff = mousePos - preMousePos;

        if (diff.magnitude < Vector3.kEpsilon)
            return;

        if (Mouse.current.middleButton.isPressed)
        {
            if (currentMode == OperateMode.Mouse)
            {
                fixedDistance.x -= diff.x * Time.deltaTime * moveSpeed;
                fixedDistance.y -= diff.y * Time.deltaTime * moveSpeed;
            }
            else
            {
                Vector3 directionToCamera = (transform.position - target.position).normalized;
                fixedDistance.x -= directionToCamera.y * diff.x * Time.deltaTime * moveSpeed;
                fixedDistance.y -= directionToCamera.y * diff.y * Time.deltaTime * moveSpeed;
            }
        }
        else if (Mouse.current.rightButton.isPressed)
        {
            fixedAngle.y += diff.x * Time.deltaTime * rotateSpeed;
            fixedAngle.x -= diff.y * Time.deltaTime * rotateSpeed;
        }

        preMousePos = mousePos;
    }

    private void UpdateCameraPositionAndRotation()
    {
        Vector3 distanceDiff = fixedDistance - prevFixedDistance;
        transform.position += distanceDiff;

        Vector3 angleDiff = fixedAngle - prevFixedAngle;

        transform.RotateAround(transform.position, transform.right, angleDiff.x);
        transform.RotateAround(transform.position, Vector3.up, angleDiff.y);
    }

    private void AutoUpdate()
    {
        if (target == null) return;

        Vector3 targetPosition = target.position;
        Quaternion targetRotation = target.rotation;

        Vector3 desiredPosition = targetPosition + transform.rotation * fixedDistance;

        Quaternion desiredRotation = Quaternion.Euler(fixedAngle.x, targetRotation.eulerAngles.y + fixedAngle.y, fixedAngle.z);

        if (useGimbal)
        {
            transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * gimbalSmoothSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, Time.deltaTime * gimbalSmoothSpeed);
        }
        else
        {
            transform.position = desiredPosition;
            transform.rotation = desiredRotation;
        }
    }
}
