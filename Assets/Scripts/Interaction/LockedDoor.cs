using UnityEngine;

public class LockedDoor : MonoBehaviour, IInteractable
{
    [SerializeField] private string requiredKeyId = "llave_1";
    [SerializeField] private GameObject doorObject;

    private bool isOpen;

    private void Awake()
    {
        if (doorObject == null) doorObject = gameObject;
    }

    public void Interact(PlayerInventory playerInventory)
    {
        if (isOpen) return;

        if (!playerInventory.HasKey(requiredKeyId))
        {
            Debug.Log($"Falta la llave: {requiredKeyId}");
            return;
        }

        isOpen = true;
        Debug.Log($"Puerta abierta con: {requiredKeyId}");
        doorObject.SetActive(false);
    }
}
