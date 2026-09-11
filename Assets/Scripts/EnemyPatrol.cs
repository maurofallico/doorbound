using UnityEngine;

/// <summary>
/// Enemigo que patrulla entre puntos (waypoints) de forma indefinida,
/// en modo bucle (Loop) o ida y vuelta (PingPong), configurable desde
/// el Inspector con el campo "patrolMode".
/// Tiene un campo de visión (FOV): si detecta al jugador dentro de ese
/// cono y con línea de visión libre (sin obstáculos), lo persigue.
/// NO ataca en ningún momento: solo si lo llega a tocar, se dispara el
/// Game Over a través de GameManager.
///
/// CONFIGURACIÓN EN EL INSPECTOR:
/// 1. Crea objetos vacíos en la escena para marcar los puntos de patrulla
///    (por ejemplo "Waypoint1", "Waypoint2", etc.) y arrástralos al array
///    "waypoints" de este componente.
/// 2. Asigna este script a tu enemigo (necesita un Collider).
/// 3. Asegurate de que el jugador tenga el Tag "Player".
/// 4. (Opcional) Configurá la capa "obstacleMask" con las capas que deban
///    bloquear la visión del enemigo (paredes, etc.) para que la
///    detección sea realista.
/// </summary>
[RequireComponent(typeof(Collider))]
public class EnemyPatrol : MonoBehaviour
{
    private enum State { Patrolling, Chasing }
    private State currentState = State.Patrolling;

    private enum PatrolMode
    {
        Loop,     // Recorre todos los waypoints y al llegar al último vuelve al primero (bucle)
        PingPong  // Va del primero al último y luego regresa al primero, tipo péndulo (ida y vuelta)
    }

    [Header("Patrullaje")]
    [Tooltip("Puntos por los que el enemigo se moverá en orden.")]
    [SerializeField] private Transform[] waypoints;

    [Tooltip("Loop = bucle (1→2→3→1→2→3...). PingPong = ida y vuelta (1→2→3→2→1→2→3...).")]
    [SerializeField] private PatrolMode patrolMode = PatrolMode.PingPong;

    [Tooltip("Velocidad de movimiento durante la patrulla.")]
    [SerializeField] private float patrolSpeed = 2f;

    [Tooltip("Distancia mínima al waypoint para considerarlo 'alcanzado'.")]
    [SerializeField] private float waypointReachedThreshold = 0.1f;

    [Tooltip("Tiempo de espera (segundos) al llegar a cada waypoint antes de continuar.")]
    [SerializeField] private float waitTimeAtWaypoint = 0f;

    [Header("Persecución")]
    [Tooltip("Velocidad de movimiento cuando detecta al jugador y lo persigue.")]
    [SerializeField] private float chaseSpeed = 3.5f;

    [Tooltip("Si pierde de vista al jugador, cuántos segundos sigue buscándolo antes de volver a patrullar.")]
    [SerializeField] private float loseSightDelay = 2f;

    [Header("Campo de visión (FOV)")]
    [Tooltip("Radio de detección del enemigo.")]
    [SerializeField] private float viewRadius = 6f;

    [Tooltip("Ángulo total del cono de visión, en grados (ej: 90 = 45° a cada lado).")]
    [Range(0, 360)]
    [SerializeField] private float viewAngle = 90f;

    [Tooltip("Capas que bloquean la línea de visión (paredes, obstáculos, etc.). Dejar vacío si no hay obstáculos.")]
    [SerializeField] private LayerMask obstacleMask;

    [Header("Detección de contacto")]
    [Tooltip("Tag que debe tener el objeto del jugador.")]
    [SerializeField] private string playerTag = "Player";

    [Tooltip("Si el enemigo debe rotar/mirar hacia la dirección en la que se mueve.")]
    [SerializeField] private bool rotateTowardsMovement = true;

    private int currentWaypointIndex = 0;
    private int patrolDirection = 1; // 1 = avanza hacia adelante, -1 = retrocede (usado en PingPong)
    private float waitTimer = 0f;
    private bool isWaiting = false;

    private Transform playerTransform;
    private float loseSightTimer = 0f;

    private void Awake()
    {
        // Busca al jugador por tag una sola vez al inicio (más eficiente que buscarlo cada frame)
        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
    }

    private void Update()
    {
        DetectPlayer();

        if (currentState == State.Chasing)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }
    }

    // --- DETECCIÓN POR CAMPO DE VISIÓN ---
    private void DetectPlayer()
    {
        if (playerTransform == null)
        {
            return;
        }
        Vector3 dirToPlayer = (playerTransform.position - transform.position);
        float distanceToPlayer = dirToPlayer.magnitude;

        bool canSeePlayer = false;

        if (distanceToPlayer <= viewRadius)
        {
            float angleToPlayer = Vector3.Angle(GetFacingDirection(), dirToPlayer);

            if (angleToPlayer <= viewAngle / 2f)
            {
                // Chequea que no haya un obstáculo entre el enemigo y el jugador
                RaycastHit hit;
                bool hitSomething = Physics.Raycast(
                    transform.position,
                    dirToPlayer.normalized,
                    out hit,
                    distanceToPlayer,
                    obstacleMask
                );

                // Si el raycast no golpea ningún obstáculo, hay línea de visión libre
                if (!hitSomething)
                {
                    canSeePlayer = true;
                }
            }
        }

        if (canSeePlayer)
        {
            currentState = State.Chasing;
            loseSightTimer = loseSightDelay;
        }
        else if (currentState == State.Chasing)
        {
            // Sigue persiguiendo un rato aunque lo haya perdido de vista,
            // antes de volver a la patrulla (simula "buscar" al jugador)
            loseSightTimer -= Time.deltaTime;
            if (loseSightTimer <= 0f)
            {
                currentState = State.Patrolling;
            }
        }
    }

    // Dirección hacia la que "mira" el enemigo actualmente
    private Vector3 GetFacingDirection()
    {
        return transform.forward;
    }

    // --- PERSECUCIÓN (sin ataque, solo se acerca al jugador) ---
    private void ChasePlayer()
    {
        if (playerTransform == null)
        {
            currentState = State.Patrolling;
            return;
        }

        Vector3 direction = playerTransform.position - transform.position;

        transform.position = Vector3.MoveTowards(
            transform.position,
            playerTransform.position,
            chaseSpeed * Time.deltaTime
        );

        if (rotateTowardsMovement && direction.sqrMagnitude > 0.0001f)
        {
            RotateTowards(direction);
        }
    }

    // --- PATRULLAJE ---
    private void Patrol()
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            return; // No hay waypoints asignados, el enemigo no se mueve.
        }

        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                isWaiting = false;
            }
            return;
        }

        Transform target = waypoints[currentWaypointIndex];
        Vector3 direction = (target.position - transform.position);
        float distance = direction.magnitude;

        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            patrolSpeed * Time.deltaTime
        );

        if (rotateTowardsMovement && direction.sqrMagnitude > 0.0001f)
        {
            RotateTowards(direction);
        }

        if (distance <= waypointReachedThreshold)
        {
            if (waitTimeAtWaypoint > 0f)
            {
                isWaiting = true;
                waitTimer = waitTimeAtWaypoint;
            }

            AdvanceWaypointIndex();
        }
    }

    private void AdvanceWaypointIndex()
    {
        if (patrolMode == PatrolMode.Loop)
        {
            // Bucle simple: 0→1→2→3→0→1→2→3...
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            return;
        }

        // --- Modo PingPong (ida y vuelta) ---
        // Solo tiene sentido "rebotar" si hay más de un waypoint
        if (waypoints.Length <= 1) return;

        currentWaypointIndex += patrolDirection;

        // Si se pasó del último waypoint, invierte dirección y retrocede
        if (currentWaypointIndex >= waypoints.Length)
        {
            currentWaypointIndex = waypoints.Length - 2; // el anteúltimo
            patrolDirection = -1;
        }
        // Si se pasó del primer waypoint, invierte dirección y avanza
        else if (currentWaypointIndex < 0)
        {
            currentWaypointIndex = 1; // el segundo
            patrolDirection = 1;
        }
    }

    private void RotateTowards(Vector3 direction)
    {
        // Ignoramos la diferencia de altura (Y) para que el enemigo no incline
        // la cabeza hacia arriba/abajo, solo rote sobre el plano horizontal.
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.0001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        transform.rotation = targetRotation;
    }

    // --- DETECCIÓN DE COLISIÓN CON EL JUGADOR (Game Over) ---
    // Usa este método si el Collider del enemigo o del jugador NO son "Is Trigger".
    private void OnCollisionEnter(Collision collision)
    {
        HandlePlayerContact(collision.gameObject);
    }

    // Usa este método si preferís que el Collider sea "Is Trigger" = true.
    private void OnTriggerEnter(Collider other)
    {
        HandlePlayerContact(other.gameObject);
    }

    private void HandlePlayerContact(GameObject other)
    {
        if (other.CompareTag(playerTag))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.GameOver();
            }
            else
            {
                Debug.LogWarning("No se encontró un GameManager en la escena. " +
                    "Agregá el script GameManager.cs a un objeto de la escena.");
            }
        }
    }

    // Ayuda visual en el editor: dibuja el camino de patrulla y el campo de visión
    private void OnDrawGizmos()
    {
        // Camino de patrulla
        if (waypoints != null && waypoints.Length > 0)
        {
            Gizmos.color = Color.red;
            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] == null) continue;
                Gizmos.DrawSphere(waypoints[i].position, 0.15f);

                Transform next = waypoints[(i + 1) % waypoints.Length];
                if (next != null)
                {
                    Gizmos.DrawLine(waypoints[i].position, next.position);
                }
            }
        }

        // Campo de visión
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewRadius);

        Vector3 facing = Application.isPlaying
            ? GetFacingDirection()
            : transform.forward; // aproximación en editor antes de correr el juego

        Vector3 leftBoundary = Quaternion.Euler(0, viewAngle / 2f, 0) * facing;
        Vector3 rightBoundary = Quaternion.Euler(0, -viewAngle / 2f, 0) * facing;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * viewRadius);
    }
}