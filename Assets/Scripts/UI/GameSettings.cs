using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

// survives scene loads. owns sensitivity and brightness, saves them between runs
public class GameSettings : MonoBehaviour
{
    public static GameSettings Instance;

    const string SensKey = "opt_sensitivity";
    const string BrightKey = "opt_brightness";

    [Header("Defaults")]
    public float defaultSensitivity = 100f;
    public float minSensitivity = 20f;
    public float maxSensitivity = 400f;

    [Tooltip("post exposure, 0 is untouched")]
    public float defaultBrightness = 0f;
    public float minBrightness = -1f;
    public float maxBrightness = 2f;

    public float Sensitivity { get; private set; }
    public float Brightness { get; private set; }

    ColorAdjustments colour;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        Sensitivity = PlayerPrefs.GetFloat(SensKey, defaultSensitivity);
        Brightness = PlayerPrefs.GetFloat(BrightKey, defaultBrightness);
    }

    void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }
    void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    void Start() { ApplyAll(); }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode) { ApplyAll(); }

    public void SetSensitivity(float value)
    {
        Sensitivity = value;
        PlayerPrefs.SetFloat(SensKey, value);
        ApplySensitivity();
    }

    public void SetBrightness(float value)
    {
        Brightness = value;
        PlayerPrefs.SetFloat(BrightKey, value);
        ApplyBrightness();
    }

    public void ApplyAll()
    {
        colour = null;
        ApplySensitivity();
        ApplyBrightness();
    }

    void ApplySensitivity()
    {
        var player = PlayerController.Instance;
        if (player != null) player.cameraSensitivity = Sensitivity;
    }

    void ApplyBrightness()
    {
        if (colour == null)
        {
            var volume = FindAnyObjectByType<Volume>();
            if (volume == null || volume.profile == null) return;
            if (!volume.profile.TryGet(out colour))
                colour = volume.profile.Add<ColorAdjustments>(true);
        }

        colour.postExposure.overrideState = true;
        colour.postExposure.value = Brightness;
    }
}