using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerNoise : MonoBehaviour
{
    [SerializeField] private float walkLoudness = 0.5f;
    [SerializeField] private float runLoudness = 1f;
    [SerializeField] private float crouchLoudness = 0f;
    [SerializeField] private float noiseInterval = 0.4f;

    private PlayerMovement movement;
    private float noiseTimer;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        if (!movement.IsMoving)
        {
            noiseTimer = 0f;
            return;
        }

        noiseTimer -= Time.deltaTime;
        if (noiseTimer > 0f)
        {
            return;
        }

        noiseTimer = noiseInterval;
        NoiseEvents.Emit(transform.position, GetCurrentLoudness());
    }

    private float GetCurrentLoudness()
    {
        if (movement.IsCrouching) return crouchLoudness;
        if (movement.IsRunning) return runLoudness;
        return walkLoudness;
    }
}
