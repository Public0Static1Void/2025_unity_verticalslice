using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class SC_PlayerMovement : SC_ObjectMovement
{
    [Header("Stats")]
    public float jumpForce;
    public float acceleration = 1.1f;
    public Vector3 vel = Vector3.zero;

    [Header("Ground")]
    public bool grounded = false;
    [Range(0, 1)]
    public float ground_friction = 0.9f;
    public float ground_offset = 0.25f;

    [Header("Air")]
    public AudioClip clip_air, clip_ground_hit, clip_jump;
    // Jump
    public bool jumping = false;
    [Range(0, 1)]
    public float air_control = 0.5f;
    private float distance_from_ground = 0;

    private Vector2 input;

    public bool moving = false;

    public AudioSource audio_effects;


    private bool big_collision = false;


    private void Start()
    {
        can_move = true;
        rb = GetComponent<Rigidbody>();

        max_speed = speed;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, transform.localScale.y + ground_offset);

        distance_from_ground = grounded ? hit.distance : transform.localScale.y + ground_offset;

        if (grounded && rb.linearVelocity.y < -6)
            big_collision = true;
        else
            big_collision = false;
    }

    private void FixedUpdate()
    {
        vel = rb.linearVelocity;

        Vector3 moveDir = (transform.forward * input.y + transform.right * input.x).normalized;
        if (!can_move) moveDir = Vector3.zero;

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

        vel.x = Mathf.Clamp(vel.x, -15, 15);
        vel.z = Mathf.Clamp(vel.z, -15, 15);
        vel.y = Mathf.Clamp(vel.y, -30, 12);

        vel = ApplyGroundFriction(vel);
        vel = ConstrainVelocity(vel);

        rb.linearVelocity = vel;

        if (!grounded && distance_from_ground >= 1.2f)
        {
            if (!audio_effects.isPlaying)
            {
                audio_effects.clip = clip_air;
                audio_effects.loop = true;
                audio_effects.Play();
            }

            audio_effects.volume = Mathf.Min(Mathf.Abs(rb.linearVelocity.y * 0.075f), 1);
        }
        else if (grounded)
        {
            if (audio_effects.clip == clip_air && audio_effects.isPlaying)
            {
                audio_effects.Stop();
                if (big_collision)
                {
                    audio_effects.clip = clip_ground_hit;
                    audio_effects.loop = false;
                    audio_effects.Play();


                    float normalizedYSpeed = Mathf.InverseLerp(0, 15, Mathf.Abs(rb.linearVelocity.y));
                    float force = Mathf.Min(0.75f,
                                            Mathf.Max(0.3f, normalizedYSpeed)
                                            );
                    Debug.Log(force);
                    SC_CameraRotation.instance.ShakeCamera(force, 0.25f);

                    StartCoroutine(DisableMovement(0.5f));
                }
                // Audio hit 
            }
        }
    }

    private IEnumerator DisableMovement(float duration)
    {
        can_move = false;
        yield return new WaitForSeconds(duration);
        can_move = true;
    }

    #region Physics functions
    public void Jump()
    {
        if (grounded && can_move)
        {
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
            SC_SoundManager.instance.InstantiateSound(clip_jump, transform.position);
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
        if (input.y < 0) // Moving backwards
            total_speed = 0.65f;
        else if (input.y > 0 && input.y < 0.1f && input.x != 0) // Moving horizontally
            total_speed = 0.75f;
        return total_speed;
    }
    #endregion

    #region Inputs
    public void PlayerMove(InputAction.CallbackContext con)
    {
        if (con.performed)
        {
            input = con.ReadValue<Vector2>();
            moving = true;
        }
        else if (con.canceled)
        {
            input = Vector2.zero;
            moving = false;
        }
    }

    public void PlayerJump(InputAction.CallbackContext con)
    {
        if (con.performed)
            Jump();
    }
    #endregion
}