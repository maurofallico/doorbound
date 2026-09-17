using UnityEngine;
using UnityEngine.AI;

public class NavMeshPatrolStrategy : IPatrolStrategy
{
    private const int MaxAttempts = 12;

    private readonly Vector3 center;
    private readonly float radius;

    private Vector3 target;
    private bool hasTarget;

    public NavMeshPatrolStrategy(Vector3 center, float radius)
    {
        this.center = center;
        this.radius = radius;
    }

    public bool HasTarget => true;

    public Vector3 GetTarget(Vector3 currentPosition)
    {
        if (!hasTarget)
        {
            PickTarget(currentPosition);
        }

        return hasTarget ? target : currentPosition;
    }

    public bool HasReachedTarget(Vector3 currentPosition, float threshold)
    {
        Vector3 remaining = GetTarget(currentPosition) - currentPosition;
        remaining.y = 0f;
        return remaining.magnitude <= Mathf.Max(threshold, 0.15f);
    }

    public void Advance()
    {
        hasTarget = false;
    }

    private void PickTarget(Vector3 currentPosition)
    {
        float sampleDistance = Mathf.Max(1f, radius);

        for (int i = 0; i < MaxAttempts; i++)
        {
            Vector2 randomPoint = Random.insideUnitCircle * radius;
            Vector3 candidate = center + new Vector3(randomPoint.x, 0f, randomPoint.y);

            if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, sampleDistance, NavMesh.AllAreas))
            {
                continue;
            }

            NavMeshPath path = new NavMeshPath();
            if (NavMesh.CalculatePath(currentPosition, hit.position, NavMesh.AllAreas, path) &&
                path.status == NavMeshPathStatus.PathComplete)
            {
                target = hit.position;
                hasTarget = true;
                return;
            }
        }
    }
}
