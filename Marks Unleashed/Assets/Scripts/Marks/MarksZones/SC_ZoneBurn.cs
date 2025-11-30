using System.Collections;
using UnityEngine;

public class SC_ZoneBurn : MonoBehaviour
{
    public float range = 3;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void StartBurn(int damage_per_second, float apply_range)
    {
        range = apply_range;
        StartCoroutine(DamagePerSecond(damage_per_second));
    }

    IEnumerator DamagePerSecond(int damage_per_second)
    {
        while (gameObject.activeSelf)
        {
            Collider[] colls = Physics.OverlapSphere(transform.position, range);
            foreach (Collider coll in colls)
            {
                SC_LifeSystem lf = coll.GetComponent<SC_LifeSystem>();
                if (lf != null)
                {
                    lf.Damage(damage_per_second);
                    lf.ApplyElement(SC_ElementManager.instance.GetElement(ElementTypes.FIRE));
                }
            }
            yield return new WaitForSeconds(1);
        }
    }
}