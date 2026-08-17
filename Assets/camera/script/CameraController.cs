using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Starter Setup (Blender Default View)")]
    [SerializeField] private Vector3 starterPosition = new Vector3(0f, 5f, -10f);
    [SerializeField] private Vector3 starterRotation = new Vector3(30f, 0f, 0f);

    [Header("Rotation Settings (Orbit)")]
    [SerializeField] private float lookSensitivity = 2f;
    [SerializeField] private float minPitch = -89f;
    [SerializeField] private float maxPitch = 89f;

    [Header("Blender Distance-Based Scaling")]
    [SerializeField] private float baseZoomSpeed = 10f;
    [SerializeField] private float minDistanceThreshold = 0.5f; // Prevents speed reaching zero
    [SerializeField] private float distanceScaleFactor = 0.5f;  // How strongly distance affects speed
    [SerializeField] private LayerMask focusLayerMask = ~0;      // Layers to detect pivot point

    private float pitch = 0f;
    private float yaw = 0f;
    private Vector3 lastMousePosition;
    private Vector3 currentPivotPoint;
    private float currentDistance = 5f;

    void Start()
    {
        // 1. Force initial position and rotation close to (0,0,0) looking inward
        transform.position = starterPosition;
        transform.rotation = Quaternion.Euler(starterRotation);

        Vector3 angles = transform.eulerAngles;
        pitch = angles.x;
        yaw = angles.y;

        // 2. Set initial pivot explicitly to world origin (0,0,0) or straight ahead
        currentPivotPoint = Vector3.zero;
        currentDistance = Vector3.Distance(transform.position, currentPivotPoint);
        
        UpdatePivotAndDistance();
    }

    void Update()
    {
        // Calculate dynamic speed multiplier based on distance to the pivot point
        float distanceMultiplier = Mathf.Max(currentDistance * distanceScaleFactor, minDistanceThreshold);

        HandleOrbitAndPan(distanceMultiplier);
        HandleZoom(distanceMultiplier);
        HandleFocus();

        lastMousePosition = Input.mousePosition;
    }

    private void UpdatePivotAndDistance()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, focusLayerMask))
        {
            currentPivotPoint = hit.point;
            currentDistance = hit.distance;
        }
        else
        {
            currentDistance = Vector3.Distance(transform.position, currentPivotPoint);
        }
    }

    private void HandleOrbitAndPan(float distanceMultiplier)
    {
        // 1. Middle Mouse Drag: Orbit around pivot point (Blender Style)
        if (Input.GetMouseButton(2) && !Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;

            yaw += delta.x * lookSensitivity * 0.1f;
            pitch -= delta.y * lookSensitivity * 0.1f;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

            // Apply rotation
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            transform.rotation = rotation;

            // Maintain position relative to the pivot point so it orbits around it
            transform.position = currentPivotPoint - (rotation * Vector3.forward * currentDistance);
        }
        // 2. Shift + Middle Mouse: Pan view laterally
        else if (Input.GetMouseButton(2) && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            Vector3 panMove = (-transform.right * delta.x - transform.up * delta.y) * (distanceMultiplier * 0.005f * lookSensitivity);
            
            transform.position += panMove;
            currentPivotPoint += panMove; // Move pivot along with camera pan
        }
    }

    private void HandleZoom(float distanceMultiplier)
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            // Zoom moves forward/backward and scales dynamically based on distance
            float zoomAmount = scroll * baseZoomSpeed * distanceMultiplier;
            transform.position += transform.forward * zoomAmount;

            // Recalculate distance after zooming
            UpdatePivotAndDistance();
        }
    }

    private void HandleFocus()
    {
        // Press 'F' to focus on the object under the mouse cursor (Blender 'F' key behavior)
        if (Input.GetKeyDown(KeyCode.F))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, focusLayerMask))
            {
                currentPivotPoint = hit.point;
                currentDistance = Vector3.Distance(transform.position, currentPivotPoint);
                
                if (currentDistance > 10f)
                {
                    currentDistance = 5f;
                    transform.position = currentPivotPoint - transform.forward * currentDistance;
                }
            }
        }
    }
}