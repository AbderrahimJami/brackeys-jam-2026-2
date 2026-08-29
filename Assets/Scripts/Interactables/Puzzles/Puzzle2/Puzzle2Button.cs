using FMOD.Studio;
using FMODUnity;
using Interactables.Puzzles.Puzzle3;
using System;
using System.Collections;
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
        [SerializeField] private float pressDepth = 0.02f;
        [SerializeField] private float pressDuration = 0.1f;

        [Header("FMOD")]
        [SerializeField] private EventReference pressButtonEvent;
        
        [SerializeField] private PushButtonDirection buttonPushDirection = PushButtonDirection.X;

        [SerializeField] private Puzzle2Key key;
        private bool _isSolved;
        private bool _isPressing;

        public void Interact(GameObject interactor)
        {
            if (_isSolved) return;

            if (isCorrect)
            {
                _isSolved = true;

                
            }
            else
            {
                Invoke(nameof(Teleport), 1f);
            }
            if (!_isPressing)
                StartCoroutine(PressRoutine());

            PlayButtonSound();

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

            if (_isSolved)
            {
                // correction solution
                key.puzzleSolved();
            }
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
        void PlayButtonSound()
        {
            EventInstance instance = RuntimeManager.CreateInstance(pressButtonEvent);
            instance.set3DAttributes(RuntimeUtils.To3DAttributes(transform));
            instance.start();
            instance.release();
        }
    }
}
    
