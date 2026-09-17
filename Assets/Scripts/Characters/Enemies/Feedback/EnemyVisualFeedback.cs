using UnityEngine;

public class EnemyVisualFeedback
{
    private static readonly int BaseColorProperty = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorProperty = Shader.PropertyToID("_Color");

    private readonly Renderer enemyRenderer;
    private readonly Color chasingColor;
    private readonly MaterialPropertyBlock materialPropertyBlock;
    private readonly Color patrolColor;
    private readonly int colorPropertyId = -1;

    public EnemyVisualFeedback(Renderer enemyRenderer, Color chasingColor)
    {
        this.enemyRenderer = enemyRenderer;
        this.chasingColor = chasingColor;

        Material sharedMaterial = enemyRenderer != null ? enemyRenderer.sharedMaterial : null;
        if (sharedMaterial == null)
        {
            return;
        }

        if (sharedMaterial.HasProperty(BaseColorProperty))
        {
            colorPropertyId = BaseColorProperty;
        }
        else if (sharedMaterial.HasProperty(ColorProperty))
        {
            colorPropertyId = ColorProperty;
        }
        else
        {
            return;
        }

        patrolColor = sharedMaterial.GetColor(colorPropertyId);
        materialPropertyBlock = new MaterialPropertyBlock();
    }

    public void Apply(bool isChasing)
    {
        if (enemyRenderer == null || materialPropertyBlock == null || colorPropertyId < 0)
        {
            return;
        }

        enemyRenderer.GetPropertyBlock(materialPropertyBlock);
        materialPropertyBlock.SetColor(colorPropertyId, isChasing ? chasingColor : patrolColor);
        enemyRenderer.SetPropertyBlock(materialPropertyBlock);
    }
}
