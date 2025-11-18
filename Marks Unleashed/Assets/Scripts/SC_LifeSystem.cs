using UnityEngine;

public class SC_LifeSystem : MonoBehaviour
{
    [Header("Stats")]
    public int max_life = 1;
    public int life = 1;
    public bool update_life_on_start = true;

    public int damage = 1;

    private void Start()
    {
        if (update_life_on_start)
            life = max_life;
    }
    public void Damage(int damage)
    {
        life -= damage;
        if (life <= 0)
        {
            Die();
        }
    }

    public virtual void Die()
    {
        Destroy(gameObject);
    }
}