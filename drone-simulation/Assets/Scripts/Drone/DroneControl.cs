using Hakoniwa.DroneService;
using UnityEngine;

public class DroneControl : MonoBehaviour
{
    public bool xr;
    public double stick_strength = 0.1;
    public double stick_yaw_strength = 1.0;
    private IDroneInput controller_input;
    public bool magnet_on = false;

    public bool IsMagnetOn()
    {
        return magnet_on;
    }

    private void Start()
    {
        if (xr)
        {
            controller_input = HakoDroneXrInputManager.Instance;
        }
        else
        {
            controller_input = HakoDroneInputManager.Instance;
        }
    }

    public void HandleInput()
    {
        Vector2 leftStick = controller_input.GetLeftStickInput();
        Vector2 rightStick = controller_input.GetRightStickInput();
        float horizontal = rightStick.x;
        float forward = rightStick.y;
        float yaw = leftStick.x;
        float pitch = leftStick.y;

        if (controller_input.IsAButtonPressed())
        {
            DroneServiceRC.PutRadioControlButton(0, 1);
        }
        else if (controller_input.IsAButtonReleased())
        {
            DroneServiceRC.PutRadioControlButton(0, 0);
        }
        if (controller_input.IsBButtonReleased())
        {
            magnet_on = IsMagnetOn() ? false : true;
        }
        DroneServiceRC.PutHorizontal(0, horizontal * stick_strength);
        DroneServiceRC.PutForward(0, -forward * stick_strength);
        DroneServiceRC.PutHeading(0, yaw * stick_yaw_strength);
        DroneServiceRC.PutVertical(0, -pitch * stick_strength);
    }

}

