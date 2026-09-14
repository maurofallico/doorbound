using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInventory))]
public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float interactDistance = 2.5f;
    [SerializeField] private LayerMask interactableLayers = ~0;

    private PlayerInventory inventory;

    private void Awake()
    {
        inventory = GetComponent<PlayerInventory>();
        if (playerCamera == null) playerCamera = Camera.main;
    }

    private void Update()
    {
        if (Keyboard.current == null || !Keyboard.current.eKey.wasPressedThisFrame) return;

        TryInteract();
    }

    private void TryInteract()
    {
        Ray ray = playerCamera != null
            ? new Ray(playerCamera.transform.position, playerCamera.transform.forward)
            : new Ray(transform.position + Vector3.up, transform.forward);

        if (!Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactableLayers)) return;

        IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();
        if (interactable == null) return;

        interactable.Interact(inventory);
    }
}
