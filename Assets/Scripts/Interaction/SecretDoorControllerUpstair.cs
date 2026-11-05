using System.Collections;
using UnityEngine;
using static Unity.Collections.AllocatorManager;

public class SecretDoorControllerUpstair : MonoBehaviour
{
    [SerializeField] private float openHeight = 3f;
    [SerializeField] private float openDuration = 2f;

    [SerializeField] private BotonPuzzle button1;
    [SerializeField] private BotonPuzzle button2;
    [SerializeField] private BotonPuzzle button3;

    public float OpenDuration => openDuration;

    private Vector3 closedPosition;
    private Vector3 openPosition;

    private void Awake()
    {
        closedPosition = transform.localPosition;
        openPosition = closedPosition + Vector3.up * openHeight;
    }

    public void Open()
    {
        button1.BlockInteraction();
        button2.BlockInteraction();
        button3.BlockInteraction();
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
        button1.RestoreControl();
        button2.RestoreControl();
        button3.RestoreControl();
    }
}