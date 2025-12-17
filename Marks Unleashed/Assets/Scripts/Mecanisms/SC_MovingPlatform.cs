using System.Collections;
using UnityEngine;

public class SC_MovingPlatform : SC_Interactable
{
    public bool start_active = false;
    public bool move = false, rotate = true;
    private Vector3 pos_start;
    public Vector3 pos_end;

    private Quaternion rot_start;
    public Vector3 rot_end;

    private bool active = false;

    private MeshRenderer mesh = null;

    public float delay = 0;


    private void Start()
    {
        pos_start = transform.position;
        rot_start = transform.rotation;

        if (start_active)
            Activate();
    }
    public override void Activate()
    {
        StartCoroutine(DelayCoroutine(true));
    }
    public override void Deactivate()
    {
        StartCoroutine(DelayCoroutine(false));
    }

    private IEnumerator DelayCoroutine(bool act)
    {
        yield return new WaitForSeconds(delay);
        if (act)
        {
            if (active)
            {
                StopCoroutine(MoveRoutine());
                StartCoroutine(ReturnRoutine());
            }
            StartCoroutine(MoveRoutine());
        }
        else
        {
            if (active)
            {
                StopCoroutine(ReturnRoutine());
                StartCoroutine(MoveRoutine());
            }
            StartCoroutine(ReturnRoutine());
        }
    }

    private IEnumerator MoveRoutine()
    {
        active = true;

        Vector3 startPos = transform.position;
        Vector3 endPos = pos_start + transform.TransformDirection(pos_end);

        Quaternion startRot = transform.rotation;
        Quaternion endRot = Quaternion.Euler(rot_end);

        float time = 0f;

        while (time < 1f)
        {
            time += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, time);

            if (move)
                transform.position = Vector3.Lerp(startPos, endPos, t);

            if (rotate)
                transform.rotation = Quaternion.Lerp(startRot, endRot, t);

            yield return null;
        }

        transform.position = endPos;
        transform.rotation = endRot;

        

        active = false;
    }
    private IEnumerator ReturnRoutine()
    {
        active = true;

        Vector3 startPos = transform.position;
        Vector3 endPos = pos_start;

        Quaternion startRot = transform.rotation;
        Quaternion endRot = rot_start;

        float time = 0f;

        while (time < 1f)
        {
            time += Time.deltaTime;

            if (move)
                transform.position = Vector3.Lerp(startPos, endPos, time);

            if (rotate)
                transform.rotation = Quaternion.Lerp(startRot, endRot, time);

            yield return null;
        }

        transform.position = endPos;
        transform.rotation = endRot;

        active = false;
    }

    private void OnDrawGizmos()
    {
        if (mesh == null)
        {
            mesh = GetComponent<MeshRenderer>();
            if (!mesh) return;
        }
        MeshFilter mf = GetComponent<MeshFilter>();
        if (!mf) return;

        Vector3 size = mesh.bounds.size;
        Bounds b = mf.sharedMesh.bounds;
        Matrix4x4 oldMatrix = Gizmos.matrix;

        Vector3 rot_start_pos = mesh.bounds.center;

        if (move)
        {
            if (!rotate)
            {
                Gizmos.matrix = Matrix4x4.TRS(
                mesh.bounds.center + pos_end,
                transform.rotation,
                Vector3.one
                );

                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(Vector3.zero, b.size);

                Gizmos.matrix = oldMatrix;
            }
            else
                rot_start_pos = mesh.bounds.center + pos_end;
        }
        if (rotate)
        {
            Gizmos.color = Color.magenta;

            Gizmos.matrix = Matrix4x4.TRS(
                rot_start_pos,
                Quaternion.Euler(rot_end),
                Vector3.one
            );

            Gizmos.color = Color.yellow;

            Gizmos.DrawWireCube(Vector3.zero, b.size);

            // Restore matrix
            Gizmos.matrix = oldMatrix;
        }
    }
}