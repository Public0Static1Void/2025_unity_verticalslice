using UnityEngine;

public class SC_ObjectSpawner : MonoBehaviour
{
    public bool can_spawn = true;
    public GameObject spawn_object;
    public Transform spawn_point;
    public bool loop = false;
    public float interval = 2;

    private float timer = 0;

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
            Instantiate(spawn_object, spawn_point.position, transform.rotation);
        }
    }

}