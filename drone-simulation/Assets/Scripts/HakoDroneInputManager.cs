using UnityEngine;

public class HakoDroneInputManager : MonoBehaviour
{
    public static HakoDroneInputManager Instance { get; private set; }
    private DroneInputActions inputActions;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // シーンをまたいで保持
        inputActions = new DroneInputActions();
        inputActions.Gameplay.Enable();
    }

    public Vector2 GetLeftStickInput()
    {
        return inputActions.Gameplay.LeftStick.ReadValue<Vector2>();
    }

    public Vector2 GetRightStickInput()
    {
        return inputActions.Gameplay.RightStick.ReadValue<Vector2>();
    }

    public bool IsXButtonPressed()
    {
        return inputActions.Gameplay.Xbutton.WasPressedThisFrame();
    }

    public bool IsXButtonReleased()
    {
        return inputActions.Gameplay.Xbutton.WasReleasedThisFrame();
    }
    public bool IsOButtonPressed()
    {
        return inputActions.Gameplay.Obutton.WasPressedThisFrame();
    }
    public bool IsOButtonReleased()
    {
        return inputActions.Gameplay.Obutton.WasReleasedThisFrame();
    }
    public bool IsRButtonPressed()
    {
        return inputActions.Gameplay.Rbutton.WasPressedThisFrame();
    }
    public bool IsRButtonReleased()
    {
        return inputActions.Gameplay.Rbutton.WasReleasedThisFrame();
    }
    private void OnDestroy()
    {
        inputActions.Gameplay.Disable();
    }
}
