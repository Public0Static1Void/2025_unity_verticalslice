using System.Collections;
using UnityEngine;

public class SC_StompMovement : MonoBehaviour
{
    public GameObject ob_right;
    public GameObject ob_left;

    public AudioClip clip_stomp, clip_move;

    public float movement_duration = 0;

    // Movement locations
    private Vector3 pos_left_start, pos_right_start;
    private Vector3 pos_left_end, pos_right_end;

    private SC_SoundManager soundManager;

    Vector3 dir;

    void Start()
    {
        soundManager = SC_SoundManager.instance;

        ob_right = gameObject;

        pos_left_start = ob_left.transform.position;
        pos_right_start = ob_right.transform.position;

        Vector3 diff = pos_right_start - pos_left_start;
        float dist = diff.magnitude;
        dir = diff.normalized;

        Vector3 pos_mid = pos_right_start + dir * (dist / 2);


        float scale = GetComponent<Renderer>().bounds.size.x * 0.9f;
        pos_left_end = pos_left_start + dir * ((dist - scale) / 2);
        pos_right_end = pos_right_start - dir * ((dist - scale) / 2);

        StartCoroutine(ApproachObjects());
    }

    private IEnumerator ApproachObjects()
    {
        float timer = 0;
        float duration = movement_duration * 0.5f;
        while (timer < duration)
        {
            float t = timer / duration;

            ob_right.transform.position = Vector3.Lerp(pos_right_start, pos_right_end, t);
            ob_left.transform.position = Vector3.Lerp(pos_left_start, pos_left_end, t);

            timer += Time.deltaTime;
            yield return null;
        }

        ob_right.transform.position = pos_right_end;
        ob_left.transform.position = pos_left_end;

        Collider[] colls = Physics.OverlapSphere(pos_right_end - dir * 4, 2, 1 << LayerMask.NameToLayer("Player"));
        if (colls.Length > 0)
        {
            colls[0].GetComponent<SC_PlayerLife>().Damage(100);
        }

        soundManager.InstantiateSound(clip_stomp, ob_right.transform.position, pitch: Random.Range(0.48f, 0.52f));

        SC_CameraRotation.instance.ShakeCamera(1, 0.75f);
        yield return new WaitForSeconds(1);

        StartCoroutine(PushObjects());
    }
    private IEnumerator PushObjects()
    {
        float timer = 0;
        float duration = movement_duration * 1.5f;
        while (timer < duration)
        {
            float t = timer / duration;
            t = Mathf.SmoothStep(0, 1, t);

            ob_right.transform.position = Vector3.Lerp(pos_right_end, pos_right_start, t);
            ob_left.transform.position = Vector3.Lerp(pos_left_end, pos_left_start, t);

            timer += Time.deltaTime;
            yield return null;
        }

        StartCoroutine(ApproachObjects());
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(pos_right_end - dir * 4, 3);
    }
}