using UnityEngine;

public class CircularPatrolStrategy
{
    private const int PointCount = 8;

    private readonly EnemyRoom room;
    private readonly float patrolSize;

    private int circularWaypoint = 0;

    public CircularPatrolStrategy(
        EnemyRoom room,
        float patrolSize
    )
    {
        this.room = room;
        this.patrolSize = patrolSize;
    }

    public Vector3 GetTarget()
    {
        return room.GetPatrolPoint(circularWaypoint, PointCount, patrolSize);
    }

    public bool HasReachedTarget(
        Vector3 currentPosition,
        float threshold
    )
    {
        Vector3 remaining =
            GetTarget() - currentPosition;

        remaining.y = 0f;

        return remaining.magnitude
            <= Mathf.Max(threshold, 0.15f);
    }

    public void Advance()
    {
        circularWaypoint =
            (circularWaypoint + 1) % PointCount;
    }

    public void SetClosestNextWaypoint(
        Vector3 currentPosition
    )
    {
        circularWaypoint =
            (
                Mathf.RoundToInt(
                    room.GetAngle(currentPosition)
                    / (Mathf.PI * 2f / PointCount)
                ) + PointCount + 1
            ) % PointCount;
    }
}
