using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class SC_PlayerMovement : MonoBehaviour
{
    [Header("Stats")]
    public float speed;
    public float jumpForce;
    public float acceleration = 1.1f;
    public Vector3 vel = Vector3.zero;

    [Header("Ground")]
    public bool grounded = false;
    [Range(0, 1)]
    public float ground_friction = 0.9f;
    public float ground_offset = 0.25f;

    [Header("Air")]
    // Jump
    private bool jumping = false;
    [Range(0, 1)]
    public float air_control = 0.5f;

    private Rigidbody rb;
    private Vector2 input;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, transform.localScale.y + ground_offset);
    }

    private void FixedUpdate()
    {
        vel = rb.linearVelocity;

        Vector3 moveDir = (transform.forward * input.y + transform.right * input.x).normalized;

        // Get the desired speed for the frame
        Vector3 desiredVel = moveDir * speed;

        Vector3 horizontalVel = new Vector3(vel.x, 0, vel.z);

        if (moveDir.magnitude > 0.1f)
        {
            Vector3 diff = desiredVel - horizontalVel;
            Vector3 accel = diff.normalized * acceleration;

            /// Get the speed by input
            float input_speed = GetSpeedByInput();
            /// Set the horizontal speed based on the acceleration and air control
            horizontalVel += accel * input_speed * (!grounded! ? air_control : 1);
        }

        vel.x = horizontalVel.x;
        vel.z = horizontalVel.z;

        vel = ApplyGroundFriction(vel);
        vel = ConstrainVelocity(vel);

        rb.linearVelocity = vel;
    }


    #region Physics functions
    public void Jump()
    {
        if (grounded)
        {
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
            jumping = true;
        }
    }
    Vector3 ConstrainVelocity(Vector3 vel)
    {
        vel.x = Mathf.Clamp(vel.x, -25, 25);
        vel.z = Mathf.Clamp(vel.z, -25, 25);
        vel.y = Mathf.Clamp(vel.y, -50, 25);

        return vel;
    }
    Vector3 ApplyGroundFriction(Vector3 vel)
    {
        if (grounded)
        {
            if (input == Vector2.zero)
            {
                vel *= ground_friction;
            }
            else if (input.x != 0 && input.y == 0) // The player is moving only horizontally
            {
                vel.z *= ground_friction;
            }
            else // The player is moving only vertically
            {
                vel.x *= ground_friction;
            }
        }
        return vel;
    }
    float GetSpeedByInput()
    {
        float total_speed = 1;
        if (input.y < 0)
            total_speed = 0.65f;
        return total_speed;
    }
    #endregion

    #region Inputs
    public void PlayerMove(InputAction.CallbackContext con)
    {
        if (con.performed)
            input = con.ReadValue<Vector2>();
        else if (con.canceled)
            input = Vector2.zero;
    }

    public void PlayerJump(InputAction.CallbackContext con)
    {
        if (con.performed)
            Jump();
    }
    #endregion
}