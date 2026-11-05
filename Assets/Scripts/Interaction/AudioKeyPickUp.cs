using UnityEngine;

public class AudioKeyPickup : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private GameObject llave;
    [SerializeField] private AudioClip[] sonidosLlave;

    private AudioSource audioSource;
    private bool llaveRecogida = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (llave == null || llaveRecogida)
            return;

        if (!llave.activeInHierarchy)
        {
            llaveRecogida = true;

            if (sonidosLlave.Length > 0)
            {
                int indice = Random.Range(0, sonidosLlave.Length);
                audioSource.PlayOneShot(sonidosLlave[indice]);
            }
        }
    }
}
