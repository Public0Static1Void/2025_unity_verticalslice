using UnityEngine;

public class SC_GeneralPhysics : MonoBehaviour
{
    public float speed = 10;
    public Vector3 velocity;

    public bool grounded = false;
    public float ground_offset = 0.5f;

    [Header("Gravity")]
    public float gravity_scale = 1.8f;
    void Start()
    {

    }

    void Update()
    {
        if (!grounded)
        {
            velocity += new Vector3(0, -gravity_scale * velocity.y, 0);
        }
        velocity *= Time.deltaTime;

        transform.position += velocity;
    }

    void FixedUpdate()
    {
        grounded = Physics.Raycast(transform.position, -transform.up, transform.localScale.y + ground_offset);
    }
}
