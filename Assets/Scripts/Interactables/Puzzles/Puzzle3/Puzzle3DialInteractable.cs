using System.Collections;
using UnityEngine;

namespace Interactables.Puzzles.Puzzle3
{
    public enum DialAction
    {
        HourUp,
        HourDown,
        MinuteUp,
        MinuteDown,
        Confirm
    }

    public class Puzzle3DialInteractable : MonoBehaviour, InteractionInterface
    {
        [SerializeField] private Puzzle3Alarm alarm;
        [SerializeField] private DialAction action;
        [SerializeField] private float pressDepth = 0.02f;
        [SerializeField] private float pressDuration = 0.1f;

        private bool _isPressing;

        public void Interact(GameObject interactor)
        {
            switch (action)
            {
                case DialAction.HourUp:
                    alarm.AdjustHour(1);
                    break;
                case DialAction.HourDown:
                    alarm.AdjustHour(-1);
                    break;
                case DialAction.MinuteUp:
                    alarm.AdjustMinute(1);
                    break;
                case DialAction.MinuteDown:
                    alarm.AdjustMinute(-1);
                    break;
                case DialAction.Confirm:
                    alarm.Submit();
                    break;
            }

            if (!_isPressing)
                StartCoroutine(PressRoutine());
        }

        private IEnumerator PressRoutine()
        {
            _isPressing = true;
            var restPos = transform.position;
            var pressedPos = restPos - transform.up * pressDepth;

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
