using Unity.Collections;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class SC_GeneralPhysics : MonoBehaviour
{
    public Vector3 velocity, target_velocity;

    [Header("Stats")]
    public float mass = 10;
    public bool controlled = false;
    [Range(0, 1)]
    public float bounciness = 0;

    [Header("Ground")]
    public bool grounded = false;
    public float ground_offset = 0.5f;
    [Range(0, 1)]
    public float ground_friction = 0.999f;

    [Header("Gravity")]
    public float gravity_scale = 0.5f;
    public float gravity_acceleration = 0.5f;

    [Header("Collision")]
    public LayerMask physics_layer;
    public LayerMask collision_layer;
    public float width = 1;

    private bool on_movement = false;

    // Timers
    float timer_air = 0;

    void Update()
    {
        ApplyGravity();
        ApplyGroundFriction();
        LerpVelocity();
        // Updates the position based on the velocity and mass
        UpdatePosition();
    }

    void FixedUpdate()
    {
        CheckGround();
    }

    #region Logic Functions
    void CheckGround()
    {
        if (Physics.Raycast(transform.position, -Vector2.up, out RaycastHit hit, transform.localScale.y + ground_offset))
        {
            grounded = true;

            // Cuánta distancia sobra respecto al suelo
            float rayLength = transform.localScale.y + ground_offset;
            float difference = rayLength - hit.distance;

            // Solo corrige en el eje Y/normal al suelo
            Vector3 correction = transform.up * difference;

            // Lerp suave
            transform.position += correction * 0.5f; // o Mathf.Lerp(...)
        }
        else
        {
            grounded = false;
        }

        Debug.DrawRay(transform.position, -transform.up * (transform.localScale.y + ground_offset), Color.green);
    }
    void LerpVelocity()
    {
        /// Lerps the velocity
        if (Vector3.Distance(velocity, target_velocity) < 0.01f)
            velocity = target_velocity;
        else
            velocity = Vector3.Lerp(velocity, target_velocity, Vector3.Distance(velocity, target_velocity));
    }
    void ApplyGravity()
    {
        if (!grounded)
        {
            // Adds gravity when it's not on the ground
            timer_air += Time.deltaTime;
            float g = gravity_scale + (1f - (Mathf.Exp(-gravity_acceleration * timer_air)));
            target_velocity += new Vector3(0, -g, 0);
        }
        else
        {
            // Resets vertical speed if it's on the ground
            if (target_velocity.y < 0 && velocity.y < 0)
            {
                target_velocity = new Vector3(target_velocity.x, 0, target_velocity.z);
                velocity = new Vector3(velocity.x, 0, velocity.z);
            }

            timer_air = 0;
        }
    }
    void ApplyGroundFriction()
    {
        on_movement = target_velocity.magnitude > 0.1f;

        if (controlled)
        {
            if (!on_movement)
            {
                target_velocity *= ground_friction;
            }
        }
        else
        {
            if (on_movement)
            {
                target_velocity *= ground_friction;
            }
        }
        

        target_velocity.y = Mathf.Clamp(target_velocity.y, -50, 50);
    }
    Vector3 ResolveCollisions(Vector3 movement)
    {
        float distance = movement.magnitude;
        if (distance < 0.001f) return movement; // If the object isn't moving avoid calculations

        Vector3 direction = movement.normalized;

        float capsuleHeight = transform.localScale.y;
        float capsuleRadius = transform.localScale.x * 0.5f;

        Vector3 point1 = transform.position + Vector3.up * (capsuleRadius);
        Vector3 point2 = transform.position + Vector3.up * (capsuleHeight - capsuleRadius);

        if (Physics.CapsuleCast(point1, point2, capsuleRadius, direction, out RaycastHit hit, distance, collision_layer))
        {
            float coll_dist = hit.distance - width;
            if (coll_dist < 0f) coll_dist = 0f;

            Vector3 normal = hit.normal;

            // Get the speed of the impact
            float impact = Vector3.Dot(velocity, normal);

            if (impact < 0) // Bounciness
            {
                float impulseMagnitude = -(1f + bounciness) * impact;
                impulseMagnitude /= mass;

                Vector3 impulse = normal * impulseMagnitude;

                velocity += impulse;
                target_velocity += impulse;
            }

            if (((1 << hit.collider.gameObject.layer) & physics_layer) != 0) // The collided object is a physics object
            {
                Debug.Log("hasLayer");
                SC_GeneralPhysics other_physics = hit.transform.GetComponent<SC_GeneralPhysics>();
                if (other_physics != null)
                {

                    float other_push = mass / (mass + other_physics.mass);
                    float self_push = other_physics.mass / (mass + other_physics.mass);

                    // Apply reflexive impulse
                    other_physics.target_velocity += direction * other_push;
                    target_velocity -= direction * self_push;
                }
            }

            // Sliding through the object
            Vector3 slide = Vector3.ProjectOnPlane(direction, normal);
            return slide * coll_dist;
        }

        return movement;
    }
    private void UpdatePosition(bool ignoreMass = false)
    {
        Vector3 move = velocity * Time.deltaTime;
        move = ResolveCollisions(move);

        transform.position += move / (ignoreMass ? 1 : mass);
    }
    #endregion

    #region Utility Functions
    /*
     * UTILITY FUNCTIONS
     */

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

    private void OnDrawGizmos()
    {
        
    }
}
