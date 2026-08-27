using UnityEngine;

namespace Interactables.Puzzles.Puzzle3
{
    public class Puzzle3Key : MonoBehaviour, InteractionInterface
    {
        public void Interact(GameObject interactor)
        {
            Destroy(gameObject);
        }
    }
}
