using UnityEngine;
using UnityEngine.InputSystem;

public class SC_Grab : MonoBehaviour
{
    [Header("Stats")]
    public float grabbed_speed = 200;
    public float grabbed_damping = 100;
    public float grab_distance = 3;

    [Header("References")]
    public GameObject grab_point;
    public LayerMask grab_layer;

    private Rigidbody rb_grabbed;
    private ConfigurableJoint rb_joint;

    private Rigidbody rb_player;

    public bool launching = false;
    public float launch_timer = 0;

    public bool grabbing = false;

    private void Start()
    {
        rb_player = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (launching)
        {
            launch_timer += Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        if (rb_grabbed != null)
        {
            Vector3 dir = (
                (grab_point.transform.position - Camera.main.transform.forward * (Mathf.Min(launch_timer, 1)))
                - rb_grabbed.transform.position);
            Vector3 vel = dir * grabbed_speed - rb_grabbed.linearVelocity * grabbed_damping;

            rb_grabbed.AddForce(vel, ForceMode.Force);
            rb_grabbed.MoveRotation(Quaternion.LookRotation(-transform.forward));
        }
    }

    private void GrabObject()
    {
        if (rb_grabbed != null) return;

        if (Physics.SphereCast(transform.position, 0.5f, Camera.main.transform.forward, out RaycastHit hit, grab_distance, grab_layer))
        {
            if (hit.rigidbody != null)
            {
                grabbing = true;

                rb_grabbed = hit.rigidbody;

                rb_grabbed.useGravity = false;
                rb_grabbed.linearVelocity = Vector3.zero;
                rb_grabbed.angularVelocity = Vector3.zero;

                rb_grabbed.transform.LookAt(grab_point.transform.parent.position);
            }
        }   
    }

    private void ReleaseObject(Vector3 release_speed)
    {
        grabbing = false;

        if (rb_grabbed == null) return;

        // Deattach the fixed object
        if (rb_joint != null)
            Destroy(rb_joint);

        rb_grabbed.useGravity = true;
        rb_grabbed.linearVelocity = release_speed;
        rb_grabbed = null;
    }

    // Inputs
    public void GrabInput(InputAction.CallbackContext con)
    {
        if (con.performed)
        {
            GrabObject();
        }
        else if (con.canceled)
        {
            ReleaseObject(rb_player.linearVelocity);
        }
    }
    public void LaunchGrabbed(InputAction.CallbackContext con)
    {
        if (rb_grabbed == null) return;

        if (con.performed)
        {
            launching = true;
        }
        else if (con.canceled)
        {
            launching = false;

            if (launch_timer >= 1)
            {
                launch_timer *= 10;
                if (launch_timer >= 20) launch_timer = 20; /// Limit the launch speed

                ReleaseObject(((rb_player.linearVelocity * 0.25f) + Camera.main.transform.forward) * launch_timer);
            }

            launch_timer = 0;
        }
    }

    // Gizmos
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.grey;
        Gizmos.DrawWireSphere(transform.position, grab_distance);
    }
}