using UnityEngine;

public class RoomBounds
{
    private readonly bool enabled;
    private readonly Vector3 center;
    private readonly Vector2 radii;
    private readonly float wallMargin;
    private readonly float heightTolerance;

    public RoomBounds(bool enabled, Vector3 center, Vector2 radii, float wallMargin, float heightTolerance)
    {
        this.enabled = enabled;
        this.center = center;
        this.radii = radii;
        this.wallMargin = wallMargin;
        this.heightTolerance = heightTolerance;
    }

    public bool IsEnabled => enabled;

    public Vector3 ClampTarget(Vector3 targetPosition)
    {
        if (!enabled)
        {
            return targetPosition;
        }

        Vector3 offset = targetPosition - center;
        offset.y = 0f;
        Vector2 movementRadii = new Vector2(
            Mathf.Max(0.1f, radii.x - wallMargin),
            Mathf.Max(0.1f, radii.y - wallMargin));
        float normalizedDistance = new Vector2(offset.x / movementRadii.x, offset.z / movementRadii.y).magnitude;
        return center + offset / Mathf.Max(1f, normalizedDistance);
    }

    public bool Contains(Vector3 position)
    {
        Vector3 offset = position - center;
        if (Mathf.Abs(offset.y) > heightTolerance) return false;
        offset.y = 0f;
        return new Vector2(offset.x / Mathf.Max(0.1f, radii.x),
            offset.z / Mathf.Max(0.1f, radii.y)).sqrMagnitude <= 1f;
    }
}
