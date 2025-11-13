using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SC_GeneralPhysics))]
public class SC_PlayerMovement : MonoBehaviour
{
    public SC_GeneralPhysics physics;
    public Vector2 input;
    public float speed = 10;

    void Start()
    {
        physics = GetComponent<SC_GeneralPhysics>();
    }

    private void Update()
    {
        if (physics != null)
        {
            physics.velocity += new Vector3(input.x * speed, 0, input.y * speed);
        }
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
}
