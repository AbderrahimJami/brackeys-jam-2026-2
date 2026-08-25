using System.Collections.Generic;
using UnityEngine;

namespace TrustNoOne.Shuffle
{
    // put this on an empty "House" object, rooms go underneath it as children
    public class HouseShuffleController : MonoBehaviour
    {
        public static HouseShuffleController Instance;

        [Header("Grid")]
        public int gridWidth = 4;
        public int gridHeight = 4;
        public float cellSize = 12f;

        [Header("Shuffle chance per event")]
        [Range(0f, 1f)] public float chanceOnKeyItem = 1f;
        [Range(0f, 1f)] public float chanceOnRoomEnter = 0.4f;
        [Range(0f, 1f)] public float chanceOnLightSwitch = 0.3f;
        [Range(0f, 1f)] public float chanceOnPickup = 0.15f;
        [Range(0f, 1f)] public float chanceOnDoor = 0.25f;
        [Range(0f, 1f)] public float chanceOnOther = 0f;

        [Header("Generation")]
        public int maxAttempts = 500;
        public bool allowRotation = true;
        public bool requireAllRoomsReachable = true;
        public bool logShuffles = true;

        [Tooltip("seconds before another shuffle can fire")]
        public float shuffleCooldown = 1.5f;

        readonly List<RoomInstance> rooms = new List<RoomInstance>();
        HouseLayout layout;
        RoomInstance playerRoom;
        HouseGenerator generator;
        float lastShuffleTime;

        void Awake()
        {
            Instance = this;
            generator = new HouseGenerator();
            rooms.AddRange(GetComponentsInChildren<RoomInstance>());
            CheckIds();
        }

        void OnEnable() { GameEvents.Interacted += HandleInteraction; }
        void OnDisable() { GameEvents.Interacted -= HandleInteraction; }

        void Start()
        {
            var res = generator.Generate(Templates(), null, null, Config());
            if (!res.Success)
                Debug.LogError("[House] initial layout failed: " + res.FailReason + " - check door sockets and forbidden lists");
            Apply(res.Layout);
        }

        void CheckIds()
        {
            var seen = new HashSet<string>();
            foreach (var r in rooms)
            {
                if (r.definition == null)
                    Debug.LogError("[House] room '" + r.Id + "' has no RoomDefinition");
                if (!seen.Add(r.Id))
                    Debug.LogError("[House] duplicate room name '" + r.Id + "' - names are used as ids, make them unique");
            }
        }

        public void SetPlayerRoom(RoomInstance room)
        {
            playerRoom = room;
        }

        // player is now safely inside this room, so it's the one that stays put
        public void NotifyRoomEntered(RoomInstance room)
        {
            if (playerRoom == room) return;
            playerRoom = room;
            GameEvents.Interact(InteractionKind.RoomEnter);
        }

        void HandleInteraction(InteractionKind kind)
        {
            float chance = ChanceFor(kind);
            if (chance <= 0f) return;
            if (Time.time - lastShuffleTime < shuffleCooldown) return;
            if (Random.value > chance) return;
            Shuffle();
        }

        float ChanceFor(InteractionKind kind)
        {
            switch (kind)
            {
                case InteractionKind.KeyItem: return chanceOnKeyItem;
                case InteractionKind.RoomEnter: return chanceOnRoomEnter;
                case InteractionKind.LightSwitch: return chanceOnLightSwitch;
                case InteractionKind.Pickup: return chanceOnPickup;
                case InteractionKind.Door: return chanceOnDoor;
                default: return chanceOnOther;
            }
        }

        [ContextMenu("Force Shuffle")]
        public void Shuffle()
        {
            string playerId = playerRoom != null ? playerRoom.Id : null;
            var res = generator.Generate(Templates(), layout, playerId, Config());

            if (!res.Success)
            {
                if (logShuffles) Debug.LogWarning("[House] shuffle failed, keeping old layout: " + res.FailReason);
                return;
            }

            Apply(res.Layout);
            lastShuffleTime = Time.time;
            if (logShuffles) Debug.Log("[House] shuffled in " + res.Attempts + " attempts");
            if (GameEvents.HouseShuffled != null) GameEvents.HouseShuffled();
        }

        void Apply(HouseLayout newLayout)
        {
            if (newLayout == null) return;
            layout = newLayout;

            // move everything first
            foreach (var placed in layout.Rooms)
            {
                var inst = Find(placed.Template.Id);
                if (inst != null) inst.ApplyPlacement(placed.X, placed.Y, placed.Rotation, cellSize);
            }

            // then doors, they need everyone's final position
            foreach (var placed in layout.Rooms)
            {
                var inst = Find(placed.Template.Id);
                if (inst == null) continue;
                for (int d = 0; d < 4; d++)
                    inst.SetDoorOpen((Dir)d, layout.Connected(placed, (Dir)d));
            }
        }

        RoomInstance Find(string id)
        {
            foreach (var r in rooms)
                if (r.Id == id) return r;
            return null;
        }

        List<RoomTemplate> Templates()
        {
            var list = new List<RoomTemplate>();
            foreach (var r in rooms)
                if (r.definition != null) list.Add(r.ToTemplate());
            return list;
        }

        ShuffleConfig Config()
        {
            return new ShuffleConfig
            {
                Width = gridWidth,
                Height = gridHeight,
                MaxAttempts = maxAttempts,
                AllowRotation = allowRotation,
                RequireAllRoomsReachable = requireAllRoomsReachable
            };
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            for (int x = 0; x < gridWidth; x++)
                for (int y = 0; y < gridHeight; y++)
                    Gizmos.DrawWireCube(new Vector3(x * cellSize, 0f, y * cellSize), new Vector3(cellSize, 0.1f, cellSize));
        }
    }
}