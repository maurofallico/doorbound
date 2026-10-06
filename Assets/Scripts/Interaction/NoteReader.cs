using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class NoteReader : MonoBehaviour, IInteractable
{
    [Header("Nota")]
    [SerializeField] private string noteTitle = "Nota";
    [SerializeField][TextArea(3, 10)] private string noteText;

    [Header("UI")]
    [SerializeField] private GameObject notePanel;
    [SerializeField] private Text titleText;
    [SerializeField] private Text contentText;

    [Header("Jugador")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerInteractor playerInteractor;

    private bool noteOpen = false;

    public string Prompt => "Presiona E para leer";

    public void Interact(PlayerInventory playerInventory)
    {
        if (noteOpen)
            return;

        if (notePanel == null)
        {
            Debug.LogWarning("No hay Note Panel asignado.");
            return;
        }

        if (titleText != null)
            titleText.text = noteTitle;

        if (contentText != null)
            contentText.text = noteText;

        notePanel.SetActive(true);
        noteOpen = true;

        GameHud.Instance?.SetPrompt("");

        if (playerMovement != null)
            playerMovement.enabled = false;

        if (playerInteractor != null)
            playerInteractor.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("La nota fue interactuada");

        if (noteOpen)
            return;


    }

    private void Update()
    {
        if (noteOpen && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CloseNote();
        }
    }

    private void CloseNote()
    {
        notePanel.SetActive(false);
        noteOpen = false;

        // Volver a permitir movimiento
        if (playerMovement != null)
            playerMovement.enabled = true;

        // Volver a permitir interacción
        if (playerInteractor != null)
            playerInteractor.enabled = true;

        // Bloquear nuevamente el mouse
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

}

