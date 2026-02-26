using NUnit.Framework;
using System.Drawing;
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
    public float moveForce = 5, slide_speed = 5;
    public float slide_acceleration = 5;
    [UnityEngine.Range(0, 1)] public float ropeDamping = 0.9f;
    public float collision_radius = 1f;


    [Header("References")]
    public LayerMask layer_collision;
    public LineRenderer ropeRenderer;
    public GameObject anchor, bottom_anchor;
    public SC_PlayerMovement playerMovement;
    public AudioClip clip_slide;

    [Header("Functions")]
    public bool scalable = false;
    public bool slideable = false;

    public int grabbed_index = 0;
    public float t = 0;
    public float current_distance = 0;

    public float start_slide_speed = 0, max_slide_speed = 8;

    private AudioSource slide_audiosource = null;


    private void Start()
    {
        slide_audiosource = SC_SoundManager.instance.InstantiateSound(clip_slide, transform.position, true).GetComponent<AudioSource>();
        slide_audiosource.Stop();

        points = new RopePoint[segmentCount];

        Vector3 start = transform.position;
        for (int i = 0; i < segmentCount; i++)
        {
            points[i] = new RopePoint(start + Vector3.left * segmentLength * i);
        }

        grabbed_index = segmentCount - 2;

        start_slide_speed = slide_speed;
        max_slide_speed = start_slide_speed + 10;
    }

    private void FixedUpdate()
    {
        ApplyGravity();

        // Process the player input
        if (playerMovement.grabbing && playerMovement.grabbedRope == this)
        {
            playerMovement.rb.useGravity = false;

            if (scalable)
            {
                ClimbRope(-playerMovement.input.y);
            }
            else
            {
                AddImpulse(playerMovement.input.y);
            }

            slide_audiosource.transform.position = playerMovement.transform.position + Vector3.up * 2;
        }

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

        for (int i = anchor != null ? 1 : 0; i < points.Length - (bottom_anchor != null ? 1 : 0); i++)
        {
            RopePoint point = points[i];

            Vector3 vel = (point.position - point.previousPosition) * ropeDamping; // Inertia
            vel += Vector3.ProjectOnPlane(vel, (point.position - points[0].position).normalized) * 0.1f;

            point.previousPosition = point.position;
            point.position += vel; // Moves the point based on the inertia stored

            point.position += Physics.gravity * (Time.fixedDeltaTime);
        }
    }
    private void ApplyConstraints()
    {
        if (anchor != null)
            points[0].position = anchor.transform.position;
        if (bottom_anchor != null)
            points[points.Length -1].position = bottom_anchor.transform.position;

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
        for (int i = anchor != null ? 1 : 0; i < points.Length - (bottom_anchor != null ? 1 : 0); i++)
        {
            RopePoint point = points[i];
            // Get the nearby colliders
            Collider[] colls = Physics.OverlapSphere(point.position, collision_radius, layer_collision);

            foreach (Collider coll in colls)
            {
                // Check if the player has to be attached to the rope
                if (coll.name == "Player" && !playerMovement.grounded && playerMovement.grabPoint == null && playerMovement.grabbedRope != this)
                {
                    HandlePlayerGrab(i, point);
                    continue;
                }
                if (coll.name == "Player" && playerMovement.grabbing)
                    continue;
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

    private void HandlePlayerGrab(int i, RopePoint point)
    {
        grabbed_index = i;
        current_distance = 1 - (Vector3.Distance(points[grabbed_index].position, points[0].position) / RopeLength());
        playerMovement.grabbing = true;
        playerMovement.grabbedRope = this;
        playerMovement.grabPoint = point;

        points[grabbed_index].position += playerMovement.rb.linearVelocity * 0.5f;
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

        if (Mathf.Abs(dir) < 0.01f) return;

        // Gets the current speed of the rope
        Vector3 rope_velocity = p.position - p.previousPosition;

        // Gets the direction of the rope point from the anchor
        Vector3 rope_dir = (p.position - points[0].position).normalized;
        Vector3 tangent = Vector3.ProjectOnPlane(playerMovement.transform.forward, rope_dir).normalized;

        float swing_dir = Vector3.Dot(rope_velocity, tangent);

        float timing = Mathf.Sign(dir) == Mathf.Sign(swing_dir) ? 1.5f : 0;

        Vector3 impulse = tangent * dir * moveForce * timing * Time.fixedDeltaTime;

        // Build up inertia
        p.previousPosition -= impulse;
        playerMovement.vel += impulse;
    }

    private void ClimbRope(float dir)
    {
        float speed = moveForce;
        if (slideable && dir == 0)
        {
            dir = GetIndexDirToLowest();
            slide_speed = Mathf.Lerp(slide_speed, max_slide_speed, Time.deltaTime * slide_acceleration);
            if (slide_speed > max_slide_speed)
                slide_speed = max_slide_speed;

            speed += slide_speed;
        }
        else
        {
            slide_speed = 0;
        }

        if (dir != 0 || (dir == 0 && slideable))
        {
            // Play slide sound
            if (!slide_audiosource.isPlaying)
            {
                slide_audiosource.Play();
            }
        }
        if ((slideable && grabbed_index == points.Length - 3) || (dir == -1 && grabbed_index == 0))
        {
            // Play slide sound
            if (slide_audiosource.isPlaying)
            {
                slide_audiosource.Stop();
            }
        }

        float segmentLength = Vector3.Distance(points[grabbed_index].position, points[grabbed_index + 1].position);
        t += (Time.fixedDeltaTime * dir * speed) / segmentLength;

        if (t >= 1)
        {
            if (grabbed_index < points.Length - 3) // Not on top
            {
                t = 0;
                grabbed_index++;
            }
            else // On top
            {
                t = 1;
            }

        }
        else if (t <= 0)
        {
            if (grabbed_index > 0) // Not on bottom
            {
                t = 1;
                grabbed_index--;
            }
            else // On bottom
            {
                t = 0;
            }
        }

        Vector3 targetPos = Vector3.Lerp(points[grabbed_index].position, points[grabbed_index + 1].position, t);
        playerMovement.rb.MovePosition(targetPos);
        playerMovement.vel = Vector3.one * speed;
    }

    private int GetIndexDirToLowest(bool search_lowest = true)
    {
        if (grabbed_index <= 0) return 1;
        if (grabbed_index >= points.Length - 1) return -1;

        float lowest = points[grabbed_index].position.y;
        float next_lowest_index = 0;
        for (int i = 0; i < points.Length; i++)
        {
            if (i == grabbed_index) continue;

            if (search_lowest)
            {
                if (points[i].position.y < lowest)
                {
                    lowest = points[i].position.y;
                    next_lowest_index = i;
                    break;
                }
            }
            else
            {
                if (points[i].position.y > lowest)
                {
                    lowest = points[i].position.y;
                    next_lowest_index = i;
                }
            }
        }

        if (grabbed_index == next_lowest_index) return 0;

        return next_lowest_index > grabbed_index ? 1 : -1;
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

    /// <summary>
    /// Gets the max distance of the rope
    /// </summary>
    float RopeLength()
    {
        float accumulated = 0;
        for (int i = 0; i < points.Length - 1; i++)
        {
            accumulated += Vector3.Distance(points[i].position, points[i + 1].position);
        }
        return accumulated;
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            Gizmos.color = UnityEngine.Color.gray;
            if (bottom_anchor == null)
            {
                for (int i = 0; i < segmentCount; i++)
                {
                    Gizmos.DrawWireSphere(transform.position + Vector3.down * segmentLength * i, collision_radius);
                }
            }
            else
            {
                Vector3 start = transform.position;
                Vector3 end = bottom_anchor.transform.position;

                float sagAmount = 3; // how much rope sags

                // midpoint between anchors
                Vector3 mid = (start + end) * 0.5f;

                // pull midpoint DOWN to create sag
                Vector3 control = mid + Vector3.down * sagAmount;

                for (int i = 0; i < segmentCount; i++)
                {
                    float t = i / (float)(segmentCount - 1);

                    Vector3 pos =
                        Mathf.Pow(1 - t, 2) * start +
                        2 * (1 - t) * t * control +
                        Mathf.Pow(t, 2) * end;

                    Gizmos.DrawWireSphere(pos, collision_radius);
                }
            }
        }
        if (points == null) return;

        Gizmos.color = UnityEngine.Color.red;

        foreach (var p in points)
            Gizmos.DrawWireSphere(p.position, collision_radius);
    }
}