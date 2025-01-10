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

    public bool IsOButtonPressed()
    {
        return OVRInput.GetDown(OVRInput.RawButton.A);
    }

    public bool IsOButtonReleased()
    {
        return OVRInput.GetUp(OVRInput.RawButton.A);
    }
    public bool IsRButtonPressed()
    {
        return OVRInput.GetDown(OVRInput.RawButton.B);
    }

    public bool IsRButtonReleased()
    {
        return OVRInput.GetUp(OVRInput.RawButton.B);
    }

    private void OnDestroy()
    {
    }
}
