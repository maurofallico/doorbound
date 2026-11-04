using UnityEngine;

public static class DoorSwing
{
    public static Vector3 LeafOffset(Transform hinge)
    {
        Renderer[] renderers = hinge.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return hinge.right;

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        Vector3 offset = bounds.center - hinge.position;
        offset.y = 0f;
        return offset;
    }

    public static float AngleAwayFrom(Vector3 hingePosition, Vector3 leafOffset, Vector3 playerPosition, float angle)
    {
        Vector3 toPlayer = playerPosition - hingePosition;
        toPlayer.y = 0f;

        Vector3 swungPositive = Quaternion.Euler(0f, angle, 0f) * leafOffset;
        return Vector3.Dot(swungPositive, toPlayer) <= 0f ? angle : -angle;
    }
}
