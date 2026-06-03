using UnityEngine;

public class SC_DeathTrigger : MonoBehaviour
{
    public virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SC_PlayerLife playerLife = other.GetComponent<SC_PlayerLife>();
            if (playerLife != null)
            {
                playerLife.Damage(999);
            }
        }
    }
}
