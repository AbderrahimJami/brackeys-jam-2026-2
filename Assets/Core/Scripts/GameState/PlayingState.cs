using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core.Scripts.GameState
{
    public class PlayingState : GameStateBase
    {
        private string _sceneName = "SampleScene";

        private void Start()
        {

        }

        private void Temporary()
        {
            Game.Instance?.TransitionToState(Game.Instance?.MainMenuState);
        }

        public override void OnStateBegin()
        {
            Debug.Log("Playing State Begin here!!");
            SceneManager.LoadScene(_sceneName);
            Invoke(nameof(Temporary), 3f);


        }

        public override void OnStateEnd()
        {
            Debug.Log("Player State End here!!");
        }

        public override void OnStateUpdate()
        {
            base.OnStateUpdate();
        }
    }
}