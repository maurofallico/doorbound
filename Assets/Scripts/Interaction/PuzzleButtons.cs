using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class PuzzleBotones : MonoBehaviour
{

    [SerializeField] private SecretDoorControllerUpstair secretDoor;
    [SerializeField] private CinemachineCamera playerCamera;
    [SerializeField] private CinemachineCamera secretDoorCamera;
    [SerializeField] private float cameraReturnDelay = 2f;

    [SerializeField] private BotonPuzzle button1;
    [SerializeField] private BotonPuzzle button2;
    [SerializeField] private BotonPuzzle button3;
    [SerializeField] private NoteReader note;


    private int pasoActual = 0;
    private bool desbloqueada = false;

    private void Start()
    {
        note.BlockNote();
    }

    public void PulsarBoton(int boton)
    {

        if (desbloqueada)
            return;

        int[] secuencia = { 0, 1, 2 };

        // El primer botón siempre inicia la secuencia.
        if (pasoActual == 0)
        {
            pasoActual = 1;
            Debug.Log("Primer botón registrado: " + boton);
            SetButtonLight(boton, true);
            switch (boton)
            {
                case 0:
                    button1.BlockButton();
                    break;
                case 1:
                    button2.BlockButton();
                    break;
                case 2:
                    button3.BlockButton();
                    break;
            }
            return;
        }

        // Evalúa el segundo y tercer botón.
        if (boton == secuencia[pasoActual])
        {
            pasoActual++;
            switch (boton)
            {
                case 0:
                    button1.BlockButton();
                    break;
                case 1:
                    button2.BlockButton();
                    break;
                case 2:
                    button3.BlockButton();
                    break;
            }
            Debug.Log("Correcto! Paso número: " + pasoActual);
            SetButtonLight(boton, true);
            if (pasoActual == secuencia.Length)
            {
                desbloqueada = true;
                Debug.Log("Puerta desbloqueada");
                note.UnlockNote();
                SetButtonLight(boton, true);
                StartCoroutine(OpenSecretDoorSequence());
            }
        }
        else
        {
            pasoActual = 0;
            Debug.Log("Orden incorrecto. Secuencia reiniciada.");
            StartCoroutine(ResetPuzzle(boton));
        }
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

    }

    private void SetButtonLight(int boton, bool enabled)
    {
        Light light1 = button1.GetComponent<Light>();
        Light light2 = button2.GetComponent<Light>();
        Light light3 = button3.GetComponent<Light>();

        switch (boton)
        {
            case 0:
                light1.enabled = enabled;
                break;

            case 1:
                light2.enabled = enabled;
                break;

            case 2:
                light3.enabled = enabled;
                break;
        }
    }

    private IEnumerator ResetPuzzle(int boton)
    {
        SetButtonLight(boton, true);

        // Espera medio segundo.
        yield return new WaitForSeconds(0.25f);

        pasoActual = 0;

        button1.RestoreButton();
        button2.RestoreButton();
        button3.RestoreButton();

        SetButtonLight(0, false);
        SetButtonLight(1, false);
        SetButtonLight(2, false);
    }

}

