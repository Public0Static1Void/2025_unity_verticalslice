using UnityEngine;

public class SC_Checkpoint : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SC_CheckpointManager.Instance.SetCheckpoint(transform.position);

            Debug.Log("CHECKPOINT Reached!!");
        }
    }
}
