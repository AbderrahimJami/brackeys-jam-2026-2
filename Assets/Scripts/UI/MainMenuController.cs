using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Tooltip("scene the Start button loads")]
    public string gameSceneName = "AK";

    [Header("Panels")]
    public GameObject mainPanel;
    public GameObject settingsPanel;
    public GameObject creditsPanel;

    void Start()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        ShowMain();
    }

    public void StartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameSceneName);
    }

    public void ShowMain() { Switch(mainPanel); }
    public void ShowSettings() { Switch(settingsPanel); }
    public void ShowCredits() { Switch(creditsPanel); }

    void Switch(GameObject on)
    {
        if (mainPanel != null) mainPanel.SetActive(mainPanel == on);
        if (settingsPanel != null) settingsPanel.SetActive(settingsPanel == on);
        if (creditsPanel != null) creditsPanel.SetActive(creditsPanel == on);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}