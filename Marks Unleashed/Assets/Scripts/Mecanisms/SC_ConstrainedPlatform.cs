using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class SC_ConstrainedPlatform : MonoBehaviour
{
    [Header("Position")]
    public float max_y = 0, min_y = 0;
    public bool return_to_start = true;

    [Header("Stats")]
    [Range(0, 1)] public float damping = 0.99f;
    public float return_force = 0.5f;
    public float return_offset = 0.1f;

    private Rigidbody _rb;
    public MeshFilter _meshFilter;
    private Mesh _mesh;

    private Vector3 _center_pos;

    [Header("Ropes")]
    public List<SC_Rope> attached_ropes;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _mesh = GetComponent<MeshFilter>().mesh;
        _center_pos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 pos = _rb.position;
        Vector3 vel = _rb.linearVelocity;
        if (pos.y < min_y)
        {
            pos.y = min_y;
            if (vel.y < 0)
                vel.y = 0;

            _rb.MovePosition(pos);
        }
        else if (pos.y > max_y)
        {
            pos.y = max_y;

            if (vel.y > 0) // only if moving further up
                vel.y = 0;

            _rb.MovePosition(pos);
        }

        if (return_to_start)
        {
            Vector3 diff = (new Vector3(_center_pos.x, 0, _center_pos.z) - new Vector3(transform.position.x, 0, transform.position.z));
            float dist = diff.magnitude;
            if (dist > return_offset)
            {
                vel += diff.normalized * return_force;
            }
        }
        
        _rb.linearVelocity = vel;
        _rb.linearVelocity = ClampVelocity(_rb.linearVelocity);
        _rb.linearVelocity = ApplyDamping(_rb.linearVelocity);
    }

    public void RopeStretchedEvent(SC_Rope rope)
    {


        //ApplyRopeConstraint(rope);
    }

    void ApplyRopeConstraint(SC_Rope rope)
    {
        Vector3 anchor = rope.anchor.transform.position;
        Vector3 pos = transform.position;

        float maxLength = rope.segmentCount * rope.segmentLength;

        Vector3 delta = pos - anchor;
        float dist = delta.magnitude;

        if (dist > maxLength)
        {
            Vector3 corrected = anchor + delta.normalized * maxLength;

            // Move directly (NO velocity, NO force)
            _rb.position = corrected;

            Vector3 dir = -delta.normalized;
            _rb.linearVelocity -= dir * (maxLength - dist);
        }
    }

    private Vector3 ClampVelocity(Vector3 vel)
    {
        vel.x = Mathf.Clamp(vel.x, -5, 5);
        vel.y = Mathf.Clamp(vel.y, -5, 5);
        vel.z = Mathf.Clamp(vel.z, -5, 5);
        return vel;
    }
    private Vector3 ApplyDamping(Vector3 vel)
    {
        vel.x *= damping;
        vel.z *= damping;
        return vel;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireMesh(_meshFilter.mesh, 0, new Vector3(transform.position.x, min_y, transform.position.z), transform.rotation, transform.localScale);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireMesh(_meshFilter.mesh, 0, new Vector3(transform.position.x, max_y, transform.position.z), transform.rotation, transform.localScale);
    }
}