using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SC_CameraRotation : MonoBehaviour
{
    public float turn_speed = 10, parent_turn_speed = 8;
    public Transform parent = null;
    private Transform camera_handle = null;
    public Vector2 input;

    public float x = 0, y = 0;

    private Rigidbody rb;

    public Slider slider_sensivity;
    public TMP_Text txt_value_sensivity;

    private float max_turn_speed = 0;

    void Start()
    {
        camera_handle = parent.GetChild(0);
        UnityEngine.Cursor.visible = false;
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;

        max_turn_speed = turn_speed;

        rb = GetComponentInParent<Rigidbody>();

       txt_value_sensivity.text = slider_sensivity.value.ToString();
    }

    void LateUpdate()
    {
        if (parent == null || camera_handle == null) return;

        x += input.x * turn_speed * Time.deltaTime;
        y += input.y * turn_speed * Time.deltaTime;

        if (x >= 360) x = 0;

        y = Mathf.Clamp(y, -80, 90);

        // Parent and camera rotation
        parent.rotation = Quaternion.Lerp(parent.rotation, Quaternion.Euler(0, x, 0), Time.deltaTime * parent_turn_speed);
        transform.rotation = Quaternion.Euler(-y, x, 0);
    }

    public void CameraTurn(InputAction.CallbackContext con)
    {
        input = con.ReadValue<Vector2>().normalized;
    }

    /// <summary>
    /// Set turn speed based on the porcentage
    /// </summary>
    public void SetTurnSpeed(float porcentage)
    {
        turn_speed = max_turn_speed * porcentage;
    }

    public void SetSensitivity()
    {
        float new_sens = slider_sensivity.value;
        turn_speed = new_sens;
        parent_turn_speed = new_sens - (new_sens * 0.5f);

        txt_value_sensivity.text = new_sens.ToString();
    }
}