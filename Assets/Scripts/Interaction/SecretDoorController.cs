using System.Collections;
using UnityEngine;

public class SecretDoorController : MonoBehaviour
{
    [SerializeField] private float openHeight = 3f;
    [SerializeField] private float openDuration = 2f;

    public float OpenDuration => openDuration;

    private Vector3 closedPosition;
    private Vector3 openPosition;
    private bool isOpen;

    private void Awake()
    {
        closedPosition = transform.localPosition;
        openPosition = closedPosition + Vector3.up * openHeight;
    }

    public void Open()
    {
        if (isOpen)
            return;

        isOpen = true;
        StartCoroutine(OpenRoutine());
    }

    private IEnumerator OpenRoutine()
    {
        float elapsed = 0f;

        while (elapsed < openDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / openDuration);

            transform.localPosition = Vector3.Lerp(
                closedPosition,
                openPosition,
                t
            );

            yield return null;
        }

        transform.localPosition = openPosition;
    }
}