using UnityEngine;

public class HakoDroneInputManager : MonoBehaviour, IDroneInput
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

    public bool IsAButtonPressed()
    {
        return inputActions.Gameplay.Sbutton.WasPressedThisFrame();
    }

    public bool IsAButtonReleased()
    {
        return inputActions.Gameplay.Sbutton.WasReleasedThisFrame();
    }
    public bool IsBButtonPressed()
    {
        return inputActions.Gameplay.Ebutton.WasPressedThisFrame();
    }
    public bool IsBButtonReleased()
    {
        return inputActions.Gameplay.Ebutton.WasReleasedThisFrame();
    }
    public bool IsXButtonPressed()
    {
        return inputActions.Gameplay.Nbutton.WasPressedThisFrame();
    }
    public bool IsXButtonReleased()
    {
        return inputActions.Gameplay.Nbutton.WasReleasedThisFrame();
    }
    public bool IsYButtonPressed()
    {
        return inputActions.Gameplay.Wbutton.WasPressedThisFrame();
    }

    public bool IsYButtonReleased()
    {
        return inputActions.Gameplay.Wbutton.WasReleasedThisFrame();
    }
    private void OnDestroy()
    {
        inputActions.Gameplay.Disable();
    }

}
