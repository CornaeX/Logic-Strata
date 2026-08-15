using UnityEngine;

public class RotaryKnob : MonoBehaviour
{
    public enum Axis { X, Y, Z }

    [Header("1. Axis & Angle Setup")]
    [Tooltip("The local axis the knob rotates around")]
    public Axis rotationAxis = Axis.Z;

    [Tooltip("Angle at minimum value (0.0)")]
    public float minAngle = -135f;

    [Tooltip("Angle at maximum value (1.0)")]
    public float maxAngle = 135f;

    [Header("2. Drag Sensitivity")]
    [Tooltip("How fast the knob turns when dragging the mouse")]
    public float mouseSensitivity = 0.5f;

    [Header("3. Initial Value")]
    [Range(0f, 1f)]
    public float normalizedValue = 0f; // 0.0 to 1.0

    [Header("4. Output Event")]
    [Tooltip("Connect your PowerSupplyDisplay methods here")]
    public UnityEventFloat onValueChanged;

    private Vector3 initialLocalRotation;
    private Vector3 lastMousePosition;

    void Start()
    {
        initialLocalRotation = transform.localEulerAngles;
        ApplyValueToRotation(normalizedValue);
    }

    void OnMouseDown()
    {
        lastMousePosition = Input.mousePosition;
    }

    void OnMouseDrag()
    {
        Vector3 delta = Input.mousePosition - lastMousePosition;
        
        // Drag up/right increases value, down/left decreases value
        float inputDelta = (delta.y + delta.x) * mouseSensitivity * 0.01f;
        
        normalizedValue = Mathf.Clamp01(normalizedValue + inputDelta);

        ApplyValueToRotation(normalizedValue);

        // Notify display script
        onValueChanged?.Invoke(normalizedValue);

        lastMousePosition = Input.mousePosition;
    }

    private void ApplyValueToRotation(float value)
    {
        // Interpolate angle based on 0.0 - 1.0 range
        float currentAngle = Mathf.Lerp(minAngle, maxAngle, value);

        Vector3 targetEuler = initialLocalRotation;

        switch (rotationAxis)
        {
            case Axis.X: targetEuler.x += currentAngle; break;
            case Axis.Y: targetEuler.y += currentAngle; break;
            case Axis.Z: targetEuler.z += currentAngle; break;
        }

        transform.localRotation = Quaternion.Euler(targetEuler);
    }

    // Call this if another script sets the knob position programmatically
    public void SetNormalizedValue(float value)
    {
        normalizedValue = Mathf.Clamp01(value);
        ApplyValueToRotation(normalizedValue);
        onValueChanged?.Invoke(normalizedValue);
    }
}

// System event wrapper to enable inspector wiring
[System.Serializable]
public class UnityEventFloat : UnityEngine.Events.UnityEvent<float> { }