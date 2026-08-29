using FMOD.Studio;
using FMODUnity;
using UnityEngine;

namespace TrustNoOne.Shuffle
{
    // one of the three doors in the safe room. only one of them is the way out
    public class ExitDoor : MonoBehaviour, InteractionInterface
    {

        [Header("FMOD")]
        [SerializeField] private EventReference doorFailEvent;
        [SerializeField] private EventReference doorOpenEvent;

        [Tooltip("0, 1 or 2. must match its slot on EndgameController")]
        public int index;

       

        public void Interact(GameObject interactor)
        {
            Debug.Log("[Endgame] interacted with exit door " + index);
            if (EndgameController.Instance != null)
                EndgameController.Instance.TryExit(this);

            PlayDoorFailSound();

        }

        void PlayDoorFailSound()
        {
            EventInstance instance = RuntimeManager.CreateInstance(doorFailEvent);
            instance.set3DAttributes(RuntimeUtils.To3DAttributes(transform));
            instance.start();
            instance.release();
        }

        void PlayDoorOpenSound()
        {
            EventInstance instance = RuntimeManager.CreateInstance(doorOpenEvent);
            instance.set3DAttributes(RuntimeUtils.To3DAttributes(transform));
            instance.start();
            instance.release();
        }
    }
}