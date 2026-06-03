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
    public float bob_speed = 1, bob_amount = 1;
    public float sway = 1;
    public bool canMoveCamera = true;

    [HideInInspector] public float base_fov;

    [Header("References")]
    public Camera mainCamera;
    public Transform parent = null;
    private Transform camera_handle = null;
    public Vector2 input;

    public float x = 0, y = 0;

    private Rigidbody rb;
    private SC_PlayerMovement playerMovement;

    public Slider slider_sensivity;
    public TMP_Text sensitivity_value_text;

    private float max_turn_speed = 0;

    [Header("CameraShake")]
    public float shake_force = 3;
    private Vector3 pos_start_local;
    private Vector3 pos_offset_current, pos_shake_current;
    private Quaternion rot_offset_current;

    private Vector3 pos_bob;
    

    [Header("UI Elements")]
    public float ui_elements_speed = 1;
    public List<RectTransform> ui_elements_to_move;
    private Vector2[] ui_elements_start_pos;

    private Coroutine routine_shake, routine_offset, routine_fov;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }
    void Start()
    {
        mainCamera = GetComponent<Camera>();

        camera_handle = parent.GetChild(0);

        playerMovement = parent.GetComponent<SC_PlayerMovement>();

        UnityEngine.Cursor.visible = false;
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;

        max_turn_speed = turn_speed;
        base_fov = Camera.main.fieldOfView;

        rb = GetComponentInParent<Rigidbody>();

        sensitivity_value_text.text = slider_sensivity.value.ToString();


        pos_start_local = transform.localPosition;
        rot_offset_current = Quaternion.identity;

        ui_elements_start_pos = new Vector2[ui_elements_to_move.Count];
        for (int i = 0; i < ui_elements_to_move.Count; i++)
        {
            ui_elements_start_pos[i] = ui_elements_to_move[i].anchoredPosition;
        }
    }

    private void FixedUpdate()
    {
        if (routine_fov == null)
        {
            float targetFov = base_fov + Mathf.Clamp(rb.linearVelocity.magnitude * (playerMovement.grounded ? 0 : fov_multiplier), 0, 10);
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, targetFov, Time.deltaTime * 10);
        }
    }

    void LateUpdate()
    {
        if (parent == null || camera_handle == null || canMoveCamera == false) return;

        x += input.x * turn_speed * Time.deltaTime;
        y += input.y * turn_speed * Time.deltaTime * 2;

        if (x >= 360) x = 0;

        y = Mathf.Clamp(y, -80, 90);

        // Parent and camera rotation
        parent.rotation = Quaternion.Lerp(parent.rotation, Quaternion.Euler(0, x, 0), Time.deltaTime * parent_turn_speed);
        transform.rotation = Quaternion.Euler(-y, x, 0);

        HandleBobEffect();

        // Add the current offset and shake
        transform.localPosition = pos_start_local + pos_offset_current + pos_shake_current + pos_bob;

        MoveUIElements();
    }

    private void HandleBobEffect()
    {
        if (playerMovement.moving)
        {
            float bob = Mathf.Sin(Time.time * bob_speed) * bob_amount;
            pos_bob = new Vector3(0, bob, 0);
        }
        else
        {
            pos_bob = Vector3.Lerp(pos_bob, Vector3.zero, Time.deltaTime * bob_speed);
        }
    }

    public void CameraTurn(InputAction.CallbackContext con)
    {
        input = con.ReadValue<Vector2>();
    }

    #region UIEffects
    public void AddUIElementToMove(RectTransform rect)
    {
        ui_elements_to_move.Add(rect);
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

    public void ShakeUIElements(float force, float duration)
    {
        StartCoroutine(ShakeUIElementsRoutine(force, duration));
    }
    public void ImpulseUIElements(Vector2 dir, float distance, float time, bool fast = false)
    {
        StartCoroutine(ImpulseUIElementsRoutine(dir, distance, time / 2, fast));
    }
    private IEnumerator ShakeUIElementsRoutine(float force, float duration)
    {
        float timer = 0;
        float seed = UnityEngine.Random.Range(0, 100);
        while (timer < duration)
        {
            float t = timer / duration;
            float apply_force = Mathf.Lerp(force, 0, t);

            float randX = Mathf.PerlinNoise(timer * 30, seed) * 2 - 1;
            float randY = Mathf.PerlinNoise(seed, timer * 30) * 2 - 1;

            for (int i = 0; i < ui_elements_to_move.Count; i++)
            {
                if (ui_elements_to_move[i] == null) continue;

                Vector2 pos = ui_elements_to_move[i].anchoredPosition;
                Vector2 inp = input.normalized;
                ui_elements_to_move[i].anchoredPosition = Vector2.Lerp(ui_elements_start_pos[i], new Vector2(randX, randY) * apply_force, t);
            }

            timer += Time.deltaTime;
            yield return null;
        }
    }
    private IEnumerator ImpulseUIElementsRoutine(Vector2 dir, float distance, float time, bool fast = false)
    {
        float timer = 0;
        float start_time = time;
        if (fast)
            time = start_time * 0.25f;
        else
            time = time * 0.5f;
        // Lerp to the extrem
        while (timer < time)
        {
            float t = timer / time;
            t = Mathf.SmoothStep(0, 1, t);

            for (int i = 0; i < ui_elements_to_move.Count; i++)
            {
                Vector2 pos = ui_elements_to_move[i].anchoredPosition;
                Vector2 inp = input.normalized;
                ui_elements_to_move[i].anchoredPosition = Vector2.Lerp(ui_elements_start_pos[i], ui_elements_start_pos[i] + (dir * distance), t);
            }

            timer += Time.deltaTime;
            yield return null;
        }
        timer = 0;
        if (fast)
            time = start_time * 0.75f;
        else
            time = start_time * 0.5f;
        // Lerp to the extrem
        while (timer < time)
        {
            float t = timer / time;
            t = Mathf.SmoothStep(0, 1, t);

            for (int i = 0; i < ui_elements_to_move.Count; i++)
            {
                Vector2 pos = ui_elements_to_move[i].anchoredPosition;
                Vector2 inp = input.normalized;
                ui_elements_to_move[i].anchoredPosition = Vector2.Lerp(ui_elements_start_pos[i] + (dir * distance), ui_elements_start_pos[i], t);
            }

            timer += Time.deltaTime;
            yield return null;
        }
    }
    #endregion

    #region CameraShake
    public void ShakeCamera(float force, float duration, float delay = 0)
    {
        // Checks if there is an active camera shake
        if (routine_shake != null) return;

        routine_shake = StartCoroutine(ShakeCameraRoutine(force, duration, delay));
    }
    public void ShakeCameraProgressive(float min_force, float max_force, float duration, float delay = 0)
    {
        // Checks if there is an active camera shake
        if (routine_shake != null) return;
        routine_shake = StartCoroutine(ShakeCameraProgressiveRoutine(min_force, max_force, duration, delay));
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
    private IEnumerator ShakeCameraProgressiveRoutine(float min_force, float max_force, float duration, float delay = 0)
    {
        yield return new WaitForSeconds(delay);

        float timer = 0;

        float seed = UnityEngine.Random.Range(0, 100);

        while (timer < duration)
        {
            float t = timer / duration;
            float apply_force = Mathf.Lerp(min_force, max_force, t);
            apply_force = Mathf.Lerp(apply_force, 0, t);

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
    public void ChangeFov(float new_fov, float duration)
    {
        if (routine_fov != null) StopCoroutine(routine_fov);
        Debug.Log($"Changing fov to {new_fov} for {duration}s");
        routine_fov = StartCoroutine(ChangeFovRoutine(new_fov, duration));
    }
    private IEnumerator ChangeFovRoutine(float new_fov, float duration)
    {
        float timer = 0;
        float start_fov = mainCamera.fieldOfView;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            Debug.Log($"Camera fov: {mainCamera.fieldOfView}");
            mainCamera.fieldOfView = Mathf.Lerp(start_fov, new_fov, t);

            yield return null;
        }

        mainCamera.fieldOfView = new_fov;

        //routine_fov = null;
    }

    public void RotateCamera(Quaternion rot, float duration)
    {
        if (routine_offset != null) return;
        if (Quaternion.Angle(rot_offset_current, rot) < 1) return;
        routine_offset = StartCoroutine(RotateCameraRoutine(rot, duration));
    }
    public void RestoreCameraOffset(float duration)
    {
        if (routine_offset != null) StopCoroutine(routine_offset);
        routine_offset = StartCoroutine(ReturnCameraOffsetRoutine(duration));
    }
    public void OffsetCamera(Vector3 direction, float distance, float duration, bool priority = false)
    {
        if (!priority && routine_offset != null)
            return;
        else if (routine_offset != null)
            StopCoroutine(routine_offset);
        routine_offset = StartCoroutine(OffsetCameraRoutine(direction, distance, duration));
    }
    public void OffsetCameraAndReturn(Vector3 direction, float distance, Quaternion rotation, float duration, bool priority = false, bool fast = false)
    {
        if (!priority && routine_offset != null)
            return;
        else if (routine_offset != null)
            StopCoroutine(routine_offset);

        routine_offset = StartCoroutine(OffsetCameraAndReturnRoutine(direction, distance, rotation, duration, fast));
    }
    private IEnumerator RotateCameraRoutine(Quaternion rotation, float duration)
    {
        float timer = 0;
        Quaternion rot_start = rot_offset_current;
        while (timer < duration)
        {
            float t = timer / duration;
            t = Mathf.SmoothStep(0, 1, t);

            rot_offset_current = Quaternion.Lerp(rot_start, rotation, t);

            timer += Time.deltaTime;
            yield return null;
        }

        rot_offset_current = rotation;

        routine_offset = null;
    }
    private IEnumerator ReturnCameraOffsetRoutine(float duration)
    {
        float timer = 0;

        Vector3 start_pos = pos_offset_current;

        while (timer < duration)
        {
            float t = timer / duration;
            t = Mathf.SmoothStep(0, 1, t);

            pos_offset_current = Vector3.Lerp(start_pos, Vector3.zero, t);

            timer += Time.deltaTime;
            yield return null;
        }

        pos_offset_current = Vector3.zero;

        routine_offset = null;
    }
    private IEnumerator OffsetCameraRoutine(Vector3 direction, float distance, float duration)
    {
        float timer = 0;

        Vector3 start_pos = Vector3.zero;
        Vector3 target_pos = transform.InverseTransformDirection(direction) * distance;

        while (timer < duration)
        {
            float t = timer / duration;
            t = Mathf.SmoothStep(0, 1, t);

            pos_offset_current = Vector3.Lerp(start_pos, target_pos, t);

            timer += Time.deltaTime;
            yield return null;
        }

        pos_offset_current = target_pos;

        routine_offset = null;
    }
    private IEnumerator OffsetCameraAndReturnRoutine(Vector3 direction, float distance, Quaternion rotation, float duration, bool fast = false)
    {
        float timer = 0;

        float start_duration = duration;
        if (!fast)
            duration /= 2;
        else
            duration *= 0.25f;

        Vector3 start_pos = Vector3.zero;
        Vector3 target_pos = (transform.InverseTransformDirection(direction) * distance);

        while (timer < duration)
        {
            float t = timer / duration;
            t = Mathf.SmoothStep(0, 1, t);

            pos_offset_current = Vector3.Lerp(start_pos, target_pos, t);
            rot_offset_current = Quaternion.Lerp(Quaternion.identity, rotation, t);

            timer += Time.deltaTime;
            yield return null;
        }

        if (fast)
            duration = start_duration * 0.75f;

        timer = 0;
        while (timer < duration)
        {
            float t = timer / duration;
            t = Mathf.SmoothStep(0, 1, t);

            pos_offset_current = Vector3.Lerp(target_pos, start_pos, t);
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

        sensitivity_value_text.text = new_sens.ToString();
    }
}