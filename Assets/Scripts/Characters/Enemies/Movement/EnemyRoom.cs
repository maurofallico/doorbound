using UnityEngine;


public class EnemyRoom : MonoBehaviour
{
    private enum Shape { Ellipse, Box }

    [SerializeField] private Shape shape = Shape.Ellipse;
    [SerializeField] private float wallMargin = 0.75f;

    public bool Contains(Vector3 position)
    {
        Vector3 local = transform.InverseTransformPoint(position);
        if (Mathf.Abs(local.y) > 0.5f) return false;

        if (shape == Shape.Box)
        {
            return Mathf.Abs(local.x) <= 0.5f && Mathf.Abs(local.z) <= 0.5f;
        }

        return new Vector2(local.x, local.z).sqrMagnitude <= 0.25f;
    }

    public Vector3 ClampTarget(Vector3 position)
    {
        Vector3 local = transform.InverseTransformPoint(position);
        Vector3 scale = transform.lossyScale;
        float halfX = Mathf.Max(0.01f, 0.5f - wallMargin / Mathf.Max(0.01f, Mathf.Abs(scale.x)));
        float halfZ = Mathf.Max(0.01f, 0.5f - wallMargin / Mathf.Max(0.01f, Mathf.Abs(scale.z)));

        if (shape == Shape.Box)
        {
            local.x = Mathf.Clamp(local.x, -halfX, halfX);
            local.z = Mathf.Clamp(local.z, -halfZ, halfZ);
        }
        else
        {
            float normalizedDistance = new Vector2(local.x / halfX, local.z / halfZ).magnitude;
            if (normalizedDistance > 1f)
            {
                local.x /= normalizedDistance;
                local.z /= normalizedDistance;
            }
        }

        Vector3 clamped = transform.TransformPoint(local);
        clamped.y = position.y;
        return clamped;
    }

    public Vector3 GetPatrolPoint(int index, int count, float sizeFraction)
    {
        float angle = index * Mathf.PI * 2f / count;
        Vector3 local = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * 0.5f * sizeFraction;
        local.y = -0.5f;
        return transform.TransformPoint(local);
    }

    public float GetAngle(Vector3 position)
    {
        Vector3 local = transform.InverseTransformPoint(position);
        return Mathf.Atan2(local.z, local.x);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.matrix = transform.localToWorldMatrix;

        if (shape == Shape.Box)
        {
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
            return;
        }

        const int segments = 48;
        for (int i = 0; i < segments; i++)
        {
            float a = i * Mathf.PI * 2f / segments;
            float b = (i + 1) * Mathf.PI * 2f / segments;
            Vector3 from = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * 0.5f;
            Vector3 to = new Vector3(Mathf.Cos(b), 0f, Mathf.Sin(b)) * 0.5f;
            Gizmos.DrawLine(from + Vector3.down * 0.5f, to + Vector3.down * 0.5f);
            Gizmos.DrawLine(from + Vector3.up * 0.5f, to + Vector3.up * 0.5f);

            if (i % (segments / 4) == 0)
            {
                Gizmos.DrawLine(from + Vector3.down * 0.5f, from + Vector3.up * 0.5f);
            }
        }
    }
}
