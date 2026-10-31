using System.Collections;
using UnityEngine;

public class FenceGate : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform leftHinge;
    [SerializeField] private Transform rightHinge;
    [SerializeField] private float leftOpenAngle = -90f;
    [SerializeField] private float rightOpenAngle = 90f;
    [SerializeField] private float openSpeed = 1f;
    [SerializeField] private float slamSpeed = 4f;
    [SerializeField] private string requiredKeyId = "llave_salida";

    private Quaternion leftClosed;
    private Quaternion rightClosed;
    private Coroutine moveRoutine;

    private bool isOpen;
    private bool isLocked;

    public string Prompt => "";

    private void Awake()
    {
        leftClosed = leftHinge.localRotation;
        rightClosed = rightHinge.localRotation;
    }

    public void Interact(PlayerInventory playerInventory)
    {
        if (isLocked)
        {
            if (!playerInventory.HasKey(requiredKeyId)) return;
            isLocked = false;
        }

        SetOpen(!isOpen, openSpeed);
    }

    public void SlamAndLock()
    {
        if (isLocked || !isOpen) return;

        isLocked = true;
        SetOpen(false, slamSpeed);
    }

    private void SetOpen(bool open, float speed)
    {
        isOpen = open;

        Quaternion leftTarget = open ? leftClosed * Quaternion.Euler(0f, leftOpenAngle, 0f) : leftClosed;
        Quaternion rightTarget = open ? rightClosed * Quaternion.Euler(0f, rightOpenAngle, 0f) : rightClosed;

        if (moveRoutine != null) StopCoroutine(moveRoutine);
        moveRoutine = StartCoroutine(Rotate(leftTarget, rightTarget, speed));
    }

    private IEnumerator Rotate(Quaternion leftTarget, Quaternion rightTarget, float speed)
    {
        Quaternion leftStart = leftHinge.localRotation;
        Quaternion rightStart = rightHinge.localRotation;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * speed;
            leftHinge.localRotation = Quaternion.Slerp(leftStart, leftTarget, t);
            rightHinge.localRotation = Quaternion.Slerp(rightStart, rightTarget, t);
            yield return null;
        }
        leftHinge.localRotation = leftTarget;
        rightHinge.localRotation = rightTarget;
    }
}
