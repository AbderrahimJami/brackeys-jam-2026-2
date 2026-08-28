using System.Collections;
using UnityEngine;

// goes on the player. brings the actual letter up in front of the camera to read,
// then puts it back where it was
public class NoteReader : MonoBehaviour
{
    public static NoteReader Instance;

    [Header("Empty object parented to the camera, sitting where the letter should float")]
    public Transform holdPoint;

    [Header("Feel")]
    public float moveTime = 0.3f;

    [Tooltip("optional dark fullscreen image so the room recedes")]
    public GameObject dimOverlay;

    [Header("Keys that put it back")]
    public KeyCode closeKey = KeyCode.E;
    public KeyCode altCloseKey = KeyCode.Escape;

    [Tooltip("stops one key press opening and closing it in the same frame")]
    public float minOpenTime = 0.25f;

    public bool IsOpen { get; private set; }

    [Tooltip("light that switches on only while reading")]
    public Light readingLight;

    ReadableInteractables held;
    Transform homeParent;
    Vector3 homePos;
    Quaternion homeRot;
    Vector3 homeScale;
    Collider heldCollider;
    Coroutine moving;
    float openedAt;

    void Awake()
    {
        // only the one with a hold point wired up gets to be the reader
        if (holdPoint == null) return;
        Instance = this;
    }

    void Start()
    {
        if (dimOverlay != null) dimOverlay.SetActive(false);
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
        if (IsOpen || note == null) return;
        if (holdPoint == null)
        {
            Debug.LogWarning("[Note] no hold point set on NoteReader, object is " + gameObject.name);
            return;
        }

        held = note;
        var t = note.transform;

        // remember exactly where it lived so we can put it back
        homeParent = t.parent;
        homePos = t.position;
        homeRot = t.rotation;
        homeScale = t.localScale;

        heldCollider = note.GetComponent<Collider>();
        if (heldCollider != null) heldCollider.enabled = false;

        t.SetParent(holdPoint, true);

        Vector3 targetScale = note.heldScale > 0f
            ? new Vector3(note.heldScale, note.heldScale, note.heldScale)
            : homeScale;

        if (moving != null) StopCoroutine(moving);
        moving = StartCoroutine(MoveTo(t, Vector3.zero, Quaternion.Euler(note.heldRotation), targetScale));

        if (dimOverlay != null) dimOverlay.SetActive(true);
        if (readingLight != null) readingLight.gameObject.SetActive(true);

        IsOpen = true;
        openedAt = Time.time;

        if (PlayerController.Instance != null) PlayerController.Instance.enabled = false;
        if (GameEvents.NoteOpened != null) GameEvents.NoteOpened(note);
    }

    public void Hide()
    {
        if (!IsOpen || held == null) return;

        var t = held.transform;
        t.SetParent(homeParent, true);

        if (moving != null) StopCoroutine(moving);
        moving = StartCoroutine(MoveHome(t));

        if (dimOverlay != null) dimOverlay.SetActive(false);
        if (readingLight != null) readingLight.gameObject.SetActive(false);

        IsOpen = false;

        if (PlayerController.Instance != null) PlayerController.Instance.enabled = true;
        if (GameEvents.NoteClosed != null) GameEvents.NoteClosed();
    }

    IEnumerator MoveTo(Transform t, Vector3 localPos, Quaternion localRot, Vector3 scale)
    {
        Vector3 startPos = t.localPosition;
        Quaternion startRot = t.localRotation;
        Vector3 startScale = t.localScale;

        float elapsed = 0f;
        while (elapsed < moveTime)
        {
            elapsed += Time.deltaTime;
            float k = Mathf.SmoothStep(0f, 1f, elapsed / moveTime);
            t.localPosition = Vector3.Lerp(startPos, localPos, k);
            t.localRotation = Quaternion.Slerp(startRot, localRot, k);
            t.localScale = Vector3.Lerp(startScale, scale, k);
            yield return null;
        }

        t.localPosition = localPos;
        t.localRotation = localRot;
        t.localScale = scale;
        moving = null;
    }

    IEnumerator MoveHome(Transform t)
    {
        Vector3 startPos = t.position;
        Quaternion startRot = t.rotation;
        Vector3 startScale = t.localScale;

        float elapsed = 0f;
        while (elapsed < moveTime)
        {
            elapsed += Time.deltaTime;
            float k = Mathf.SmoothStep(0f, 1f, elapsed / moveTime);
            t.position = Vector3.Lerp(startPos, homePos, k);
            t.rotation = Quaternion.Slerp(startRot, homeRot, k);
            t.localScale = Vector3.Lerp(startScale, homeScale, k);
            yield return null;
        }

        t.position = homePos;
        t.rotation = homeRot;
        t.localScale = homeScale;

        if (heldCollider != null) heldCollider.enabled = true;
        heldCollider = null;
        held = null;
        moving = null;
    }
}