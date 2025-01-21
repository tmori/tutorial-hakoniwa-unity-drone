using UnityEngine;
using Hakoniwa.DroneService;

[RequireComponent(typeof(BoxCollider))]
public class DroneCollision : MonoBehaviour
{
    [SerializeField]
    private LayerMask collisionLayer; // 衝突を検出するレイヤー

    public GameObject pos_obj;

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
        if (pos_obj == null)
        {
            pos_obj = this.gameObject;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // レイヤーマスクに基づいて対象をフィルタリング
        if (IsLayerInMask(other.gameObject.layer, collisionLayer))
        {
            //HandleTriggerCollision(other);
            TargetColliderdInfo info = TargetColliderdInfo.GetInfo(other);
            if (info != null)
            {
                Debug.Log("Info: " + this.pos_obj.name + " collided with " + info.GetName());
                HandleTriggerImpulseCollision(info, other);
            }
        }
    }
    private Vector3 ConvertToRosVector(Vector3 unityVector)
    {
        return new Vector3(
            unityVector.z,
            -unityVector.x,
            unityVector.y
        );
    }

    private Vector3 ConvertToRosAngular(Vector3 unityAngular)
    {
        return new Vector3(
            -unityAngular.z,
            unityAngular.x,
            -unityAngular.y
        );
    }


    private void HandleTriggerImpulseCollision(TargetColliderdInfo info, Collider other)
    {
        // Calculate Contact point in world frame
        Vector3 contactPoint = other.ClosestPoint(this.pos_obj.transform.position);
        Debug.Log($"Contact Point (World Frame): {contactPoint}");

        // Calculate collision relative vector
        Vector3 selfContactVector = contactPoint - this.pos_obj.transform.position;
        Debug.Log($"Center of Drone: {this.pos_obj.transform.position}");
        Debug.Log($"Collision Relative Vector: {selfContactVector}");
        Vector3 targetContactVector = contactPoint - info.Position;

        // Calculate normal
        //Vector3 normal = info.GetNormal(contactPoint);
        Vector3 normal = info.GetNormal(this.pos_obj.transform.position);

        // Calculate TargetVelocity
        Vector3 targetVelocity = info.Velocity;

        // Calculate TargetAngularVelocity
        Vector3 targetAngularVelocity = info.AngularVelocity;

        Vector3 targetEuler = Vector3.zero;

        DroneServiceRC.PutImpulseByCollision(
            this.index,
            info.IsStatic,
            ConvertToRosVector(targetVelocity),
            ConvertToRosAngular(targetAngularVelocity),
            ConvertToRosAngular(targetEuler),
            ConvertToRosVector(selfContactVector),
            ConvertToRosVector(targetContactVector),
            info.Inertia,
            ConvertToRosVector(normal),
            info.Mass,
            info.RestitutionCoefficient
        );

        Debug.Log($"Impulse collision handled with {other.name}");
    }

    private void HandleTriggerCollision(Collider other)
    {
        // コライダーの最も近いポイントを取得
        Vector3 contactPoint = other.ClosestPoint(this.pos_obj.transform.position);
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
