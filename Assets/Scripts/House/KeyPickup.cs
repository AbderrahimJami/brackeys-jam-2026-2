using UnityEngine;

namespace TrustNoOne.Shuffle
{
    public class KeyPickup : MonoBehaviour, InteractionInterface
    {
        public void Interact(GameObject interactor)
        {
            if (EndgameController.Instance != null)
                EndgameController.Instance.AddKey();

            Debug.Log("Setting to FALSE on PICKUP");
            gameObject.SetActive(false);
        }
    }
}