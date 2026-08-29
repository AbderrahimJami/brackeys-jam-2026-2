using UnityEngine;
using Random = UnityEngine.Random;

namespace Interactables.Puzzles.Puzzle3
{
    public class Puzzle3Clock : MonoBehaviour
    {
        [Header("Clock Hands")]
        [SerializeField] private GameObject minuteHand;
        [SerializeField] private GameObject hourHand;
        [SerializeField] private GameObject secondHand;

        [Header("Display Settings")]
        [Tooltip("The fixed time this clock displays for the whole puzzle.")]
        [Range(1, 12)] [SerializeField] private int startHour = 12;
        [Range(0, 55)] [SerializeField] private int startMinute;
        [SerializeField] private bool setRandomStartTime;

        [Header("Puzzle Role")]
        [Tooltip("Check ONLY on the one functional clock. Only its second hand moves.")]
        [SerializeField] private bool isWorkingClock;
        [SerializeField] private float secondHandSpeed = 6f;

        private int _currentHour;
        private int _currentMinute;

        public int CurrentHour => _currentHour;
        public int CurrentMinute => _currentMinute;
        public bool IsWorkingClock => isWorkingClock;

        private void Start()
        {
            if (setRandomStartTime)
            {
                startHour = Random.Range(1, 13);
                startMinute = Random.Range(0, 12) * 5;
            }

            _currentHour = startHour;
            _currentMinute = startMinute;
            SetHandsRotation();
        }

        private void Update()
        {
            if (!isWorkingClock || secondHand == null) return;
            secondHand.transform.Rotate(0f, 0f, secondHandSpeed * Time.deltaTime, Space.Self);
        }

        private void SetHandsRotation()
        {
            var minuteAngle = _currentMinute * 6f;
            var hourAngle = (_currentHour % 12) * 30f;
            minuteHand.transform.localRotation = Quaternion.Euler(0, minuteHand.transform.localRotation.y, minuteAngle);
            hourHand.transform.localRotation = Quaternion.Euler(0, minuteHand.transform.localRotation.y, hourAngle);
        }
    }
}
