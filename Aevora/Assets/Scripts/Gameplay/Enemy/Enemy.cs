using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    private NavMeshAgent navMeshAgent;

    [SerializeField] private Transform [] waypoints;
    private int currentWaypointIndex = 0;

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        MoveToNextWaypoint();
    }

    void Update()
    {
        if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance + 0.1f)
        {
            MoveToNextWaypoint();
        }
    }

    private void MoveToNextWaypoint()
    {
        if (waypoints.Length == 0) return;
        navMeshAgent.destination = waypoints[currentWaypointIndex].position;
        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
    }
}
