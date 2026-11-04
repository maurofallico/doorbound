using UnityEngine;
using UnityEngine.UI;

public class StaminaBar : MonoBehaviour
{
    [SerializeField] private PlayerStamina stamina;
    [SerializeField] private Image fill;
    [SerializeField] private RectTransform thresholdMarker;
    [SerializeField] private CanvasGroup group;
    [SerializeField] private Color normalColor = new Color(0.9f, 0.9f, 0.85f, 1f);
    [SerializeField] private Color lowColor = new Color(0.95f, 0.65f, 0.25f, 1f);
    [SerializeField] private Color exhaustedColor = new Color(0.75f, 0.12f, 0.1f, 1f);
    [SerializeField, Range(0f, 1f)] private float lowThreshold = 0.3f;
    [SerializeField] private float fillSmoothing = 10f;
    [SerializeField] private float fadeSpeed = 3f;
    [SerializeField] private float pulseSpeed = 6f;
    [SerializeField] private float pulseScale = 0.06f;

    private float displayedFill = 1f;

    private void Awake()
    {
        if (stamina == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) stamina = player.GetComponent<PlayerStamina>();
        }

        if (group != null) group.alpha = 0f;
    }

    private void Update()
    {
        if (stamina == null || fill == null) return;

        displayedFill = Mathf.Lerp(displayedFill, stamina.Normalized, fillSmoothing * Time.deltaTime);
        fill.fillAmount = displayedFill;
        fill.color = GetColor();

        UpdateThresholdMarker();
        UpdatePulse();
        UpdateFade();
    }

    private Color GetColor()
    {
        if (stamina.IsExhausted) return exhaustedColor;
        if (stamina.Normalized <= lowThreshold) return lowColor;
        return normalColor;
    }

    private void UpdateThresholdMarker()
    {
        if (thresholdMarker == null) return;

        thresholdMarker.gameObject.SetActive(stamina.IsExhausted);
        thresholdMarker.anchorMin = new Vector2(stamina.RecoverThreshold, 0f);
        thresholdMarker.anchorMax = new Vector2(stamina.RecoverThreshold, 1f);
    }

    private void UpdatePulse()
    {
        float scale = 1f;
        if (stamina.IsExhausted)
        {
            scale += Mathf.Abs(Mathf.Sin(Time.time * pulseSpeed)) * pulseScale;
        }
        transform.localScale = new Vector3(1f, scale, 1f);
    }

    private void UpdateFade()
    {
        if (group == null) return;

        float targetAlpha = stamina.Normalized < 0.999f || stamina.IsExhausted ? 1f : 0f;
        group.alpha = Mathf.MoveTowards(group.alpha, targetAlpha, fadeSpeed * Time.deltaTime);
    }
}
