using UnityEngine;

public class PuzzleBotones : MonoBehaviour
{
    public GameObject puerta;

    private int pasoActual = 0;
    private bool desbloqueada = false;

    public void PulsarBoton(int boton)
    {
        if (desbloqueada)
            return;

        int[] secuencia = { 0, 1, 2 };

        if (boton == secuencia[pasoActual])
        {
            pasoActual++;

            Debug.Log("Correcto! Paso número: " + pasoActual);

            if (pasoActual == secuencia.Length)
            {
                desbloqueada = true;
                Debug.Log("Puerta desbloqueada");

                puerta.SetActive(false);
            }
        }
        else
        {
            pasoActual = 0;
            Debug.Log("Orden incorrecto. Intenta de nuevo.");
        }
    }
}

