using UnityEngine;
using System.Collections.Generic;

public class SC_ObjectSpawner : MonoBehaviour
{
    public bool can_spawn = true;
    public bool loop = false;
    public float interval = 2;

    private float timer = 0;

    public float spawn_force = 5, recoil_force = 20;

    [Header("Spawned cubes")]
    public int spawned_max = 5;
    [HideInInspector]
    public int spawned = 0;

    private List<GameObject> _spawned;

    [Header("References")]
    public GameObject spawn_object;
    public Transform spawn_point;
    public ParticleSystem parts_spawn;

    [Header("AudioClips")]
    public List<AudioClip> clips_spawn;

    private Rigidbody _rb;

    private void Start()
    {
        _spawned = new List<GameObject>();
        _rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (!can_spawn) return;

        timer += Time.deltaTime;
        if (timer >= interval)
        {
            if (!loop)
            {
                can_spawn = false;
            }
            timer = 0;

            SpawnObject();
        }
    }

    public void SpawnObject()
    {
        List<int> to_remove = new List<int>();
        for (int i = 0; i < _spawned.Count; i++)
        {
            if (_spawned[i] == null)
            {
                to_remove.Add(i);
            }
        }
        for (int i = 0; i < to_remove.Count; i++)
        {
            if (i > _spawned.Count - 1) break;

            _spawned.RemoveAt(to_remove[i]);
            spawned--;
        }
        if (spawned < 0) spawned = 0;

        if (spawned >= spawned_max) return;

        GameObject instancied = Instantiate(spawn_object, spawn_point.position, transform.rotation);
        Rigidbody rb = instancied.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(transform.forward * spawn_force, ForceMode.Impulse);
        }

        SC_Cube cube = instancied.GetComponent<SC_Cube>(); // Assign this parent spawner
        if (cube != null)
            cube.parent_spawner = this;

        _spawned.Add(instancied);

        // Add a recoil force
        _rb.AddForce(-transform.forward * recoil_force, ForceMode.Impulse);
        // Spawn the particles
        parts_spawn.Play();
        // Play a sound
        SC_SoundManager.instance.InstantiateSound(clips_spawn[Random.Range(0, clips_spawn.Count)], transform.position);

        spawned++;
    }
}