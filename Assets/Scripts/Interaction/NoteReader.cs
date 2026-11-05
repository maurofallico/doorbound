using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class NoteReader : MonoBehaviour, IInteractable
{
    [Header("Nota")]
    [SerializeField] private string noteTitle = "Nota";
    [SerializeField][TextArea(3, 10)] private string noteText;

    [Header("Sonido")]
    [SerializeField] private AudioSource sonidoNota;
    [SerializeField] private AudioSource sonidoNotaCerrar;

    [Header("UI")]
    [SerializeField] private GameObject notePanel;
    [SerializeField] private Text titleText;
    [SerializeField] private Text contentText;

    [Header("Jugador")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerInteractor playerInteractor;

    private bool noteOpen = false;

    private bool interactionBlocked;
    public bool CanInteract => !interactionBlocked;

    [SerializeField] private string prompt = "[E] Leer";
    public string Prompt => prompt;

    public void Interact(PlayerInventory playerInventory)
    {
        if (!CanInteract) return;

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
        if (sonidoNota != null)
            sonidoNota.Play();
        else if (sonidoNotaCerrar == null)
            DoorboundAudio.PlayIfUnassigned(DoorboundAudio.Cue.NoteOpen, this);

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

        if (playerMovement != null)
            playerMovement.enabled = true;

        if (playerInteractor != null)
            playerInteractor.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (sonidoNotaCerrar != null)
            sonidoNotaCerrar.Play();
        else if (sonidoNota == null)
            DoorboundAudio.PlayIfUnassigned(DoorboundAudio.Cue.NoteClose, this);
    }

    public void BlockNote()
    {
        interactionBlocked = true;
        prompt = "";
    }

    public void UnlockNote()
    {
        interactionBlocked = false;
        prompt = "[E] Leer";
    }
}

