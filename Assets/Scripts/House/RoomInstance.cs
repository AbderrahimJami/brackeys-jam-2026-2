using UnityEngine;

namespace TrustNoOne.Shuffle
{
    // goes on each room prefab. pivot must be the CENTER of the room box
    [RequireComponent(typeof(BoxCollider))]
    public class RoomInstance : MonoBehaviour
    {
        public RoomDefinition definition;

        [Header("Index 0-3 = N/E/S/W at rotation 0")]
        public GameObject[] doorways = new GameObject[4];
        public GameObject[] wallFillers = new GameObject[4];

        public int CurrentRotation { get; private set; }
        public Vector2Int Cell { get; private set; }

        public string Id { get { return gameObject.name; } }

        public RoomTemplate ToTemplate()
        {
            return definition.ToTemplate(Id);
        }

        public void ApplyPlacement(int x, int y, int rotation, float cellSize)
        {
            Cell = new Vector2Int(x, y);
            CurrentRotation = rotation;
            transform.position = new Vector3(x * cellSize, transform.position.y, y * cellSize);
            transform.rotation = Quaternion.Euler(0f, rotation * 90f, 0f);
        }

        // facing is a world direction, map it back to the local socket
        public void SetDoorOpen(Dir facing, bool open)
        {
            int idx = (((int)facing - CurrentRotation) % 4 + 4) % 4;
            if (doorways[idx] != null) doorways[idx].SetActive(open);
            if (wallFillers[idx] != null) wallFillers[idx].SetActive(!open);
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.GetComponentInParent<PlayerController>() == null) return;
            if (HouseShuffleController.Instance != null)
                HouseShuffleController.Instance.SetPlayerRoom(this);
        }

        void OnTriggerExit(Collider other)
        {
            if (other.GetComponentInParent<PlayerController>() == null) return;
            if (HouseShuffleController.Instance != null)
                HouseShuffleController.Instance.NotifyRoomExit(this);
        }
    }
}