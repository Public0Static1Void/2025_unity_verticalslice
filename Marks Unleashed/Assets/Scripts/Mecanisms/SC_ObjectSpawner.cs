using UnityEngine;

public class SC_ObjectSpawner : MonoBehaviour
{
    public bool can_spawn = true;
    public GameObject spawn_object;
    public Transform spawn_point;
    public bool loop = false;
    public float interval = 2;

    private float timer = 0;

    public float spawn_force = 5;

    [Header("Spawned cubes")]
    public int spawned_max = 5;
    [HideInInspector]
    public int spawned = 0;

    private void Update()
    {
        if (!can_spawn) return;

        timer += Time.deltaTime;
        if (spawned < spawned_max && timer >= interval)
        {
            if (!loop)
            {
                can_spawn = false;
            }
            timer = 0;
            GameObject instancied = Instantiate(spawn_object, spawn_point.position, transform.rotation);
            Rigidbody rb = instancied.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(transform.up * spawn_force, ForceMode.Impulse);
            }

            instancied.GetComponent<SC_Cube>().parent_spawner = this; // Assign this parent spawner

            spawned++;
        }
    }
}