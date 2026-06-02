using System.Collections.Generic;
using UnityEngine;

public class SC_ZoneIce : SC_Zone
{
    private LayerMask layer_movable;

    public PhysicsMaterial noFriction_material;


    private SC_PlayerMovement playerMovement;
    private float player_start_friction = 0.9f;

    private AudioClip clip_snow_slide;

    private List<Vector3> start_speeds;
    private List<GameObject> ob_affected;
    private AudioSource as_player;


    public void InitIceZone(float zone_range, LayerMask layermask, GameObject parts, List<GameObject> affected)
    {
        range = zone_range;
        range_horizontal = range;
        layer_interactable = layermask;
        particles = parts;
        ob_affected = affected;

        if (Physics.Raycast(transform.position + transform.forward, -transform.forward, out RaycastHit hit, 5))
        {
            transform.position = hit.point + hit.normal * 100;
        }
    }
    protected override void Start()
    {
        base.Start();

        start_speeds = new List<Vector3>();
        
        clip_snow_slide = SC_ElementManager.instance.clip_snow_slide;

        as_player = SC_SoundManager.instance.InstantiateSoundAlone(clip_snow_slide, transform.position, true, volume: 0.25f);
        as_player.Stop();

        layer_movable = (1 << LayerMask.NameToLayer("Grababble")) | (1 << LayerMask.NameToLayer("Physic Object"));

        noFriction_material = new PhysicsMaterial("NoFriction_runtime");
        noFriction_material.dynamicFriction = 0;
        noFriction_material.staticFriction = 0;
        noFriction_material.frictionCombine = PhysicsMaterialCombine.Minimum;

        playerMovement = SC_PlayerMovement.instance;
        player_start_friction = playerMovement.ground_friction;

        

        range_horizontal = range;
    }
    private void Update()
    {
        // Search for new objects
        string[] names_on_range = new string[0];

        CheckForNewObjects();
        ApplyNoFrictionMaterial();
    }

    protected override void AddedObject(GameObject ob)
    {
        start_speeds.Add(Vector3.zero);
    }
    protected override void RemovedObject(GameObject ob, int index = -1)
    {
        ob.GetComponent<Collider>().material = null;
        if (ob.CompareTag("Player"))
        {
            SC_PlayerMovement pm = ob.GetComponent<SC_PlayerMovement>();
            pm.ground_friction = pm._ground_start_friction;
            pm.sliding = false;
            as_player.Pause();
        }
    }

    private void ApplyNoFrictionMaterial()
    {
        if (affected_rb.Count <= 0) return;

        for (int i = 0; i < affected_rb.Count; i++)
        {
            Rigidbody rb = affected_rb[i];
            if (rb == null) continue;                

            Collider coll = rb.GetComponent<Collider>();
            coll.material = noFriction_material;

            if (rb.CompareTag("Player"))
            {
                SC_PlayerMovement pm = SC_PlayerMovement.instance;
                pm.ground_friction = 1;
                pm.sliding = true;

                if (!as_player.isPlaying && pm.rb.linearVelocity.magnitude > 0.1f)
                {
                    as_player.Play();
                }
                    
                as_player.transform.position = rb.position;
            }

            if (start_speeds[i] == Vector3.zero)
            {
                rb.linearVelocity *= 1.5f;
                start_speeds[i] = rb.linearVelocity;
            }
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        playerMovement.ground_friction = player_start_friction;
        if (affected_rb.Count > 0)
        {
            for (int i = 0; i < affected_rb.Count; i++)
            {
                Rigidbody rb = affected_rb[i];
                if (rb == null) continue;
                rb.gameObject.GetComponent<Collider>().material = null;

                if (rb.CompareTag("Player"))
                {
                    SC_PlayerMovement pm = rb.GetComponent<SC_PlayerMovement>();
                    pm.ground_friction = pm._ground_start_friction;
                    pm.sliding = false;
                }
            }

            foreach (GameObject ob in ob_affected)
            {
                // Stop the current routine
                SC_CoroutineRunner.instance.RemoveActiveRoutine(ob.GetInstanceID());
                SC_Mark_Ice.EndFreeze(null, ob);
            }
            Destroy(as_player.gameObject);
        }
    }
}