using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Unity.VisualScripting;
using UnityEngine;

public class SC_ObjectSpawner : SC_Interactable
{
    public bool can_spawn = true;
    public bool loop = false;
    public float interval = 2;

    private float timer = 0;

    public float spawn_force = 5, recoil_force = 20, force_variation = 10;

    [Header("Spawned cubes")]
    public int spawned_max = 5;
    [HideInInspector]
    public int spawned = 0;

    private List<GameObject> _spawned;

    [Header("References")]
    public GameObject spawn_object;
    public Transform spawn_point;
    public ParticleSystem parts_spawn;
    public MeshRenderer spawn_mesh;

    [Header("AudioClips")]
    public List<AudioClip> clips_spawn;

    private Rigidbody _rb;

    private Coroutine _coroutine_shoot;

    private MaterialPropertyBlock _mpb;
    private Material _mat_spawner;
    private Color _col_start;
    private Vector3 _start_position;

    private SC_Cube _cube;

    private void Start()
    {
        _spawned = new List<GameObject>();
        _rb = GetComponent<Rigidbody>();
        _cube = GetComponent<SC_Cube>();
        _mat_spawner = spawn_mesh.material;
        _col_start = GetMaterialColor(spawn_mesh);

        _mpb = new MaterialPropertyBlock();

        _start_position = transform.localPosition;
    }

    private void Update()
    {
        if (!can_spawn || !loop) return;

        timer += Time.deltaTime;
        if (timer >= interval)
        {
            if (!loop)
            {
                can_spawn = false;
            }
            timer = 0;

            PrepareShoot();
        }
    }

    public void SpawnObject()
    {
        if (!can_spawn) return;

        GameObject instancied = Instantiate(spawn_object, spawn_point.position, transform.rotation);
        Rigidbody rb = instancied.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(transform.forward * (spawn_force + Random.Range(-force_variation, force_variation)), ForceMode.Impulse);
        }

        if (_cube != null) // Assign this parent spawner
            _cube.parent_spawner = this;

        _spawned.Add(instancied);

        // Add a recoil force
        if (_rb != null)
            _rb.AddForce(-transform.forward * recoil_force, ForceMode.Impulse);
        else if (_coroutine_shoot == null)
            _coroutine_shoot = StartCoroutine(RecoilRoutine(0.5f, transform.localPosition + transform.InverseTransformDirection(transform.forward) * 0.75f));
        // Spawn the particles
        parts_spawn.Play();
        // Play a sound
        if (clips_spawn.Count > 0)
            SC_SoundManager.instance.InstantiateSound(clips_spawn[Random.Range(0, clips_spawn.Count)], transform.position);

        spawned++;
    }

    public override void Activate()
    {
        foreach (GameObject ob in _spawned)
        {
            Destroy(ob);
        }
        _spawned.Clear();
        spawned = 0;
        PrepareShoot();
    }

    public void PrepareShoot()
    {
        if (spawned >= spawned_max)
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
                try
                {
                    _spawned.RemoveAt(to_remove[i]);
                }
                catch { }
                spawned--;
            }
            if (spawned < 0) spawned = 0;

            if (spawned >= spawned_max) return;
        }
        StartCoroutine(PrepareShootRoutine());
    }
    private IEnumerator PrepareShootRoutine()
    {
        StartCoroutine(RecoilRoutine(0.5f, transform.localPosition - transform.InverseTransformDirection(transform.forward) * 0.5f, false));
        yield return new WaitForSeconds(0.7f);
        SpawnObject();
    }
    private IEnumerator RecoilRoutine(float duration, Vector3 pos_end, bool return_position = true)
    {
        Vector3 pos_start = transform.localPosition;

        float timer = 0;
        if (return_position)
            duration *= 0.25f;
        while (timer < duration)
        {
            float t = timer / duration;
            t = Mathf.SmoothStep(0, 1, t);

            transform.localPosition = Vector3.Lerp(pos_start, pos_end, t);

            if (can_spawn)
                timer += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = pos_end;

        if (return_position)
        {
            duration *= 1.25f;
            timer = 0;
            while (timer < duration)
            {
                float t = timer / duration;
                t = Mathf.SmoothStep(0, 1, t);

                transform.localPosition = Vector3.Lerp(pos_end, _start_position, t);

                if (can_spawn)
                    timer += Time.deltaTime;
                yield return null;
            }
            transform.localPosition = _start_position;
        }
        else
        {
            yield return new WaitForSeconds(0.2f);
        }


        _coroutine_shoot = null;
    }

    public void Freeze()
    {
        spawn_mesh.GetPropertyBlock(_mpb);
        _mpb.SetColor("_BaseColor", Color.cyan);
        _mpb.SetColor("_1st_ShadeColor", Color.cyan);
        spawn_mesh.SetPropertyBlock(_mpb);
    }
    public void UnFreeze()
    {
        spawn_mesh.GetPropertyBlock(_mpb);
        _mpb.SetColor("_BaseColor", Color.white);
        _mpb.SetColor("_1st_ShadeColor", Color.gray);
        spawn_mesh.SetPropertyBlock(_mpb);
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
}