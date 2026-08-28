using UnityEngine;
using FMODUnity;
using FMOD.Studio;

// a note, letter or scrap of paper the player can read.
// needs a collider on the Interactable layer
public class ReadableInteractables : MonoBehaviour, InteractionInterface
{
    [TextArea(3, 10)]
    public string text = "Take your pills at 10:00";

    [Tooltip("optional, shown above the body text")]
    public string heading;

    [Tooltip("off if the player should only be able to read it once")]
    public bool rereadable = true;

    public bool HasBeenRead { get; private set; }

    [Header("FMOD")]
    [SerializeField] private EventReference manSayEvent;

    public void Interact(GameObject interactor)
    {
        if (HasBeenRead && !rereadable) return;

        if (NoteReader.Instance == null)
        {
            Debug.LogWarning("[Note] no NoteReader in the scene, nothing to show it on");
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