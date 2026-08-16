using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Sensitivity Settings")]
    [SerializeField] private float panSpeed = 2f;      // Reduced for small objects
    [SerializeField] private float zoomSpeed = 2f;     // Reduced for close zooming
    [SerializeField] private float rotateSpeed = 5f;

    private Vector3 lastMousePosition;

    void Update()
    {
        // Middle Mouse + Shift: Pan
        if (Input.GetMouseButton(2) && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            Vector3 move = (-transform.right * delta.x - transform.up * delta.y) * (panSpeed * 0.0005f);
            transform.position += move;
        }
        // Middle Mouse alone: Orbit
        else if (Input.GetMouseButton(2))
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            transform.RotateAround(transform.position + transform.forward * 2f, Vector3.up, delta.x * rotateSpeed * 0.1f);
            transform.RotateAround(transform.position + transform.forward * 2f, transform.right, -delta.y * rotateSpeed * 0.1f);
        }

        // Scroll Wheel: Zoom
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            transform.position += transform.forward * (scroll * zoomSpeed);
        }

        lastMousePosition = Input.mousePosition;
    }
}