using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core.Scripts.GameState
{
    public class MainMenuState : GameStateBase
    {
        private string _sceneName = "MainMenu";

        private void Start()
        {
        }

        private void Temporary()
        {
            Game.Instance?.TransitionToState(Game.Instance?.PlayingState);
        }

        public override void OnStateBegin()
        {
            Debug.Log("MainMenu State Begin here!!");
            if (SceneManager.GetActiveScene().name == "MainMenu")
            {
                // Calls "MyFunction" after 3 seconds
                Invoke(nameof(Temporary), 3f);
                return;
            }
            SceneManager.LoadScene(_sceneName);

        }

        public override void OnStateEnd()
        {
            Debug.Log("MainMenu State End here!!");
        }

        public override void OnStateUpdate()
        {
            base.OnStateUpdate();
        }
    }
}