using UnityEngine;

namespace Interactables
{
    public class Puzzle1Key : MonoBehaviour, InteractionInterface
    {
        private MeshRenderer _meshRenderer;
        private Collider _collider;
        [SerializeField] private GameObject watermelonSlice;
        [SerializeField] private GameObject fork;
        [SerializeField] private GameObject iceCream;
        [SerializeField] private Quaternion watermelonSliceRot;
        [SerializeField] private Quaternion forkRot;
        [SerializeField] private Quaternion iceCreamRot;
        [SerializeField] private float rotationTolerance = 1f;

        private void Start()
        {
            _collider = GetComponent<Collider>();
            _meshRenderer = GetComponent<MeshRenderer>();
            _meshRenderer.enabled = false;
            _collider.enabled = false;
            watermelonSlice.GetComponent<Puzzle1Interactable>().OnRotate += EvaluatePuzzleState;
            fork.GetComponent<Puzzle1Interactable>().OnRotate += EvaluatePuzzleState;
            iceCream.GetComponent<Puzzle1Interactable>().OnRotate += EvaluatePuzzleState;
        }

        private void EvaluatePuzzleState()
        {
            if (IsAtTarget(watermelonSliceRot, watermelonSlice.transform) &&
                IsAtTarget(iceCreamRot, iceCream.transform) && IsAtTarget(forkRot, fork.transform))
            {
                Debug.Log("FOUND THE COMBINATION");
                _meshRenderer.enabled = true;
                _collider.enabled = true;

            }

            Debug.Log(
                $"Watermelon local rot is {watermelonSlice.transform.localRotation.eulerAngles.ToString()} " +
                $"Ice Cream is: {iceCream.transform.localRotation.eulerAngles.ToString()} " +
                $"and fork is {fork.transform.localRotation.eulerAngles.ToString()}");
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