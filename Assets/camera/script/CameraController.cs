using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Movement Settings (WASD)")]
    [SerializeField] private float baseMoveSpeed = 10f;
    [SerializeField] private float fastMoveMultiplier = 2f;

    [Header("Rotation Settings")]
    [SerializeField] private float lookSensitivity = 2f;
    [SerializeField] private float minPitch = -80f;
    [SerializeField] private float maxPitch = 80f;

    [Header("Blender Distance-Based Scaling")]
    [SerializeField] private float baseZoomSpeed = 5f;
    [SerializeField] private float minDistanceThreshold = 0.5f; // Prevents speed reaching zero
    [SerializeField] private float distanceScaleFactor = 0.15f; // How strongly distance affects speed
    [SerializeField] private LayerMask focusLayerMask = ~0;      // Layers to detect pivot point

    private float pitch = 0f;
    private float yaw = 0f;
    private Vector3 lastMousePosition;
    private Vector3 currentPivotPoint;
    private float currentDistance = 5f;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        pitch = angles.x;
        yaw = angles.y;

        UpdatePivotAndDistance();
    }

    void Update()
    {
        UpdatePivotAndDistance();

        // Calculate dynamic speed multiplier based on distance to the pivot point
        float distanceMultiplier = Mathf.Max(currentDistance * distanceScaleFactor, minDistanceThreshold);

        HandleWASDMovement(distanceMultiplier);
        HandleLookAndPan(distanceMultiplier);
        HandleZoom(distanceMultiplier);
        HandleFocus();

        lastMousePosition = Input.mousePosition;
    }

    private void UpdatePivotAndDistance()
    {
        // Raycast forward from screen center to find the pivot point
        Ray ray = new Ray(transform.position, transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, focusLayerMask))
        {
            currentPivotPoint = hit.point;
            currentDistance = hit.distance;
        }
        else
        {
            // Fallback if looking at empty sky
            currentDistance = Vector3.Distance(transform.position, currentPivotPoint);
        }
    }

    private void HandleWASDMovement(float distanceMultiplier)
    {
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputZ = Input.GetAxisRaw("Vertical");
        float inputY = 0f;

        if (Input.GetKey(KeyCode.E)) inputY = 1f;
        if (Input.GetKey(KeyCode.Q)) inputY = -1f;

        Vector3 moveDirection = (transform.right * inputX + transform.up * inputY + transform.forward * inputZ).normalized;

        float speed = baseMoveSpeed * distanceMultiplier;
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            speed *= fastMoveMultiplier;
        }

        transform.position += moveDirection * speed * Time.deltaTime;
    }

    private void HandleLookAndPan(float distanceMultiplier)
    {
        // Middle Mouse Drag: Orbit / Look Around
        if (Input.GetMouseButton(2) && !Input.GetKey(KeyCode.LeftShift))
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;

            yaw += delta.x * lookSensitivity * 0.1f;
            pitch -= delta.y * lookSensitivity * 0.1f;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }
        // Middle Mouse + Shift: Screen Pan (Scales with distance)
        else if (Input.GetMouseButton(2) && Input.GetKey(KeyCode.LeftShift))
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            Vector3 move = (-transform.right * delta.x - transform.up * delta.y) * (distanceMultiplier * 0.01f);
            transform.position += move;
        }
    }

    private void HandleZoom(float distanceMultiplier)
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            // Zoom speed dynamically scales with distance to center
            float zoomAmount = scroll * baseZoomSpeed * distanceMultiplier;
            transform.position += transform.forward * zoomAmount;
        }
    }

    private void HandleFocus()
    {
        // Press 'F' to focus and jump closer to target object under cursor
        if (Input.GetKeyDown(KeyCode.F))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, focusLayerMask))
            {
                currentPivotPoint = hit.point;
                transform.position = hit.point - transform.forward * 2f;
            }
        }
    }
}