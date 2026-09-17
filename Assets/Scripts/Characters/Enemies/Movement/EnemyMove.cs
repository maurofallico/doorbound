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

    public EnemyMove(NavMeshAgent agent, Transform transform, bool rotateTowardsMovement)
        : this(agent, rotateTowardsMovement)
    {
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
}
