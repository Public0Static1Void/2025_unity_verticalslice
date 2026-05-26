using UnityEngine;

public class SC_DeathZone : MonoBehaviour
{
    protected bool can_kill = true;
    private void OnTriggerEnter(Collider other)
    {
        if (!can_kill || other.CompareTag("MainCamera")) return;

        if (other.CompareTag("Player"))
        {
            SC_PlayerLife playerLife = other.GetComponent<SC_PlayerLife>();
            if (playerLife != null) 
            {
                playerLife.Damage(999);
            }
        }
        else
        {
            Destroy(other.gameObject);
        }
    }
}