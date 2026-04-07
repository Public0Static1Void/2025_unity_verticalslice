using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SC_ClampRbVelocity : MonoBehaviour
{
    public Vector3 velocity_limit;
    [Range(0, 1)]public float dampen = 0.99f;

    private Rigidbody _rb;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        _rb.linearVelocity = ClampVelocity(_rb.linearVelocity);
        _rb.linearVelocity *= dampen;
    }

    private Vector3 ClampVelocity(Vector3 vel)
    {
        vel.x = Mathf.Clamp(vel.x, -velocity_limit.x, velocity_limit.x);
        vel.y = Mathf.Clamp(vel.y, -velocity_limit.y, velocity_limit.y);
        vel.z = Mathf.Clamp(vel.z, -velocity_limit.z, velocity_limit.z);
        return vel;
    }
}