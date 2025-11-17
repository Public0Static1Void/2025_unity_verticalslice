using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SC_GeneralPhysics))]
public class SC_PlayerMovement : MonoBehaviour
{
    public SC_GeneralPhysics physics;
    public Vector2 input;
    public float speed = 10;
    public float jump_force = 100;

    void Start()
    {
        physics = GetComponent<SC_GeneralPhysics>();
    }

    private void Update()
    {
        if (physics != null)
        {
            Vector3 d = (transform.forward * input.y + transform.right * input.x) * speed;
            physics.target_velocity = new Vector3(d.x, physics.target_velocity.y, d.z);
        }
    }

    public void Jump()
    {
        physics.AddForce(transform.up * jump_force);
    }

    public void PlayerMove(InputAction.CallbackContext con)
    {
        if (con.performed)
        {
            input = con.ReadValue<Vector2>();
        }
        else if (con.canceled)
        {
            input = Vector2.zero;
        }
    }
    public void PlayerJump(InputAction.CallbackContext con)
    {
        if (con.performed)
        {
            Jump();
        }
    }
}