using UnityEngine;

public class ReadableInteractables : MonoBehaviour, InteractionInterface
{
    public void Interact(GameObject interactor)
    {
        GameManager.Instance.setSoundRepresentsTruth(!GameManager.Instance.getSoundRepresentsTruth());
    }

}

