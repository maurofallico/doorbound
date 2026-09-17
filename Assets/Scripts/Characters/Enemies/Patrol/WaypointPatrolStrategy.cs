using UnityEngine;

public class WaypointPatrolStrategy : IPatrolStrategy
{
    private readonly Transform[] waypoints;
    private readonly bool pingPong;

    private int currentWaypointIndex = 0;
    private int patrolDirection = 1;

    public WaypointPatrolStrategy(
        Transform[] waypoints,
        bool pingPong
    )
    {
        this.waypoints = waypoints;
        this.pingPong = pingPong;
    }

    public bool HasTarget => FindValidWaypointIndex() >= 0;

    public Vector3 GetTarget(Vector3 currentPosition)
    {
        if (!HasTarget)
            return currentPosition;

        currentWaypointIndex = FindValidWaypointIndex();
        return waypoints[currentWaypointIndex].position;
    }

    public bool HasReachedTarget(
        Vector3 currentPosition,
        float threshold
    )
    {
        Vector3 direction =
            GetTarget(currentPosition) - currentPosition;

        direction.y = 0f;

        return direction.magnitude <= threshold;
    }

    public void Advance()
    {
        if (waypoints == null || waypoints.Length == 0)
            return;

        if (!pingPong)
        {
            currentWaypointIndex =
                (currentWaypointIndex + 1) % waypoints.Length;

            return;
        }

        if (waypoints.Length <= 1)
            return;

        currentWaypointIndex += patrolDirection;

        if (currentWaypointIndex >= waypoints.Length)
        {
            currentWaypointIndex = waypoints.Length - 2;
            patrolDirection = -1;
        }
        else if (currentWaypointIndex < 0)
        {
            currentWaypointIndex = 1;
            patrolDirection = 1;
        }
    }

    private int FindValidWaypointIndex()
    {
        if (waypoints == null || waypoints.Length == 0) return -1;

        for (int i = 0; i < waypoints.Length; i++)
        {
            int index = (currentWaypointIndex + i) % waypoints.Length;
            if (waypoints[index] != null)
            {
                return index;
            }
        }

        return -1;
    }
}
