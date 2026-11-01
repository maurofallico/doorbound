
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using static ClockHover;
public class ClockInteraction : MonoBehaviour, IInteractable
{
    [Header("Interaction")]
    [SerializeField] private string prompt = "[E] Inspeccionar";
    [SerializeField] private PlayerInteractor playerInteractor;

    [Header("Camera")]
    [SerializeField] private Camera playerCamera;

    [Header("Clock")]
    [SerializeField] private Transform face;
    [SerializeField] private Transform pivot;
    [SerializeField] private Transform[] hands;

    [SerializeField] private CinemachineCamera playerVirtualCamera;
    [SerializeField] private CinemachineCamera clockVirtualCamera;
    [SerializeField] private ClockPuzzleManager puzzleManager;
    [SerializeField] private Transform clockViewPoint;
    private ClockHoverDetector hoverDetector;

    private bool interactionBlocked;

    public bool CanInteract => !interactionBlocked;

    public bool IsFocused => isFocused;
    private int interactionFrame;

    public int CurrentHour { get; private set; } = 12;
    public int CurrentMinute { get; private set; } = 15;

    private bool exitRequested;

    public string Prompt => prompt;

    private bool isFocused;

    [Header("Player Control")]
    [SerializeField] private Behaviour[] playerControlsToDisable;

    private bool[] previousControlStates;

    private void Awake()
    {
        hoverDetector = GetComponent<ClockHoverDetector>();
    }

    public void BlockInteraction()
    {
        interactionBlocked = true;
        prompt = "";
    }

    public void RotateHand(ClockHover hand)
    {
        if (hand == null || hand.RotationPivot == null)
            return;

        hand.RotationPivot.localRotation *= Quaternion.Euler(0f, 0f, 30f);

        if (hand.HandType == ClockHandType.Hour)
        {
            CurrentHour++;

            if (CurrentHour > 12)
                CurrentHour = 1;
        }
        else if (hand.HandType == ClockHandType.Minute)
        {
            CurrentMinute += 5;

            if (CurrentMinute >= 60)
            {
                CurrentMinute = 0;
            }
        }

        if (puzzleManager != null)
        {
            puzzleManager.CheckPuzzle();
        }
    }

    public void Interact(PlayerInventory inventory)
    {
        if (interactionBlocked)
            return;

        previousControlStates =
            new bool[playerControlsToDisable.Length];

        for (int i = 0; i < playerControlsToDisable.Length; i++)
        {
            if (playerControlsToDisable[i] == null)
                continue;

            previousControlStates[i] =
                playerControlsToDisable[i].enabled;

            playerControlsToDisable[i].enabled = false;
        }

        clockVirtualCamera.transform.SetPositionAndRotation(
            clockViewPoint.position,
            clockViewPoint.rotation
        );

        playerVirtualCamera.Priority.Value = 0;
        clockVirtualCamera.Priority.Value = 20;

        exitRequested = false;
        interactionFrame = Time.frameCount;
        isFocused = true;

        if (playerInteractor != null)
            playerInteractor.SetInteractionLocked(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Update()
    {
        if (!isFocused)
            return;

        if (Time.frameCount <= interactionFrame)
            return;

        if (Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            exitRequested = true;
        }
    }

    private void LateUpdate()
    {
        if (exitRequested)
        {
            exitRequested = false;
            ExitClock();
        }
    }
    public void ExitClock()
    {
        isFocused = false;

        if (hoverDetector != null)
            hoverDetector.ClearHover();

        clockVirtualCamera.Priority.Value = 0;
        playerVirtualCamera.Priority.Value = 20;

        if (playerInteractor != null)
            playerInteractor.SetInteractionLocked(false);

        if (previousControlStates != null)
        {
            for (int i = 0; i < playerControlsToDisable.Length; i++)
            {
                Behaviour control = playerControlsToDisable[i];

                if (control != null)
                    control.enabled = previousControlStates[i];
            }

            previousControlStates = null;
        }

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void LockControls()
    {
        if (previousControlStates == null ||
            previousControlStates.Length != playerControlsToDisable.Length)
        {
            previousControlStates = new bool[playerControlsToDisable.Length];

            for (int i = 0; i < playerControlsToDisable.Length; i++)
            {
                if (playerControlsToDisable[i] != null)
                    previousControlStates[i] = playerControlsToDisable[i].enabled;
            }
        }

        for (int i = 0; i < playerControlsToDisable.Length; i++)
        {
            if (playerControlsToDisable[i] != null)
                playerControlsToDisable[i].enabled = false;
        }
    }    
    public void UnlockControls()
    {
        if (previousControlStates == null)
            return;

        for (int i = 0; i < playerControlsToDisable.Length; i++)
        {
            if (playerControlsToDisable[i] != null)
                playerControlsToDisable[i].enabled = previousControlStates[i];
        }

        previousControlStates = null;
    }

}
