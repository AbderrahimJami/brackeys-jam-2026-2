using UnityEngine;

public class PickableInteractables : MonoBehaviour, InteractionInterface
{

    public void Interact(GameObject interactor)
    {
        Inventory inventory = PlayerController.Instance.getInventory();
        inventory.addInventory(interactor);
        gameObject.SetActive(false);
    }
}

