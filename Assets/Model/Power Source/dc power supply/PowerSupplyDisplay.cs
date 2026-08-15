using UnityEngine;
using TMPro;

public class PowerSupplyDisplay : MonoBehaviour
{
    [Header("1. Digit Text References")]
    public TextMeshProUGUI voltageText;
    public TextMeshProUGUI currentText;
    public TextMeshProUGUI wattageText;

    [Header("2. Static Unit Label References")]
    public TextMeshProUGUI voltageUnitText; // "V"
    public TextMeshProUGUI currentUnitText; // "A"
    public TextMeshProUGUI wattageUnitText; // "W"

    [Header("3. Display Colors (Realism Settings)")]
    [Tooltip("Color of digits/labels when the device is OFF (dark/ghosting)")]
    public Color offColor = new Color(0.05f, 0.08f, 0.05f, 0.25f); // Very dark/semi-transparent

    [Tooltip("Color of digits/labels when powered ON (HDR multiplier for bloom glow)")]
    [ColorUsage(true, true)]
    public Color onColor = Color.green * 5.0f; // High-intensity HDR green

    [Header("4. Output Ranges & Knobs")]
    public float maxVoltage = 30.0f;
    public float maxCurrent = 5.0f;

    [Header("5. Live Values")]
    [Range(0f, 30f)] public float currentVoltage = 0.0f;
    [Range(0f, 5f)]  public float currentAmperage = 0.0f;

    private bool isPoweredOn = false;
    private Material onStateMaterial;
    private Material offStateMaterial;

    void Start()
    {
        InitializeMaterials();
        ApplyVisualState(false);
    }

    void Update()
    {
        if (isPoweredOn)
        {
            UpdateDisplayValues();
        }
    }

    private void InitializeMaterials()
    {
        // Get base material from one of the texts to clone setup
        if (voltageText != null)
        {
            // Create dedicated material instances for ON and OFF states
            onStateMaterial = new Material(voltageText.fontMaterial);
            onStateMaterial.name = "TMP_Display_ON_Material";
            onStateMaterial.SetColor("_FaceColor", onColor);
            onStateMaterial.EnableKeyword("OUTLINE_ON");
            onStateMaterial.EnableKeyword("GLOW_ON");

            offStateMaterial = new Material(voltageText.fontMaterial);
            offStateMaterial.name = "TMP_Display_OFF_Material";
            offStateMaterial.SetColor("_FaceColor", offColor);
        }
    }

    public void SetPowerState(bool powerState)
    {
        isPoweredOn = powerState;
        ApplyVisualState(isPoweredOn);

        if (isPoweredOn)
        {
            UpdateDisplayValues();
        }
    }

    private void UpdateDisplayValues()
    {
        float calculatedWatts = currentVoltage * currentAmperage;

        if (voltageText != null) voltageText.text = $"{currentVoltage:00.00}";
        if (currentText != null) currentText.text = $"{currentAmperage:00.00}";
        if (wattageText != null) wattageText.text = $"{calculatedWatts:000.0}";
    }

    private void ApplyVisualState(bool state)
    {
        Material activeMaterial = state ? onStateMaterial : offStateMaterial;
        Color targetColor = state ? onColor : offColor;

        // Apply material instance to allow unclipped HDR values
        AssignMaterialAndColor(voltageText, activeMaterial, targetColor);
        AssignMaterialAndColor(currentText, activeMaterial, targetColor);
        AssignMaterialAndColor(wattageText, activeMaterial, targetColor);

        AssignMaterialAndColor(voltageUnitText, activeMaterial, targetColor);
        AssignMaterialAndColor(currentUnitText, activeMaterial, targetColor);
        AssignMaterialAndColor(wattageUnitText, activeMaterial, targetColor);

        if (!state)
        {
            if (voltageText != null) voltageText.text = "00.00";
            if (currentText != null) currentText.text = "00.00";
            if (wattageText != null) wattageText.text = "000.0";
        }
    }

    private void AssignMaterialAndColor(TextMeshProUGUI tmpText, Material mat, Color col)
    {
        if (tmpText != null)
        {
            if (mat != null)
            {
                tmpText.fontSharedMaterial = mat;
            }
            tmpText.color = col;
        }
    }

    public void SetVoltage(float normalizedValue)
    {
        currentVoltage = Mathf.Clamp(normalizedValue * maxVoltage, 0f, maxVoltage);
    }

    public void SetCurrent(float normalizedValue)
    {
        currentAmperage = Mathf.Clamp(normalizedValue * maxCurrent, 0f, maxCurrent);
    }
}