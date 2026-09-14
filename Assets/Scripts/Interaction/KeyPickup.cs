using UnityEngine;

public class KeyPickup : MonoBehaviour, IInteractable
{
    [SerializeField] private string keyId = "llave_1";

    public void Interact(PlayerInventory playerInventory)
    {
        playerInventory.AddKey(keyId);
        gameObject.SetActive(false);
    }
}
