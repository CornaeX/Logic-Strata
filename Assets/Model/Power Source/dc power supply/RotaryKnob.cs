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
    [Tooltip("Base speed when moving the mouse slowly")]
    public float baseSensitivity = 0.5f;

    [Tooltip("How much mouse velocity boosts rotation speed")]
    public float velocityMultiplier = 0.05f;

    [Tooltip("Maximum allowed sensitivity cap to prevent wild over-spinning")]
    public float maxSensitivityCap = 5.0f;

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
        Vector3 currentMousePosition = Input.mousePosition;
        Vector3 delta = currentMousePosition - lastMousePosition;

        // Calculate mouse movement speed (pixels per second)
        float mouseDistance = delta.magnitude;
        float mouseSpeed = mouseDistance / Mathf.Max(Time.deltaTime, 0.0001f);

        // Dynamic sensitivity: increases linearly with mouse speed
        float dynamicSensitivity = baseSensitivity + (mouseSpeed * velocityMultiplier * 0.01f);
        dynamicSensitivity = Mathf.Min(dynamicSensitivity, maxSensitivityCap);

        // Calculate direction (-1 for down/left, +1 for up/right)
        float direction = Mathf.Sign(delta.y + delta.x);
        if (delta.x == 0 && delta.y == 0) direction = 0;

        // Apply dynamic rotation step
        float inputDelta = direction * mouseDistance * dynamicSensitivity * 0.001f;

        normalizedValue = Mathf.Clamp01(normalizedValue + inputDelta);

        ApplyValueToRotation(normalizedValue);

        // Notify display script
        onValueChanged?.Invoke(normalizedValue);

        lastMousePosition = currentMousePosition;
    }

    private void ApplyValueToRotation(float value)
    {
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

    public void SetNormalizedValue(float value)
    {
        normalizedValue = Mathf.Clamp01(value);
        ApplyValueToRotation(normalizedValue);
        onValueChanged?.Invoke(normalizedValue);
    }
}

[System.Serializable]
public class UnityEventFloat : UnityEngine.Events.UnityEvent<float> { }