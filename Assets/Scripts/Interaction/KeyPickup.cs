using UnityEngine;

public class KeyPickup : MonoBehaviour, IInteractable
{
    [SerializeField] private string keyId = "llave_1";
    private bool interactionBlocked;
    public bool CanInteract => !interactionBlocked;

    [SerializeField] private string prompt = "[E] Recoger Llave";
    public string Prompt => prompt;

    public void Interact(PlayerInventory playerInventory)
    {
        if (!CanInteract) return;
        playerInventory.AddKey(keyId);
        DoorboundAudio.PlayKey(this);
        gameObject.SetActive(false);
    }

    public void BlockKey()
    {
        interactionBlocked = true;
        prompt = "";
    }

    public void UnlockKey()
    {
        interactionBlocked = false;
        prompt = "[E] Recoger Llave";
    }
}
