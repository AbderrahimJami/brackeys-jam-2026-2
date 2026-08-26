using UnityEngine;

namespace TrustNoOne.Shuffle
{
    // one of the three doors in the safe room. only one of them is the way out
    public class ExitDoor : MonoBehaviour, InteractionInterface
    {
        [Tooltip("0, 1 or 2. must match its slot on EndgameController")]
        public int index;

        public void Interact(GameObject interactor)
        {
            Debug.Log("[Endgame] interacted with exit door " + index);
            if (EndgameController.Instance != null)
                EndgameController.Instance.TryExit(this);
        }
    }
}