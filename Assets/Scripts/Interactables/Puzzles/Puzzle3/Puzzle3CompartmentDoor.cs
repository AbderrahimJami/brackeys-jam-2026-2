using System.Collections;
using UnityEngine;

namespace Interactables.Puzzles.Puzzle3
{
    public class Puzzle3CompartmentDoor : MonoBehaviour
    {
        [SerializeField] private float openAngle = 90f;
        [SerializeField] private float duration = 1f;

        private bool _isOpen;

        public void Open()
        {
            if (_isOpen) return;
            _isOpen = true;
            StartCoroutine(OpenRoutine());
        }

        private IEnumerator OpenRoutine()
        {
            var startRotation = transform.localRotation;
            var targetRotation = startRotation * Quaternion.Euler(0, openAngle, 0);
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
        }
    }
}
