using UnityEngine;

public enum EnemyDetectionResult
{
    NoPlayer,
    OutsideRoom,
    Hidden,
    Visible
}

public class EnemyVision
{
    private readonly Transform transform;
    private readonly EnemyRoom room;
    private readonly float viewRadius;
    private readonly float viewAngle;
    private readonly LayerMask obstacleMask;
    private readonly float playerTargetHeight;

    public EnemyVision(Transform transform, EnemyRoom room, float viewRadius, float viewAngle, LayerMask obstacleMask, float playerTargetHeight)
    {
        this.transform = transform;
        this.room = room;
        this.viewRadius = viewRadius;
        this.viewAngle = viewAngle;
        this.obstacleMask = obstacleMask;
        this.playerTargetHeight = playerTargetHeight;
    }

    public EnemyDetectionResult Detect(Transform playerTransform, bool isChasing)
    {
        if (playerTransform == null)
        {
            return EnemyDetectionResult.NoPlayer;
        }

        if (!room.Contains(playerTransform.position))
        {
            return EnemyDetectionResult.OutsideRoom;
        }

        Vector3 dirToPlayer = playerTransform.position + Vector3.up * playerTargetHeight - transform.position;
        float distanceToPlayer = dirToPlayer.magnitude;

        if (distanceToPlayer > viewRadius)
        {
            return EnemyDetectionResult.Hidden;
        }

        Vector3 horizontalDirectionToPlayer = dirToPlayer;
        horizontalDirectionToPlayer.y = 0f;
        float angleToPlayer = Vector3.Angle(transform.forward, horizontalDirectionToPlayer);
        bool isInsideViewAngle = isChasing || angleToPlayer <= viewAngle / 2f;

        if (!isInsideViewAngle)
        {
            return EnemyDetectionResult.Hidden;
        }

        bool hitSomething = Physics.Raycast(
            transform.position,
            dirToPlayer.normalized,
            out RaycastHit hit,
            distanceToPlayer,
            obstacleMask,
            QueryTriggerInteraction.Ignore
        );

        bool blocked = hitSomething && !hit.transform.IsChildOf(playerTransform);
        return blocked ? EnemyDetectionResult.Hidden : EnemyDetectionResult.Visible;
    }
}
