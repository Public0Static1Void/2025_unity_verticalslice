using UnityEngine;
using System.Collections.Generic;

public class SC_ChangeGameObjectOnEnter : MonoBehaviour
{
    public float activation_distance = 1;

    public Transform target;
    public GameObject ob_change;
    public AudioClip clip_change;

    private List<Rigidbody> rbs;

    private void Start()
    {
        rbs = new List<Rigidbody>();
        for (int i = 0; i < ob_change.transform.childCount; i++)
        {
            rbs.Add(ob_change.transform.GetChild(i).GetComponent<Rigidbody>());
        }
    }

    void Update()
    {
        if (Vector3.Distance(transform.position, target.position) < activation_distance)
        {
            if (ob_change != null)
                ob_change.SetActive(true);
            gameObject.SetActive(false);
            SC_SoundManager.instance.InstantiateSound(clip_change, transform.position, false, 1, 0.75f);

            foreach (Rigidbody rb in rbs)
            {
                if (rb == null) continue;
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, Random.Range(-1.5f, -0.5f), rb.linearVelocity.z);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, activation_distance);
    }
}