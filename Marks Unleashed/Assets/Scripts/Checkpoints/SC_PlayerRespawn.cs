using UnityEngine;

public class SC_PlayerRespawn : MonoBehaviour
{
    private Vector3 startingPos;
    SC_PlayerLife playerLife;

    private void Start()
    {
        startingPos = transform.position; //Guarda posición de spawn
        playerLife = GetComponent<SC_PlayerLife>();
    }

    public void Respawn()
    {
        if (SC_CheckpointManager.Instance.hasCheckpoint)
        {
            transform.position = SC_CheckpointManager.Instance.currentCheckpointPos;
        }
        else
        {
            transform.position = startingPos;
        }

        if (playerLife != null)
            playerLife.life = playerLife.max_life;
    }
}