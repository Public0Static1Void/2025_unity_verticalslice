using System.Collections.Generic;
using UnityEngine;

public class SC_ZoneIce : SC_Zone
{
    private LayerMask layer_movable;

    public PhysicsMaterial noFriction_material;

    public List<Collider> affected_col;
    public List<string> affected_names;
    private List<GameObject> attached_audio;

    private SC_PlayerMovement playerMovement;
    private float player_start_friction = 0.9f;

    private AudioClip clip_snow_slide;

    protected override void Start()
    {
        base.Start();

        affected_col = new List<Collider>();
        affected_names = new List<string>();
        attached_audio = new List<GameObject>();

        layer_movable = (1 << LayerMask.NameToLayer("Grababble")) | (1 << LayerMask.NameToLayer("Physic Object"));

        noFriction_material = new PhysicsMaterial("NoFriction_runtime");
        noFriction_material.dynamicFriction = 0;
        noFriction_material.staticFriction = 0;
        noFriction_material.frictionCombine = PhysicsMaterialCombine.Minimum;

        playerMovement = SC_PlayerMovement.instance;
        player_start_friction = playerMovement.ground_friction;

        clip_snow_slide = SC_ElementManager.instance.clip_snow_slide;

        range = 6;
        range_horizontal = range;
    }
    private void Update()
    {
        // Search for new objects
        string[] names_on_range = new string[0];

        Collider[] colls = Physics.OverlapSphere(transform.position, range, layer_movable);
        if (colls.Length > 0)
        {
            names_on_range = new string[colls.Length];
            for (int i = 0; i < colls.Length; i++)
            {
                names_on_range[i] = colls[i].name;

                // Don't check twice the same rigidbody
                if (affected_names.Contains(colls[i].name)) continue;

                // Save the rigidbody
                Rigidbody rb = colls[i].GetComponent<Rigidbody>();
                if (rb != null)
                {
                    // Set the no friction material
                    colls[i].material = noFriction_material;
                    if (colls[i].name == "Player")
                        playerMovement.ground_friction = 1;

                    // Plays the slide audio
                    GameObject audioSource = SC_SoundManager.instance.InstantiateSound(clip_snow_slide, colls[i].transform.position, true);
                    attached_audio.Add(audioSource);
                    SC_SoundManager.instance.AttachAudioToObject(audioSource, colls[i].gameObject);

                    affected_col.Add(colls[i]);
                    affected_names.Add(colls[i].name);
                }
            }
        }
        /// Removes the names that aren't on the range
        for (int i = 0; i < affected_names.Count; i++)
        {
            bool name_on_list = false;
            for (int j = 0; j < names_on_range.Length; j++)
            {
                if (affected_names[i] == names_on_range[j])
                {
                    name_on_list = true;
                    break;
                }
            }
            if (!name_on_list)
            {
                // Remove the no friction material
                affected_col[i].material = null;
                // Reset the friction to the player
                if (affected_names[i] == "Player")
                    playerMovement.ground_friction = player_start_friction;
                // Stops the active audio
                Destroy(attached_audio[i]);
                attached_audio.RemoveAt(i);
                affected_names.RemoveAt(i);
                affected_col.RemoveAt(i);
            }
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        playerMovement.ground_friction = player_start_friction;
        if (affected_col.Count > 0)
        {
            for (int i = 0; i < affected_col.Count; i++)
            {                    
                affected_col[i].material = null;
                Destroy(attached_audio[i]);
            }
        }
    }
}