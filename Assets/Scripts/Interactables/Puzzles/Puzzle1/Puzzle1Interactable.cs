using System;
using System.Collections;
using UnityEngine;

namespace Interactables
{
    public class Puzzle1Interactable : MonoBehaviour, InteractionInterface
    {
        [SerializeField] private float duration = 0.5f;
        private bool _isRotating;

        public void Interact(GameObject interactor)
        {
            if (!_isRotating)
            {
                StartCoroutine(nameof(RotateRoutine));
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

        public event Action OnRotate;
    }
}