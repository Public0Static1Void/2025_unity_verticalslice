using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SC_CameraRotation : MonoBehaviour
{
    public static SC_CameraRotation instance {  get; private set; }

    [Header("Stats")]
    public float turn_speed = 10;
    public float parent_turn_speed = 8;
    [Range(0, 1)] public float fov_multiplier = 0.5f;

    private float base_fov;

    [Header("References")]
    public Camera mainCamera;
    public Transform parent = null;
    private Transform camera_handle = null;
    public Vector2 input;

    public float x = 0, y = 0;

    private Rigidbody rb;
    private SC_PlayerMovement playerMovement;

    public Slider slider_sensivity;
    public TMP_Text txt_value_sensivity;

    private float max_turn_speed = 0;

    [Header("CameraShake")]
    public float shake_force = 3;
    private Vector3 pos_start_local;
    private Vector3 pos_offset_current, pos_shake_current;
    private Quaternion rot_offset_current;
    private Coroutine routine_shake, routine_offset;

    [Header("UI Elements")]
    public float ui_elements_speed = 1;
    public List<RectTransform> ui_elements_to_move;

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

        playerMovement = parent.GetComponent<SC_PlayerMovement>();

        UnityEngine.Cursor.visible = false;
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;

        max_turn_speed = turn_speed;
        base_fov = Camera.main.fieldOfView;

        rb = GetComponentInParent<Rigidbody>();

        txt_value_sensivity.text = slider_sensivity.value.ToString();


        pos_start_local = transform.localPosition;
        rot_offset_current = Quaternion.identity;
    }

    private void FixedUpdate()
    {
        float targetFov = base_fov + Mathf.Clamp(rb.linearVelocity.magnitude * (playerMovement.grounded ? 0 : fov_multiplier), 0, 10);
        mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, targetFov, Time.deltaTime * 10);
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
        transform.rotation = Quaternion.Euler(-y, x, 0) * rot_offset_current;

        // Add the current offset and shake
        transform.localPosition = pos_start_local + pos_offset_current + pos_shake_current;

        MoveUIElements();
    }

    private void MoveUIElements()
    {
        for (int i = 0; i < ui_elements_to_move.Count; i++)
        {
            Vector2 pos = ui_elements_to_move[i].anchoredPosition;
            Vector2 inp = input.normalized;
            ui_elements_to_move[i].anchoredPosition = new Vector2(
                Mathf.Lerp(pos.x, inp.x * (ui_elements_to_move[i].rect.x + (ui_elements_to_move[i].rect.width * ui_elements_to_move[i].localScale.x) * 0.75f), Time.deltaTime * ui_elements_speed),
                Mathf.Lerp(pos.y, inp.y * (ui_elements_to_move[i].rect.y + (ui_elements_to_move[i].rect.height * ui_elements_to_move[i].localScale.y) * 0.75f), Time.deltaTime * ui_elements_speed)
            );
        }
    }

    public void CameraTurn(InputAction.CallbackContext con)
    {
        input = con.ReadValue<Vector2>();
    }



    #region CameraShake
    public void ShakeCamera(float force, float duration, float delay = 0)
    {
        // Checks if there is an active camera shake
        if (routine_shake != null) return;

        routine_shake = StartCoroutine(ShakeCameraRoutine(force, duration, delay));
    }
    private IEnumerator ShakeCameraRoutine(float force, float duration, float delay)
    {
        yield return new WaitForSeconds(delay);

        float timer = 0;

        float seed = UnityEngine.Random.Range(0, 100);

        while (timer < duration)
        {
            float t = timer / duration;
            float apply_force = Mathf.Lerp(force, 0, t);

            float noiseX = Mathf.PerlinNoise(seed, timer * 30);
            float noiseY = Mathf.PerlinNoise(timer * 30, seed);

            timer += Time.deltaTime;

            pos_shake_current = new Vector3(noiseX, noiseY, 0) * apply_force;
            yield return null;
        }

        pos_shake_current = Vector3.zero;

        routine_shake = null;
    }
    #endregion

    #region CameraOffsets
    public void OffsetCamera(Vector3 direction, float distance, Quaternion rotation, float duration)
    {
        if (routine_offset != null) return;

        routine_offset = StartCoroutine(OffsetCameraRoutine(direction, distance, rotation, duration));
    }
    private IEnumerator OffsetCameraRoutine(Vector3 direction, float distance, Quaternion rotation, float duration)
    {
        float timer = 0;

        duration /= 2;

        Vector3 start_pos = pos_start_local;
        Vector3 target_pos = pos_start_local + (direction * distance);

        while (timer < duration)
        {
            float t = timer / duration;
            t = Mathf.SmoothStep(0, 1, t);

            pos_offset_current = Vector3.Lerp(start_pos, target_pos, t) - pos_start_local;
            rot_offset_current = Quaternion.Lerp(Quaternion.identity, rotation, t);

            timer += Time.deltaTime;
            yield return null;
        }

        timer = 0;
        while (timer < duration)
        {
            float t = timer / duration;
            t = Mathf.SmoothStep(0, 1, t);

            pos_offset_current = Vector3.Lerp(target_pos, start_pos, t) - pos_start_local;
            rot_offset_current = Quaternion.Lerp(rotation, Quaternion.identity, t);

            timer += Time.deltaTime;
            yield return null;
        }

        pos_offset_current = Vector3.zero;
        rot_offset_current = Quaternion.identity;

        routine_offset = null;
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