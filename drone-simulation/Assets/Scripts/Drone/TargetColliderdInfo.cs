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

    [Header("Physical Properties")]
    public Vector3 Inertia = new Vector3(1.0f, 1.0f, 1.0f); // 慣性テンソル
    public double Mass = 1.0; // 質量
    public double RestitutionCoefficient = 0.5; // 反発係数

    public Rigidbody rb = null;
    public Collider collider_obj = null;
    private Vector3 previousPosition;
    private Quaternion previousRotation;

    public string GetName()
    {
        return this.name;
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
    }
    private void OnDestroy()
    {
        if (GetComponent<Collider>() != null)
        {
            colliderInfoMap.Remove(GetComponent<Collider>());
        }
    }
    private void FixedUpdate()
    {
        if (IsStatic) return;
        UpdatePosition();
        UpdateVelocity();
        UpdateAngularVelocity();
    }
    private void UpdatePosition()
    {
        if (rb != null)
        {
            Position = rb.position;
            Rotation = rb.rotation;
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
                Vector3 velocity = (currentPosition - previousPosition) / Time.fixedDeltaTime;
                previousPosition = currentPosition;
                return velocity;
            }
        }
        return Vector3.zero;
    }

    public Vector3 GetAngularVelocity()
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
                Quaternion deltaRotation = currentRotation * Quaternion.Inverse(previousRotation);
                previousRotation = currentRotation;

                float angle;
                Vector3 axis;
                deltaRotation.ToAngleAxis(out angle, out axis);
                return (axis * angle * Mathf.Deg2Rad) / Time.fixedDeltaTime;
            }
        }
        return Vector3.zero;
    }
}
