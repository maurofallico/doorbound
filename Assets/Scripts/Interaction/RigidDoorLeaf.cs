using UnityEngine;

// A rigid, unit-scale leaf follows the original door animation without inheriting its stretched hierarchy.
public sealed class RigidDoorLeaf : MonoBehaviour, IInteractable
{
    [SerializeField] private MonoBehaviour interactionTarget;
    [SerializeField] private Transform motionDriver;
    [SerializeField] private Quaternion driverClosedRotation = Quaternion.identity;
    [SerializeField] private float driverTravelAngle = 90f;
    [SerializeField] private float openingAngle = 90f;
    private Quaternion closedRotation;
    public float OpeningProgress { get; private set; }
    public string Prompt => (interactionTarget as IInteractable)?.Prompt ?? "";

    private void Awake() => closedRotation = transform.localRotation;

    private void LateUpdate()
    {
        if (motionDriver == null) return;
        OpeningProgress = Mathf.Clamp01(Quaternion.Angle(driverClosedRotation, motionDriver.rotation) / Mathf.Max(1f, driverTravelAngle));
        transform.localRotation = closedRotation * Quaternion.Euler(0f, openingAngle * OpeningProgress, 0f);
    }

    public void Interact(PlayerInventory playerInventory)
    {
        (interactionTarget as IInteractable)?.Interact(playerInventory);
    }
}
