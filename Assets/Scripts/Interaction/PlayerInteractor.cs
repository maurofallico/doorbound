using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInventory))]
public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float interactDistance = 2.5f;
    [SerializeField] private float nearbyInteractRadius = 2.5f;
    [SerializeField] private LayerMask interactableLayers = ~0;

    private PlayerInventory inventory;
    private IInteractable currentInteractable;
    private readonly Collider[] nearbyColliders = new Collider[12];

    private void Awake()
    {
        inventory = GetComponent<PlayerInventory>();
        if (playerCamera == null) playerCamera = Camera.main;
    }

    private void Update()
    {
        currentInteractable = FindInteractable();
        GameHud.Instance?.SetPrompt(currentInteractable?.Prompt ?? "");

        if (Keyboard.current == null || !Keyboard.current.eKey.wasPressedThisFrame) return;

        currentInteractable?.Interact(inventory);
    }

    private IInteractable FindInteractable()
    {
        Ray ray = playerCamera != null
            ? new Ray(playerCamera.transform.position, playerCamera.transform.forward)
            : new Ray(transform.position + Vector3.up, transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactableLayers))
        {
            IInteractable aimedInteractable = hit.collider.GetComponentInParent<IInteractable>();
            if (aimedInteractable != null) return aimedInteractable;
        }

        int count = Physics.OverlapSphereNonAlloc(transform.position, nearbyInteractRadius, nearbyColliders, interactableLayers);
        IInteractable nearestInteractable = null;
        float nearestDistance = float.PositiveInfinity;

        for (int i = 0; i < count; i++)
        {
            IInteractable interactable = nearbyColliders[i].GetComponentInParent<IInteractable>();
            if (interactable == null) continue;

            float distance = Vector3.Distance(transform.position, nearbyColliders[i].transform.position);
            if (distance >= nearestDistance) continue;

            nearestDistance = distance;
            nearestInteractable = interactable;
        }

        return nearestInteractable;
    }
}
