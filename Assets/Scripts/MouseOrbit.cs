using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    public Transform target;  // The target to orbit around (your player character)
    public Transform firstPersonViewTarget; // The position for first-person view (e.g., a transform in front of the player's face)

    public float rotationSpeed = 150f;    // Speed of camera rotation
    public float zoomSpeed = 5f;          // Speed of zoom with scroll wheel
    public float minZoom = 1f;            // Minimum zoom distance (first-person)
    public float maxZoom = 10f;           // Maximum zoom distance
    public float firstPersonThreshold = 1.5f; // Distance at which we switch to first-person view
    public float minYAngle = -20f;        // Minimum vertical angle
    public float maxYAngle = 80f;         // Maximum vertical angle
    public float mouseSensitivity = 100f; // Sensitivity of the camera in first-person mode
    public float bodyRotationThreshold = 90f; // Angle threshold for rotating the body in first-person mode
    public float rotationSmoothing = 0.1f;   // Smoothing time for rotation
    public float bodyRotationSmoothing = 0.2f; // Smoothing for the body rotation

    public LayerMask terrainLayer; // LayerMask for terrain
    public float collisionOffset = 0.3f; // How far from the terrain the camera should stay
    public float cameraRadius = 0.5f;    // Radius for the SphereCast to simulate the camera size

    public GameObject mouseIndicator;    // Reference to the mouse indicator game object

    private float currentXRotation = 0f;
    private float currentYRotation = 0f;
    private float currentZoom;
    private bool isFirstPerson = false;   // Tracks whether the camera is in first-person mode
    private Quaternion targetRotation;    // For smooth rotation


    private void Start()
    {
        // Initialize the current zoom to the default maximum zoom distance
        currentZoom = maxZoom;

        // Initialize target rotation to the current rotation
        targetRotation = transform.rotation;

        // Make sure the mouse indicator is initially hidden
        mouseIndicator.SetActive(false);
    }

    void Update()
    {
        HandleMouseOrbit();
        HandleZoom();
        UpdateCameraView();
    }

    void HandleMouseOrbit()
    {
        if (Input.GetMouseButton(1) && !isFirstPerson)
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            currentXRotation += mouseX * rotationSpeed * Time.deltaTime;
            currentYRotation -= mouseY * rotationSpeed * Time.deltaTime;

            currentYRotation = Mathf.Clamp(currentYRotation, minYAngle, maxYAngle);
        }

        if (!isFirstPerson)
        {
            Quaternion targetRotation = Quaternion.Euler(currentYRotation, currentXRotation, 0);
            Vector3 desiredPosition = target.position - (targetRotation * Vector3.forward * currentZoom);

            RaycastHit hit;
            Vector3 directionToCamera = desiredPosition - target.position;
            float distanceToCamera = directionToCamera.magnitude;

            if (Physics.SphereCast(target.position, cameraRadius, directionToCamera.normalized, out hit, distanceToCamera, terrainLayer))
            {
                transform.position = hit.point - directionToCamera.normalized * collisionOffset;
            }
            else
            {
                transform.position = desiredPosition;
            }

            transform.LookAt(target);
        }
    }

    void HandleZoom()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        currentZoom -= scrollInput * zoomSpeed;
        currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
    }

    void UpdateCameraView()
    {
        if (currentZoom <= firstPersonThreshold && !isFirstPerson)
        {
            SwitchToFirstPerson();
        }
        else if (currentZoom > firstPersonThreshold && isFirstPerson)
        {
            SwitchToThirdPerson();
        }

        if (isFirstPerson)
        {
            HandleFirstPersonCamera();
        }
    }

    void SwitchToFirstPerson()
    {
        isFirstPerson = true;

        // Reset the camera position and rotation to the first-person view target
        transform.position = firstPersonViewTarget.position;
        transform.rotation = firstPersonViewTarget.rotation;

        // Show the mouse indicator
        mouseIndicator.SetActive(true);

        // Lock the cursor for better first-person camera control
        Cursor.lockState = CursorLockMode.Locked;
    }

    void SwitchToThirdPerson()
    {
        isFirstPerson = false;

        // Hide the mouse indicator
        mouseIndicator.SetActive(false);

        // Unlock the cursor when leaving first-person view
        Cursor.lockState = CursorLockMode.None;
    }

    void HandleFirstPersonCamera()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        currentXRotation += mouseX;
        currentYRotation -= mouseY;
        currentYRotation = Mathf.Clamp(currentYRotation, minYAngle, maxYAngle);

        targetRotation = Quaternion.Euler(currentYRotation, currentXRotation, 0);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothing * 10f);
        transform.position = firstPersonViewTarget.position;

        float cameraYawRelativeToBody = Mathf.DeltaAngle(target.eulerAngles.y, transform.eulerAngles.y);

        if (Mathf.Abs(cameraYawRelativeToBody) > bodyRotationThreshold)
        {
            target.rotation = Quaternion.Slerp(target.rotation, Quaternion.Euler(0, transform.eulerAngles.y, 0), bodyRotationSmoothing);
        }
    }
}