using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SC_CameraRotation : MonoBehaviour
{
    public static SC_CameraRotation instance {  get; private set; }
    public float turn_speed = 10, parent_turn_speed = 8;
    public Transform parent = null;
    private Transform camera_handle = null;
    public Vector2 input;

    public float x = 0, y = 0;

    private Rigidbody rb;

    public Slider slider_sensivity;
    public TMP_Text txt_value_sensivity;

    private float max_turn_speed = 0;

    [Header("CameraShake")]
    public float shake_force = 3;
    private Vector3 pos_start_local;
    private Coroutine routine_shake;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }
    void Start()
    {
        camera_handle = parent.GetChild(0);
        UnityEngine.Cursor.visible = false;
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;

        max_turn_speed = turn_speed;

        rb = GetComponentInParent<Rigidbody>();

       txt_value_sensivity.text = slider_sensivity.value.ToString();


        pos_start_local = transform.localPosition;
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
        input = con.ReadValue<Vector2>();
    }



    #region CameraShake
    public void ShakeCamera(float force, float duration)
    {
        // Checks if there is an active camera shake
        if (routine_shake != null) return;

        routine_shake = StartCoroutine(ShakeCameraRoutine(force, duration));
    }
    private IEnumerator ShakeCameraRoutine(float force, float duration)
    {
        float timer = 0;

        float seed = UnityEngine.Random.Range(0, 100);

        while (timer < duration)
        {
            float t = timer / duration;
            float apply_force = Mathf.Lerp(force, 0, t);

            float noiseX = Mathf.PerlinNoise(seed, timer * 30);
            float noiseY = Mathf.PerlinNoise(timer * 30, seed);

            timer += Time.deltaTime;

            transform.localPosition = pos_start_local + new Vector3(noiseX, noiseY, 0) * apply_force;
            yield return null;
        }

        transform.localPosition = pos_start_local;

        routine_shake = null;
    }
    #endregion



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
        parent_turn_speed = Mathf.Max(new_sens - (new_sens * 0.5f), 10);

        txt_value_sensivity.text = new_sens.ToString();
    }
}