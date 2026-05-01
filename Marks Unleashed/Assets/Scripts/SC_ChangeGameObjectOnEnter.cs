using UnityEngine;

public class SC_ChangeGameObjectOnEnter : MonoBehaviour
{
    public float activation_distance = 1;

    public Transform target;
    public GameObject ob_change;
    public AudioClip clip_change;

    void Update()
    {
        if (Vector3.Distance(transform.position, target.position) < activation_distance)
        {
            if (ob_change != null)
                ob_change.SetActive(true);
            gameObject.SetActive(false);
            SC_SoundManager.instance.InstantiateSound(clip_change, transform.position);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, activation_distance);
    }
}