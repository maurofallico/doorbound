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
    }

    private void Update()
    {
        LookCamera();
        UpdateAnimations();
    }

    private void FixedUpdate()
    {
        Move();
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

        // Dirección horizontal de la cámara
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        // Movimiento relativo a la cámara
        Vector3 movement =
            forward * moveInput.y +
            right * moveInput.x;

        movement = Vector3.ClampMagnitude(movement, 1f);

        // El personaje SOLO rota mientras se está moviendo
        if (movement.sqrMagnitude > 0.01f)
        {

            Quaternion targetRotation = Quaternion.LookRotation(movement);

            Quaternion newRotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * 100f * Time.fixedDeltaTime
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