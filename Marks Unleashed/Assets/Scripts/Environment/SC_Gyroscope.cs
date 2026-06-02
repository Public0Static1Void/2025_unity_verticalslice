using System.Collections;
using UnityEditor;
using UnityEngine;

public class SC_Gyroscope : SC_Interactable
{
    public Transform rotate_parent;
    public float angle_rotation = 90;
    public Vector3 rotate_axis = Vector3.zero;
    public bool constant = false;
    public bool start_active = false;
    public float duration = 3;
    public float delay = 0;

    private Quaternion _rot_current;
    private Quaternion _rot_target;

    private Coroutine _routine_rotate;

    private Rigidbody _rb;
    private Rigidbody[] rbs;

    private SC_PlayerMovement sc_playerMovement;
    private Transform p_parent;

    private bool _active = true;
    private float _timer = 0;
    private float _freeze_duration = 4;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _rot_current = rotate_parent.rotation;
        _rot_target = _rot_current;

        _rb = rotate_parent.GetComponent<Rigidbody>();
        rbs = SC_GameManager.GetChildsComponents<Rigidbody>(rotate_parent, true);

        sc_playerMovement = SC_PlayerMovement.instance;
        p_parent = sc_playerMovement.transform.parent;

        if (start_active)
            Rotate();
    }

    private void Update()
    {
        if (!_active)
        {
            _timer += Time.deltaTime;
            if (_timer > _freeze_duration)
            {
                _active = true;
                _timer = 0;
            }
        }
    }

    public override void Activate()
    {
        Rotate();
    }

    public void Rotate()
    {
        if (_routine_rotate != null || !_active) return;

        _rot_target = Quaternion.AngleAxis(angle_rotation, rotate_axis) * _rot_target;

        SC_CameraRotation.instance.ShakeCamera(0.5f, duration);
        _routine_rotate = StartCoroutine(RotateRoutine(duration));
    }
    private IEnumerator RotateRoutine(float duration)
    {
        sc_playerMovement.rb.isKinematic = false;
        sc_playerMovement.transform.SetParent(rotate_parent, true);
        

        // Make all the rigidbodies kinematic to avoid unwanted movements
        //SetRbsState(true);

        float timer = 0;
        while (timer < duration)
        {
            float t = timer / duration;
            if (!constant)
                t = Mathf.SmoothStep(0, 1, t);

            _rb.MoveRotation(Quaternion.Lerp(_rot_current, _rot_target, t));
            if (_active)
            {
                timer += Time.deltaTime;
                if (!sc_playerMovement.rb.isKinematic)
                    sc_playerMovement.rb.isKinematic = true;
            }

            if (Physics.Raycast(sc_playerMovement.rb.position, sc_playerMovement.transform.up, out RaycastHit hit, transform.localScale.y + 1))
            {
                if (hit.distance < transform.localScale.y)
                {
                    sc_playerMovement.transform.position = new Vector3(sc_playerMovement.transform.position.x, sc_playerMovement.transform.position.y + Time.deltaTime, sc_playerMovement.transform.position.z);
                }
            }

            yield return new WaitForFixedUpdate();
        }

        _rb.MoveRotation(_rot_target);
        _rot_current = _rot_target;

        // Restore physics to all the rigidbodies
        SetRbsState(false);

        _routine_rotate = null;

        // Teleports the player inside if it has clipped through the walls
        if (!Physics.Raycast(sc_playerMovement.transform.position, -sc_playerMovement.transform.up))
        {
            Vector3 dir = (transform.position - sc_playerMovement.transform.position).normalized;
            if (Physics.Raycast(sc_playerMovement.transform.position, dir, out RaycastHit hit))
            {
                sc_playerMovement.transform.position = hit.point + dir * sc_playerMovement.transform.localScale.magnitude;
            }
        }

        sc_playerMovement.transform.SetParent(p_parent, true);
        sc_playerMovement.rb.isKinematic = false;

        if (constant)
        {
            yield return new WaitForSeconds(delay);
            Rotate();
        }
    }

    public void StopRotation(float duration)
    {
        _active = false;
        _timer = 0;
        _freeze_duration = duration;

        sc_playerMovement.rb.isKinematic = false;
    }

    private void SetRbsState(bool v)
    {
        foreach (Rigidbody rb in rbs)
        {
            if (rb != null)
                rb.isKinematic = v;
        }
    }
}
