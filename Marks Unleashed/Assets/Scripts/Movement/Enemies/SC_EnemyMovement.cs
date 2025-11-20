using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class SC_EnemyMovement : SC_ObjectMovement
{
    private NavMeshAgent agent;

    private float attack_range = 2;

    void Start()
    {
        
    }

    public void SetDestination(Vector3 destination)
    {
        agent.SetDestination(destination);
    }
}