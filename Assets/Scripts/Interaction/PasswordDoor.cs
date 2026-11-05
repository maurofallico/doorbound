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
    [SerializeField] private Transform barrotes;
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
    public string Prompt => prompt;
    private bool interactionBlocked;
    public bool CanInteract => !interactionBlocked;
    [SerializeField] private string prompt = "[E] Introducir código";

    public void Interact(PlayerInventory playerInventory)
    {
        if (interactionBlocked)
        {
            prompt = "";
            return;
        }

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
        Debug.Log("CHECK PASSWORD EJECUTADO");

        if (inputField == null)
        {
            Debug.LogError("InputField no asignado");
            return;
        }

        Debug.Log("Código ingresado: " + inputField.text);

        if (inputField.text == correctCode)
        {
            Debug.Log("CÓDIGO CORRECTO");
            OpenDoor();
        }
        else
        {
            Debug.Log("CÓDIGO INCORRECTO");

            if (errorText != null)
                errorText.text = "Código incorrecto";

            inputField.text = "";
            inputField.ActivateInputField();
        }
    }

    private void OpenDoor()
    {
        interactionBlocked = true;
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

        if (sonidoBarrotes != null)
            sonidoBarrotes.Play();
        else
            DoorboundAudio.PlayIfUnassigned(DoorboundAudio.Cue.Bars, this);
    }

    IEnumerator MoverBarrotes()
    {
        Vector3 posicionInicial = barrotes.position;

        Vector3 posicionFinal =
            posicionInicial + Vector3.up * distanciaApertura;

        float tiempo = 0f;

        while (tiempo < duracionApertura)
        {
            tiempo += Time.deltaTime;

            float progreso = Mathf.Clamp01(
                tiempo / duracionApertura
            );

            barrotes.position = Vector3.Lerp(
                posicionInicial,
                posicionFinal,
                progreso
            );

            yield return null;
        }

        barrotes.position = posicionFinal;
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
