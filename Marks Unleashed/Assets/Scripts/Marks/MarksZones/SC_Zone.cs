using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SC_Zone : MonoBehaviour
{
    [Header("Stats")]
    public float range = 3;
    public float range_horizontal = 3;
    protected SphereCollider zone_collider;
    public bool destroyable = false;

    [Header("References")]
    public GameObject particles;
    public LayerMask layer_interactable;

    protected List<string> affected_names;
    protected List<Rigidbody> affected_rb;

    public bool just_spawned = true;

    protected virtual void Start()
    {

        gameObject.layer = LayerMask.NameToLayer("Mark");

        affected_rb = new List<Rigidbody>();
        affected_names = new List<string>();

        just_spawned = true;
        StartCoroutine(inv_time());
    }
    private IEnumerator inv_time()
    {
        yield return new WaitForSecondsRealtime(2);
        just_spawned = false;
    }

    public virtual void InitZone(float new_range, float new_horizontal_range, GameObject new_particles, bool can_destroy = false)
    {
        range = new_range;
        range_horizontal = new_horizontal_range;
        particles = new_particles;
        destroyable = can_destroy;
        particles.transform.SetParent(transform, true);
    }

    protected virtual void CheckForNewObjects()
    {
        string[] names_inside = new string[0];
        Vector3 origin_pos = transform.position - transform.forward;
        Collider[] colls = Physics.OverlapSphere(origin_pos, range, layer_interactable);
        if (colls.Length > 0)
        {
            names_inside = new string[colls.Length];
            for (int i = 0; i < colls.Length; i++)
            {
                names_inside[i] = colls[i].name;

                // Check if there is a direct line of vision between the object and the player
                Vector3 closest_point = colls[i].ClosestPoint(transform.position);
                Vector3 direction = (colls[i].transform.position - origin_pos).normalized;
                float distance = Vector3.Distance(origin_pos, colls[i].transform.position);

                float distance_from_center = Vector3.Distance(origin_pos, new Vector3(closest_point.x, transform.position.y, closest_point.z));
                if (distance_from_center > range_horizontal)
                {
                    RemoveName(colls[i].name);
                    continue;
                }

                if (Physics.Raycast(origin_pos, direction, out RaycastHit hit, distance))
                {
                    if (hit.collider != colls[i])
                    {
                        RemoveName(colls[i].name);
                        continue;
                    }
                    if (hit.collider.transform == transform.parent)
                        continue;

                    Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
                    if (rb != null && !affected_names.Contains(colls[i].name))
                    {
                        affected_names.Add(colls[i].name);
                        affected_rb.Add(rb);
                        AddedObject(rb.gameObject);
                    }

                    Debug.DrawLine(origin_pos, hit.point, Color.green);
                }
                else
                {
                    RemoveName(colls[i].name);

                    Debug.DrawLine(origin_pos, origin_pos + direction * distance, Color.red);
                    continue;
                }
            }

            List<int> remove_indexs = new List<int>();
            for (int i = 0; i < affected_names.Count; i++)
            {
                bool name_inside = false;
                for (int j = 0; j < names_inside.Length; j++)
                {
                    if (affected_names[i] == names_inside[j])
                        name_inside = true;
                }
                if (!name_inside)
                {
                    remove_indexs.Add(i);
                }
            }

            for (int i = 0; i < remove_indexs.Count; i++)
            {
                RemoveName(affected_names[i]);
            }
        }
    }

    private void RemoveName(string name)
    {
        if (affected_names.Contains(name))
        {
            int indx = affected_names.IndexOf(name);
            if (indx != -1)
            {
                if (affected_rb[indx] == null) return;
                RemovedObject(affected_rb[indx].gameObject, indx);
                affected_names.RemoveAt(indx);
                affected_rb.RemoveAt(indx);
            }
        }
    }

    protected virtual void AddedObject(GameObject ob)
    {

    }

    protected virtual void RemovedObject(GameObject ob, int index = -1)
    {

    }

    public void DestroyZone()
    {
        if (just_spawned) return;
        try
        {
            Debug.Log("Destroying zone");
            if (gameObject != null)
                Destroy(gameObject);
        } catch { }
    }
    protected virtual void OnDestroy()
    {
        if (particles == null) return;
        particles.transform.SetParent(null, true);
        particles.GetComponent<ParticleSystem>().Stop();
        Transform tr = particles.transform.GetChild(0);
        if (tr != null)
        {
            tr.GetComponent<ParticleSystem>().Stop();
        }
        Destroy(particles, 10);
        Debug.Log($"Destroyed: {name}");
        Debug.Log($"Destroyed object: {name}", this);
        Debug.Log(StackTraceUtility.ExtractStackTrace());
    }

    public void HighlightZone()
    {
        //Debug.Log($"Looking at: {name}");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, range);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, range_horizontal);
    }
}