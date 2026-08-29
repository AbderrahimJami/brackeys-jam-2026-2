using UnityEngine;
using FMOD.Studio;
using FMODUnity;

namespace TrustNoOne.Shuffle
{
    public class KeyPickup : MonoBehaviour, InteractionInterface
    {
        [Header("FMOD")]
        [SerializeField] private EventReference takeKeyEvent;
        public void Interact(GameObject interactor)
        {
            if (EndgameController.Instance != null)
                EndgameController.Instance.AddKey();

            PlayTakeKeySound();

            Debug.Log("Setting to FALSE on PICKUP");
            gameObject.SetActive(false);
        }
        void PlayTakeKeySound()
        {
            EventInstance instance = RuntimeManager.CreateInstance(takeKeyEvent);
            instance.set3DAttributes(RuntimeUtils.To3DAttributes(transform));
            instance.start();
            instance.release();
        }
    }
}