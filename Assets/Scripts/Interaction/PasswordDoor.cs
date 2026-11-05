using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;
public class PasswordDoor : MonoBehaviour, IInteractable
{
    [Header("Desbloqueo")]
    [SerializeField] private string correctCode = "123456";
    [SerializeField] private float distanciaApertura = 6f;
    [SerializeField] private float duracionApertura = 6f;
    private bool puertaAbierta = false;

    [Header("UI")]
    [SerializeField] private GameObject passwordPanel;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TMP_Text errorText;

    [Header("Jugador")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerInteractor playerInteractor;

    [Header("Sonido")]
    [SerializeField] private AudioSource sonidoBarrotes;

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

        GameHud.Instance?.SetPrompt("");

        if (playerMovement != null)
            playerMovement.enabled = false;

        if (playerInteractor != null)
            playerInteractor.enabled = false;

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

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerMovement != null)
            playerMovement.enabled = true;

        if (playerInteractor != null)
            playerInteractor.enabled = true;

        if (puertaAbierta) return;

        puertaAbierta = true;

        StartCoroutine(MoverBarrotes());

        sonidoBarrotes.Play();
    }

    IEnumerator MoverBarrotes()
    {
        Vector3 posicionInicial = transform.position;

        Vector3 posicionFinal =
        posicionInicial + Vector3.right * distanciaApertura;

        float tiempo = 0f;

        while (tiempo < duracionApertura)
        {
            tiempo += Time.deltaTime;

            float progreso = Mathf.Clamp01(
                tiempo / duracionApertura
            );

            transform.position = Vector3.Lerp(
                posicionInicial,
                posicionFinal,
                progreso
            );

            yield return null;
        }

        transform.position = posicionFinal;
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