using System.Collections;
using UnityEngine;

public class SC_ExpandReveal : MonoBehaviour
{
    public float max_progress;
    public float duration = 5;
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

            if (start_active)
            {
                Grow();
            }
        }       
    }

    public void Grow()
    {
        if (_mat == null) return;
        StartCoroutine(GrowRoutine());
    }

    private IEnumerator GrowRoutine()
    {
        float timer = 0;

        while (timer < duration)
        {
            float t = timer / duration;
            t = Mathf.SmoothStep(0, 1, t);

            _progress = max_progress * t;
            _mat.SetFloat("_Progress", _progress);

            timer += Time.deltaTime;
            yield return null;
        }
    }
}
