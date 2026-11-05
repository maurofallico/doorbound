using UnityEngine;
using UnityEngine.InputSystem;

public class EnemyRoomLever : MonoBehaviour, IInteractable
{
    [SerializeField] private EnemyRoomExitDoor exitDoor;

    [Header("Interaccion")]
    [SerializeField] private float holdDuration = 3f;
    [SerializeField] private float maxHoldDistance = 3.5f;

    [Header("Ruido")]
    [SerializeField] private float noiseLoudness = 3f;

    [Header("Modelo")]
    [SerializeField] private GameObject leverModel;
    [SerializeField] private AnimationClip switchClip;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip activatedClip;

    private Transform holder;
    private float holdTimer;
    private bool isHolding;

    public string Prompt
    {
        get
        {
            if (isHolding) return $"Accionando la palanca... {Mathf.RoundToInt(holdTimer / holdDuration * 100f)}%";
            if (exitDoor == null || !exitDoor.CanOpen) return "";
            return "Mantén presionado E para accionar la palanca";
        }
    }

    private void Awake()
    {
        SetLeverPose(0f);
    }

    private void OnEnable()
    {
        if (exitDoor != null) exitDoor.Closed += Rearm;
    }

    private void OnDisable()
    {
        if (exitDoor != null) exitDoor.Closed -= Rearm;
    }

    public void Interact(PlayerInventory playerInventory)
    {
        if (isHolding || exitDoor == null || !exitDoor.CanOpen) return;

        holder = playerInventory.transform;
        holdTimer = 0f;
        isHolding = true;
    }

    private void Update()
    {
        if (!isHolding) return;

        if (!IsStillHolding())
        {
            CancelHold();
            return;
        }

        holdTimer += Time.deltaTime;
        SetLeverPose(holdTimer / holdDuration);

        if (holdTimer >= holdDuration) Activate();
    }

    private bool IsStillHolding()
    {
        if (Keyboard.current == null || !Keyboard.current.eKey.isPressed) return false;
        return Vector3.Distance(holder.position, transform.position) <= maxHoldDistance;
    }

    private void CancelHold()
    {
        isHolding = false;
        holdTimer = 0f;
        SetLeverPose(0f);
    }

    private void Activate()
    {
        isHolding = false;
        SetLeverPose(1f);

        if (audioSource != null && activatedClip != null) audioSource.PlayOneShot(activatedClip);

        NoiseEvents.Emit(holder.position, noiseLoudness);
        exitDoor.OpenTimed();
    }

    private void Rearm()
    {
        holdTimer = 0f;
        SetLeverPose(0f);
    }

    private void SetLeverPose(float progress)
    {
        if (leverModel == null || switchClip == null) return;

        switchClip.SampleAnimation(leverModel, switchClip.length * Mathf.Clamp01(progress));
    }
}
