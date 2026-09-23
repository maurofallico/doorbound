using UnityEngine;

public class LockedDoor : MonoBehaviour, IInteractable
{
    [SerializeField] private bool requiresKey;
    [SerializeField] private string requiredKeyId;
    [SerializeField] private GameObject prefabToSpawnOnOpen;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private bool spawnOnlyOnce = true;

    private bool hasSpawned;
    [SerializeField] private Animator animator;

    [SerializeField] private bool startsOpen;

    [SerializeField] private string openTrigger = "Open";
    [SerializeField] private string closeTrigger = "Close";

    [SerializeField] private string openStateName = "DoorOpen";
    [SerializeField] private string closeStateName = "DoorClose";

    private bool isOpen;
    private bool isUnlocked;

    public string Prompt =>
        isOpen ? "Presiona E para cerrar" : "Presiona E para abrir";

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        isOpen = startsOpen;
        isUnlocked = !requiresKey;
    }

    private void Start()
    {
        if (animator == null)
            return;

        animator.Play(
            isOpen ? openStateName : closeStateName,
            0,
            1f
        );
    }

    public void Interact(PlayerInventory playerInventory)
    {
        if (!isOpen && !isUnlocked)
        {
            if (!playerInventory.HasKey(requiredKeyId))
            {
                string message = "Necesitas la llave " + requiredKeyId;

                GameHud.Instance?.ShowMessage(message);
                return;
            }

            isUnlocked = true;
        }

        isOpen = !isOpen;
        if (isOpen)
        {
            SpawnOnOpen();
        }
        if (animator != null)
        {
            animator.SetTrigger(isOpen ? openTrigger : closeTrigger);
        }
    }

    private void SpawnOnOpen()
    {
        if (spawnOnlyOnce && hasSpawned) return;
        if (prefabToSpawnOnOpen == null || spawnPoint == null) return;

        Instantiate(prefabToSpawnOnOpen, spawnPoint.position, spawnPoint.rotation);
        hasSpawned = true;
    }
}