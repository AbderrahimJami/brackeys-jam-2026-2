using System.Collections.Generic;
using UnityEngine;

namespace TrustNoOne.Shuffle
{
    public enum RoomType { SafeRoom, Hallway, Bedroom, Kitchen, Bathroom, LivingRoom, DiningRoom, Study, Basement }

    [CreateAssetMenu(fileName = "Room", menuName = "TrustNoOne/Room Definition")]
    public class RoomDefinition : ScriptableObject
    {
        public RoomType roomType;

        [Header("Doors at rotation 0")]
        public bool doorNorth;
        public bool doorEast;
        public bool doorSouth;
        public bool doorWest;

        [Header("Rules")]
        public List<RoomType> forbiddenNeighbours = new List<RoomType>();

        [Tooltip("never moves. safe room should be on")]
        public bool anchored;
        public Vector2Int fixedCell;

        public bool allowRotation = true;

        [Tooltip("must always be reachable after a shuffle")]
        public bool required = true;

        public RoomTemplate ToTemplate(string id)
        {
            var t = new RoomTemplate();
            t.Id = id;
            t.Type = roomType.ToString();
            t.Doors[0] = doorNorth;
            t.Doors[1] = doorEast;
            t.Doors[2] = doorSouth;
            t.Doors[3] = doorWest;
            foreach (var f in forbiddenNeighbours) t.ForbiddenNeighbours.Add(f.ToString());
            t.Anchored = anchored;
            t.FixedX = fixedCell.x;
            t.FixedY = fixedCell.y;
            t.AllowRotation = allowRotation;
            t.Required = required;
            return t;
        }
    }
}