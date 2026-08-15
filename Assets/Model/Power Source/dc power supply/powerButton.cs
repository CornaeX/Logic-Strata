using UnityEngine;

public class powerButton : MonoBehaviour
{
    [Header("1. Physical Button Movement")]
    [Tooltip("The Transform of the push button mesh")]
    public Transform buttonTransform;
    [Tooltip("How far the button pushes inward when ON (Local Offset)")]
    public Vector3 pushedOffset = new Vector3(0f, 0f, -0.005f); 

    [Header("2. LED Material Settings")]
    [Tooltip("The MeshRenderer component of the power button")]
    public MeshRenderer buttonRenderer;
    [Tooltip("Index of the LED face material slot (0 for 1st slot, 1 for 2nd slot)")]
    public int ledMaterialIndex = 1;

    [Header("3. Emission Colors")]
    public Color offEmissionColor = Color.black; // OFF State (No glow)
    
    [ColorUsage(true, true)] 
    public Color onEmissionColor = Color.red * 4.0f; // ON State (Bright HDR Red)

    [Header("4. Screen Control")]
    public PowerSupplyDisplay targetDisplay;

    private bool isPoweredOn = false;
    private Vector3 unpushedLocalPosition;
    private Material instanceLedMaterial;

    // Direct property IDs from your console log
    private static readonly int EmissiveFactorProp = Shader.PropertyToID("emissiveFactor");
    private static readonly int EmissionColorProp = Shader.PropertyToID("_EmissionColor");

    void Start()
    {
        // Debug.Log("[powerButton] Start initialized on: " + gameObject.name);

        // 1. Check for Collider
        if (GetComponent<Collider>() == null)
        {
            // Debug.LogWarning("[powerButton] WARNING: No Collider found on " + gameObject.name + "! OnMouseDown() will NOT work without a BoxCollider or MeshCollider attached.");
        }

        // 2. Setup Position Reference
        if (buttonTransform != null)
        {
            unpushedLocalPosition = buttonTransform.localPosition;
            // Debug.Log("[powerButton] Stored Resting Position: " + unpushedLocalPosition);
        }
        else
        {
            // Debug.LogError("[powerButton] ERROR: buttonTransform is NOT assigned in the Inspector!");
        }

        // 3. Setup Material Reference
        if (buttonRenderer != null)
        {
            if (ledMaterialIndex < buttonRenderer.materials.Length)
            {
                instanceLedMaterial = buttonRenderer.materials[ledMaterialIndex];
                // Debug.Log("[powerButton] Material target successfully assigned: " + instanceLedMaterial.name + " at index " + ledMaterialIndex);
            }
            else
            {
                // Debug.LogError("[powerButton] ERROR: ledMaterialIndex " + ledMaterialIndex + " is out of bounds! MeshRenderer only has " + buttonRenderer.materials.Length + " material slots.");
            }
        }
        else
        {
            // Debug.LogError("[powerButton] ERROR: buttonRenderer is NOT assigned in the Inspector!");
        }

        // Apply starting state (OFF)
        ApplyPowerState(false);
    }

    public void TogglePower()
    {
        isPoweredOn = !isPoweredOn;
        // Debug.Log("[powerButton] TogglePower called. New Power State: " + (isPoweredOn ? "ON" : "OFF"));
        ApplyPowerState(isPoweredOn);
    }

    private void ApplyPowerState(bool state)
    {
        // A. Physical Push Motion
        if (buttonTransform != null)
        {
            Vector3 targetPosition = state ? unpushedLocalPosition + pushedOffset : unpushedLocalPosition;
            buttonTransform.localPosition = targetPosition;
            // Debug.Log("[powerButton] Button localPosition set to: " + targetPosition);
        }

        // B. LED Emission Toggle
        if (instanceLedMaterial != null)
        {
            Color targetColor = state ? onEmissionColor : offEmissionColor;

            // Target glTF Shader Graph property (emissiveFactor) and standard property (_EmissionColor)
            instanceLedMaterial.SetColor(EmissiveFactorProp, targetColor);
            instanceLedMaterial.SetColor(EmissionColorProp, targetColor);

            // Keep required keywords enabled for Shader Graph passes
            instanceLedMaterial.EnableKeyword("_EMISSIVE");
            instanceLedMaterial.EnableKeyword("_EMISSION");

            // Debug.Log("[powerButton] Applied Emission Color: " + targetColor);
        }

        if (targetDisplay != null)
        {
            targetDisplay.SetPowerState(state);
        }
    }

    private void OnMouseDown()
    {
        // Debug.Log("[powerButton] Mouse Clicked on " + gameObject.name);
        TogglePower();
    }
}