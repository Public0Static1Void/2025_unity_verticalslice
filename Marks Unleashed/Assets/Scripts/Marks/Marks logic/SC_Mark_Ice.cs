using UnityEngine;

public class SC_Mark_Ice : SC_Mark
{
    public float range = 6;

    public override void ActivateMark()
    {
        base.ActivateMark();

        // Init the ice floor decal
        GameObject parts = Instantiate(particles, transform.position, Quaternion.LookRotation(transform.up, transform.forward));
        parts.transform.localScale = new Vector3(range * 2, 2, range * 2);
        parts.GetComponent<SC_FadeOutDecal>().maxTime = range;

        // Spawn the ice zone
        GameObject iceZone = new GameObject("Ice zone");
        SC_ZoneIce zone = iceZone.AddComponent<SC_ZoneIce>();
        zone.range = range;
        iceZone.transform.position = transform.position;

        Destroy(iceZone, range);
        Destroy(parts, range);

        DesactivateMark();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}