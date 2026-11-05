
using UnityEngine;
using UnityEngine.Audio;

public class AudioPasos : MonoBehaviour
{
    [Header("Sonidos de pasos")]
    [SerializeField] private AudioClip[] sonidosPasos = System.Array.Empty<AudioClip>();
    [SerializeField] private AudioMixerGroup grupoSFX;

    [Header("Configuración")]
    [SerializeField] private float intervaloPasos = 0.45f;
    [SerializeField] private float volumenMinimo = 0.65f;
    [SerializeField] private float volumenMaximo = 0.85f;

    private AudioSource audioSource;
    private CharacterController characterController;
    private float temporizador;
    private Vector3 ultimaPosicion;
    private int ultimoPaso = -1;
    private AudioClip[] pasosExterior;
    private AudioClip[] ultimoBanco;
    private bool audioDeRespaldo;

    public void ConfigureFallback(AudioClip[] interior, AudioClip[] exterior, AudioMixerGroup output)
    {
        if (sonidosPasos.Length != 0)
        {
            if (audioSource.outputAudioMixerGroup == null)
                audioSource.outputAudioMixerGroup = output;
            return;
        }

        audioDeRespaldo = true;
        sonidosPasos = interior;
        pasosExterior = exterior;
        audioSource.outputAudioMixerGroup = output;
    }

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null || audioSource.clip != null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;
        if (grupoSFX != null)
            audioSource.outputAudioMixerGroup = grupoSFX;

        characterController = GetComponent<CharacterController>();
        ultimaPosicion = transform.position;
    }

    void Update()
    {
        if (characterController == null) return;

        Vector3 desplazamiento = transform.position - ultimaPosicion;
        desplazamiento.y = 0f;
        ultimaPosicion = transform.position;

        float velocidad = Time.deltaTime > 0f ? desplazamiento.magnitude / Time.deltaTime : 0f;

        bool caminando = velocidad > 0.1f
                         && characterController.isGrounded;

        temporizador = Mathf.Max(0f, temporizador - Time.deltaTime);
        if (caminando && temporizador == 0f)
        {
            ReproducirPaso();
            temporizador = intervaloPasos;
        }
    }

    void ReproducirPaso()
    {
        if (audioDeRespaldo && DoorboundAudio.IsProtected(transform)) return;

        var banco = audioDeRespaldo && DoorboundAudio.UsesExteriorSteps(transform)
            ? pasosExterior
            : sonidosPasos;
        if (banco.Length == 0) return;

        if (banco != ultimoBanco)
        {
            ultimoBanco = banco;
            ultimoPaso = -1;
        }

        int indice = Random.Range(0, banco.Length);

        if (banco.Length > 1 && ultimoPaso >= 0)
        {
            indice = Random.Range(0, banco.Length - 1);
            if (indice >= ultimoPaso) indice++;
        }

        ultimoPaso = indice;

        float volumen = Random.Range(
            volumenMinimo,
            volumenMaximo
        );

        audioSource.PlayOneShot(banco[indice], volumen);
    }
}
