using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CarStabilizer : MonoBehaviour
{
    [Header("Stability Settings")]
    public float centerOfMassYOffset = -1f; // lower CoM to reduce tipping
    public float uprightTorque = 50f; // how strong to right itself

    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        // push down the center of mass
        rb.centerOfMass += Vector3.up * centerOfMassYOffset;
        // lock X/Z rotation in inspector or via code
        rb.constraints |= RigidbodyConstraints.FreezeRotationX
                       | RigidbodyConstraints.FreezeRotationZ;
    }

    void FixedUpdate()
    {
        // 1) compute tilt axis & angle between 'up' and world up
        Vector3 axis = Vector3.Cross(transform.up, Vector3.up);
        float angle = Vector3.Angle(transform.up, Vector3.up);

        // only correct if tilted more than a degree
        if (angle > 1f)
        {
            // torque magnitude ∝ tilt angle
            float torqueMag = angle * uprightTorque * 0.1f;

            // project out any yaw component
            Vector3 torque = Vector3.ProjectOnPlane(axis.normalized * torqueMag, Vector3.up);

            rb.AddTorque(torque, ForceMode.Acceleration);
        }
    }
}