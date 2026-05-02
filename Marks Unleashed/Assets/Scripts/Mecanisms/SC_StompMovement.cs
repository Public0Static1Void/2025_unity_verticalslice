using System.Collections;
using UnityEngine;

public class SC_StompMovement : MonoBehaviour
{
    public GameObject ob_right;
    public GameObject ob_left;

    public AudioClip clip_stomp;

    public float movement_duration = 0;

    // Movement locations
    private Vector3 pos_left_start, pos_right_start;
    private Vector3 pos_left_end, pos_right_end;
    
    void Start()
    {
        ob_right = gameObject;

        pos_left_start = ob_left.transform.position;
        pos_right_start = ob_right.transform.position;

        Vector3 diff = pos_right_start - pos_left_start;
        float dist = diff.magnitude;
        Vector3 dir = diff.normalized;

        Vector3 pos_mid = pos_right_start + dir * (dist / 2);

        pos_left_end = pos_mid - (dir * ob_left.transform.localScale.magnitude);
        pos_right_end = pos_mid + (dir * ob_left.transform.localScale.magnitude);
    }

    private IEnumerator ApproachObjects()
    {
        float timer = 0;
        while (timer < movement_duration)
        {
            float t = timer / movement_duration;



            timer += Time.deltaTime;
            yield return null;
        }
    }
}