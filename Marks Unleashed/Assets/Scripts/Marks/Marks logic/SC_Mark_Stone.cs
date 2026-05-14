using UnityEngine;

public class SC_Mark_Stone : SC_Mark
{
    public float range = 3;
    
    public override void ActivateMark()
    {
        base.ActivateMark();
        //Instancia partículas y las destruye desués de X segundos
        int layer = ~LayerMask.GetMask("Player");
        float dist = 9;
        if (Physics.Raycast(transform.position + transform.forward, transform.up, out RaycastHit hit, 9, layer))
        {
            Debug.Log($"Hit {hit.distance}");
            dist = hit.distance;
        }
        GameObject stone_wall = Instantiate(particles, transform.position + transform.up * (9 - (9 - dist)), transform.rotation);
        Destroy(stone_wall, 5);

        DesactivateMark();
    }
}