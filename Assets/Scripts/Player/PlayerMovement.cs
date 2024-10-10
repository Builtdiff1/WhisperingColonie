using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    public Transform cameraTransform;  // Reference to the player camera (assign in the Inspector)
    public float speed = 6f;
    public float sprintSpeed = 12f;  // Speed when sprinting
    public float gravity = -9.81f;
    public float jumpHeight = 3f;

    private Vector3 velocity;
    private bool isGrounded;
    private PlayerInputActions inputActions;
    private Vector2 moveInput;
    private bool jump;
    private bool canJump = true;  // Controls whether the player can jump
    private bool isSprinting;  // Tracks whether the player is sprinting

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;
    public float rotationSpeed = 720f;  // Speed of rotation in degrees per second
    public float movementSmoothing = 0.1f;  // Smoothing time for movement
    public float rotationSmoothing = 0.1f;  // Smoothing time for rotation

    private Vector3 smoothMoveVelocity;
    private Vector3 currentDirection;

    private void Awake()
    {
        inputActions = new PlayerInputActions();

        // Get input for movement and jump
        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;
        inputActions.Player.Jump.performed += ctx => jump = true;

        // Get input for sprinting
        inputActions.Player.Sprint.performed += ctx => isSprinting = true;
        inputActions.Player.Sprint.canceled += ctx => isSprinting = false;
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
    }

    void Update()
    {
        // Ground check
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
            canJump = true;  // Reset jump when grounded
        }

        // Determine the current speed based on whether the player is sprinting
        float currentSpeed = isSprinting ? sprintSpeed : speed;

        // Calculate movement relative to the camera's orientation
        Vector3 targetDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;

        if (targetDirection.magnitude >= 0.1f)
        {
            // Get the camera's forward and right vectors (ignoring vertical direction)
            Vector3 forward = cameraTransform.forward;
            forward.y = 0f;  // Ignore the camera's vertical rotation
            forward.Normalize();

            Vector3 right = cameraTransform.right;
            right.y = 0f;  // Ignore the camera's vertical rotation
            right.Normalize();

            // Calculate movement direction based on camera orientation
            Vector3 moveDirection = forward * targetDirection.z + right * targetDirection.x;

            // Smoothly interpolate movement direction using Slerp
            currentDirection = Vector3.Slerp(currentDirection, moveDirection, movementSmoothing / Time.deltaTime);

            // Rotate the player smoothly to face the direction of movement
            if (currentDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(currentDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothing / Time.deltaTime);
            }

            // Move the player
            controller.Move(currentDirection * currentSpeed * Time.deltaTime);
        }

        // Jumping (only allow jump when grounded and canJump is true)
        if (jump && isGrounded && canJump)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jump = false;
            canJump = false;  // Prevent further jumping until grounded again
        }

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}