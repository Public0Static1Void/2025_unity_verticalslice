using UnityEngine;

public class SC_Cube : MonoBehaviour
{
    public SC_ObjectSpawner parent_spawner = null;

    private void OnDestroy()
    {
        if (parent_spawner != null)
        {
            parent_spawner.spawned--;
        }
    }
}