using UnityEngine;

public interface IPatrolStrategy
{
    bool HasTarget { get; }

    Vector3 GetTarget(Vector3 currentPosition);

    bool HasReachedTarget(
        Vector3 currentPosition,
        float threshold
    );

    void Advance();
}
