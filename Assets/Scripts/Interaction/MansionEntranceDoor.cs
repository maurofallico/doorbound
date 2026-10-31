using System.Collections;
using UnityEngine;

public class MansionEntranceDoor : MonoBehaviour, IInteractable
{
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float openSpeed = 1f;

    private Quaternion closedRotation;
    private Vector3 leafOffset;
    private Coroutine moveRoutine;
    private bool isOpen;

    public string Prompt => "";

    private void Awake()
    {
        closedRotation = transform.localRotation;
        leafOffset = DoorSwing.LeafOffset(transform);
    }

    public void Interact(PlayerInventory playerInventory)
    {
        isOpen = !isOpen;

        Quaternion target = closedRotation;
        if (isOpen)
        {
            float angle = DoorSwing.AngleAwayFrom(transform.position, leafOffset, playerInventory.transform.position, Mathf.Abs(openAngle));
            target = closedRotation * Quaternion.Euler(0f, angle, 0f);
        }

        if (moveRoutine != null) StopCoroutine(moveRoutine);
        moveRoutine = StartCoroutine(Rotate(target));
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
