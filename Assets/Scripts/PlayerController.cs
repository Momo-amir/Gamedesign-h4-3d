using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    public float  acceleration = 30f;   // units/sec²
    public float  turnSpeed    = 100f;  // degrees/sec
    Rigidbody   rb;
    Vector2     input;                // x=steer, y=throttle

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;

    }

    void FixedUpdate()
    {
        // 1) ACCELERATE in XZ plane
        rb.AddForce(transform.forward * input.y * acceleration,
                    ForceMode.Acceleration);

        // 2) STEER around Y
        float turn = input.x * turnSpeed * Time.fixedDeltaTime;
        rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, turn, 0f));
    }

    public void Move(InputAction.CallbackContext ctx)
    {
        input = ctx.ReadValue<Vector2>();
    }
}
