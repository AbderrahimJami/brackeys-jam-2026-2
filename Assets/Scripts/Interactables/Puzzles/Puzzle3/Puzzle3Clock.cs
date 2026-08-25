using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Interactables.Puzzles.Puzzle3
{
    public class Puzzle3Clock : MonoBehaviour
    {

        [Header("Clock Hands")] [SerializeField]
        private GameObject minuteHand;

        [SerializeField] private GameObject hourHand;

        [Header("Puzzle Settings")]
        [Tooltip("The time this specific clock displays when the game starts.")]
        [Range(1, 12)] [SerializeField] private int startHour = 12;
        [Range(0, 55)] [SerializeField] private int startMinute;

        [Tooltip("Check this ONLY on the one functional clock that solves the puzzle.")]
        [SerializeField] private bool isWorkingClock;
        [SerializeField] private bool setRandomStartTime;

        [SerializeField] private int targetHour = 3;
        [SerializeField] private int targetMinute = 30;
        [SerializeField] private float timeStepInterval = 1.0f;

        private int _currentHour;
        private int _currentMinute;
        private bool _isSolved;
        private float _timer;

        private void Start()
        {
            if (setRandomStartTime)
            {
                startMinute = Random.Range(1, 13);
                startHour = Random.Range(0, 13) * 5;
            }
            _currentMinute = startMinute / 5 * 5;
            _currentHour = startHour;

            SetHandsRotation();
            Debug.Log($"Clock set to start with this time -> {_currentHour:00}:{_currentHour:00}");

        }

        private void Update()
        {
            if (!isWorkingClock || _isSolved)
                return;
            _timer += Time.deltaTime;
            if (_timer >= timeStepInterval)
            {
                _timer = 0f;
                AdvanceClockTime();
            }
        }


        private void AdvanceClockTime()
        {
            if (_isSolved) return;
            _currentMinute += 5;
            if (_currentMinute >= 60)
            {
                _currentMinute = 0;
                _currentHour++;
                if (_currentHour > 12) _currentHour = 1;
            }

            SetHandsRotation();
        }

        private void SetHandsRotation()
        {
            var minuteAngle = -(_currentMinute * 6f);
            var hourAngle = -(_currentHour * 30f);
            minuteHand.transform.localRotation = Quaternion.Euler(0, 0, -minuteAngle);
            hourHand.transform.localRotation = Quaternion.Euler(0, 0, -hourAngle);
        }

        private void CheckSolutionCondition()
        {
            if (_currentHour == targetHour && _currentMinute == targetMinute) SolvePuzzle();
        }

        private void SolvePuzzle()
        {
            _isSolved = true;
            Debug.Log("Solved! Spawning Key...");
        }
    }
}