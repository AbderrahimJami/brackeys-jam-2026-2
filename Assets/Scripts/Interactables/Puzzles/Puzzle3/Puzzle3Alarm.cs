using TMPro;
using UnityEngine;

namespace Interactables.Puzzles.Puzzle3
{
    public class Puzzle3Alarm : MonoBehaviour
    {
        [Header("Puzzle References")]
        [SerializeField] private Puzzle3Clock workingClock;
        [SerializeField] private Puzzle3CompartmentDoor compartmentDoor;

        [Header("Display")]
        [SerializeField] private TextMeshPro hourText;
        [SerializeField] private TextMeshPro minuteText;

        [Header("Dial Settings")]
        [SerializeField] private int minuteStep = 5;

        private int _dialedHour = 12;
        private int _dialedMinute;
        private bool _isSolved;

        private void Start()
        {
            UpdateDisplay();
        }

        public void AdjustHour(int delta)
        {
            if (_isSolved) return;
            _dialedHour = ((_dialedHour - 1 + delta) % 12 + 12) % 12 + 1;
            UpdateDisplay();
        }

        public void AdjustMinute(int delta)
        {
            if (_isSolved) return;
            _dialedMinute = ((_dialedMinute + delta * minuteStep) % 60 + 60) % 60;
            UpdateDisplay();
        }

        public void Submit()
        {
            if (_isSolved || workingClock == null) return;

            var correct = _dialedHour == workingClock.CurrentHour &&
                          _dialedMinute == workingClock.CurrentMinute;

            if (!correct)
            {
                Debug.Log($"Incorrect time entered: {_dialedHour:00}:{_dialedMinute:00}");
                return;
            }

            _isSolved = true;
            compartmentDoor.Open();
        }

        private void UpdateDisplay()
        {
            if (hourText != null) hourText.text = _dialedHour.ToString("00");
            if (minuteText != null) minuteText.text = _dialedMinute.ToString("00");
        }
    }
}
