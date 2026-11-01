using UnityEngine;

public class BotonPuzzle : MonoBehaviour, IInteractable
{
    public PuzzleBotones puzzleManager;
    public int numeroBoton;

    public string Prompt => "Presiona E para pulsar el botón";

    public void Interact(PlayerInventory playerInventory)
    {
        puzzleManager.PulsarBoton(numeroBoton);
    }
}



