using System.Collections;
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

        // Stop the movement of all nearby rigidbody for range / 2 seconds
        Collider[] colls = Physics.OverlapSphere(transform.position, range);
        foreach (Collider coll in colls)
        {
            Debug.Log(coll.name);
            if (coll.name.Contains("Rope"))
                coll.transform.parent.GetComponent<SC_Rope>().FixRope(range / 2);

            Rigidbody rb = coll.GetComponent<Rigidbody>();
            if (rb != null)
            {
                
                switch (coll.name)
                {
                    case "Player":
                        SC_PlayerMovement.instance.playerLife.SetVignetteValue(0.5f, Color.blue);
                        SC_PlayerMovement.instance.can_move = false;
                        SC_CameraRotation.instance.ShakeCamera(0.5f, 0.5f);
                        break;
                }
                if (coll.name.Contains("StonePillarPivot"))
                    coll.gameObject.GetComponent<SC_Test_Mark_Stone>().can_move = false;
                SC_CoroutineRunner.instance.StartCoroutine(FreezeVelocity(rb, rb.linearVelocity));
            }

        }

        DesactivateMark();
    }

    private IEnumerator FreezeVelocity(Rigidbody rb, Vector3 start_vel)
    {
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;
        yield return new WaitForSeconds(range / 2);
        if (rb != null)
        {
            rb.isKinematic = false;
            switch (rb.name)
            {
                case "Player":
                    SC_PlayerMovement.instance.playerLife.SetVignetteValue(0, Color.blue);
                    SC_PlayerMovement.instance.can_move = true;
                    break;
            }
            if (rb.name.Contains("StonePillarPivot"))
            {
                rb.gameObject.GetComponent<SC_Test_Mark_Stone>().can_move = true;
                rb.isKinematic = true;
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}