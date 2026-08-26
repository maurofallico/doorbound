using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
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

    private Rigidbody rb;
    private Animator animator;

    private Vector2 moveInput;
    private Vector2 lookInput;

    private bool isRunning;
    private float yaw;
    private float pitch;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
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
            Quaternion targetRotation =
                Quaternion.LookRotation(forward);

            Quaternion newRotation = Quaternion.RotateTowards(
                rb.rotation,
                targetRotation,
                rotationSpeed * 100f * Time.fixedDeltaTime
            );

            rb.MoveRotation(newRotation);
        }

        rb.MovePosition(
            rb.position +
            movement * currentSpeed * Time.fixedDeltaTime
        );
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
        bool movingForward = moveInput.y > 0.1f;
        bool movingBackward = moveInput.y < -0.1f;
        bool strafeRight = moveInput.x > 0.1f;
        bool strafeLeft = moveInput.x < -0.1f;

        animator.SetBool("running", isRunning);
        animator.SetBool("forward", movingForward);
        animator.SetBool("backward", movingBackward);
        animator.SetBool("strafeRight", strafeRight);
        animator.SetBool("strafeLeft", strafeLeft);
    }
}