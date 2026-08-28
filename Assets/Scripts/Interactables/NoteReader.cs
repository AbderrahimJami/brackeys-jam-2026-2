using UnityEngine;
using TMPro;

// one of these in the scene, on the canvas. owns the panel that shows note text
public class NoteReader : MonoBehaviour
{
    public static NoteReader Instance;

    [Header("Wire these up")]
    public GameObject panel;
    public TMP_Text headingLabel;
    public TMP_Text bodyLabel;

    [Header("Keys that close the note")]
    public KeyCode closeKey = KeyCode.E;
    public KeyCode altCloseKey = KeyCode.Escape;

    [Tooltip("stops the same key press opening and closing it in one frame")]
    public float minOpenTime = 0.25f;

    public bool IsOpen { get; private set; }

    float openedAt;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (panel != null) panel.SetActive(false);
    }

    void Update()
    {
        if (!IsOpen) return;
        if (Time.time - openedAt < minOpenTime) return;

        if (Input.GetKeyDown(closeKey) || Input.GetKeyDown(altCloseKey))
            Hide();
    }

    public void Show(ReadableInteractables note)
    {
        if (note == null) return;

        if (headingLabel != null)
        {
            bool hasHeading = !string.IsNullOrEmpty(note.heading);
            headingLabel.gameObject.SetActive(hasHeading);
            headingLabel.text = note.heading;
        }
        if (bodyLabel != null) bodyLabel.text = note.text;
        if (panel != null) panel.SetActive(true);

        IsOpen = true;
        openedAt = Time.time;

        // stop the player wandering off while reading
        if (PlayerController.Instance != null) PlayerController.Instance.enabled = false;

        if (GameEvents.NoteOpened != null) GameEvents.NoteOpened(note);
    }

    public void Hide()
    {
        if (panel != null) panel.SetActive(false);
        IsOpen = false;

        if (PlayerController.Instance != null) PlayerController.Instance.enabled = true;

        if (GameEvents.NoteClosed != null) GameEvents.NoteClosed();
    }
}