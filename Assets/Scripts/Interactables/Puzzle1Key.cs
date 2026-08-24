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
            if (watermelonSliceRot == watermelonSlice.transform.rotation &&
                iceCreamRot == iceCream.transform.rotation && forkRot == fork.transform.rotation)
            {
                Debug.Log("FOUND THE COMBINATION");
                _meshRenderer.enabled = true;
                _collider.enabled = true;
                
            }

            Debug.Log(
                $"Watermelon rot is {watermelonSlice.transform.rotation.eulerAngles.ToString()} " +
                $"Ice Cream is: {iceCream.transform.rotation.eulerAngles.ToString()} " +
                $"and fork is {fork.transform.rotation.eulerAngles.ToString()}");
        }


        public void Interact(GameObject interactor)
        {
            Debug.Log("Key Picked Up Now!");
            Destroy(gameObject);
        }
    }
}