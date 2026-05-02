using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SC_MovingPlatform : SC_Interactable
{
    public bool start_active = false, return_to_position = false, can_move = true;
    public float duration = 1;
    public bool move = false, rotate = true;
    private Vector3 pos_start;
    public Vector3 pos_end;

    private Quaternion rot_start;
    public Vector3 rot_end;

    private bool active = false;

    private MeshRenderer mesh = null;
    private MaterialPropertyBlock _mpb;
    private Color _col_start, _col_start_2;

    [Header("Color")]
    public Color col_freeze_1;
    public Color col_freeze_2;

    public float delay = 0;

    public UnityEngine.UI.Image im_timer;

    private AudioSource _audioSource;
    private SC_SoundManager soundManager;

    protected float speed = 1;

    protected virtual void Start()
    {
        soundManager = SC_SoundManager.instance;
        _audioSource = GetComponent<AudioSource>();

        pos_start = transform.position;
        rot_start = transform.rotation;

        mesh = GetComponent<MeshRenderer>();

        if (mesh != null)
        {
            _col_start = GetMaterialColor(mesh);
            _col_start_2 = GetMaterialColor2(mesh);
        }
            

        if (start_active)
            Activate();

        _mpb = new MaterialPropertyBlock();
    }

    public override void Activate()
    {
        StartCoroutine(DelayCoroutine(true));
    }
    public override void Deactivate()
    {
        StartCoroutine(DelayCoroutine(false));
    }

    private IEnumerator TimerRoutine(float d)
    {
        if (im_timer != null)
        {
            im_timer.gameObject.SetActive(true);
            float t = 0;
            while (t < d)
            {
                t += Time.deltaTime;
                im_timer.fillAmount = t / d;
                yield return null;
            }
            im_timer.fillAmount = 1;
            im_timer.gameObject.SetActive(false);
        }
    }

    private IEnumerator DelayCoroutine(bool act)
    {
        StartCoroutine(TimerRoutine(act ? delay : delay * 2));
        yield return new WaitForSeconds(act ? delay : delay * 2);

        if (_audioSource != null)
            _audioSource.Play();

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

        Vector3 endPos = pos_start + transform.TransformDirection(pos_end);

        Quaternion endRot = Quaternion.Euler(rot_end);

        float time = 0f;

        while (time < duration)
        {
            if (can_move)
            {
                time += Time.fixedDeltaTime * speed;

                float t = time / duration;
                t = Mathf.SmoothStep(0f, 1f, t);

                if (move)
                    transform.position = Vector3.Lerp(pos_start, endPos, t);
                if (rotate)
                    transform.rotation = Quaternion.Lerp(rot_start, endRot, t);
            }

            yield return new WaitForFixedUpdate();
        }

        if (move)
            transform.position = endPos;
        if (rotate)
            transform.rotation = endRot;

        active = false;

        if (return_to_position)
            Deactivate();
    }
    private IEnumerator ReturnRoutine()
    {
        active = true;

        Vector3 startPos = transform.position;
        Vector3 endPos = pos_start;

        Quaternion startRot = transform.rotation;
        Quaternion endRot = rot_start;

        float time = 0f;

        while (time < duration)
        {
            if (can_move)
            {
                time += Time.fixedDeltaTime * speed;

                float t = time / duration;
                t = Mathf.SmoothStep(0f, 1f, t);

                if (move)
                    transform.position = Vector3.Lerp(startPos, pos_start, t);
                if (rotate)
                    transform.rotation = Quaternion.Lerp(startRot, rot_start, t);
            }

            yield return new WaitForFixedUpdate();
        }

        if (move)
            transform.position = pos_start;
        if (rotate)
            transform.rotation = rot_start;

        active = false;

        if (return_to_position)
            Activate();
    }

    public void SetCanMove(bool v)
    {
        can_move = v;
    }
    public void Slow(float slow, float duration)
    {
        StartCoroutine(SlowRoutine(slow, duration));
    }
    private IEnumerator SlowRoutine(float slow, float duration)
    {
        speed = slow;

        float timer = 0;
        while (timer < duration)
        {
            float t = timer / duration;
            speed = Mathf.Lerp(slow, 1, t);

            timer += Time.deltaTime;
            yield return null;
        }
        speed = 1;
    }


    #region MatColor
    // Change material color
    public void ChangeMatColor(Color new_col1, Color new_col2, string action = "")
    {
        if (mesh == null) return;
        if (action == "Freeze")
        {
            new_col1 = col_freeze_1;
            new_col2 = col_freeze_2;
        }
        SetOutlineColor(new_col1, new_col2);
    }
    public void RestoreMtColor()
    {
        if (mesh == null) return;
        SetOutlineColor(_col_start, _col_start_2);
    }

    private Color GetMaterialColor(Renderer renderer)
    {
        // Always use material instance, never sharedMaterial
        Material mat = renderer.material;

        if (mat == null) return Color.black;

        if (mat.HasProperty("_BaseColor"))
            return mat.GetColor("_BaseColor");

        return Color.black;
    }
    private Color GetMaterialColor2(Renderer renderer)
    {
        // Always use material instance, never sharedMaterial
        Material mat = renderer.material;

        if (mat == null) return Color.black;

        if (mat.HasProperty("_1st_ShadeColor"))
            return mat.GetColor("_1st_ShadeColor");

        return Color.black;
    }
    public void SetOutlineColor(Color color1, Color color2)
    {
        if (mesh == null) return;
        mesh.GetPropertyBlock(_mpb);
        _mpb.SetColor("_BaseColor", color1);
        _mpb.SetColor("_1st_ShadeColor", color2);
        mesh.SetPropertyBlock(_mpb);
    }
    #endregion

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

        bool isDefaultCube = b.size == Vector3.one && b.center == Vector3.zero;

        Vector3 drawSize = isDefaultCube ? transform.localScale : Vector3.Scale(b.size, transform.lossyScale);

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
                Gizmos.DrawWireCube(Vector3.zero, drawSize);

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

            Gizmos.DrawWireCube(Vector3.zero, drawSize);

            // Restore matrix
            Gizmos.matrix = oldMatrix;
        }
    }
}