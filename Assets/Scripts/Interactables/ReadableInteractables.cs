using UnityEngine;

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

    public void Interact(GameObject interactor)
    {
        if (HasBeenRead && !rereadable) return;
        if (NoteReader.Instance == null)
        {
            Debug.LogWarning("[Note] no NoteReader on the player, can't hold this up");
            return;
        }

        HasBeenRead = true;
        NoteReader.Instance.Show(this);
    }
}