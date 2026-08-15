using UnityEngine;
using TMPro;

public class PowerSupplyDisplay : MonoBehaviour
{
    [Header("1. UI Text References (TextMeshPro)")]
    public TextMeshProUGUI voltageText;
    public TextMeshProUGUI currentText;
    public TextMeshProUGUI wattageText;

    [Header("2. Output Ranges & Knobs")]
    public float maxVoltage = 30.0f; // Max Volts (e.g. 0-30V)
    public float maxCurrent = 5.0f;  // Max Amps (e.g. 0-5A)

    [Header("3. Live Values")]
    [Range(0f, 30f)] public float currentVoltage = 0.0f;
    [Range(0f, 5f)]  public float currentAmperage = 0.0f;

    private bool isPoweredOn = false;

    void Start()
    {
        // Initialize screen state
        UpdateDisplay();
    }

    void Update()
    {
        // Live updates while powered on (useful when turning knobs)
        if (isPoweredOn)
        {
            UpdateDisplay();
        }
    }

    /// <summary>
    /// Call this from your powerButton script when turning the device ON/OFF
    /// </summary>
    public void SetPowerState(bool powerState)
    {
        isPoweredOn = powerState;

        if (!isPoweredOn)
        {
            // Clear or dim the screen when powered OFF
            if (voltageText != null) voltageText.text = "00.00";
            if (currentText != null) currentText.text = "0.000";
            if (wattageText != null) wattageText.text = "000.0";
        }
        else
        {
            UpdateDisplay();
        }
    }

    /// <summary>
    /// Formats floating point values into digital display readouts
    /// </summary>
    private void UpdateDisplay()
    {
        if (!isPoweredOn) return;

        // Calculate Wattage: P = V * I
        float calculatedWatts = currentVoltage * currentAmperage;

        // Format to fixed decimal digits matching real power supplies
        if (voltageText != null) 
            voltageText.text = $"{currentVoltage:00.00}";

        if (currentText != null) 
            currentText.text = $"{currentAmperage:0.000}";

        if (wattageText != null) 
            wattageText.text = $"{calculatedWatts:000.0}";
    }

    // Call this helper method from your Rotary Knob interaction script
    public void SetVoltage(float normalizedValue)
    {
        currentVoltage = Mathf.Clamp(normalizedValue * maxVoltage, 0f, maxVoltage);
    }

    // Call this helper method from your Current Knob interaction script
    public void SetCurrent(float normalizedValue)
    {
        currentAmperage = Mathf.Clamp(normalizedValue * maxCurrent, 0f, maxCurrent);
    }
}