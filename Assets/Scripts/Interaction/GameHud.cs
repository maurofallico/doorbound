using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameHud : MonoBehaviour
{
    public static GameHud Instance { get; private set; }

    [SerializeField] private Image keyIcon;
    [SerializeField] private Text keyNameText;
    [SerializeField] private Text promptText;
    [SerializeField] private Text messageText;
    [SerializeField] private float messageSeconds = 2f;
    [SerializeField] private float emptyKeyAlpha = 0.25f;

    private Coroutine messageRoutine;

    private void Awake()
    {
        Instance = this;
        SetInventory("");
        SetPrompt("");
        SetMessage("");
    }

    public void SetInventory(string inventory)
    {
        bool hasKey = !string.IsNullOrWhiteSpace(inventory);

        if (keyIcon != null)
        {
            Color color = keyIcon.color;
            color.a = hasKey ? 1f : emptyKeyAlpha;
            keyIcon.color = color;
        }

        if (keyNameText != null)
        {
            keyNameText.text = hasKey ? inventory : "";
            keyNameText.enabled = hasKey;
        }
    }

    public void SetPrompt(string prompt)
    {
        if (promptText == null) return;

        promptText.text = prompt;
        promptText.enabled = !string.IsNullOrWhiteSpace(prompt);
    }

    public void ShowMessage(string message)
    {
        if (messageRoutine != null) StopCoroutine(messageRoutine);

        messageRoutine = StartCoroutine(ShowMessageRoutine(message));
    }

    private IEnumerator ShowMessageRoutine(string message)
    {
        SetMessage(message);
        yield return new WaitForSeconds(messageSeconds);
        SetMessage("");
    }

    private void SetMessage(string message)
    {
        if (messageText == null) return;

        messageText.text = message;
        messageText.enabled = !string.IsNullOrWhiteSpace(message);
    }
}
