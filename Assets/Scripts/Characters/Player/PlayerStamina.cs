using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerStamina : MonoBehaviour
{
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float drainPerSecond = 20f;
    [SerializeField] private float regenPerSecond = 14f;
    [SerializeField] private float regenDelay = 1f;
    [SerializeField, Range(0f, 1f)] private float recoverThreshold = 0.35f;
    [SerializeField, Range(0.1f, 1f)] private float exhaustedSpeedMultiplier = 0.8f;

    [Header("Jadeo")]
    [SerializeField] private float breathingLoudness = 1.5f;
    [SerializeField] private float breathingInterval = 0.5f;

    private PlayerMovement movement;
    private float stamina;
    private float regenTimer;
    private float breathingTimer;

    public float Normalized => stamina / maxStamina;
    public float RecoverThreshold => recoverThreshold;
    public bool IsExhausted { get; private set; }
    public bool CanRun => !IsExhausted && stamina > 0f;
    public float SpeedMultiplier => IsExhausted ? exhaustedSpeedMultiplier : 1f;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        stamina = maxStamina;
    }

    private void Update()
    {
        if (movement.IsRunning && movement.IsMoving)
        {
            Drain();
        }
        else if (movement.WantsToRun)
        {
            regenTimer = regenDelay;
        }
        else
        {
            Regenerate();
        }

        if (IsExhausted)
        {
            EmitBreathing();
        }
    }

    private void Drain()
    {
        stamina = Mathf.Max(0f, stamina - drainPerSecond * Time.deltaTime);
        regenTimer = regenDelay;

        if (stamina <= 0f)
        {
            IsExhausted = true;
        }
    }

    private void Regenerate()
    {
        if (regenTimer > 0f)
        {
            regenTimer -= Time.deltaTime;
            return;
        }

        stamina = Mathf.Min(maxStamina, stamina + regenPerSecond * Time.deltaTime);

        if (IsExhausted && Normalized >= recoverThreshold)
        {
            IsExhausted = false;
        }
    }

    private void EmitBreathing()
    {
        breathingTimer -= Time.deltaTime;
        if (breathingTimer > 0f) return;

        breathingTimer = breathingInterval;
        NoiseEvents.Emit(transform.position, breathingLoudness);
    }
}
