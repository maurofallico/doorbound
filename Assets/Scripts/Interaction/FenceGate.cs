using System.Collections;
using UnityEngine;

public class FenceGate : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform leftHinge;
    [SerializeField] private Transform rightHinge;
    [SerializeField] private Transform insideMarker;
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float openSpeed = 1f;
    [SerializeField] private float slamSpeed = 4f;
    [SerializeField] private string requiredKeyId = "llave_salida";

    private Quaternion leftClosed;
    private Quaternion rightClosed;
    private Vector3 leftLeafOffset;
    private Vector3 rightLeafOffset;
    private Coroutine moveRoutine;
    private bool isOpen;

    public string Prompt => "";

    private void Awake()
    {
        leftClosed = leftHinge.localRotation;
        rightClosed = rightHinge.localRotation;
        leftLeafOffset = DoorSwing.LeafOffset(leftHinge);
        rightLeafOffset = DoorSwing.LeafOffset(rightHinge);
    }

    public void Interact(PlayerInventory playerInventory)
    {
        if (isOpen)
        {
            Close(openSpeed);
            return;
        }

        Vector3 playerPosition = playerInventory.transform.position;

        if (IsInside(playerPosition) && !playerInventory.HasKey(requiredKeyId)) return;

        isOpen = true;
        float leftAngle = DoorSwing.AngleAwayFrom(leftHinge.position, leftLeafOffset, playerPosition, openAngle);
        float rightAngle = DoorSwing.AngleAwayFrom(rightHinge.position, rightLeafOffset, playerPosition, openAngle);
        MoveTo(leftClosed * Quaternion.Euler(0f, leftAngle, 0f), rightClosed * Quaternion.Euler(0f, rightAngle, 0f), openSpeed);
    }

    public void SlamShut()
    {
        if (!isOpen) return;

        Close(slamSpeed);
    }

    private void Close(float speed)
    {
        isOpen = false;
        MoveTo(leftClosed, rightClosed, speed);
    }

    private bool IsInside(Vector3 position)
    {
        if (insideMarker == null) return false;

        Vector3 center = (leftHinge.position + rightHinge.position) * 0.5f;
        Vector3 inward = insideMarker.position - center;
        inward.y = 0f;
        Vector3 toPlayer = position - center;
        toPlayer.y = 0f;
        return Vector3.Dot(inward, toPlayer) > 0f;
    }

    private void MoveTo(Quaternion leftTarget, Quaternion rightTarget, float speed)
    {
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
