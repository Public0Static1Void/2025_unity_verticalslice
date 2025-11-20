using UnityEngine;
using UnityEngine.InputSystem;

public class SC_CameraRotation : MonoBehaviour
{
    public float turn_speed = 10, parent_turn_speed = 8;
    public Transform parent = null;
    private Transform camera_handle = null;
    public Vector2 input;

    private float x = 0, y = 0;

    private Rigidbody rb;

    void Start()
    {
        camera_handle = parent.GetChild(0);
        UnityEngine.Cursor.visible = false;
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;

        rb = GetComponentInParent<Rigidbody>();
    }

    void LateUpdate()
    {
        if (parent == null || camera_handle == null) return;

        x += input.x * turn_speed * Time.deltaTime;
        y += input.y * turn_speed * Time.deltaTime;

        y = Mathf.Clamp(y, -40, 60);

        // Parent and camera rotation
        rb.MoveRotation(Quaternion.Lerp(parent.rotation, Quaternion.Euler(0, x, 0), Time.deltaTime * parent_turn_speed));
        transform.rotation = Quaternion.Euler(-y, x, 0);
    }

    public void CameraTurn(InputAction.CallbackContext con)
    {
        input = con.ReadValue<Vector2>().normalized;
    }
}
