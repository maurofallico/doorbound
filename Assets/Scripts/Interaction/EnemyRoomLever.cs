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

    [Header("Manija")]
    [SerializeField] private Transform handle;
    [SerializeField] private Vector3 offRotation = new Vector3(-45f, 0f, 0f);
    [SerializeField] private Vector3 onRotation = new Vector3(45f, 0f, 0f);

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
        SetHandle(0f);
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
        SetHandle(holdTimer / holdDuration);

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
        SetHandle(0f);
    }

    private void Activate()
    {
        isHolding = false;
        SetHandle(1f);

        if (audioSource != null && activatedClip != null) audioSource.PlayOneShot(activatedClip);

        // Se emite desde el jugador: EnemyHearing ignora los ruidos fuera de EnemyRoom
        // y la palanca, al estar en la pared, puede quedar fuera.
        NoiseEvents.Emit(holder.position, noiseLoudness);
        exitDoor.OpenTimed();
    }

    private void Rearm()
    {
        holdTimer = 0f;
        SetHandle(0f);
    }

    private void SetHandle(float progress)
    {
        if (handle == null) return;

        handle.localRotation = Quaternion.Slerp(
            Quaternion.Euler(offRotation),
            Quaternion.Euler(onRotation),
            progress
        );
    }
}
