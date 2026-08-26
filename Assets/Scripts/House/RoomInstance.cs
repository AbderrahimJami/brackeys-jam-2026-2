using UnityEngine;
using UnityEngine.Rendering;

namespace TrustNoOne.Shuffle
{
    // goes on each room prefab. pivot must be the CENTER of the room box
    [RequireComponent(typeof(BoxCollider))]
    public class RoomInstance : MonoBehaviour
    {
        public RoomDefinition definition;

        [Header("Index 0-3 = N/E/S/W at rotation 0")]
        public GameObject[] doorways = new GameObject[4];
        //[HideInInspector]
        public GameObject[] wallFillers = new GameObject[4];
        //[HideInInspector]
        public GameObject[] lockedDoors = new GameObject[4];

        public int CurrentRotation { get; private set; }
        public Vector2Int Cell { get; private set; }

        public string Id { get { return gameObject.name; } }

        private void Awake()
        {

            //wallFillers = new GameObject[doorways.Length];
            //lockedDoors = new GameObject[lockedDoors.Length];

            //for (int i = 0; i < doorways.Length; i++)
            //{


            //    wallFillers[i] = null;
            //    lockedDoors[i] = null;

            //    if (doorways[i] == null) continue;

            //    Transform parent = doorways[i].gameObject.transform.parent;

            //    Transform wall = parent.Find("Wall");
            //    Transform lockedDoor = parent.Find("LockedDoor");

            //    if (wall == null)
            //    {
            //        Debug.LogError("Wall cannot be NULL when Doorway is present - GameObect = " + parent.parent.name);
            //    }


            //    if (lockedDoor == null)
            //    {
            //        Debug.LogError("lockedDoor cannot be NULL when Doorway is present - GameObect = " + parent.parent.name);
            //    }


            //    wallFillers[i] = wall.gameObject;
            //    lockedDoors[i] = lockedDoor.gameObject;

            //}    
        }

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
        public void SetDoorState(Dir facing, DoorState state)
        {
            int idx = RoomTemplate.LocalIndex(facing, CurrentRotation);
            if (wallFillers[idx] != null) wallFillers[idx].SetActive(state == DoorState.Wall);
            if (doorways[idx] != null) doorways[idx].SetActive(state != DoorState.Wall);
            if (lockedDoors[idx] != null) lockedDoors[idx].SetActive(state == DoorState.Locked);
        }

        void OnTriggerEnter(Collider other)
        {
            if (!IsPlayer(other)) return;
            if (HouseShuffleController.Instance != null)
                HouseShuffleController.Instance.NotifyRoomEntered(this);
        }

        void OnTriggerExit(Collider other)
        {
            if (!IsPlayer(other)) return;
            if (HouseShuffleController.Instance != null)
                HouseShuffleController.Instance.NotifyRoomLeft(this);
        }

        static bool IsPlayer(Collider c)
        {
            return c.GetComponentInParent<PlayerController>() != null;
        }
    }
}