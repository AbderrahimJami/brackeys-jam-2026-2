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

        [Header("Progression")]
        [Tooltip("keys the player holds. locked doors open as this goes up")]
        public int playerKeys = 0;

        [Header("Generation")]
        public int maxAttempts = 3000;
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
        int roomsOccupied;   // how many room triggers the player is inside right now
        bool shufflePending; // asked to shuffle while in a doorway, run it once clear

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
            roomsOccupied++;
            if (playerRoom == room) return;
            playerRoom = room;
            GameEvents.Interact(InteractionKind.RoomEnter);
        }

        public void NotifyRoomLeft(RoomInstance room)
        {
            roomsOccupied--;
            if (roomsOccupied < 0) roomsOccupied = 0;

            // fully inside one room again, so run whatever we held back
            if (shufflePending && roomsOccupied == 1)
            {
                shufflePending = false;
                Shuffle();
            }
        }

        // call this when a key is picked up. reopens doors without moving anything
        public void SetPlayerKeys(int keys)
        {
            playerKeys = keys;
            Apply(layout);
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

        // prints the map + where the player is. use this when something looks wrong
        [ContextMenu("Log Layout")]
        public void LogLayout()
        {
            Debug.Log("[House] player in: " + (playerRoom != null ? playerRoom.Id : "NONE")
                + ", keys: " + playerKeys + "\n" + HouseGenerator.Print(layout));
        }

        [ContextMenu("Force Shuffle")]
        public void Shuffle()
        {
            // if the player straddles two rooms (a doorway) a shuffle can slide a room
            // onto them and seal them in. hold it until they're clear
            if (playerRoom != null && roomsOccupied != 1)
            {
                shufflePending = true;
                return;
            }

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
                {
                    int lockLevel = layout.ConnectionLock(placed, (Dir)d);
                    DoorState st;
                    if (lockLevel < 0) st = DoorState.Wall;
                    else if (lockLevel > playerKeys) st = DoorState.Locked;
                    else st = DoorState.Open;
                    inst.SetDoorState((Dir)d, st);
                }
            }

            WarnIfPlayerSealed();
        }

        // the validator should make this impossible, so shout loudly if it happens
        void WarnIfPlayerSealed()
        {
            if (playerRoom == null || layout == null) return;
            var placed = layout.ById(playerRoom.Id);
            if (placed == null) return;

            for (int d = 0; d < 4; d++)
            {
                int lockLevel = layout.ConnectionLock(placed, (Dir)d);
                if (lockLevel >= 0 && lockLevel <= playerKeys) return;
            }
            Debug.LogError("[House] player is sealed in " + playerRoom.Id + " with " + playerKeys + " keys");
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
                RequireAllRoomsReachable = requireAllRoomsReachable,
                PlayerKeys = playerKeys
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