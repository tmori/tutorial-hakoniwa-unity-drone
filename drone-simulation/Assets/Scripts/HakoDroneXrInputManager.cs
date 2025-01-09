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

    public bool IsXButtonPressed()
    {
        return OVRInput.GetDown(OVRInput.RawButton.A);
    }

    public bool IsXButtonReleased()
    {
        return OVRInput.GetUp(OVRInput.RawButton.A);
    }

    private void OnDestroy()
    {
    }
}
