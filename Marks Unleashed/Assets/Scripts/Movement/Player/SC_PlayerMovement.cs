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
    public AnimationCurve curve_acceleration;
    public float t_speed;

    [Header("Ground")]
    public bool grounded = false;
    [Range(0, 1)]
    public float ground_friction = 0.9f;
    public float ground_offset = 0.25f;
    [HideInInspector] public float _ground_start_friction = 0;
    public float coyote_time = 1;
    private float _timer_coyote = 0;

    [Header("Air")]
    public AudioClip clip_air, clip_ground_hit, clip_jump;
    public AudioClip clip_ground_hit_slow;
    // Jump
    public bool jumping = false;
    [Range(0, 1)]
    public float air_control = 0.5f;
    public float fall_multiplier = 1.1f;
    public Volume camera_volume;
    private MotionBlur camera_blur;
    private float _timer_air = 0;

    public bool moving = false;

    [Header("Grab")]
    public float ropeJumpStrength = 5;
    public bool grabbing = false;
    public RopePoint grabPoint;
    public SC_Rope grabbedRope;

    private float distance_from_ground = 0;

    public Vector2 input;


    public AudioSource audio_effects;

    [HideInInspector]
    public SC_PlayerLife playerLife;

    [Header("Particles")]
    public ParticleSystem parts_smoke;
    public ParticleSystem parts_smoke_walk;


    private bool big_collision = false;

    private SC_CameraRotation _cameraRotation;

    private bool _vignette_active = false;

    // Ground attachment
    private Transform currentGround;
    private Vector3 lastGroundPos;
    private Quaternion lastGroundRot;

    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        can_move = true;

        rb = GetComponent<Rigidbody>();
        playerLife = GetComponent<SC_PlayerLife>();
        _cameraRotation = SC_CameraRotation.instance;

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

        audio_effects.Play();
        audio_effects.volume = 1;
        audio_effects.loop = true;

        _ground_start_friction = ground_friction;
    }

    private void Update()
    {
        bool start_grounded = grounded;
        grounded = Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, transform.localScale.y + ground_offset);

        distance_from_ground = grounded ? hit.distance : transform.localScale.y + ground_offset;

        if (grounded && rb.linearVelocity.magnitude >= 15)
            big_collision = true;
        else
            big_collision = false;

        // Check if the player touched the ground in this frame
        if (!start_grounded && grounded)
        {
            _cameraRotation.ShakeUIElements(10 * Mathf.Abs(rb.linearVelocity.y), 0.25f);
            SC_SoundManager.instance.InstantiateSound(clip_ground_hit_slow, transform.position);
            var emission = parts_smoke.emission;
            var burst = emission.GetBurst(0);
            burst.count = 10;
            emission.SetBurst(0, burst);
            parts_smoke.Play();
        }
    }

    private void FixedUpdate()
    {
        if (grabbing && grabPoint != null)
        {
            _timer_air = 0;
            if (!grabbedRope.slideable && !grabbedRope.scalable)
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
                Vector3 accel = diff * acceleration;
                /// Get the speed by input
                float input_speed = GetSpeedByInput();
                /// Set the horizontal speed based on the acceleration and air control
                horizontalVel += accel * input_speed * (!grounded ? air_control : 1);
            }

            vel.x = horizontalVel.x;
            vel.z = horizontalVel.z;
            if (vel.y < 0 && vel.y > -2.5f)
                vel.y *= fall_multiplier;

            vel.x = Mathf.Clamp(vel.x, -15, 15);
            vel.z = Mathf.Clamp(vel.z, -15, 15);
            vel.y = Mathf.Clamp(vel.y, -40, 15);

            vel = ApplyGroundFriction(vel);
            vel = ConstrainVelocity(vel);

            HandleCameraDirection(input);

            rb.linearVelocity = vel;

            if (rb.linearVelocity.magnitude > 5)
            {
                if (!SC_GameManager.AreColorsSimilar(playerLife.vignette_color, playerLife.target_color, 0.01f))
                {
                    playerLife.SetVignetteValue(0.25f, Color.black);
                    _vignette_active = true;
                }
                
            }

            if (!grounded)
            {
                if (Mathf.Abs(rb.linearVelocity.y) > 2)
                {
                    _timer_air += Time.deltaTime;
                    if (_timer_air > 3)
                        _timer_air = 3;
                    float t = _timer_air / 3;
                    t = Mathf.SmoothStep(0, 1, t);
                    audio_effects.volume = t * 2;

                    if (!audio_effects.isPlaying || audio_effects.clip != clip_air)
                    {
                        audio_effects.clip = clip_air;
                        audio_effects.Play();
                    }
                }
            }
            else
            {
                _timer_air = 0;

                camera_blur.intensity.value = 0;

                if (can_move && _vignette_active)
                {
                    playerLife.SetVignetteValue(0, Color.black);
                    _vignette_active = false;
                }

                // The player just collisioned with the ground
                if (audio_effects.isPlaying)
                {
                    audio_effects.volume = 0;
                    //audio_effects.Pause();

                    grabPoint = null;
                    grabbedRope = null;
                }
            }
        }

        // Handle the walk particles
        if (grounded && !parts_smoke_walk.isPlaying)
        {
            parts_smoke_walk.Play();
        }
        else if (!grounded || !moving)
        {
            parts_smoke_walk.Stop();
        }
            
        // Handles what happen when there's a big collision
        if (big_collision)
        {
            SC_SoundManager.instance.InstantiateSound(clip_ground_hit, transform.position);

            HandleBigCollision();
        }

        HandleCameraBlur();
        HandleGroundAttachment();
    }

    private void HandleGroundAttachment()
    {
        if (currentGround != null && grounded)
        {
            Rigidbody groundRb = currentGround.GetComponent<Rigidbody>();

            Vector3 groundDelta = Vector3.zero;

            if (groundRb != null)
            {
                // Correct for Rigidbody-driven movement
                groundDelta = groundRb.linearVelocity * Time.fixedDeltaTime * 0.6f;
            }
            else
            {
                // Fallback for transform-driven movement
                groundDelta = currentGround.position - lastGroundPos;
            }

            // Rotation (still valid for both cases)
            Quaternion rotDelta = currentGround.rotation * Quaternion.Inverse(lastGroundRot);

            Vector3 dir = rb.position - currentGround.position;
            dir = rotDelta * dir;

            Vector3 targetPos = currentGround.position + dir;

            rb.MovePosition(rb.position + groundDelta + (targetPos - rb.position));

            // Save for next frame
            lastGroundPos = currentGround.position;
            lastGroundRot = currentGround.rotation;
        }
    }
    private void HandleCameraDirection(Vector2 dir)
    {
        if (!grounded) return;


        if (dir == Vector2.zero)
        {
            _cameraRotation.RotateCamera(Quaternion.Euler(0, 0, 0), 1);
        }
        else if (dir.x != 0)
        {
            float dir_x = Mathf.Sign(dir.x);
            _cameraRotation.RotateCamera(Quaternion.Euler(0, 2.5f * dir_x, 0), 0.25f);
        }
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
        SC_CameraRotation.instance.ImpulseUIElements(Vector2.up, 30, 1);

        // The player is falling and collided with the ground
        if (rb.linearVelocity.y < 0)
        {
            SC_CameraRotation.instance.OffsetCameraAndReturn(Vector3.down, 0.75f, Quaternion.Euler(3, 0, 0), 1, true);
            // Play smoke particles
            var main = parts_smoke.main;
            main.startDelay = 0.1f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.5f, 1);
            var emission = parts_smoke.emission;
            var burst = emission.GetBurst(0);
            burst.count = 30;
            emission.SetBurst(0, burst);
            parts_smoke.Play();
        }

        // Disables the player movement
        StartCoroutine(DisableMovement(0.75f));
    }

    private IEnumerator DisableMovement(float duration)
    {
        can_move = false;
        yield return new WaitForSeconds(duration);
        can_move = true;
    }

    #region Physics functions

    private void OnCollisionStay(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            // Check if it's below the player
            if (Vector3.Dot(contact.normal, Vector3.up) > 0.5f)
            {
                currentGround = collision.transform;
                lastGroundPos = currentGround.position;
                lastGroundRot = currentGround.rotation;
                return;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.transform == currentGround)
            currentGround = null;
    }

    public void Jump()
    {
        if ((grounded && can_move) || (!grounded && grabbing))
        {
            float up_force = jumpForce;
            if (grabbing)
            {
                rb.useGravity = true;
                rb.linearVelocity = (Camera.main.transform.forward + Vector3.up) * (vel.magnitude * ropeJumpStrength);
                grabPoint = null;
            }

            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
            SC_SoundManager.instance.InstantiateSound(clip_jump, transform.position);
            _cameraRotation.ImpulseUIElements(Vector2.down, 20, 0.5f, true);
            jumping = true;

            var main = parts_smoke.main;
            main.startDelay = 0;
            main.startSize = new ParticleSystem.MinMaxCurve(0.25f, 0.5f);
            var emission = parts_smoke.emission;
            var burst = emission.GetBurst(0);
            burst.count = 10;
            emission.SetBurst(0, burst);
            parts_smoke.Play();

            grabbing = false;
        }
    }
    Vector3 ConstrainVelocity(Vector3 vel)
    {
        vel.x = Mathf.Clamp(vel.x, -50, 50);
        vel.z = Mathf.Clamp(vel.z, -50, 50);
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