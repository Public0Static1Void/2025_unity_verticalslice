using UnityEngine;

public class SC_Cube : MonoBehaviour
{
    public SC_ObjectSpawner parent_spawner = null;

    public AudioClip clip_hit;

    public float hit_timer = 0;
    private const float hit_sound_recoil = 0.5f;

    private bool can_sound = true;

    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = clip_hit;
    }

    private void Update()
    {
        if (!can_sound)
        {
            hit_timer += Time.deltaTime;
            if (hit_timer >= hit_sound_recoil)
            {
                hit_timer = 0;
                can_sound = true;
            }
        }
    }

    private void OnDestroy()
    {
        if (parent_spawner != null)
        {
            parent_spawner.spawned--;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (can_sound)
        {
            audioSource.Play();
            can_sound = false;
        }   
    }
}