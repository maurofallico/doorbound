
using UnityEngine;
using UnityEngine.Audio;

public class AudioPasos : MonoBehaviour
{
    [Header("Sonidos de pasos")]
    [SerializeField] private AudioClip[] sonidosPasos;
    [SerializeField] private AudioMixerGroup grupoSFX;

    [Header("Configuración")]
    [SerializeField] private float intervaloPasos = 0.45f;
    [SerializeField] private float volumenMinimo = 0.65f;
    [SerializeField] private float volumenMaximo = 0.85f;

    private AudioSource audioSource;
    private CharacterController characterController;
    private float temporizador;
    private int ultimoPaso = -1;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;
        audioSource.outputAudioMixerGroup = grupoSFX;

        characterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (characterController == null) return;

        Vector3 velocidad = characterController.velocity;
        velocidad.y = 0f;

        bool caminando = velocidad.magnitude > 0.1f
                         && characterController.isGrounded;

        if (caminando)
        {
            temporizador += Time.deltaTime;

            if (temporizador >= intervaloPasos)
            {
                ReproducirPaso();
                temporizador = 0f;
            }
        }
        else
        {
            temporizador = 0f;
        }
    }

    void ReproducirPaso()
    {
        if (sonidosPasos.Length == 0) return;

        int indice = Random.Range(0, sonidosPasos.Length);

        if (sonidosPasos.Length > 1)
        {
            while (indice == ultimoPaso)
                indice = Random.Range(0, sonidosPasos.Length);
        }

        ultimoPaso = indice;

        float volumen = Random.Range(
            volumenMinimo,
            volumenMaximo
        );

        audioSource.PlayOneShot(sonidosPasos[indice], volumen);
    }
}
