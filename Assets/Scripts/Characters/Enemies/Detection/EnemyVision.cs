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
    private readonly RoomBounds roomBounds;
    private readonly float viewRadius;
    private readonly float viewAngle;
    private readonly LayerMask obstacleMask;

    public EnemyVision(Transform transform, RoomBounds roomBounds, float viewRadius, float viewAngle, LayerMask obstacleMask)
    {
        this.transform = transform;
        this.roomBounds = roomBounds;
        this.viewRadius = viewRadius;
        this.viewAngle = viewAngle;
        this.obstacleMask = obstacleMask;
    }

    public EnemyDetectionResult Detect(Transform playerTransform, bool isChasing)
    {
        if (playerTransform == null)
        {
            return EnemyDetectionResult.NoPlayer;
        }

        if (roomBounds.IsEnabled && !roomBounds.Contains(playerTransform.position))
        {
            return EnemyDetectionResult.OutsideRoom;
        }

        Vector3 dirToPlayer = playerTransform.position - transform.position;
        float distanceToPlayer = dirToPlayer.magnitude;

        if (!roomBounds.IsEnabled && distanceToPlayer > viewRadius)
        {
            return EnemyDetectionResult.Hidden;
        }

        Vector3 horizontalDirectionToPlayer = dirToPlayer;
        horizontalDirectionToPlayer.y = 0f;
        float angleToPlayer = Vector3.Angle(transform.forward, horizontalDirectionToPlayer);
        bool isInsideViewAngle = roomBounds.IsEnabled || isChasing || angleToPlayer <= viewAngle / 2f;

        if (!isInsideViewAngle)
        {
            return EnemyDetectionResult.Hidden;
        }

        bool hitSomething = Physics.Raycast(
            transform.position,
            dirToPlayer.normalized,
            out _,
            distanceToPlayer,
            obstacleMask
        );

        return hitSomething ? EnemyDetectionResult.Hidden : EnemyDetectionResult.Visible;
    }
}
