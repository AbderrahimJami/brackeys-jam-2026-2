using UnityEngine;
using UnityEngine.UI;
using TMPro;

// drives the two sliders. works in the title screen and the pause menu
public class SettingsPanel : MonoBehaviour
{
    public Slider sensitivitySlider;
    public Slider brightnessSlider;
    public TMP_Text sensitivityValue;
    public TMP_Text brightnessValue;

    bool loading;

    void OnEnable()
    {
        var s = GameSettings.Instance;
        if (s == null) return;

        loading = true;

        if (sensitivitySlider != null)
        {
            sensitivitySlider.minValue = s.minSensitivity;
            sensitivitySlider.maxValue = s.maxSensitivity;
            sensitivitySlider.value = s.Sensitivity;
        }
        if (brightnessSlider != null)
        {
            brightnessSlider.minValue = s.minBrightness;
            brightnessSlider.maxValue = s.maxBrightness;
            brightnessSlider.value = s.Brightness;
        }

        loading = false;
        RefreshLabels();
    }

    public void OnSensitivityChanged(float value)
    {
        if (loading || GameSettings.Instance == null) return;
        GameSettings.Instance.SetSensitivity(value);
        RefreshLabels();
    }

    public void OnBrightnessChanged(float value)
    {
        if (loading || GameSettings.Instance == null) return;
        GameSettings.Instance.SetBrightness(value);
        RefreshLabels();
    }

    void RefreshLabels()
    {
        var s = GameSettings.Instance;
        if (s == null) return;

        if (sensitivityValue != null) sensitivityValue.text = Mathf.RoundToInt(s.Sensitivity).ToString();
        if (brightnessValue != null) brightnessValue.text = s.Brightness.ToString("0.0");
    }
}