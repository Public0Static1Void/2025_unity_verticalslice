using UnityEngine;

public class SC_Mark_Stone : SC_Mark
{
    public float range = 3;

    public override void Activate()
    {
        Destroy(Instantiate(particles, transform.position, transform.rotation), 5);

        DesactivateMark();
    }
}