using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(SC_PlayerMovement))]
public class SC_PlayerSounds : MonoBehaviour
{
    private enum GroundType { STONE, GRASS }
    private SC_PlayerMovement _playerMovement;
    private SC_SoundManager _soundManager;

    [Header("Detection")]
    public LayerMask ground_type_layer;
    public float ray_distance = 1;
    private GroundType _current_ground;

    [Header("Sounds")]
    public List<AudioClip> clips_walks_stone;
    public List<AudioClip> clips_walk_grass;

    [Header("Walk")]
    private float _walk_timer = 0;
    public float walk_sound_delay = 0.75f;
    public float sprint_sound_delay = 0.5f;

    private float _current_sound_delay = 0.5f;


    private float _timer = 0;


    void Start()
    {
        _playerMovement = GetComponent<SC_PlayerMovement>();
        _soundManager = SC_SoundManager.instance;
    }

    void Update()
    {
        if (_playerMovement.moving && _playerMovement.grounded)
        {
            // Check for the ground type
            _timer += Time.fixedDeltaTime;
            if (_timer > 0.1f) /// Avoids checks every frame
            {
                if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, ray_distance, ground_type_layer))
                {
                    switch (hit.transform.tag)
                    {
                        case "Grass":
                            _current_ground = GroundType.GRASS;
                            break;
                        case "Stone":
                            _current_ground = GroundType.GRASS;
                            break;
                    }
                }
                else
                    _current_ground = GroundType.STONE;

                _timer = 0;
            }

            _current_sound_delay = _playerMovement.sprinting ? sprint_sound_delay : walk_sound_delay;
            _walk_timer += Time.deltaTime;
            if (_walk_timer > _current_sound_delay)
            {
                AudioClip clip = null;
                switch (_current_ground)
                {
                    case GroundType.STONE:
                        clip = clips_walks_stone[Random.Range(0, clips_walks_stone.Count)];
                        break;
                    case GroundType.GRASS:
                        clip = clips_walk_grass[Random.Range(0, clips_walk_grass.Count)];
                        break;
                }
                _soundManager.InstantiateSound(clip, transform.position);
                _walk_timer = 0;
            }
        }
        else
            _walk_timer = 0;
    }
}