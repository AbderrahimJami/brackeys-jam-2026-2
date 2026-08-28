using UnityEngine;

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

    public void Interact(GameObject interactor)
    {
        if (HasBeenRead && !rereadable) return;

        if (NoteReader.Instance == null)
        {
            Debug.LogWarning("[Note] no NoteReader in the scene, nothing to show it on");
            return;
        }

        HasBeenRead = true;
        NoteReader.Instance.Show(this);
    }
}