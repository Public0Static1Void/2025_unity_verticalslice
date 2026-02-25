using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Rigidbody))]
public class SC_PlayerMovement : SC_ObjectMovement
{
    public static SC_PlayerMovement instance { get; private set; }

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
    public Volume camera_volume;
    private MotionBlur camera_blur;

    public bool moving = false;

    [Header("Grab")]
    public bool grabbing = false;
    public RopePoint grabPoint;
    public SC_Rope grabbedRope;

    private float distance_from_ground = 0;

    public Vector2 input;


    public AudioSource audio_effects;

    private SC_PlayerLife playerLife;


    private bool big_collision = false;


    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        can_move = true;
        rb = GetComponent<Rigidbody>();
        playerLife = GetComponent<SC_PlayerLife>();

        camera_volume.profile.TryGet<MotionBlur>(out camera_blur);
        if (camera_blur != null)
        {
            camera_blur.intensity.value = 0;
        }
        else
            Debug.LogWarning("Motion blur not added to the camera volume");

        max_speed = speed;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, transform.localScale.y + ground_offset);

        distance_from_ground = grounded ? hit.distance : transform.localScale.y + ground_offset;

        if (grounded && rb.linearVelocity.magnitude > 10)
            big_collision = true;
        else
            big_collision = false;
    }

    private void FixedUpdate()
    {
        if (grabbing && grabPoint != null)
        {
            rb.MovePosition(grabPoint.position);
        }
        else
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
            vel.y = Mathf.Clamp(vel.y, -40, 15);

            vel = ApplyGroundFriction(vel);
            vel = ConstrainVelocity(vel);

            rb.linearVelocity = vel;

            if (!grounded && distance_from_ground >= 1.2f)
            {
                float current_speed = rb.linearVelocity.magnitude / 15;
                if (current_speed > 0.65f)
                    playerLife.SetVignetteValue(Mathf.Min(current_speed, 0.2f), Color.black);
                if (!audio_effects.isPlaying)
                {
                    audio_effects.clip = clip_air;
                    audio_effects.loop = true;
                    audio_effects.Play();
                }

                audio_effects.volume = Mathf.Min(Mathf.Abs(rb.linearVelocity.y * 0.09f), 1);

            }
            else if (grounded)
            {
                camera_blur.intensity.value = 0;

                // The player just collisioned with the ground
                if (audio_effects.clip == clip_air && audio_effects.isPlaying)
                {
                    audio_effects.Stop();
                    playerLife.SetVignetteValue(0, Color.black);
                    if (big_collision)
                    {
                        audio_effects.clip = clip_ground_hit;
                        audio_effects.loop = false;
                        audio_effects.Play();

                        HandleBigCollision();
                    }

                    grabPoint = null;
                }
            }
        }

        HandleCameraBlur();
    }

    private void HandleCameraBlur()
    {
        if (rb.linearVelocity.magnitude < 8)
        {
            camera_blur.intensity.value = 0;
            return;
        }

        camera_blur.intensity.value = Mathf.Max(Mathf.InverseLerp(8, max_speed, rb.linearVelocity.magnitude), 0.5f);
        //Debug.Log(camera_blur.intensity.value);
    }

    /// <summary>
    /// Handles the collisions at high speed
    /// </summary>
    private void HandleBigCollision()
    {
        float normalizedYSpeed = Mathf.InverseLerp(0, 15, Mathf.Abs(rb.linearVelocity.y));
        // Clamps the camera shake force
        float force = Mathf.Min(0.75f,
                                Mathf.Max(0.35f, normalizedYSpeed)
                                );
        SC_CameraRotation.instance.ShakeCamera(force, 0.25f);

        // The player is falling and collided with the ground
        if (rb.linearVelocity.y < 0)
            SC_CameraRotation.instance.OffsetCamera(Vector3.down, 0.3f, Quaternion.Euler(3, 0, 0), 0.75f);

        StartCoroutine(DisableMovement(0.75f));
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
        if ((grounded && can_move) || (!grounded && grabbing))
        {
            float up_force = jumpForce;
            if (grabbing)
            {
                rb.useGravity = true;
                rb.AddForce(Camera.main.transform.forward * vel.magnitude, ForceMode.Force);
            }

            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
            SC_SoundManager.instance.InstantiateSound(clip_jump, transform.position);
            jumping = true;
            
            grabbing = false;
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