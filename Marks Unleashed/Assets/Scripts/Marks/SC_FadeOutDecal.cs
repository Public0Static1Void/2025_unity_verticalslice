using UnityEngine;

public class SC_FadeOutDecal : MonoBehaviour
{
    [SerializeField] private MeshRenderer meshRenderer;

    public float maxTime = 6;
    public float timer = 0.25f;


    void Update()
    {
        timer += Time.deltaTime / maxTime;
        if (timer > 1) timer = 1;
        meshRenderer.material.SetFloat("_DissolveAmount", timer);
    }
}