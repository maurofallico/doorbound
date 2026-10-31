using System.Collections;
using UnityEngine;

public class MansionEntranceDoor : MonoBehaviour, IInteractable
{
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float openSpeed = 1f;

    private Quaternion closedRotation;
    private Quaternion openRotation;
    private Coroutine moveRoutine;
    private bool isOpen;

    public string Prompt => "";

    private void Awake()
    {
        closedRotation = transform.localRotation;
        openRotation = closedRotation * Quaternion.Euler(0f, openAngle, 0f);
    }

    public void Interact(PlayerInventory playerInventory)
    {
        isOpen = !isOpen;

        if (moveRoutine != null) StopCoroutine(moveRoutine);
        moveRoutine = StartCoroutine(Rotate(isOpen ? openRotation : closedRotation));
    }

    private IEnumerator Rotate(Quaternion target)
    {
        Quaternion start = transform.localRotation;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * openSpeed;
            transform.localRotation = Quaternion.Slerp(start, target, t);
            yield return null;
        }
        transform.localRotation = target;
    }
}
