using UnityEngine;
using FMODUnity;
using FMOD.Studio;

// a note or letter the player can pick up and read up close.
// needs a collider on the Interactable layer
public class ReadableInteractables : MonoBehaviour, InteractionInterface
{
    [Header("How it sits when held up")]
    [Tooltip("extra rotation so the written side faces the player")]
    public Vector3 heldRotation = Vector3.zero;

    [Tooltip("0 = keep its normal size")]
    public float heldScale = 0f;

    [Header("Optional")]
    [Tooltip("off if the player should only read it once")]
    public bool rereadable = true;

    public bool HasBeenRead { get; private set; }

    [Header("FMOD")]
    [SerializeField] private EventReference manSayEvent;

    public void Interact(GameObject interactor)
    {
        Debug.Log("[Note] interact on " + gameObject.name);
        if (HasBeenRead && !rereadable) return;
        if (NoteReader.Instance == null)
        {
            Debug.LogWarning("[Note] no NoteReader on the player, can't hold this up");
            return;
        }

        PlayManSay();

        HasBeenRead = true;
        NoteReader.Instance.Show(this);
    }
    void PlayManSay()
    {
        RuntimeManager.StudioSystem.setParameterByNameWithLabel("SayCategory", "Read");
        EventInstance instance = RuntimeManager.CreateInstance(manSayEvent);
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(transform));
        instance.start();
        instance.release();
    }
}