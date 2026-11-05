using System;
using UnityEngine;

[Serializable]
public class CinematicShot
{
    public Transform from;
    public Transform to;
    public Transform lookAt;
    public float duration = 3f;
}
