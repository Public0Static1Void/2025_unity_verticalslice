using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(SC_PlayerMovement))]
public class SC_PlayerSounds : MonoBehaviour
{
    private SC_PlayerMovement _playerMovement;
    private SC_SoundManager _soundManager;

    [Header("Sounds")]
    public List<AudioClip> clips_walks;

    [Header("Walk")]
    private float _walk_timer = 0;
    public float walk_sound_delay = 0.75f;
    public float sprint_sound_delay = 0.5f;

    private float _current_sound_delay = 0.5f;
    void Start()
    {
        _playerMovement = GetComponent<SC_PlayerMovement>();
        _soundManager = SC_SoundManager.instance;
    }

    void Update()
    {
        if (_playerMovement.moving && _playerMovement.grounded)
        {
            _current_sound_delay = _playerMovement.sprinting ? sprint_sound_delay : walk_sound_delay;
            _walk_timer += Time.deltaTime;
            if (_walk_timer > _current_sound_delay)
            {
                _soundManager.InstantiateSound(clips_walks[Random.Range(0, clips_walks.Count)], transform.position);
                _walk_timer = 0;
            }
        }
        else
            _walk_timer = 0;
    }
}