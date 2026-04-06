using System.Collections;
using UnityEngine;

public class SC_Mark_Ice : SC_Mark
{
    public float range = 8;

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
        zone.InitIceZone(range, layer_interactable);
        iceZone.transform.position = transform.position;

        Destroy(iceZone, range);
        Destroy(parts, range);

        // Stop the movement of all nearby rigidbody for range / 2 seconds
        Collider[] colls = Physics.OverlapSphere(transform.position, range);
        foreach (Collider coll in colls)
        {
            if (coll.name.Contains("Rope"))
                coll.transform.parent.GetComponent<SC_Rope>().FixRope(range / 2);

            float freeze_time = range * 0.5f;

            Rigidbody rb = coll.GetComponent<Rigidbody>();
            if (rb != null)
            {
                switch (coll.name)
                {
                    case "Player":
                        // Don't affect the player if it's near
                        if (Vector3.Distance(transform.position, coll.transform.position) > range / 2) continue;

                        SC_PlayerMovement.instance.playerLife.SetVignetteValue(0.5f, Color.blue);
                        SC_PlayerMovement.instance.can_move = false;
                        SC_CameraRotation.instance.ShakeCamera(0.5f, 0.5f);

                        freeze_time = range * 0.25f;
                        break;
                }
                if (coll.name.Contains("StonePillarPivot"))
                    coll.gameObject.GetComponent<SC_Test_Mark_Stone>().can_move = false;
                
                
            }

            Debug.Log($"{coll.name}: {coll.CompareTag("MovingPlatform")}");
            if (coll.CompareTag("MovingPlatform"))
            {
                SC_MovingPlatform movingPlatform = coll.gameObject.GetComponent<SC_MovingPlatform>();
                if (movingPlatform != null)
                {
                    movingPlatform.ChangeMatColor(Color.blue, Color.blue, "Freeze");
                    movingPlatform.can_move = false;
                }
            }
            
            SC_CoroutineRunner.instance.StartCoroutine(FreezeVelocity(rb, rb != null ? rb.linearVelocity : Vector3.zero, coll.gameObject, freeze_time));
        }

        DesactivateMark();
    }

    private IEnumerator FreezeVelocity(Rigidbody rb, Vector3 start_vel, GameObject ob, float duration)
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;
        }
        yield return new WaitForSeconds(duration);
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
        if (ob != null && ob.CompareTag("MovingPlatform"))
        {
            SC_MovingPlatform movingPlatform = ob.GetComponent<SC_MovingPlatform>();
            if (movingPlatform != null)
            {
                movingPlatform.can_move = true;
                movingPlatform.RestoreMtColor();
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}