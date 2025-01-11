using UnityEngine;
using Hakoniwa.DroneService;

[RequireComponent(typeof(BoxCollider))]
public class DroneCollision : MonoBehaviour
{
    [SerializeField]
    private LayerMask collisionLayer; // 衝突を検出するレイヤー

    private int index;
    public void SetIndex(int inx)
    {
        this.index = inx;
    }

    private void Awake()
    {
        // BoxCollider をトリガーとして設定
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        boxCollider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // レイヤーマスクに基づいて対象をフィルタリング
        if (IsLayerInMask(other.gameObject.layer, collisionLayer))
        {
            HandleTriggerCollision(other);
        }
    }

    private void HandleTriggerCollision(Collider other)
    {
        // コライダーの最も近いポイントを取得
        Vector3 contactPoint = other.ClosestPoint(transform.position);
        Debug.Log($"Collision detected with {other.name} at {contactPoint}");

        // ワールド座標を ROS 座標に変換
        Vector3 ros_pos = new Vector3
        {
            x = contactPoint.z,
            z = contactPoint.y,
            y = -contactPoint.x
        };

        // 衝突情報を DroneServiceRC に送信
        DroneServiceRC.PutCollision(this.index, ros_pos.x, ros_pos.y, ros_pos.z, 1.0);

        // デバッグ表示 (衝突点を緑のラインで表示)
        Debug.DrawRay(contactPoint, Vector3.up * 0.5f, Color.green, 1.0f, false);
    }

    private bool IsLayerInMask(int layer, LayerMask layerMask)
    {
        return (layerMask.value & (1 << layer)) > 0;
    }
}
