using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInventory))]
public class PlayerInteractor : MonoBehaviour
{

    public bool InteractionLocked { get; private set; }
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float interactDistance = 2.5f;
    [SerializeField] private float nearbyInteractRadius = 2.5f;
    [SerializeField] private LayerMask interactableLayers = ~0;


    [SerializeField] private float clockAssistRadius = 0.8f;
    [SerializeField] private float clockAssistDistance = 3.5f;
    [SerializeField] private float clockAssistVerticalOffset = 0.5f;


    private PlayerInventory inventory;
    private IInteractable currentInteractable;
    private readonly Collider[] nearbyColliders = new Collider[12];

    public void SetInteractionLocked(bool locked)
    {
        InteractionLocked = locked;

        if (locked)
        {
            currentInteractable = null;
            GameHud.Instance?.SetPrompt("");
        }
    }

    private void Awake()
    {
        inventory = GetComponent<PlayerInventory>();
        if (playerCamera == null) playerCamera = Camera.main;
    }

    private void Update()
    {
        if (InteractionLocked)
        {
            GameHud.Instance?.SetPrompt("");
            return;
        }

        currentInteractable = FindInteractable();
        GameHud.Instance?.SetPrompt(currentInteractable?.Prompt ?? "");

        if (Keyboard.current == null || !Keyboard.current.eKey.wasPressedThisFrame) return;

        currentInteractable?.Interact(inventory);
    }



    private IInteractable FindInteractable()
    {
        Vector3 chestPosition = transform.position + Vector3.up * 1.2f;

        Ray ray = playerCamera != null
            ? playerCamera.ScreenPointToRay(
                new Vector2(Screen.width * 0.5f, Screen.height * 0.5f))
            : new Ray(chestPosition, transform.forward);

        // Rayo real de detección desde el centro de la pantalla.
        Debug.DrawRay(
            ray.origin,
            ray.direction * interactDistance,
            Color.red
        );

        if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            interactDistance,
            interactableLayers))
        {
            // Punto exacto donde impactó el raycast.
            Debug.DrawLine(
                ray.origin,
                hit.point,
                Color.yellow
            );

            // Línea visual desde el pecho hasta el impacto.
            Debug.DrawLine(
                chestPosition,
                hit.point,
                Color.green
            );

            IInteractable aimedInteractable =
                hit.collider.GetComponentInParent<IInteractable>();

            if (aimedInteractable != null)
                return aimedInteractable;
        }

        IInteractable assistedInteractable =
    FindNearbyInteractableAbove();

        if (assistedInteractable != null)
            return assistedInteractable;

        // Detección alternativa por proximidad.
        int count = Physics.OverlapSphereNonAlloc(
            transform.position,
            nearbyInteractRadius,
            nearbyColliders,
            interactableLayers
        );

        IInteractable nearestInteractable = null;
        float nearestDistance = float.PositiveInfinity;

        for (int i = 0; i < count; i++)
        {
            IInteractable interactable =
                nearbyColliders[i].GetComponentInParent<IInteractable>();

            if (interactable == null)
                continue;

            float distance = Vector3.Distance(
                transform.position,
                nearbyColliders[i].transform.position
            );

            if (distance >= nearestDistance)
                continue;

            nearestDistance = distance;
            nearestInteractable = interactable;
        }

        return nearestInteractable;
    }

    private IInteractable FindNearbyInteractableAbove()
    {
        if (playerCamera == null)
            return null;

        Vector3 targetPoint =
            playerCamera.transform.position +
            playerCamera.transform.forward * clockAssistDistance +
            Vector3.up * clockAssistVerticalOffset;

        Collider[] colliders = Physics.OverlapSphere(
            targetPoint,
            clockAssistRadius,
            interactableLayers
        );

        foreach (Collider col in colliders)
        {
            IInteractable interactable =
                col.GetComponentInParent<IInteractable>();

            if (interactable != null)
                return interactable;
        }

        return null;
    }


}
