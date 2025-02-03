using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TargetColliderdInfo : MonoBehaviour
{
    private static Dictionary<Collider, TargetColliderdInfo> colliderInfoMap = new Dictionary<Collider, TargetColliderdInfo>();
    public static TargetColliderdInfo GetInfo(Collider collider)
    {
        if (colliderInfoMap.TryGetValue(collider, out TargetColliderdInfo info))
        {
            return info;
        }
        Debug.LogWarning($"Collider {collider.name} に対応するTargetColliderdInfoが見つかりません。");
        return null;
    }
    private static void PutInfo(Collider collider, TargetColliderdInfo obj)
    {
        colliderInfoMap[collider] = obj;
    }


    [Header("Static or Dynamic")]
    public bool IsStatic = false; // 静的オブジェクトかどうか

    [Header("Dynamic Properties")]
    public Vector3 Position = new Vector3(0, 0, 0);
    public Quaternion Rotation = new Quaternion(0, 0, 0, 0);
    public Vector3 Velocity = new Vector3(0, 0, 0);
    public Vector3 AngularVelocity = new Vector3(0, 0, 0);
    public Vector3 Euler = new Vector3(0, 0, 0);

    [Header("Physical Properties")]
    public Vector3 Inertia = new Vector3(1.0f, 1.0f, 1.0f); // 慣性テンソル
    public double Mass = 1.0; // 質量
    public double RestitutionCoefficient = 0.5; // 反発係数

    public Rigidbody rb = null;
    public Collider collider_obj = null;


    private Vector3 lastValidVelocity = Vector3.zero; // 前回の有効な速度
    private Vector3 previousPosition;
    private float velocity_lastUpdateTime = 0f;

    private Vector3 lastValidAngularVelocity = Vector3.zero; // 前回の有効な角速度
    private Quaternion previousRotation;
    private float rotation_lastUpdateTime = 0f;

    private float currentTime = 0f;

    public string GetName()
    {
        return this.name;
    }
    public Vector3 GetNormal(Vector3 contactPoint, Vector3 contactedTartgetCenterPoint)
    {
        if (collider_obj is BoxCollider)
        {
            return GetNormalBoxCollider(contactPoint, contactedTartgetCenterPoint);
        }
        else
        {
            return GetNormalSphere(contactedTartgetCenterPoint);
        }
    }
    public Vector3 GetNormalSphere(Vector3 contactedTartgetCenterPoint)
    {
        Vector3 normal = (contactedTartgetCenterPoint - this.Position).normalized;
        return normal;
    }
    private Vector3 GetNormalBoxCollider(Vector3 contactPoint, Vector3 contactedTartgetCenterPoint)
    {
        Debug.Log("BoxCollider");

        // ボックスの半径（ローカル座標系の範囲）
        Vector3 halfSize = collider_obj.bounds.size / 2f;
        Debug.Log($"Half Size: {halfSize}");

        // ボックスの中心位置
        Vector3 center = collider_obj.bounds.center;
        Debug.Log($"Box Center: {center}");

        // ワールド座標系の接触点をローカル座標系に変換
        Vector3 localPoint = Quaternion.Inverse(collider_obj.transform.rotation) * (contactedTartgetCenterPoint - center);
        Debug.Log($"Contact Point (World): {contactedTartgetCenterPoint}");
        Debug.Log($"Contact Point (Local): {localPoint}");

        // 各軸の絶対値を比較して最も近い面を判定
        float absX = Mathf.Abs(localPoint.x);
        float absY = Mathf.Abs(localPoint.y);
        float absZ = Mathf.Abs(localPoint.z);
        Debug.Log($"Absolute Differences: X={absX}, Y={absY}, Z={absZ}");

        Vector3 localNormal;

        if (absX > absY && absX > absZ)
        {
            localNormal = new Vector3(Mathf.Sign(localPoint.x), 0, 0); // X軸の面
            Debug.Log($"Closest Axis: X, Normal: {localNormal}");
        }
        else if (absY > absX && absY > absZ)
        {
            localNormal = new Vector3(0, Mathf.Sign(localPoint.y), 0); // Y軸の面
            Debug.Log($"Closest Axis: Y, Normal: {localNormal}");
        }
        else
        {
            localNormal = new Vector3(0, 0, Mathf.Sign(localPoint.z)); // Z軸の面
            Debug.Log($"Closest Axis: Z, Normal: {localNormal}");
        }

        // ローカル座標系の法線をワールド座標系に変換
        Vector3 worldNormal = collider_obj.transform.rotation * localNormal;
        Debug.Log($"World Normal: {worldNormal}");

        return worldNormal;
    }



    private void Awake()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }
        if (collider_obj == null)
        {
            collider_obj = GetComponent<Collider>();
        }
        if (collider_obj == null)
        {
            Debug.LogError($"{name}: Colliderが設定されていません。正しく動作しない可能性があります。");
            return;
        }
        if (rb != null)
        {
            previousPosition = rb.position;
            previousRotation = rb.rotation;
        }
        else
        {
            IsStatic = true;
        }
        PutInfo(collider_obj, this);
        currentTime = 0f;
    }
    private void OnDestroy()
    {
        if (collider_obj!= null)
        {
            colliderInfoMap.Remove(collider_obj);
            collider_obj = null;
        }
    }
    private void FixedUpdate()
    {
        //if (IsStatic) return;
        UpdatePosition();
        UpdateVelocity();
        UpdateAngularVelocity();
        currentTime += Time.fixedDeltaTime;
    }
    private void UpdatePosition()
    {
        if (rb != null)
        {
            Position = rb.position;
            Rotation = rb.rotation;
            Euler = rb.rotation.eulerAngles;
        }
    }

    private void UpdateVelocity()
    {
        this.Velocity = GetVelocity();
    }
    private void UpdateAngularVelocity()
    {
        this.AngularVelocity = GetAngularVelocity();
    }

    private Vector3 GetVelocity()
    {
        if (rb != null)
        {
            if (!rb.isKinematic)
            {
                return rb.linearVelocity;
            }
            else
            {
                Vector3 currentPosition = rb.position;

                // 位置が変わっていないなら前回の速度を維持
                if (currentPosition == previousPosition)
                {
                    return lastValidVelocity;
                }

                // 時間補正
                float deltaTime = currentTime - velocity_lastUpdateTime;
                velocity_lastUpdateTime = currentTime;

                // 速度計算（微小な変化を無視）
                Vector3 velocity = (currentPosition - previousPosition) / (deltaTime > 0 ? deltaTime : Time.fixedDeltaTime);
                if (velocity.magnitude < 0.0001f) // しきい値調整
                {
                    return lastValidVelocity;
                }

                // 位置更新 & 速度更新
                previousPosition = currentPosition;
                lastValidVelocity = velocity;
                return velocity;
            }
        }
        return Vector3.zero;
    }

    private Vector3 GetAngularVelocity()
    {
        if (rb != null)
        {
            if (!rb.isKinematic)
            {
                return rb.angularVelocity;
            }
            else
            {
                Quaternion currentRotation = rb.rotation;

                // 回転が変化していないなら前回の角速度を維持
                if (currentRotation == previousRotation)
                {
                    return lastValidAngularVelocity;
                }

                // 時間補正
                float deltaTime = currentTime - rotation_lastUpdateTime;
                rotation_lastUpdateTime = currentTime;

                // 角速度計算
                Quaternion deltaRotation = currentRotation * Quaternion.Inverse(previousRotation);
                previousRotation = currentRotation;

                float angle;
                Vector3 axis;
                deltaRotation.ToAngleAxis(out angle, out axis);
                Vector3 angularVelocity = (axis * angle * Mathf.Deg2Rad) / (deltaTime > 0 ? deltaTime : Time.fixedDeltaTime);

                // 角速度の微小変化を無視
                if (angularVelocity.magnitude < 0.0001f)
                {
                    return lastValidAngularVelocity;
                }

                lastValidAngularVelocity = angularVelocity;
                return angularVelocity;
            }
        }
        return Vector3.zero;
    }
}
