using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SC_PlayerRespawn : MonoBehaviour
{
    private Vector3 startingPos;
    SC_PlayerLife playerLife;
    SC_PlayerMovement _playerMovement;
    public float respawn_time = 1;

    public Image im_fadeout;

    private float _respawn_timer = 0;
    private bool _respawning = false;

    private void Start()
    {
        startingPos = transform.position; //Guarda posición de spawn
        playerLife = GetComponent<SC_PlayerLife>();
        _playerMovement = GetComponent<SC_PlayerMovement>();
    }

    private void Update()
    {
        // Make a countdown to teleport the player
        if (_respawning)
        {
            _respawn_timer += Time.deltaTime;
            if (_respawn_timer > 1)
            {
                transform.position = SC_CheckpointManager.Instance.currentCheckpointPos;
                _playerMovement.rb.useGravity = true;
                _respawning = false;
                _respawn_timer = 0;
            }
        }
    }

    public void Respawn()
    {
        if (SC_CheckpointManager.Instance.hasCheckpoint)
        {
            _respawning = true;
        }
        else
        {
            transform.position = startingPos;
        }

        _playerMovement.StopMovement(respawn_time);
        StartCoroutine(FadeInFadeOutRoutine(respawn_time));

        _playerMovement.rb.useGravity = false;
        _playerMovement.rb.linearVelocity *= 0.1f;

        if (playerLife != null)
            playerLife.life = playerLife.max_life;
    }

    private IEnumerator FadeInFadeOutRoutine(float duration)
    {
        float timer = 0;
        duration /= 2;

        Color col = im_fadeout.color;

        im_fadeout.gameObject.SetActive(true);

        while (timer < duration)
        {
            float t = timer / duration;
            t = Mathf.SmoothStep(0, 1, t);

            im_fadeout.color = new Color(col.r, col.g, col.b, t);

            timer += Time.deltaTime;
            yield return null;
        }
        timer = 0;
        while (timer < duration)
        {
            float t = timer / duration;
            t = Mathf.SmoothStep(0, 1, t);

            im_fadeout.color = new Color(col.r, col.g, col.b, 1 - t);

            timer += Time.deltaTime;
            yield return null;
        }

        im_fadeout.color = new Color(col.r, col.g, col.b, 0);
        im_fadeout.gameObject.SetActive(false);

        _playerMovement.rb.useGravity = true;
    }
}