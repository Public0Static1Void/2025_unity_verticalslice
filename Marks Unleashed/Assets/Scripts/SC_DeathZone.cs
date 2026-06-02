using System.Collections;
using UnityEngine;

public class SC_DeathZone : MonoBehaviour
{
    protected bool can_kill = true;

    public Material mat_destroy;
    public virtual void OnTriggerEnter(Collider other)
    {
        if (!can_kill || other.CompareTag("MainCamera")) return;

        if (other.CompareTag("Player"))
        {
            SC_PlayerLife playerLife = other.GetComponent<SC_PlayerLife>();
            if (playerLife != null) 
            {
                playerLife.Damage(999);
            }
        }
        else
        {
            StartCoroutine(Decompose(other.gameObject, 3));
        }
    }

    private IEnumerator Decompose(GameObject ob, float duration)
    {
        MeshRenderer mr = ob.GetComponent<MeshRenderer>();
        if (mr != null)
        {
            mr.material = mat_destroy;
            Material mat = mr.material;
            float timer = 0;
            while (timer < duration)
            {
                float t = timer / duration;

                mat.SetFloat("_DecomposeAmount", t * 2);

                timer += Time.deltaTime;
                yield return null;
            }
        }

        Destroy(ob);
    }
}