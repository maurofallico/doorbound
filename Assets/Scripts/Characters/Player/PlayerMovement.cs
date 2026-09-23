using UnityEngine;
using UnityEngine.InputSystem;

// =====================================================================
// CHANGELOG
// [Axel] - Fix camara:
//          Move() estaba en FixedUpdate, ahora esta en Update junto con
//          LookCamera(), para que el movimiento y la rotacion de camara
//          vayan al mismo ritmo (frame a frame).
// =====================================================================

public class PlayerMovement : MonoBehaviour
{

    [SerializeField] private float gravity = -9.81f;
    private float verticalVelocity;

    // MOVEMENT
    [SerializeField] private float walkSpeed = 1.5f;
    [SerializeField] private float runSpeed = 3.2f;
    [SerializeField] private float rotationSpeed = 10f;

    // CAMERA
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Transform cameraPivot;

    [SerializeField] private float verticalLookSpeed = 0.1f;
    [SerializeField] private float minPitch = -15f;
    [SerializeField] private float maxPitch = 20f;

    private CharacterController controller;
    private Animator animator;

    private Vector2 moveInput;
    private Vector2 lookInput;

    private bool isRunning;
    private float yaw;
    private float pitch;
    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // --- FIX (Axel): el movimiento estaba en FixedUpdate() mientras la camara
    // se actualiza cada frame en Update()/LateUpdate(). Ese desfase entre el tick
    // fisico fijo y el framerate real es lo que causaba un "tiron" al girar la camara.
    // Solucion: CharacterController no es un Rigidbody, asi que Move() no necesita
    // FixedUpdate. Ahora todo corre junto, en el mismo frame.
    private void Update()
    {
        LookCamera();
        Move();
        UpdateAnimations();
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        EnemyPatrol enemy = hit.collider.GetComponentInParent<EnemyPatrol>();
        if (enemy != null)
        {
            enemy.HandlePlayerContact(gameObject);
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        isRunning = context.ReadValueAsButton();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    private void Move()
    {
        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        // Direccion horizontal de la camara
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        // Movimiento relativo a la camara
        Vector3 movement =
            forward * moveInput.y +
            right * moveInput.x;

        movement = Vector3.ClampMagnitude(movement, 1f);

        // El personaje SOLO rota mientras se esta moviendo
        if (movement.sqrMagnitude > 0.01f)
        {

            Quaternion targetRotation = Quaternion.LookRotation(movement);

            Quaternion newRotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                // (Axel) Time.deltaTime en vez de fixedDeltaTime, porque ahora Move() vive en Update()
                rotationSpeed * 100f * Time.deltaTime
            );

            transform.rotation = newRotation;
        }
        // Gravedad
        verticalVelocity += gravity * Time.deltaTime;
        movement.y = verticalVelocity;

        // Movimiento
        controller.Move(movement * currentSpeed * Time.deltaTime);
    }

    private void LookCamera()
    {
        yaw += lookInput.x * 0.1f;
        pitch -= lookInput.y * verticalLookSpeed;

        pitch = Mathf.Clamp(
            pitch,
            minPitch,
            maxPitch
        );

        cameraPivot.localRotation =
            Quaternion.Euler(pitch, yaw, 0f);
    }

    private void UpdateAnimations()
    {
        bool isMoving = moveInput.y != 0 || moveInput.x !=0;

        animator.SetBool("running", isRunning);
        animator.SetBool("moving", isMoving);
    }
}
