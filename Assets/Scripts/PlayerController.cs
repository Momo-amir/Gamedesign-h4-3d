using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    public float  moveSpeed = 20f;   // units/sec
    public float  turnSpeed = 100f;  // degrees/sec
    Rigidbody   rb;
    Vector2     input;              // x=steer, y=throttle

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        // only yaw, lock other rotations
        rb.constraints = RigidbodyConstraints.FreezeRotationX
                       | RigidbodyConstraints.FreezeRotationZ;
    }

    void FixedUpdate()
    {
        // 1) DRIVE – preserve existing Y velocity (gravity/falls)
        float yVel = rb.linearVelocity.y;
        Vector3 forwardVel = transform.forward * input.y * moveSpeed;
        rb.linearVelocity = new Vector3(forwardVel.x, yVel, forwardVel.z);

        // 2) STEER
        float turnDeg    = input.x * turnSpeed * Time.fixedDeltaTime;
        Quaternion delta = Quaternion.Euler(0f, turnDeg, 0f);
        rb.MoveRotation(rb.rotation * delta);
    }

    public void Move(InputAction.CallbackContext ctx)
    {
        input = ctx.ReadValue<Vector2>();
    }
}
