using FMODUnity;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static bool IsPaused;

    [Header("Panels, leave them off in the scene")]
    public GameObject pausePanel;
    public GameObject settingsPanel;

    public string menuSceneName = "MainMenu";
    public KeyCode pauseKey = KeyCode.Escape;

    void Start()
    {
        IsPaused = false;
        if (pausePanel != null) pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    void OnDisable()
    {
        IsPaused = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.L)) return;

        // escape closes a note first, don't pause on that same press
        //if (NoteReader.Instance != null && NoteReader.Instance.IsOpen) return;

        if (settingsPanel != null && settingsPanel.activeSelf) { ShowPausePanel(); return; }

        if (IsPaused) Resume(); else Pause();
    }

    public void Pause()
    {
        IsPaused = true;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (PlayerController.Instance != null) PlayerController.Instance.enabled = false;
        ShowPausePanel();
    }

    public void Resume()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (PlayerController.Instance != null) PlayerController.Instance.enabled = true;
        if (pausePanel != null) pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    public void ShowPausePanel()
    {
        if (pausePanel != null) pausePanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    public void ShowSettings()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void QuitToMenu()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);

        //Music transitions to menu state
        RuntimeManager.StudioSystem.setParameterByName("GameStart", 0);
    }
}