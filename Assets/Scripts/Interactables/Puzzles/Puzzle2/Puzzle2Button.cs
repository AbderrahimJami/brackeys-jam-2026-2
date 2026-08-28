using System;
using System.Collections;
using Interactables.Puzzles.Puzzle3;
using UnityEngine;

namespace Interactables.Puzzles.Puzzle2
{
    
    public class Puzzle2Button : MonoBehaviour, InteractionInterface
    {
        
        private enum PushButtonDirection
        {
            X,
            Y,
            Z
        }
        
        
        [SerializeField] private bool isCorrect;
        [SerializeField] private Puzzle2Button[] otherButtons;
        [SerializeField] private Transform punishmentLocation;
        [SerializeField] private Puzzle3CompartmentDoor compartmentDoor;
        [SerializeField] private float pressDepth = 0.02f;
        [SerializeField] private float pressDuration = 0.1f;
        [SerializeField] private PushButtonDirection buttonPushDirection = PushButtonDirection.X;
        private bool _isSolved;
        private bool _isPressing;

        public void Interact(GameObject interactor)
        {
            if (_isSolved) return;

            if (isCorrect)
            {
                _isSolved = true;
                compartmentDoor.Open();

                foreach (var other in otherButtons)
                    other.SetSolved();
            }
            else
            {
                Invoke(nameof(Teleport), 1f);
            }
            if (!_isPressing)
                StartCoroutine(PressRoutine());

        }

        private void Teleport()
        {
            PlayerController.Instance.TeleportToSafeRoom();
        }

        public void SetSolved()
        {
            _isSolved = true;
        }
        
        
        private IEnumerator PressRoutine()
        {
            _isPressing = true;
            var restPos = transform.position;
            Vector3 direction;
            switch (buttonPushDirection)
            {
                case PushButtonDirection.X:
                    direction = transform.right;
                    break;
                case PushButtonDirection.Y:
                    direction = transform.up;
                    break;
                case PushButtonDirection.Z:
                    direction = transform.forward;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            var pressedPos = restPos - direction * pressDepth;

            yield return MoveOverTime(restPos, pressedPos, pressDuration);
            yield return MoveOverTime(pressedPos, restPos, pressDuration);

            _isPressing = false;
        }

        private IEnumerator MoveOverTime(Vector3 from, Vector3 to, float time)
        {
            var elapsedTime = 0f;
            while (elapsedTime < time)
            {
                elapsedTime += Time.deltaTime;
                transform.position = Vector3.Lerp(from, to, elapsedTime / time);
                yield return null;
            }

            transform.position = to;
        }
        
    }
}
