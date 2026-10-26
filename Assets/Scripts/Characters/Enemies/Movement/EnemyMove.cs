using UnityEngine;
using UnityEngine.AI;

public class EnemyMove
{
    private readonly NavMeshAgent agent;

    public EnemyMove(NavMeshAgent agent, bool rotateTowardsMovement)
    {
        this.agent = agent;
        this.agent.updateRotation = rotateTowardsMovement;
    }

    public void MoveTowards(Vector3 targetPosition, float speed)
    {
        if (!agent.isOnNavMesh)
        {
            return;
        }

        agent.speed = speed;
        agent.SetDestination(targetPosition);
    }

    public void Stop()
    {
        if (agent.isOnNavMesh && agent.hasPath)
        {
            agent.ResetPath();
        }
    }

    public void FaceTowards(Vector3 targetPosition, float turnSpeed)
    {
        Vector3 direction = targetPosition - agent.transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.0001f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        agent.transform.rotation = Quaternion.Slerp(agent.transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
    }

    public bool HasArrived(float threshold)
    {
        if (!agent.isOnNavMesh)
        {
            return true;
        }

        return !agent.pathPending && agent.remainingDistance <= Mathf.Max(agent.stoppingDistance, threshold);
    }
}
