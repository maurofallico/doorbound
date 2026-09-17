using UnityEngine;

public class CircularPatrolStrategy : IPatrolStrategy
{
    private readonly Vector3 roomCenter;
    private readonly float patrolRadius;

    private int circularWaypoint = 0;

    public CircularPatrolStrategy(
        Vector3 roomCenter,
        float patrolRadius
    )
    {
        this.roomCenter = roomCenter;
        this.patrolRadius = patrolRadius;
    }

    public bool HasTarget => true;

    public Vector3 GetTarget(Vector3 currentPosition)
    {
        float angle =
            circularWaypoint * Mathf.PI / 4f;

        return roomCenter +
            new Vector3(
                Mathf.Cos(angle),
                0f,
                Mathf.Sin(angle)
            ) * patrolRadius;
    }

    public bool HasReachedTarget(
        Vector3 currentPosition,
        float threshold
    )
    {
        Vector3 remaining =
            GetTarget(currentPosition) - currentPosition;

        remaining.y = 0f;

        return remaining.magnitude
            <= Mathf.Max(threshold, 0.15f);
    }

    public void Advance()
    {
        circularWaypoint =
            (circularWaypoint + 1) % 8;
    }

    public void SetClosestNextWaypoint(
        Vector3 currentPosition
    )
    {
        Vector3 offset =
            currentPosition - roomCenter;

        circularWaypoint =
            (
                Mathf.RoundToInt(
                    Mathf.Atan2(offset.z, offset.x)
                    / (Mathf.PI / 4f)
                ) + 9
            ) % 8;
    }
}
