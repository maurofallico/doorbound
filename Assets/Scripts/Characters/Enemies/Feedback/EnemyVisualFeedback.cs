using UnityEngine;

public class EnemyVisualFeedback
{
    private static readonly int BaseColorProperty = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorProperty = Shader.PropertyToID("_Color");

    private readonly Renderer[] enemyRenderers;
    private readonly Color chasingColor;

    public EnemyVisualFeedback(Renderer[] enemyRenderers, Color chasingColor)
    {
        this.enemyRenderers = enemyRenderers;
        this.chasingColor = chasingColor;
    }

    public void Apply(bool isChasing)
    {
        if (enemyRenderers == null)
        {
            return;
        }

        foreach (Renderer renderer in enemyRenderers)
        {
            if (renderer == null || renderer.sharedMaterial == null)
            {
                continue;
            }

            int colorPropertyId = renderer.sharedMaterial.HasProperty(BaseColorProperty)
                ? BaseColorProperty
                : renderer.sharedMaterial.HasProperty(ColorProperty)
                    ? ColorProperty
                    : -1;

            if (colorPropertyId < 0)
            {
                continue;
            }

            // A cleared property block shows the material exactly as imported
            // from the downloaded model, including its original textures.
            if (!isChasing)
            {
                renderer.SetPropertyBlock(null);
                continue;
            }

            MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(colorPropertyId, chasingColor);
            renderer.SetPropertyBlock(propertyBlock);
        }
    }
}
