using UnityEngine;
using System.Collections;
using NUnit.Framework.Constraints;

[RequireComponent(typeof(Rigidbody))]
public class SC_ObjectMovement : MonoBehaviour
{
    [Header("Stats")]
    public float speed;
    public float max_speed = 5;

    public bool can_move = true;

    public Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        max_speed = speed;
    }

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

    public void SetSpeed(float new_speed)
    {
        speed = new_speed;
    }

    public void Slow(float percentage, float seconds)
    {
        if (percentage < 0) percentage = 0;
        StartCoroutine(SlowCoroutine(percentage, seconds));
    }
    private IEnumerator SlowCoroutine(float percentage, float seconds)
    {
        speed *= percentage;
        yield return new WaitForSeconds(seconds);
        speed = max_speed;
    }
}