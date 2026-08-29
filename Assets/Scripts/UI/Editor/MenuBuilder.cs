using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEditor;
using UnityEditor.Events;
using TMPro;

// Tools > TrustNoOne > Build Title Screen / Build Pause Menu
public class MenuBuilder
{
    static readonly Color Ink = new Color(0.90f, 0.86f, 0.78f);      // aged paper
    static readonly Color Dim = new Color(0.62f, 0.58f, 0.52f);
    static readonly Color Backdrop = new Color(0.04f, 0.04f, 0.05f, 0.92f);
    static readonly Color ButtonIdle = new Color(1f, 1f, 1f, 0.05f);
    static readonly Color SliderBg = new Color(1f, 1f, 1f, 0.10f);

    const float ButtonW = 360f;
    const float ButtonH = 62f;

    [MenuItem("Tools/TrustNoOne/Build Title Screen")]
    public static void BuildTitle()
    {
        var canvas = MakeCanvas("TitleCanvas");
        var menu = canvas.gameObject.AddComponent<MainMenuController>();

        MakeBackdrop(canvas.transform, new Color(0.03f, 0.03f, 0.04f, 1f));

        // ---- main panel
        var main = MakePanel(canvas.transform, "MainPanel", false);

        var title = MakeText(main.transform, "Title", "REMIND ME", 96, Ink);
        Place(title.rectTransform, 0, 250, 1000, 140);
        title.alignment = TextAlignmentOptions.Center;
        title.characterSpacing = 14f;

        var tagline = MakeText(main.transform, "Tagline", "the house keeps moving", 30, Dim);
        Place(tagline.rectTransform, 0, 160, 900, 50);
        tagline.alignment = TextAlignmentOptions.Center;
        tagline.fontStyle = FontStyles.Italic;

        var start = MakeButton(main.transform, "StartButton", "Start", 0, 20);
        var credits = MakeButton(main.transform, "CreditsButton", "Credits", 0, -60);
        var settings = MakeButton(main.transform, "SettingsButton", "Settings", 0, -140);
        var quit = MakeButton(main.transform, "QuitButton", "Quit", 0, -220);

        Hook(start, menu.StartGame);
        Hook(credits, menu.ShowCredits);
        Hook(settings, menu.ShowSettings);
        Hook(quit, menu.QuitGame);

        // ---- settings panel
        var settingsPanel = MakePanel(canvas.transform, "SettingsPanel", true);
        BuildSettingsBody(settingsPanel.transform, menu.ShowMain);

        // ---- credits panel
        var creditsPanel = MakePanel(canvas.transform, "CreditsPanel", true);

        var ch = MakeText(creditsPanel.transform, "Header", "Credits", 64, Ink);
        Place(ch.rectTransform, 0, 240, 800, 90);
        ch.alignment = TextAlignmentOptions.Center;

        var names = MakeText(creditsPanel.transform, "Names",
            "made for brackeys game jam 2026\n\n" +
            "programming\nzeref  .  jami  .  donags\n\n" +
            "art\nhal\n\n" +
            "sound\nbrommerman", 30, Dim);
        Place(names.rectTransform, 0, 0, 800, 400);
        names.alignment = TextAlignmentOptions.Center;
        names.lineSpacing = 12f;

        var backC = MakeButton(creditsPanel.transform, "BackButton", "Back", 0, -250);
        Hook(backC, menu.ShowMain);

        menu.mainPanel = main;
        menu.settingsPanel = settingsPanel;
        menu.creditsPanel = creditsPanel;

        Selection.activeGameObject = canvas.gameObject;
        Debug.Log("[Menu] title screen built. set Game Scene Name on MainMenuController");
    }

    [MenuItem("Tools/TrustNoOne/Build Pause Menu")]
    public static void BuildPause()
    {
        var canvas = MakeCanvas("PauseCanvas");
        var pause = canvas.gameObject.AddComponent<PauseMenu>();

        var panel = MakePanel(canvas.transform, "PausePanel", true);
        MakeBackdrop(panel.transform, Backdrop);

        var header = MakeText(panel.transform, "Header", "Paused", 72, Ink);
        Place(header.rectTransform, 0, 200, 800, 100);
        header.alignment = TextAlignmentOptions.Center;

        var resume = MakeButton(panel.transform, "ResumeButton", "Resume", 0, 40);
        var settings = MakeButton(panel.transform, "SettingsButton", "Settings", 0, -40);
        var quit = MakeButton(panel.transform, "MenuButton", "Main Menu", 0, -120);

        Hook(resume, pause.Resume);
        Hook(settings, pause.ShowSettings);
        Hook(quit, pause.QuitToMenu);

        var settingsPanel = MakePanel(canvas.transform, "SettingsPanel", true);
        MakeBackdrop(settingsPanel.transform, Backdrop);
        BuildSettingsBody(settingsPanel.transform, pause.ShowPausePanel);

        pause.pausePanel = panel;
        pause.settingsPanel = settingsPanel;

        Selection.activeGameObject = canvas.gameObject;
        Debug.Log("[Menu] pause menu built");
    }

    // ---------- shared settings body

    static void BuildSettingsBody(Transform parent, UnityAction onBack)
    {
        var panel = parent.GetComponent<SettingsPanel>();
        if (panel == null) panel = parent.gameObject.AddComponent<SettingsPanel>();

        var header = MakeText(parent, "Header", "Settings", 64, Ink);
        Place(header.rectTransform, 0, 240, 800, 90);
        header.alignment = TextAlignmentOptions.Center;

        TMP_Text sensVal, brightVal;
        var sens = MakeSlider(parent, "Sensitivity", "Mouse Sensitivity", 0, 80, out sensVal);
        var bright = MakeSlider(parent, "Brightness", "Brightness", 0, -30, out brightVal);

        panel.sensitivitySlider = sens;
        panel.brightnessSlider = bright;
        panel.sensitivityValue = sensVal;
        panel.brightnessValue = brightVal;

        UnityEventTools.AddPersistentListener(sens.onValueChanged,
            new UnityAction<float>(panel.OnSensitivityChanged));
        UnityEventTools.AddPersistentListener(bright.onValueChanged,
            new UnityAction<float>(panel.OnBrightnessChanged));

        var back = MakeButton(parent, "BackButton", "Back", 0, -220);
        Hook(back, onBack);
    }

    // ---------- pieces

    static Canvas MakeCanvas(string name)
    {
        var go = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(go, "Build Menu");

        if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            Undo.RegisterCreatedObjectUndo(es, "Build Menu");
        }

        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        go.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    static GameObject MakePanel(Transform parent, string name, bool startOff)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        Stretch(go.GetComponent<RectTransform>());
        go.SetActive(!startOff);
        return go;
    }

    static void MakeBackdrop(Transform parent, Color colour)
    {
        var go = new GameObject("Backdrop", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        go.transform.SetAsFirstSibling();
        Stretch(go.GetComponent<RectTransform>());

        var img = go.AddComponent<Image>();
        img.color = colour;
        img.raycastTarget = false;
    }

    static TMP_Text MakeText(Transform parent, string name, string content, float size, Color colour)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        var text = go.AddComponent<TextMeshProUGUI>();
        text.text = content;
        text.fontSize = size;
        text.color = colour;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        return text;
    }

    static Button MakeButton(Transform parent, string name, string label, float x, float y)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        Place(go.GetComponent<RectTransform>(), x, y, ButtonW, ButtonH);

        var img = go.AddComponent<Image>();
        img.color = Color.white;   // tint comes from the button colours below

        var button = go.AddComponent<Button>();
        button.targetGraphic = img;

        var colours = button.colors;
        colours.normalColor = ButtonIdle;
        colours.highlightedColor = new Color(1f, 1f, 1f, 0.18f);
        colours.pressedColor = new Color(1f, 1f, 1f, 0.30f);
        colours.selectedColor = ButtonIdle;
        colours.fadeDuration = 0.12f;
        button.colors = colours;

        var text = MakeText(go.transform, "Label", label, 34, Ink);
        Stretch(text.rectTransform);
        text.alignment = TextAlignmentOptions.Center;

        return button;
    }

    static Slider MakeSlider(Transform parent, string name, string label, float x, float y, out TMP_Text valueLabel)
    {
        var holder = new GameObject(name, typeof(RectTransform));
        holder.transform.SetParent(parent, false);
        Place(holder.GetComponent<RectTransform>(), x, y, 700, 90);

        var caption = MakeText(holder.transform, "Caption", label, 30, Dim);
        Place(caption.rectTransform, -110, 28, 460, 44);
        caption.alignment = TextAlignmentOptions.Left;

        valueLabel = MakeText(holder.transform, "Value", "0", 30, Ink);
        Place(valueLabel.rectTransform, 300, 28, 120, 44);
        valueLabel.alignment = TextAlignmentOptions.Right;

        var sliderGo = new GameObject("Slider", typeof(RectTransform));
        sliderGo.transform.SetParent(holder.transform, false);
        Place(sliderGo.GetComponent<RectTransform>(), 0, -18, 700, 26);

        var slider = sliderGo.AddComponent<Slider>();

        var bg = new GameObject("Background", typeof(RectTransform));
        bg.transform.SetParent(sliderGo.transform, false);
        Stretch(bg.GetComponent<RectTransform>());
        var bgImg = bg.AddComponent<Image>();
        bgImg.color = SliderBg;

        var fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(sliderGo.transform, false);
        Stretch(fillArea.GetComponent<RectTransform>());

        var fill = new GameObject("Fill", typeof(RectTransform));
        fill.transform.SetParent(fillArea.transform, false);
        Stretch(fill.GetComponent<RectTransform>());
        var fillImg = fill.AddComponent<Image>();
        fillImg.color = Ink;

        var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
        handleArea.transform.SetParent(sliderGo.transform, false);
        Stretch(handleArea.GetComponent<RectTransform>());

        var handle = new GameObject("Handle", typeof(RectTransform));
        handle.transform.SetParent(handleArea.transform, false);
        var handleRect = handle.GetComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(22, 42);
        var handleImg = handle.AddComponent<Image>();
        handleImg.color = Ink;

        slider.fillRect = fill.GetComponent<RectTransform>();
        slider.handleRect = handleRect;
        slider.targetGraphic = handleImg;
        slider.direction = Slider.Direction.LeftToRight;

        return slider;
    }

    // ---------- layout helpers

    static void Place(RectTransform rect, float x, float y, float w, float h)
    {
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(x, y);
        rect.sizeDelta = new Vector2(w, h);
    }

    static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    static void Hook(Button button, UnityAction action)
    {
        UnityEventTools.AddPersistentListener(button.onClick, action);
    }
}