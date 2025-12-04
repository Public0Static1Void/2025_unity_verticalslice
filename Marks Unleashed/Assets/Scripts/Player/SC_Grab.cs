using UnityEngine;
using UnityEngine.InputSystem;

public class SC_Grab : MonoBehaviour
{
    public float grab_distance = 3;
    public Rigidbody grab_point;
    public LayerMask grab_layer;

    private Rigidbody rb_grabbed;
    private FixedJoint rb_joint;

    private Rigidbody rb_player;

    private bool launching = false;
    public float launch_timer = 0;

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

    private void GrabObject()
    {
        if (rb_grabbed != null) return;

        if (Physics.SphereCast(transform.position, 0.5f, Camera.main.transform.forward, out RaycastHit hit, grab_distance, grab_layer))
        {
            if (hit.distance <= grab_distance)
            {
                rb_grabbed = hit.rigidbody;
                rb_joint = rb_grabbed.gameObject.AddComponent<FixedJoint>();

                rb_joint.connectedBody = grab_point;
                rb_joint.breakForce = 100;
                rb_joint.breakTorque = 100;

                rb_grabbed.useGravity = false;
            }
        }   
    }

    private void ReleaseObject(Vector3 release_speed)
    {
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
                if (launch_timer >= 10) launch_timer = 10; /// Limit the launch speed

                ReleaseObject((rb_player.linearVelocity + Camera.main.transform.forward) * launch_timer);
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
