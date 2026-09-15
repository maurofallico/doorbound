using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CharacterController))]
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

    [Header("Circular Room")]
    [SerializeField] private bool restrictToRoom;
    [SerializeField] private Vector3 roomCenter;
    [SerializeField] private Vector2 roomRadii = new Vector2(10.65f, 13.25f);
    [SerializeField] private float wallMargin = 0.75f;
    [SerializeField] private float circularPatrolRadius = 5f;
    [SerializeField] private float roomHeightTolerance = 2f;
    private int circularWaypoint;

   
    [SerializeField] private PatrolMode patrolMode = PatrolMode.PingPong;

    [SerializeField] private float patrolSpeed = 2f;

    [SerializeField] private float waypointReachedThreshold = 0.1f;

    [SerializeField] private float waitTimeAtWaypoint = 0f;

    [SerializeField] private float chaseSpeed = 3.5f;

    [Header("Grounding")]
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float groundedVerticalSpeed = -2f;

    [SerializeField] private float loseSightDelay = 2f;

    [SerializeField] private float viewRadius = 6f;

    [Range(0, 360)]
    [SerializeField] private float viewAngle = 90f;

    [SerializeField] private LayerMask obstacleMask;

    [SerializeField] private string playerTag = "Player";

    [SerializeField] private bool rotateTowardsMovement = true;

    [Header("Visual Feedback")]
    [SerializeField] private Renderer enemyRenderer;
    [SerializeField] private Color chasingColor = Color.red;

    private int currentWaypointIndex = 0;
    private int patrolDirection = 1; 
    private float waitTimer = 0f;
    private bool isWaiting = false;

    private Transform playerTransform;
    private CharacterController controller;
    private MaterialPropertyBlock materialPropertyBlock;
    private Color patrolColor;
    private int colorPropertyId = -1;
    private float verticalVelocity;
    private float loseSightTimer = 0f;
    private bool defeatPending;

    private static readonly int BaseColorProperty = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorProperty = Shader.PropertyToID("_Color");

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        InitializeVisualFeedback();

        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
    }

    private void Update()
    {
        if (defeatPending) return;
        DetectPlayer();

        if (currentState == State.Chasing)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }

        ApplyGravity();
    }

    private void LateUpdate()
    {
        if (!defeatPending) return;
        enabled = false;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // --- DETECCIÓN POR CAMPO DE VISIÓN ---
    private void DetectPlayer()
    {
        if (playerTransform == null)
        {
            return;
        }
        if (restrictToRoom && !IsInsideRoom(playerTransform.position))
        {
            loseSightTimer = 0f;
            SetState(State.Patrolling);
            return;
        }
        Vector3 dirToPlayer = (playerTransform.position - transform.position);
        float distanceToPlayer = dirToPlayer.magnitude;

        bool canSeePlayer = false;

        // Inside the room, entry alerts the enemy regardless of distance or facing.
        if (restrictToRoom || distanceToPlayer <= viewRadius)
        {
            Vector3 horizontalDirectionToPlayer = dirToPlayer;
            horizontalDirectionToPlayer.y = 0f;
            float angleToPlayer = Vector3.Angle(
                GetFacingDirection(),
                horizontalDirectionToPlayer
            );
            bool isInsideViewAngle =
                restrictToRoom ||
                currentState == State.Chasing ||
                angleToPlayer <= viewAngle / 2f;

            if (isInsideViewAngle)
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
            SetState(State.Chasing);
            loseSightTimer = loseSightDelay;
        }
        else if (currentState == State.Chasing)
        {
            
            loseSightTimer -= Time.deltaTime;
            if (loseSightTimer <= 0f)
            {
                SetState(State.Patrolling);
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
            SetState(State.Patrolling);
            return;
        }

        MoveTowardsTarget(playerTransform.position, chaseSpeed);
    }

    // --- PATRULLAJE ---
    private void Patrol()
    {
        if (restrictToRoom)
        {
            float angle = circularWaypoint * Mathf.PI / 4f;
            Vector3 targetPoint = roomCenter + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * circularPatrolRadius;
            MoveTowardsTarget(targetPoint, patrolSpeed);
            Vector3 remaining = targetPoint - transform.position;
            remaining.y = 0f;
            if (remaining.magnitude <= Mathf.Max(waypointReachedThreshold, 0.15f))
                circularWaypoint = (circularWaypoint + 1) % 8;
            return;
        }
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
        Vector3 direction = target.position - transform.position;
        direction.y = 0f;
        float distance = direction.magnitude;

        MoveTowardsTarget(target.position, patrolSpeed);

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

    private void MoveTowardsTarget(Vector3 targetPosition, float speed)
    {
        if (restrictToRoom)
        {
            Vector3 offset = targetPosition - roomCenter;
            offset.y = 0f;
            Vector2 movementRadii = new Vector2(
                Mathf.Max(0.1f, roomRadii.x - wallMargin),
                Mathf.Max(0.1f, roomRadii.y - wallMargin));
            float normalizedDistance = new Vector2(offset.x / movementRadii.x, offset.z / movementRadii.y).magnitude;
            targetPosition = roomCenter + offset / Mathf.Max(1f, normalizedDistance);
        }
        Vector3 direction = targetPosition - transform.position;
        direction.y = 0f;

        float distance = direction.magnitude;
        if (distance < 0.0001f)
        {
            return;
        }

        float movementDistance = Mathf.Min(speed * Time.deltaTime, distance);
        controller.Move(direction.normalized * movementDistance);

        if (rotateTowardsMovement)
        {
            RotateTowards(direction);
        }
    }

    private void ApplyGravity()
    {
        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = groundedVerticalSpeed;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        controller.Move(Vector3.up * verticalVelocity * Time.deltaTime);
    }

    private void InitializeVisualFeedback()
    {
        if (enemyRenderer == null)
        {
            enemyRenderer = GetComponent<Renderer>();
        }

        Material sharedMaterial = enemyRenderer != null ? enemyRenderer.sharedMaterial : null;
        if (sharedMaterial == null)
        {
            return;
        }

        if (sharedMaterial.HasProperty(BaseColorProperty))
        {
            colorPropertyId = BaseColorProperty;
        }
        else if (sharedMaterial.HasProperty(ColorProperty))
        {
            colorPropertyId = ColorProperty;
        }
        else
        {
            return;
        }

        patrolColor = sharedMaterial.GetColor(colorPropertyId);
        materialPropertyBlock = new MaterialPropertyBlock();
        ApplyStateColor();
    }

    private void SetState(State nextState)
    {
        if (currentState == nextState)
        {
            return;
        }

        currentState = nextState;
        if (restrictToRoom && nextState == State.Patrolling)
        {
            Vector3 offset = transform.position - roomCenter;
            circularWaypoint = (Mathf.RoundToInt(Mathf.Atan2(offset.z, offset.x) / (Mathf.PI / 4f)) + 9) % 8;
        }
        ApplyStateColor();
    }

    private bool IsInsideRoom(Vector3 position)
    {
        Vector3 offset = position - roomCenter;
        if (Mathf.Abs(offset.y) > roomHeightTolerance) return false;
        offset.y = 0f;
        return new Vector2(offset.x / Mathf.Max(0.1f, roomRadii.x),
            offset.z / Mathf.Max(0.1f, roomRadii.y)).sqrMagnitude <= 1f;
    }

    private void ApplyStateColor()
    {
        if (enemyRenderer == null || materialPropertyBlock == null || colorPropertyId < 0)
        {
            return;
        }

        enemyRenderer.GetPropertyBlock(materialPropertyBlock);
        Color stateColor = currentState == State.Chasing ? chasingColor : patrolColor;
        materialPropertyBlock.SetColor(colorPropertyId, stateColor);
        enemyRenderer.SetPropertyBlock(materialPropertyBlock);
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

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        HandlePlayerContact(hit.gameObject);
    }

    
    private void OnTriggerEnter(Collider other)
    {
        HandlePlayerContact(other.gameObject);
    }

    public void HandlePlayerContact(GameObject other)
    {
        if (defeatPending) return;

        // Colliders may be on a child of the tagged player object.
        for (Transform contact = other.transform; contact != null; contact = contact.parent)
        {
            if (contact.CompareTag(playerTag))
            {
                defeatPending = true;
                return;
            }
        }
    }

    
    private void OnDrawGizmos()
    {
        if (restrictToRoom)
        {
            Vector3 previous = roomCenter + Vector3.right * roomRadii.x;
            Gizmos.color = Color.green;
            for (int i = 1; i <= 64; i++)
            {
                float angle = i * Mathf.PI * 2f / 64f;
                Vector3 next = roomCenter + new Vector3(Mathf.Cos(angle) * roomRadii.x, 0f, Mathf.Sin(angle) * roomRadii.y);
                Gizmos.DrawLine(previous, next);
                previous = next;
            }
        }
        
        if (!restrictToRoom && waypoints != null && waypoints.Length > 0)
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
