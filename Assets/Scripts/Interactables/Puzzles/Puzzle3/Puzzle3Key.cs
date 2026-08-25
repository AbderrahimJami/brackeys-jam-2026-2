using System;
using UnityEngine;

namespace Interactables.Puzzles.Puzzle3
{
    public class Puzzle3Key : MonoBehaviour, InteractionInterface
    {
        [SerializeField] private GameObject workingClockPrefab;

        private void Start()
        {
            throw new NotImplementedException();
        }

        public void Interact(GameObject interactor)
        {
            Debug.Log("Hello World");
        }
    }
}