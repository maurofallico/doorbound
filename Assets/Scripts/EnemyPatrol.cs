using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class EnemyPatrol : MonoBehaviour
{
    private enum State { Patrolling, Chasing }
    private State currentState = State.Patrolling;

    private enum PatrolMode
    {
        Loop,    
        PingPong 
    }

   
    [SerializeField] private Transform[] waypoints;

   
    [SerializeField] private PatrolMode patrolMode = PatrolMode.PingPong;

    [SerializeField] private float patrolSpeed = 2f;

    [SerializeField] private float waypointReachedThreshold = 0.1f;

    [SerializeField] private float waitTimeAtWaypoint = 0f;

    [SerializeField] private float chaseSpeed = 3.5f;

    [SerializeField] private float loseSightDelay = 2f;

    [SerializeField] private float viewRadius = 6f;

    [Range(0, 360)]
    [SerializeField] private float viewAngle = 90f;

    [SerializeField] private LayerMask obstacleMask;

    [SerializeField] private string playerTag = "Player";

    [SerializeField] private bool rotateTowardsMovement = true;

    private int currentWaypointIndex = 0;
    private int patrolDirection = 1; 
    private float waitTimer = 0f;
    private bool isWaiting = false;

    private Transform playerTransform;
    private float loseSightTimer = 0f;

    private void Awake()
    {
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
                RaycastHit hit;
                bool hitSomething = Physics.Raycast(
                    transform.position,
                    dirToPlayer.normalized,
                    out hit,
                    distanceToPlayer,
                    obstacleMask
                );

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
            
            loseSightTimer -= Time.deltaTime;
            if (loseSightTimer <= 0f)
            {
                currentState = State.Patrolling;
            }
        }
    }

    
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
            return;
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
           
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            return;
        }

       
        if (waypoints.Length <= 1) return;

        currentWaypointIndex += patrolDirection;

        
        if (currentWaypointIndex >= waypoints.Length)
        {
            currentWaypointIndex = waypoints.Length - 2; 
            patrolDirection = -1;
        }
        
        else if (currentWaypointIndex < 0)
        {
            currentWaypointIndex = 1; 
            patrolDirection = 1;
        }
    }

    private void RotateTowards(Vector3 direction)
    {
       
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.0001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        transform.rotation = targetRotation;
    }

    // --- DETECCIÓN DE COLISIÓN CON EL JUGADOR (Game Over) ---
    
    private void OnCollisionEnter(Collision collision)
    {
        HandlePlayerContact(collision.gameObject);
    }

    
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
                SceneManager.LoadScene("GameOver");
            }
           
        }
    }

    
    private void OnDrawGizmos()
    {
        
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

       
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewRadius);

        Vector3 facing = Application.isPlaying
            ? GetFacingDirection()
            : transform.forward; 

        Vector3 leftBoundary = Quaternion.Euler(0, viewAngle / 2f, 0) * facing;
        Vector3 rightBoundary = Quaternion.Euler(0, -viewAngle / 2f, 0) * facing;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * viewRadius);
    }
}