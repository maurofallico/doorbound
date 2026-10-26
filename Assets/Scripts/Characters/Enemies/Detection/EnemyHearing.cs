using UnityEngine;

public class EnemyHearing
{
    private readonly Transform transform;
    private readonly EnemyRoom room;
    private readonly float hearingRadius;

    public EnemyHearing(Transform transform, EnemyRoom room, float hearingRadius)
    {
        this.transform = transform;
        this.room = room;
        this.hearingRadius = hearingRadius;
    }

    public bool CanHear(Vector3 noisePosition, float loudness)
    {
        if (loudness <= 0f)
        {
            return false;
        }

        if (!room.Contains(noisePosition))
        {
            return false;
        }

        Vector3 offset = noisePosition - transform.position;
        offset.y = 0f;
        return offset.magnitude <= hearingRadius * loudness;
    }
}
