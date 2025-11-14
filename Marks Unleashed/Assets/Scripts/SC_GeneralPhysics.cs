using Unity.Collections;
using UnityEngine;

public class SC_GeneralPhysics : MonoBehaviour
{
    public Vector3 velocity, target_velocity;

    [Header("Stats")]
    public float mass = 10;

    [Header("Ground")]
    public bool grounded = false;
    public float ground_offset = 0.5f;
    [Range(0, 1)]
    public float ground_friction = 0.999f;

    [Header("Gravity")]
    public float gravity_scale = 0.5f;
    public float gravity_acceleration = 0.5f;
    

    private bool on_movement = false;

    // Timers
    float timer_air = 0;
    void Start()
    {

    }

    void Update()
    {
        on_movement = target_velocity.magnitude > 0;

        if (!grounded)
        {
            // Adds gravity when it's not on the ground
            timer_air += Time.deltaTime;
            float g = gravity_scale + (1f - (Mathf.Exp(-gravity_acceleration * timer_air)));
            target_velocity += new Vector3(0, -g, 0);
        }
        else
        {
            if (on_movement)
            {
                target_velocity *= ground_friction;
            }
            // Resets vertical speed if it's on the ground
            target_velocity =   new Vector3(target_velocity.x,  0, target_velocity.z);
            velocity        =   new Vector3(velocity.x,         0, velocity.z);

            timer_air = 0;
        }

        target_velocity.y = Mathf.Clamp(target_velocity.y, -25, 25);
        
        /// Lerps the velocity
        if (Vector3.Distance(velocity, target_velocity) < 0.01f)
            velocity = target_velocity;
        else
            velocity = Vector3.Lerp(velocity, target_velocity, Vector3.Distance(velocity, target_velocity));

        // Updates the position based on the velocity and mass
        UpdatePosition();
    }

    void FixedUpdate()
    {
        grounded = Physics.Raycast(transform.position, -transform.up, transform.localScale.y + ground_offset);
        Debug.DrawRay(transform.position, -transform.up * (transform.localScale.y + ground_offset), Color.green);
    }



    #region Utility Functions
    /*
     * UTILITY FUNCTIONS
     */
    private void UpdatePosition(bool ignoreMass = false)
    {
        transform.position += velocity * Time.deltaTime / (ignoreMass ? 1 : mass);
    }
    public void AddForce(Vector3 force, bool ignoreMass = false)
    {
        target_velocity += force;
        if (ignoreMass)
        {
            velocity += force;
            UpdatePosition(true);
        }
    }
    #endregion
}
