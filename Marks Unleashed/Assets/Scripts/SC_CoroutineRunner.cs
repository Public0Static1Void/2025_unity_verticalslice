using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class dedicated to play coroutines
/// </summary>
public class SC_CoroutineRunner : MonoBehaviour
{
    public static SC_CoroutineRunner instance {  get; private set; }

    public Dictionary<int, Coroutine> active_coroutines = new Dictionary<int, Coroutine>();

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Removes an already started routine if the same is passed
    /// </summary>
    public Coroutine StartTrackedRoutine(int id, IEnumerator routine)
    {
        RemoveActiveRoutine(id);

        Coroutine c = StartCoroutine(RemoveRoutineOnEnd(id, routine));
        active_coroutines[id] = c;
        return c;
    }

    public void RemoveActiveRoutine(int id)
    {
        // Remove the active routine
        if (active_coroutines.TryGetValue(id, out Coroutine existing))
        {
            StopCoroutine(existing);
            active_coroutines.Remove(id);
        }
    }

    private IEnumerator RemoveRoutineOnEnd(int id, IEnumerator routine)
    {
        while (routine.MoveNext())
            yield return routine.Current;
        active_coroutines.Remove(id);
    }
}