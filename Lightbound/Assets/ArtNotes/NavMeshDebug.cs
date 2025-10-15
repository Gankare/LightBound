using UnityEngine;
using UnityEngine.AI;

public class NavMeshDebug : MonoBehaviour
{
    NavMeshAgent navMeshAgent;

    private void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }
    void Update()
    {
        if (navMeshAgent.isPathStale)
        {
            Debug.LogWarning("Path has become stale, recalculating path");
            navMeshAgent.Warp(transform.position);
        }

        if (navMeshAgent.pathPending)
        {
            Debug.LogWarning("Path is pending, waiting for path calculation to finish");
        }

        if (navMeshAgent.hasPath && navMeshAgent.path.status == NavMeshPathStatus.PathInvalid)
        {
            Debug.LogError("Path is invalid, make sure the destination is on the NavMesh");
        }
    }
}
