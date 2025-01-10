using UnityEngine;

public class HakoDroneXrInputManager : MonoBehaviour
{
    public static HakoDroneXrInputManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // シーンをまたいで保持
    }

    public Vector2 GetLeftStickInput()
    {
        return OVRInput.Get(OVRInput.RawAxis2D.LThumbstick);
    }

    public Vector2 GetRightStickInput()
    {
        return OVRInput.Get(OVRInput.RawAxis2D.RThumbstick);
    }

    public bool IsAButtonPressed()
    {
        return OVRInput.GetDown(OVRInput.RawButton.A);
    }

    public bool IsAButtonReleased()
    {
        return OVRInput.GetUp(OVRInput.RawButton.A);
    }
    public bool IsBButtonPressed()
    {
        return OVRInput.GetDown(OVRInput.RawButton.B);
    }

    public bool IsBButtonReleased()
    {
        return OVRInput.GetUp(OVRInput.RawButton.B);
    }
    public bool IsXButtonPressed()
    {
        return OVRInput.GetDown(OVRInput.RawButton.X);
    }

    public bool IsXButtonReleased()
    {
        return OVRInput.GetUp(OVRInput.RawButton.X);
    }
    public bool IsYButtonPressed()
    {
        return OVRInput.GetDown(OVRInput.RawButton.Y);
    }

    public bool IsYButtonReleased()
    {
        return OVRInput.GetUp(OVRInput.RawButton.Y);
    }

    private void OnDestroy()
    {
    }
}
