using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Movement Settings (WASD)")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float fastMoveMultiplier = 2f; // Hold Shift to speed up

    [Header("Rotation Settings")]
    [SerializeField] private float lookSensitivity = 2f;
    [SerializeField] private float minPitch = -80f; // Limit looking up
    [SerializeField] private float maxPitch = 80f;  // Limit looking down

    [Header("Zoom & Pan Settings")]
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float panSpeed = 2f;

    private float pitch = 0f;
    private float yaw = 0f;
    private Vector3 lastMousePosition;

    void Start()
    {
        // Initialize rotation variables from current camera rotation
        Vector3 angles = transform.eulerAngles;
        pitch = angles.x;
        yaw = angles.y;
    }

    void Update()
    {
        HandleWASDMovement();
        HandleLookAndPan();
        HandleZoom();

        lastMousePosition = Input.mousePosition;
    }

    private void HandleWASDMovement()
    {
        // Calculate WASD input vectors
        float inputX = Input.GetAxisRaw("Horizontal"); // A/D
        float inputZ = Input.GetAxisRaw("Vertical");   // W/S
        float inputY = 0f;

        // Q and E keys for Vertical Up/Down movement
        if (Input.GetKey(KeyCode.E)) inputY = 1f;
        if (Input.GetKey(KeyCode.Q)) inputY = -1f;

        Vector3 moveDirection = (transform.right * inputX + transform.up * inputY + transform.forward * inputZ).normalized;

        float speed = moveSpeed;
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            speed *= fastMoveMultiplier;
        }

        transform.position += moveDirection * speed * Time.deltaTime;
    }

    private void HandleLookAndPan()
    {
        // Middle Mouse Button Clicked & Dragged: Smooth Look Around / Orbit
        if (Input.GetMouseButton(2) && !Input.GetKey(KeyCode.LeftShift))
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;

            yaw += delta.x * lookSensitivity * 0.1f;
            pitch -= delta.y * lookSensitivity * 0.1f;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

            // Apply rotation cleanly using Euler angles (prevents camera roll)
            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }
        // Middle Mouse + Shift: Screen Pan
        else if (Input.GetMouseButton(2) && Input.GetKey(KeyCode.LeftShift))
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            Vector3 move = (-transform.right * delta.x - transform.up * delta.y) * (panSpeed * 0.001f);
            transform.position += move;
        }
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            transform.position += transform.forward * (scroll * zoomSpeed);
        }
    }
}