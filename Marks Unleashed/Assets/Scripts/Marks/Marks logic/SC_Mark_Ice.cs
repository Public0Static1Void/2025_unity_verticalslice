using System.Collections;
using System.Collections.Generic;
using UnityEditor;
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

        

        // Store the affected gameobjects
        List<GameObject> affected_ob = new List<GameObject>();

        // Stop the movement of all nearby rigidbody for range / 2 seconds

        Collider[] colls = Physics.OverlapSphere(transform.position, range);
        foreach (Collider coll in colls)
        {
            if (coll.name.Contains("Rope"))
                coll.transform.parent.GetComponent<SC_Rope>().FixRope(range / 2);
            else if (coll.name.Contains("Arrow spawner"))
            {
                SC_ObjectSpawner spawner = coll.gameObject.GetComponent<SC_ObjectSpawner>();
                spawner.can_spawn = false;
                spawner.Freeze();
            }
            else if (coll.name.Contains("FreezableWater"))
                coll.GetComponent<SC_FreezableWater>().SetFreeze();
            else if (coll.name.Contains("Button"))
                coll.GetComponent<SC_Button_Hold>().active = false;
            else if (coll.name.Contains("Air zone"))
                coll.GetComponent<SC_ZoneAir>().Pause();

                float freeze_time = range * 0.5f;

            Rigidbody rb = coll.GetComponent<Rigidbody>();
            if (rb != null)
            {
                switch (coll.name)
                {
                    case "Player":
                        /*
                        // Don't affect the player if it's near
                        if (Vector3.Distance(transform.position, coll.transform.position) > range / 2) continue;

                        //SC_PlayerMovement.instance.playerLife.SetVignetteValue(0.5f, Color.blue);
                        SC_PlayerMovement.instance.can_move = false;
                        SC_CameraRotation.instance.ShakeCamera(0.5f, 0.5f);

                        freeze_time = range * 0.25f;
                        coll.GetComponent<SC_PlayerLife>().Damage(0, SC_LifeSystem.Damage_type.ICE);
                        */
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

            affected_ob.Add(coll.gameObject);
            
            SC_CoroutineRunner.instance.StartTrackedRoutine(
                coll.gameObject.GetInstanceID(),
                FreezeVelocity(rb, rb != null ? rb.linearVelocity : Vector3.zero, coll.gameObject, freeze_time)
            );
        }

        // Spawn the ice zone
        GameObject iceZone = new GameObject("Ice zone");
        SC_ZoneIce zone = iceZone.AddComponent<SC_ZoneIce>();
        zone.InitIceZone(range, layer_interactable, parts, affected_ob);
        iceZone.transform.position = transform.position;

        Destroy(iceZone, range);
        Destroy(parts, range);

        DesactivateMark();
    }

    private IEnumerator FreezeVelocity(Rigidbody rb, Vector3 start_vel, GameObject ob, float duration)
    {
        if (rb != null && rb.name != "Player")
        {
            rb.linearVelocity = Vector3.zero;
        }
        yield return new WaitForSeconds(duration);
        EndFreeze(rb, ob);
    }

    public static void EndFreeze(Rigidbody rb, GameObject ob)
    {
        if (rb != null)
        {
            switch (rb.name)
            {
                case "Player":
                    //SC_PlayerMovement.instance.playerLife.SetVignetteValue(0, Color.blue);
                    //SC_PlayerMovement.instance.can_move = true;
                    break;
            }
            if (rb.name.Contains("StonePillarPivot"))
            {
                rb.gameObject.GetComponent<SC_Test_Mark_Stone>().can_move = true;
                rb.isKinematic = true;
            }

        }
        if (ob != null)
        {
            if (ob.CompareTag("MovingPlatform"))
            {
                SC_MovingPlatform movingPlatform = ob.GetComponent<SC_MovingPlatform>();
                if (movingPlatform != null)
                {
                    movingPlatform.can_move = true;
                    movingPlatform.RestoreMtColor();
                    movingPlatform.Slow(0, 4);
                }
            }
            else if (ob.name.Contains("Arrow spawner"))
            {
                SC_ObjectSpawner spawner = ob.gameObject.GetComponent<SC_ObjectSpawner>();
                spawner.can_spawn = true;
                spawner.UnFreeze();
            }
            else if (ob.name.Contains("FreezableWater"))
                ob.GetComponent<SC_FreezableWater>().UnFreeze();
            else if (ob.name.Contains("Button"))
                ob.GetComponent<SC_Button_Hold>().active = true;
            else if (ob.name.Contains("Air zone"))
                ob.GetComponent<SC_ZoneAir>().Resume();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}