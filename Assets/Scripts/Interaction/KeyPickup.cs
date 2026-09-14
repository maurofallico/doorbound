using UnityEngine;

public class KeyPickup : MonoBehaviour, IInteractable
{
    [SerializeField] private string keyId = "llave_1";

    public string Prompt => "Presiona E para recoger llave";

    public void Interact(PlayerInventory playerInventory)
    {
        playerInventory.AddKey(keyId);
        gameObject.SetActive(false);
    }
}
