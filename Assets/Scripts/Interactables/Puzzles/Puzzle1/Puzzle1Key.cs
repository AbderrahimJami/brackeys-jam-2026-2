using Interactables.Puzzles.Puzzle3;
using UnityEngine;

namespace Interactables
{
    public class Puzzle1Key : MonoBehaviour, InteractionInterface
    {
        [SerializeField] private GameObject item1;
        [SerializeField] private GameObject item2;
        [SerializeField] private GameObject item3;
        [SerializeField] private Quaternion item1Rot;
        [SerializeField] private Quaternion item2Rot;
        [SerializeField] private Quaternion item3Rot;
        [SerializeField] private float rotationTolerance = 1f;
        [SerializeField] private Puzzle3CompartmentDoor compartmentDoor;

        
        private bool _isSolved;

        private void Start()
        {
            item1.GetComponent<Puzzle1Interactable>().OnRotate += EvaluatePuzzleState;
            item2.GetComponent<Puzzle1Interactable>().OnRotate += EvaluatePuzzleState;
            item3.GetComponent<Puzzle1Interactable>().OnRotate += EvaluatePuzzleState;
        }

        private void EvaluatePuzzleState()
        {
            if (IsAtTarget(item1Rot, item1.transform) &&
                IsAtTarget(item3Rot, item3.transform) && IsAtTarget(item2Rot, item2.transform) && !_isSolved)
            {
                Debug.Log("FOUND THE COMBINATION");
                item1.GetComponent<Puzzle1Interactable>().OnRotate -= EvaluatePuzzleState;
                item2.GetComponent<Puzzle1Interactable>().OnRotate -= EvaluatePuzzleState;
                item3.GetComponent<Puzzle1Interactable>().OnRotate -= EvaluatePuzzleState;
                compartmentDoor.Open();
            }

            Debug.Log(
                $"Watermelon local rot is {item1.transform.localRotation.eulerAngles.ToString()} " +
                $"Ice Cream is: {item3.transform.localRotation.eulerAngles.ToString()} " +
                $"and fork is {item2.transform.localRotation.eulerAngles.ToString()}");
        }

        private bool IsAtTarget(Quaternion target, Transform t) =>
            Quaternion.Angle(target, t.localRotation) < rotationTolerance;


        public void Interact(GameObject interactor)
        {
            Debug.Log("Key Picked Up Now!");
            Destroy(gameObject);
        }
    }
}