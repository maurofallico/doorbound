using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class ClockPuzzleManager : MonoBehaviour
{
    [SerializeField] private ClockInteraction clock1;
    [SerializeField] private ClockInteraction clock2;
    [SerializeField] private ClockInteraction clock3;

    [SerializeField] private SecretDoorController secretDoor;

    [Header("Cameras")]
    [SerializeField] private CinemachineCamera playerCamera;
    [SerializeField] private CinemachineCamera secretDoorCamera;

    [SerializeField] private float cameraReturnDelay = 2f;
    [Header("Player Control")]

    public bool puzzleSolved;
    public void CheckPuzzle()
    {

        if (puzzleSolved)
            return;
        
        bool clock1Correct =
            clock1.CurrentHour == 1 &&
            clock1.CurrentMinute == 30;

        bool clock2Correct =
            clock2.CurrentHour == 3 &&
            clock2.CurrentMinute == 45;

        bool clock3Correct =
            clock3.CurrentHour == 9 &&
            clock3.CurrentMinute == 55;

        if (clock1Correct && clock2Correct && clock3Correct)
        {
            SolvePuzzle();
        }
    }

    private void SolvePuzzle()
    {
        puzzleSolved = true;

        clock1.BlockInteraction();
        clock2.BlockInteraction();
        clock3.BlockInteraction();

        StartCoroutine(OpenSecretDoorSequence());
    }

    private IEnumerator OpenSecretDoorSequence()
    {
        if (secretDoorCamera == null || playerCamera == null || secretDoor == null)
        {
            yield break;
        }

        secretDoorCamera.Priority.Value = 30;
        playerCamera.Priority.Value = 0;

        secretDoor.Open();

        yield return new WaitForSeconds(secretDoor.OpenDuration);
        yield return new WaitForSeconds(cameraReturnDelay);

        secretDoorCamera.Priority.Value = 0;
        playerCamera.Priority.Value = 20;
        clock1.ExitClock();
        clock2.ExitClock();
        clock3.ExitClock();

    }


}