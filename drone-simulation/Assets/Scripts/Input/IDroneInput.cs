using UnityEngine;

public interface IDroneInput
{
    public Vector2 GetLeftStickInput();
    public Vector2 GetRightStickInput();
    /*
     * A
     */
    public bool IsAButtonPressed();
    public bool IsAButtonReleased();
    /*
     * B
     */
    public bool IsBButtonPressed();
    public bool IsBButtonReleased();
    /*
     * X
     */
    public bool IsXButtonPressed();
    public bool IsXButtonReleased();
    /*
     * Y
     */
    public bool IsYButtonPressed();
    public bool IsYButtonReleased();
}
