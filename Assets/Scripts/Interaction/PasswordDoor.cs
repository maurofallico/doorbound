using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class PasswordDoor : MonoBehaviour, IInteractable
{
    [Header("Codigo")]
    [SerializeField] private string correctCode = "123456";

    [Header("UI")]
    [SerializeField] private GameObject passwordPanel;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TMP_Text errorText;

    [Header("Jugador")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerInteractor playerInteractor;

    private bool panelOpen = false;

    public string Prompt => "Presiona E para introducir el código";

    public void Interact(PlayerInventory playerInventory)
    {
        if (panelOpen)
            return;

        if (passwordPanel == null)
        {
            Debug.LogWarning("No hay Password Panel asignado");
            return;
        }

        panelOpen = true;

        passwordPanel.SetActive(true);

        if (inputField != null)
        {
            inputField.text = "";
            inputField.Select();
            inputField.ActivateInputField();
        }

        if (errorText != null)
            errorText.text = "";

        // Ocultar prompt
        GameHud.Instance?.SetPrompt("");

        // Detener jugador
        if (playerMovement != null)
            playerMovement.enabled = false;

        // Desactivar interacción
        if (playerInteractor != null)
            playerInteractor.enabled = false;

        // Mostrar mouse
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CheckPassword()
    {
        if (inputField == null)
            return;

        if (inputField.text == correctCode)
        {
            OpenDoor();
        }
        else
        {
            if (errorText != null)
                errorText.text = "Código incorrecto";

            inputField.text = "";
            inputField.ActivateInputField();
        }
    }

    private void OpenDoor()
    {
        passwordPanel.SetActive(false);
        panelOpen = false;

        // Volver a bloquear el mouse
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Devolver control al jugador
        if (playerMovement != null)
            playerMovement.enabled = true;

        if (playerInteractor != null)
            playerInteractor.enabled = true;

        // Desaparecer la puerta
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (panelOpen &&
            Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ClosePanel();
        }
    }

    private void ClosePanel()
    {
        passwordPanel.SetActive(false);
        panelOpen = false;

        if (playerMovement != null)
            playerMovement.enabled = true;

        if (playerInteractor != null)
            playerInteractor.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}