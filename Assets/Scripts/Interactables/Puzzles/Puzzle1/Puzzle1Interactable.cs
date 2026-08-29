using System;
using System.Collections;
using UnityEngine;
using FMOD.Studio;
using FMODUnity;


namespace Interactables
{
    public class Puzzle1Interactable : MonoBehaviour, InteractionInterface
    {
        [SerializeField] private float duration = 0.5f;
        private bool _isRotating;


        [Header("FMOD")]
        [SerializeField] private EventReference rotateItemEvent;

        public void Interact(GameObject interactor)
        {
            if (!_isRotating)
            {
                StartCoroutine(nameof(RotateRoutine));
                PlayRotateSound();
            }
        }

        private IEnumerator RotateRoutine()
        {
            _isRotating = true;
            var startRotation = transform.localRotation;
            var targetRotation = Quaternion.Euler(0, 45, 0) * startRotation;
            var elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                var t = elapsedTime / duration;
                var smoothT = Mathf.SmoothStep(0, 1, t);
                transform.localRotation = Quaternion.Slerp(startRotation, targetRotation, smoothT);
                yield return null;
            }

            transform.localRotation = targetRotation;
            OnRotate?.Invoke();
            _isRotating = false;
        }

        void PlayRotateSound()
        {
            EventInstance instance = RuntimeManager.CreateInstance(rotateItemEvent);
            instance.set3DAttributes(RuntimeUtils.To3DAttributes(transform));
            instance.start();
            instance.release();
        }

        public event Action OnRotate;
    }
}