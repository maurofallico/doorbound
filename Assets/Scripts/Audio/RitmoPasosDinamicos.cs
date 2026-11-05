using System.Reflection;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Script propio que extiende AudioPasos SIN modificarlo.
/// Lee la velocidad del personaje (Player o Enemigo) y, según su estado,
/// modifica en tiempo de ejecución DOS parámetros:
///   1) El intervalo entre pasos de AudioPasos (más rápido al correr).
///   2) El pitch del AudioSource que usa AudioPasos (más agudo al correr).
///
/// Se agrega en el mismo GameObject que AudioPasos. No requiere clips.
/// </summary>
[RequireComponent(typeof(AudioPasos))]
[DefaultExecutionOrder(-10)] // corre antes que AudioPasos para que use el intervalo nuevo en el mismo frame
public class RitmoPasosDinamico : MonoBehaviour
{
    private enum Estado { Quieto, Caminando, Corriendo }

    [Header("Umbrales de velocidad (mirar dbgVelocidad en Play)")]
    [SerializeField] private float velocidadCaminar = 2f;
    [SerializeField] private float velocidadCorrer = 6f;

    [Header("Parámetro 1: intervalo entre pasos (segundos)")]
    [SerializeField] private float intervaloCaminando = 0.45f;
    [SerializeField] private float intervaloCorriendo = 0.25f;

    [Header("Parámetro 2: pitch")]
    [SerializeField] private float pitchCaminando = 1f;
    [SerializeField] private float pitchCorriendo = 1.12f;
    [Tooltip("Qué tan rápido se acomoda el pitch al cambiar de estado.")]
    [SerializeField] private float suavizadoPitch = 4f;

    [Header("Diagnóstico (solo lectura)")]
    [SerializeField] private Estado dbgEstado;
    [SerializeField] private float dbgVelocidad;
    [SerializeField] private float dbgIntervalo;
    [SerializeField] private float dbgPitch;

    private AudioPasos audioPasos;
    private AudioSource fuentePasos;
    private CharacterController controller;
    private NavMeshAgent agente;
    private Vector3 posicionAnterior;
    private Estado estadoAnterior;

    // Acceso a los campos privados de AudioPasos sin editar ese script
    private FieldInfo campoIntervalo;
    private FieldInfo campoTemporizador;

    void Awake()
    {
        audioPasos = GetComponent<AudioPasos>();
        controller = GetComponent<CharacterController>();
        agente = GetComponent<NavMeshAgent>();

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        campoIntervalo = typeof(AudioPasos).GetField("intervaloPasos", flags);
        campoTemporizador = typeof(AudioPasos).GetField("temporizador", flags);

        if (campoIntervalo == null)
            Debug.LogError("[RitmoPasosDinamico] No encontré 'intervaloPasos' en AudioPasos.", this);
    }

    void Start()
    {
        // AudioPasos usa (o crea en su Awake) un AudioSource SIN clip asignado:
        // ese es el que reproduce los pasos con PlayOneShot.
        foreach (var fuente in GetComponents<AudioSource>())
        {
            if (fuente.clip == null) { fuentePasos = fuente; break; }
        }

        if (fuentePasos == null)
            Debug.LogWarning("[RitmoPasosDinamico] No encontré el AudioSource de AudioPasos; solo se va a modificar el intervalo.", this);

        posicionAnterior = transform.position;
    }

    void Update()
    {
        float velocidad = ObtenerVelocidadHorizontal();
        Estado estado = DeterminarEstado(velocidad);

        // 0 = caminando, 1 = corriendo a fondo
        float t = Mathf.InverseLerp(velocidadCaminar, velocidadCorrer, velocidad);
        float intervalo = Mathf.Lerp(intervaloCaminando, intervaloCorriendo, t);

        // --- Parámetro 1: intervalo de AudioPasos ---
        if (campoIntervalo != null)
            campoIntervalo.SetValue(audioPasos, intervalo);

        // Al pasar a correr, se acorta la espera que quedó pendiente del intervalo
        // de caminar, para que la aceleración se note en el acto.
        if (estado == Estado.Corriendo && estadoAnterior != Estado.Corriendo && campoTemporizador != null)
        {
            float pendiente = (float)campoTemporizador.GetValue(audioPasos);
            campoTemporizador.SetValue(audioPasos, Mathf.Min(pendiente, intervalo));
        }

        // --- Parámetro 2: pitch del AudioSource de pasos ---
        if (fuentePasos != null)
        {
            float pitchObjetivo = estado == Estado.Quieto
                ? pitchCaminando
                : Mathf.Lerp(pitchCaminando, pitchCorriendo, t);

            fuentePasos.pitch = Mathf.MoveTowards(fuentePasos.pitch, pitchObjetivo, suavizadoPitch * Time.deltaTime);
            dbgPitch = fuentePasos.pitch;
        }

        estadoAnterior = estado;
        dbgEstado = estado;
        dbgVelocidad = velocidad;
        dbgIntervalo = intervalo;
    }

    Estado DeterminarEstado(float velocidad)
    {
        if (velocidad <= 0.1f) return Estado.Quieto;

        float puntoMedio = (velocidadCaminar + velocidadCorrer) * 0.5f;
        return velocidad >= puntoMedio ? Estado.Corriendo : Estado.Caminando;
    }

    float ObtenerVelocidadHorizontal()
    {
        Vector3 v;

        if (controller != null)
            v = controller.velocity;                    // Player
        else if (agente != null)
            v = agente.velocity;                        // Enemigo con NavMesh
        else if (Time.deltaTime > 0f)
            v = (transform.position - posicionAnterior) / Time.deltaTime;
        else
            v = Vector3.zero;

        posicionAnterior = transform.position;
        v.y = 0f;
        return v.magnitude;
    }
}
