using UnityEngine;
using UnityEngine.InputSystem;

public class CubeController : MonoBehaviour
{
    public float moveSpeed = 5f; // 移動速度
    public float drag = 1f; // 空気抵抗（減速率）

    private Vector3 velocity = Vector3.zero; // 現在の速度ベクトル
    private Vector3 moveInput = Vector3.zero; // プレイヤー入力による移動方向

    private void Update()
    {
        // 入力値に基づいて速度を加算
        velocity += moveInput * moveSpeed * Time.deltaTime;

        // 空気摩擦（減速）を適用
        velocity = Vector3.Lerp(velocity, Vector3.zero, drag * Time.deltaTime);

        // 現在の速度に基づいてオブジェクトを移動
        transform.Translate(velocity * Time.deltaTime, Space.World);
    }

    private void FixedUpdate()
    {
        // 入力値を更新
        UpdateMovementInput();
    }

    private void UpdateMovementInput()
    {
        moveInput = Vector3.zero;

        // Y軸移動
        if (Keyboard.current.wKey.isPressed) moveInput.y += 1; // Wキーで上移動
        if (Keyboard.current.sKey.isPressed) moveInput.y -= 1; // Sキーで下移動

        // X軸移動
        if (Keyboard.current.iKey.isPressed) moveInput.z += 1; // Iキーで右移動
        if (Keyboard.current.kKey.isPressed) moveInput.z -= 1; // Kキーで左移動

        // Z軸移動
        if (Keyboard.current.jKey.isPressed) moveInput.x -= 1; // Jキーで前進
        if (Keyboard.current.lKey.isPressed) moveInput.x += 1; // Lキーで後退

        // 正規化（斜め移動の速度が速くなるのを防ぐ）
        moveInput = moveInput.normalized;
    }
}
