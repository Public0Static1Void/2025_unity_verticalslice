using System.Collections;
using UnityEngine;

public class SC_ExpandReveal : MonoBehaviour
{
    public float max_progress;
    public float duration = 5;
    public float start_progress = 0;
    private float _progress;

    private MeshRenderer _meshRenderer;
    private Material _mat;

    public bool start_active = false;

    private void Start()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        if (_meshRenderer != null)
        {
            _mat = _meshRenderer.sharedMaterial;
            _mat.SetFloat("_Progress", start_progress);

            if (start_active)
            {
                Grow();
            }
        }       
    }

    public void Grow()
    {
        if (_mat == null) return;
        StartCoroutine(GrowRoutine(1));
    }
    public void Shrink()
    {
        if (_mat == null) return;
        StartCoroutine(GrowRoutine(0));
    }

    private IEnumerator GrowRoutine(float target)
    {
        float timer = 0;

        while (timer < duration)
        {
            float t = timer / duration;
            t = Mathf.SmoothStep(0, 1, t);

            _progress = max_progress * (target > 0 ? t : 1 - t);
            _mat.SetFloat("_Progress", _progress);

            timer += Time.deltaTime;
            yield return null;
        }
    }
}
