using UnityEngine;
using UnityEngine.InputSystem;

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

    // CROUCHING
    [SerializeField] private float standingHeight = 1.8f;
    [SerializeField] private float crouchingHeight = 1.0f;

    [SerializeField] private float standingCenterY = 0.43f;
    [SerializeField] private float crouchingCenterY = 0.03f;

    private CharacterController controller;
    private Animator animator;

    private Vector2 moveInput;
    private Vector2 lookInput;

    private bool isRunning;
    private bool isCrouching;
    private bool runInput;
    private bool crouchInput;
    private float yaw;
    private float pitch;
    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        LookCamera();

        UpdateStates();

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
        runInput = context.ReadValueAsButton();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        crouchInput = context.ReadValueAsButton();
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
                !isRunning ? rotationSpeed * 100f * Time.deltaTime : rotationSpeed * 100f * 1.5f * Time.deltaTime
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

    private void UpdateStates()
    {
        isCrouching = crouchInput;
        isRunning = runInput && !isCrouching;
        UpdateCharacterController();
    }

    private void UpdateCharacterController()
    {
        if (isCrouching)
        {
            controller.height = crouchingHeight;

            Vector3 center = controller.center;
            center.y = crouchingCenterY;
            controller.center = center;
        }
        else
        {
            controller.height = standingHeight;

            Vector3 center = controller.center;
            center.y = standingCenterY;
            controller.center = center;
        }
    }

    private void UpdateAnimations()
    {
        bool isMoving = moveInput.sqrMagnitude > 0.01f;

        float moveAmount = 0f;

        if (isMoving)
        {
            moveAmount = isRunning ? 1f : 0.5f;
        }

        animator.SetFloat("moveAmount", moveAmount, 0.1f, Time.deltaTime);
        animator.SetBool("crouching", isCrouching);
    }
}
