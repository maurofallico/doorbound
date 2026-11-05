using UnityEngine;

public class BotonPuzzle : MonoBehaviour, IInteractable
{
    public PuzzleBotones puzzleManager;
    public int numeroBoton;

    private bool interactionBlocked;
    public bool CanInteract => !interactionBlocked;

    [SerializeField] private string prompt = "[E] Presionar";
    [SerializeField] private Behaviour[] playerControlsToDisable;

    public string Prompt => prompt;

    public void Interact(PlayerInventory playerInventory)
    {
        if (!CanInteract) return;

        DoorboundAudio.PlayIfUnassigned(DoorboundAudio.Cue.Button, this);
        puzzleManager.PulsarBoton(numeroBoton);
    }

    public void BlockInteraction()
    {
        interactionBlocked = true;
        playerControlsToDisable[0].enabled = false;
        prompt = "";
    }
    public void BlockButton()
    {
        interactionBlocked = true;
        prompt = "";
    }

    public void RestoreButton()
    {
        interactionBlocked = false;
        prompt = "[E] Presionar";
    }

    public void RestoreControl()
    {
        playerControlsToDisable[0].enabled = true;
    }

}



