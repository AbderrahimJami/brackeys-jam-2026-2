using UnityEngine;
using UnityEngine.SceneManagement;

namespace TrustNoOne.Shuffle
{
    // owns keys, the three exits, and how the run ends
    public class EndgameController : MonoBehaviour
    {
        public static EndgameController Instance;

        [Header("Progression")]
        public int keysNeeded = 3;

        [Header("Exits")]
        public ExitDoor[] exitDoors;

        [Tooltip("-1 picks a random true exit each run")]
        public int trueExitIndex = -1;

        [Header("Screens, leave them off in the scene")]
        public GameObject winScreen;
        public GameObject loseScreen;

        public KeyCode restartKey = KeyCode.R;

        public int KeysCollected { get; private set; }
        public bool RunOver { get; private set; }

        void Awake()
        {
            Instance = this;
            if (trueExitIndex < 0 && exitDoors != null && exitDoors.Length > 0)
                trueExitIndex = Random.Range(0, exitDoors.Length);
        }

        void Start()
        {
            if (winScreen != null) winScreen.SetActive(false);
            if (loseScreen != null) loseScreen.SetActive(false);
            Debug.Log("[Endgame] true exit is door " + trueExitIndex);
        }

        void Update()
        {
            if (RunOver && Input.GetKeyDown(restartKey)) Restart();
        }

        public void AddKey()
        {
            KeysCollected++;
            Debug.Log("[Endgame] keys: " + KeysCollected + "/" + keysNeeded);

            // open the locked doors first, then let the shuffle run on the new count
            if (HouseShuffleController.Instance != null)
                HouseShuffleController.Instance.SetPlayerKeys(KeysCollected);

            if (GameEvents.KeysChanged != null) GameEvents.KeysChanged(KeysCollected);
            GameEvents.Interact(InteractionKind.KeyItem);
        }

        public void TryExit(ExitDoor door)
        {
            if (RunOver) return;

            if (KeysCollected < keysNeeded)
            {
                int short_ = keysNeeded - KeysCollected;
                Debug.Log("[Endgame] exit locked, still need " + short_ + " key(s)");
                if (GameEvents.ExitRefused != null) GameEvents.ExitRefused(short_);
                return;
            }

            End(door.index == trueExitIndex);
        }

        void End(bool won)
        {
            RunOver = true;

            if (won && winScreen != null) winScreen.SetActive(true);
            if (!won && loseScreen != null) loseScreen.SetActive(true);

            if (PlayerController.Instance != null) PlayerController.Instance.enabled = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            Debug.Log(won ? "[Endgame] escaped. press " + restartKey + " to restart"
                          : "[Endgame] wrong door. press " + restartKey + " to restart");

            if (GameEvents.RunEnded != null) GameEvents.RunEnded(won);
        }

        [ContextMenu("Restart")]
        public void Restart()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}