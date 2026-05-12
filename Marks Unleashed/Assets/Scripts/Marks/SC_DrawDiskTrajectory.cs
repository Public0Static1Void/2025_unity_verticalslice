using UnityEngine;
using UnityEngine.InputSystem;

public class SC_DrawDiskTrajectory : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public int steps = 15;
    public float timeStep = 0.1f;
    public GameObject ob_launch_preview;

    public Transform firePoint;
    public float force;

    private bool _selecting = false;

    private SC_MarksPlacer _marksPlacer;

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
        Vector3 startVel = firePoint.forward * (_marksPlacer.disk_launch_force * _marksPlacer.launch_timer);

        DrawTrajectory(startPos, startVel);

        Vector3 diff_to_end = lineRenderer.GetPosition(lineRenderer.positionCount - 1) - startPos;
        Vector3 dir = diff_to_end.normalized;
        if (Physics.Raycast(startPos + transform.forward, dir, out RaycastHit hit))
        {
            if (hit.collider.gameObject == gameObject) return;

            ob_launch_preview.transform.position = hit.point;
            ob_launch_preview.transform.rotation = Quaternion.LookRotation(hit.normal);
        }
    }

    void DrawTrajectory(Vector3 startPos, Vector3 startVel)
    {
        lineRenderer.positionCount = steps;

        for (int i = 0; i < steps; i++)
        {
            float t = i * timeStep;

            Vector3 pos = startPos
                + startVel * t
                + 0.5f * Physics.gravity * t * t;

            lineRenderer.SetPosition(i, pos);
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