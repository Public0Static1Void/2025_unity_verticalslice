using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.Controls;

public class RopePoint
{
    public Vector3 position;
    public Vector3 previousPosition;

    public RopePoint(Vector3 pos)
    {
        this.position = pos;
        this.previousPosition = pos;
    }
}
public class SC_Rope : MonoBehaviour
{
    RopePoint[] points;

    [Header("Stats")]
    public int segmentCount = 5;
    public float segmentLength = 0.5f;
    public int calculationNum = 5;
    public float moveForce = 5;
    [Range(0, 1)] public float ropeDamping = 0.9f;
    public float collision_radius = 1f;


    [Header("References")]
    public LayerMask layer_collision;
    public LineRenderer ropeRenderer;
    public GameObject anchor;
    public SC_PlayerMovement playerMovement;

    public int grabbed_index = 0;
    public float t = 0;


    private void Start()
    {
        points = new RopePoint[segmentCount];

        Vector3 start = transform.position;
        for (int i = 0; i < segmentCount; i++)
        {
            points[i] = new RopePoint(start + Vector3.right * segmentLength * i);
        }

        grabbed_index = segmentCount - 2;
    }

    private void Update()
    {
        // Process the player input
        if (playerMovement.grabbing && playerMovement.grabbedRope == this)
        {
            playerMovement.rb.useGravity = false;
            //ClimbRope(playerMovement.input.y);
            AddImpulse(playerMovement.input.y);
        }
    }

    private void FixedUpdate()
    {
        ApplyGravity();
        for (int i = 0; i < calculationNum; i++)
        {
            CheckCollisions();
            ApplyConstraints();
        }
    }
    private void LateUpdate()
    {
        DrawRope();
    }
    /// <summary>
    /// Uses the verlet method to create a swing-like movement.
    /// It moves the point based on the difference in the previous position and the new one (inertia)
    /// </summary>
    private void ApplyGravity()
    {
        // Simulate gravity

        for (int i = anchor != null ? 1 : 0; i < points.Length; i++)
        {
            RopePoint point = points[i];

            Vector3 vel = (point.position - point.previousPosition) * ropeDamping; // Inertia

            point.previousPosition = point.position;
            point.position += vel; // Moves the point based on the inertia stored

            point.position += Physics.gravity * Time.fixedDeltaTime;
        }
    }
    private void ApplyConstraints()
    {
        if (anchor != null)
            points[0].position = anchor.transform.position;

        for (int i = 0; i < points.Length - 1; i++)
        {
            Vector3 diff = points[i + 1].position - points[i].position;

            float dist = diff.magnitude;

            float error = dist - segmentLength; // Gets the difference between the desired distance and the real one
                                                // If dist > segmentLength it will retract, and viceversa

            Vector3 change = diff.normalized * error * 0.5f * 0.8f;

            if (i != 0) // Don't move the first point
            {
                points[i].position += change;
            }
            if (i != points.Length - 1) // Don't move the second point
            {
                points[i + 1].position -= change;
            }
        }
    }

    private void CheckCollisions()
    {
        for (int i = anchor != null ? 1 : 0; i < points.Length; i++)
        {
            RopePoint point = points[i];
            // Get the nearby colliders
            Collider[] colls = Physics.OverlapSphere(point.position, collision_radius, layer_collision);

            foreach (Collider coll in colls)
            {
                Vector3 closest = coll.ClosestPoint(point.position);

                Vector3 repel_dir = (point.position - closest); // Gets the repel direction
                float dist = repel_dir.magnitude;

                Vector3 normal = Vector3.zero;
                if (dist <= 0.001f) // Expulses the rope point in case it's inside the collision
                {
                    normal = (point.position - coll.bounds.center).normalized;
                    dist = 0.001f;
                }
                else
                    normal = repel_dir / dist;

                float penetration = collision_radius - dist; /// Calculate how inside the collision is
                if (penetration > 0)
                {
                    point.position += normal * penetration * ropeDamping;
                }

            }
        }
    }

    private void DrawRope()
    {
        ropeRenderer.positionCount = segmentCount;
        
        for (int i = 0; i < points.Length; i++)
        {
            ropeRenderer.SetPosition(i, points[i].position);
        }
    }


    private void AddImpulse(float dir)
    {
        RopePoint p = points[grabbed_index];

        Vector3 impulse = playerMovement.transform.forward * (dir * Time.fixedDeltaTime * moveForce);

        p.previousPosition -= impulse; // Adds inertia

        Vector3 player_vel = p.position - playerMovement.rb.position;

        float springStrength = 30;
        float damping = 4;

        Vector3 player_force = player_vel * springStrength - playerMovement.rb.linearVelocity * damping;

        playerMovement.rb.AddForce(player_force, ForceMode.Acceleration);
    }

    private void ClimbRope(float dir)
    {
        t += Time.fixedDeltaTime * 3 * -dir;
        if (t >= 1)
        {
            if (grabbed_index < points.Length - 3) // Not on top
            {
                t = 0;
                grabbed_index++;
            }
            else // On top
                t = 1;
            
        }
        else if (t <= 0)
        {
            if (grabbed_index > 0) // Not on bottom
            {
                t = 1;
                grabbed_index--;
            }
            else // On bottom
                t = 0;
        }

        Vector3 targetPos = Vector3.Lerp(points[grabbed_index].position, points[grabbed_index + 1].position, t);
        playerMovement.rb.MovePosition(targetPos);
    }

    public void GrabRope(Vector3 point)
    {
        // Find the closest point
        float minDist = float.MaxValue;
        int closest = 0;
        for (int i = 0; i < points.Length; i++)
        {
            float dist = Vector3.Distance(points[i].position, point);
            if (dist < minDist)
            {
                closest = i;
                minDist = dist;
            }
        }

        playerMovement.grabbing = true;
        playerMovement.grabPoint = points[closest];

        playerMovement.rb.constraints = RigidbodyConstraints.FreezeRotation;
    }


    private void OnDrawGizmos()
    {
        if (points == null) return;

        Gizmos.color = Color.red;

        foreach (var p in points)
            Gizmos.DrawWireSphere(p.position, collision_radius);
    }
}