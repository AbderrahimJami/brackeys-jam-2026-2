using UnityEngine;

namespace Core.Scripts.GameState
{
    public class GameStateBase : MonoBehaviour
    {
        public virtual void OnStateBegin()
        {
        }


        public virtual void OnStateEnd()
        {
        }

        public virtual void OnStateUpdate()
        {
            // Debug.Log("MainMenu State Update here!!");
        }
    }
}