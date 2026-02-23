using UnityEngine;

public class SC_Mark_Stone : SC_Mark
{
    public float range = 3;
    
    public override void ActivateMark()
    {
        //Instancia partículas y las destruye desués de X segundos
        GameObject stone_wall = Instantiate(particles, transform.position, particles.transform.rotation);
        Destroy(stone_wall, 5);

        DesactivateMark();
    }
}