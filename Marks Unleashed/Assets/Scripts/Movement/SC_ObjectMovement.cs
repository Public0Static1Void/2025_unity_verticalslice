using UnityEngine;
using System.Collections;

public class SC_ObjectMovement : MonoBehaviour
{
    [Header("Stats")]
    public float speed;

    public bool can_move = false;
    

    public virtual void StopMovement(float seconds)
    {
        StartCoroutine(StopMovementCoroutine(seconds));
    }

    private IEnumerator StopMovementCoroutine(float seconds)
    {
        can_move = false;
        yield return new WaitForSeconds(seconds);
        can_move = true;
    }
}