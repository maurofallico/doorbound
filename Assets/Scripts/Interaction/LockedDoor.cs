using UnityEngine;

public class LockedDoor : MonoBehaviour, IInteractable
{
    [SerializeField] private bool requiresKey;
    [SerializeField] private string requiredKeyId = "llave_maestra";

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
                string message = playerInventory.HasAnyKey
                    ? "Esta llave no es la indicada"
                    : "Necesitas una llave";

                GameHud.Instance?.ShowMessage(message);
                return;
            }

            isUnlocked = true;
        }

        isOpen = !isOpen;

        if (animator != null)
        {
            animator.SetTrigger(isOpen ? openTrigger : closeTrigger);
        }
    }
}