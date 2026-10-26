using System;
using UnityEngine;

public static class NoiseEvents
{
    public static event Action<Vector3, float> NoiseEmitted;

    public static void Emit(Vector3 position, float loudness)
    {
        NoiseEmitted?.Invoke(position, loudness);
    }
}
