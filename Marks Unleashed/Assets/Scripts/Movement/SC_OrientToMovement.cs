using UnityEngine;

public class SC_OrientToMovement : MonoBehaviour
{
    public Transform target;

    private Vector3 previous_pos;
    [HideInInspector]public Rigidbody _rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    public virtual void Update()
    {
        if (target == null) return;
        if (_rb != null && _rb.linearVelocity.magnitude < 5) return;

        // Rotate the disk with the current movement direction
        Vector3 dir = (transform.position - previous_pos).normalized;
        if (dir.sqrMagnitude > 0.0001f)
            target.rotation = Quaternion.LookRotation(dir);

        /// Store the last frame position
        previous_pos = transform.position;
    }
}
