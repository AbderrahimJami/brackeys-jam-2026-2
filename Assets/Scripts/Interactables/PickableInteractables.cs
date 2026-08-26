using UnityEngine;

public class PickableInteractables : MonoBehaviour, InteractionInterface
{
    [SerializeField]
    public AudioClip soundToPlayOnInterect = null;

    public void Interact(GameObject interactor)
    {
        Inventory inventory = PlayerController.Instance.getInventory();
        inventory.addInventory(interactor);
        gameObject.SetActive(false);

        if (soundToPlayOnInterect != null)
        {
            // Play audio
        }

    }
}

