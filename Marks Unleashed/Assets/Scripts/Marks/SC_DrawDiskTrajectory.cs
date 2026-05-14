using UnityEngine;
using UnityEngine.InputSystem;

public class SC_DrawDiskTrajectory : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public int steps = 15;
    public float timeStep = 0.1f;
    public GameObject ob_launch_preview;
    private Vector3 _pos_preview;

    public Transform firePoint;
    public float force;

    private bool _selecting = false;

    private SC_MarksPlacer _marksPlacer;

    public LayerMask layer_terrain;

    private void Start()
    {
        _marksPlacer = SC_MarksPlacer.Instance;
    }

    void Update()
    {
        if (_selecting) return;

        lineRenderer.enabled = _marksPlacer.placing_mark;
        ob_launch_preview.SetActive(_marksPlacer.placing_mark);

        if (_marksPlacer.launch_timer < 0.025f) return;

        Vector3 startPos = firePoint.position - firePoint.transform.up * 1.05f;
        Vector3 startVel = firePoint.forward * ((_marksPlacer.disk_launch_force) * _marksPlacer.launch_timer);

        DrawTrajectory(startPos, startVel);

        ob_launch_preview.transform.position = Vector3.Lerp(ob_launch_preview.transform.position, _pos_preview, Time.deltaTime * 10);
    }

    void DrawTrajectory(Vector3 startPos, Vector3 startVel)
    {
        lineRenderer.positionCount = steps;

        bool found_coll = false;
        for (int i = 0; i < steps; i++)
        {
            float t = i * timeStep;

            Vector3 pos = startPos
                + startVel * t
                + 0.5f * Physics.gravity * t * t;

            lineRenderer.SetPosition(i, pos);

            if (!found_coll)
            {
                Vector3 diff = pos - startPos;
                float dist = diff.magnitude;
                Vector3 dir = diff.normalized;

                found_coll = Physics.Raycast(startPos + Vector3.up, dir, out RaycastHit hit, dist, layer_terrain);
                if (found_coll)
                {
                    _pos_preview = hit.point;
                    Debug.Log(hit.collider.name);
                }
            }
        }
    }

    public void OpenRuneMenuInput(InputAction.CallbackContext con)
    {
        if (con.performed)
        {
            _selecting = true;
            lineRenderer.positionCount = 0;
            ob_launch_preview.transform.position = transform.position - transform.forward * 100;
        }
        if (con.canceled)
        {
            _selecting = false;
        }
    }
}