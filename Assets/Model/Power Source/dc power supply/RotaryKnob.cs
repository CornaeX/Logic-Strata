using UnityEngine;

public class RotaryKnob : MonoBehaviour
{
    public enum Axis { X, Y, Z }

    [Header("1. Axis Setup")]
    [Tooltip("The local axis the knob rotates around")]
    public Axis rotationAxis = Axis.Z;

    [Header("2. Drag Sensitivity & Speed")]
    [Tooltip("Base rotation speed (increase for faster turning)")]
    public float baseSensitivity = 3.0f;

    [Tooltip("How much mouse speed boosts rotation speed")]
    public float velocityMultiplier = 0.2f;

    [Tooltip("Maximum allowed sensitivity cap to prevent wild over-spinning")]
    public float maxSensitivityCap = 25.0f;

    [Header("3. Value Scaling")]
    [Tooltip("How much normalizedValue (0.0 to 1.0) changes per full 360-degree rotation")]
    public float valuePerTurn = 0.2f; // e.g., 5 full 360-degree turns = 0 to 1 range (0V to 30V)

    [Header("4. Current Value")]
    [Range(0f, 1f)]
    public float normalizedValue = 0f; // Clamped for display values (0.0 to 1.0)

    [Header("5. Output Event")]
    [Tooltip("Connect your PowerSupplyDisplay methods here")]
    public UnityEventFloat onValueChanged;

    private Vector3 initialLocalRotation;
    private Vector3 lastMousePosition;
    private float currentContinuousAngle = 0f;

    void Start()
    {
        initialLocalRotation = transform.localEulerAngles;
        
        // Initialize continuous angle based on starting value
        currentContinuousAngle = (normalizedValue / valuePerTurn) * 360f;
        ApplyRotation(currentContinuousAngle);
    }

    void OnMouseDown()
    {
        lastMousePosition = Input.mousePosition;
    }

    void OnMouseDrag()
    {
        Vector3 currentMousePosition = Input.mousePosition;
        Vector3 delta = currentMousePosition - lastMousePosition;

        // Calculate mouse movement speed
        float mouseDistance = delta.magnitude;
        float mouseSpeed = mouseDistance / Mathf.Max(Time.deltaTime, 0.0001f);

        // Dynamic sensitivity boost
        float dynamicSensitivity = baseSensitivity + (mouseSpeed * velocityMultiplier * 0.01f);
        dynamicSensitivity = Mathf.Min(dynamicSensitivity, maxSensitivityCap);

        // Determine direction:
        // Moving mouse right (+delta.x) or up (+delta.y) increases value (+1)
        // Moving mouse left (-delta.x) or down (-delta.y) decreases value (-1)
        float dragAmount = delta.x + delta.y;
        float direction = Mathf.Sign(dragAmount);
        if (Mathf.Approximately(dragAmount, 0f)) direction = 0f;

        // Calculate maximum allowed angle limits based on valuePerTurn
        float maxAllowedAngle = (1.0f / valuePerTurn) * 360f;

        // Calculate proposed new angle step
        float angleDelta = direction * mouseDistance * dynamicSensitivity;
        
        // Clamp angle strictly between 0 and max limit
        currentContinuousAngle = Mathf.Clamp(currentContinuousAngle + angleDelta, 0f, maxAllowedAngle);

        // Update physical mesh rotation
        ApplyRotation(-currentContinuousAngle);

        // Convert clamped angle directly to normalized value
        normalizedValue = (currentContinuousAngle / 360f) * valuePerTurn;

        // Notify display script
        onValueChanged?.Invoke(normalizedValue);

        lastMousePosition = currentMousePosition;
    }

    private void ApplyRotation(float angle)
    {
        Vector3 targetEuler = initialLocalRotation;

        switch (rotationAxis)
        {
            case Axis.X: targetEuler.x += angle; break;
            case Axis.Y: targetEuler.y += angle; break;
            case Axis.Z: targetEuler.z += angle; break;
        }

        transform.localRotation = Quaternion.Euler(targetEuler);
    }

    public void SetNormalizedValue(float value)
    {
        normalizedValue = Mathf.Clamp01(value);
        currentContinuousAngle = (normalizedValue / valuePerTurn) * 360f;
        ApplyRotation(currentContinuousAngle);
        onValueChanged?.Invoke(normalizedValue);
    }
}

[System.Serializable]
public class UnityEventFloat : UnityEngine.Events.UnityEvent<float> { }