using UnityEngine;
using UnityEditor;

namespace TrustNoOne.Shuffle
{
    // Tools > TrustNoOne > Build Test House
    public class TestHouseBuilder
    {
        const float CellSize = 12f;
        const float WallHeight = 3f;
        const float WallThick = 0.3f;
        const float DoorWidth = 3f;
        const string DefFolder = "Assets/ScriptableObjects/Rooms";

        struct Spec
        {
            public string name;
            public RoomType type;
            public bool n, e, s, w;
            public bool anchored;
            public int fx, fy;
            public int minKeys;
            public int[] locks;
            public RoomType[] forbidden;
        }

        static Spec S(string name, RoomType type, string doors, bool anchored = false, int fx = 0, int fy = 0, RoomType[] forbidden = null, int minKeys = 0, int[] locks = null)
        {
            var sp = new Spec();
            sp.name = name; sp.type = type; sp.anchored = anchored; sp.fx = fx; sp.fy = fy;
            sp.minKeys = minKeys;
            sp.locks = locks == null ? new int[4] : locks;
            sp.forbidden = forbidden == null ? new RoomType[0] : forbidden;
            sp.n = doors.Contains("N"); sp.e = doors.Contains("E");
            sp.s = doors.Contains("S"); sp.w = doors.Contains("W");
            return sp;
        }

        [MenuItem("Tools/TrustNoOne/Build Test House")]
        public static void Build()
        {
            var specs = new Spec[]
            {
                S("SafeRoom",   RoomType.SafeRoom,   "NESW", true, 1, 1),
                S("Hallway_A",  RoomType.Hallway,    "NESW"),
                S("Hallway_B",  RoomType.Hallway,    "NESW"),
                S("Kitchen",    RoomType.Kitchen,    "NE",   false, 0, 0, new[] { RoomType.Bathroom, RoomType.Bedroom }),
                S("LivingRoom", RoomType.LivingRoom, "NEW"),
                S("Bedroom",    RoomType.Bedroom,    "SE",   false, 0, 0, new[] { RoomType.Kitchen }),
                S("Bathroom",   RoomType.Bathroom,   "S",    false, 0, 0, new[] { RoomType.Kitchen }),
                // vault room, both doors need 2 keys
                S("Study",      RoomType.Study,      "NW",   false, 0, 0, null, 2, new[] { 2, 0, 0, 2 }),
            };

            EnsureFolder();

            var houseGo = new GameObject("House");
            Undo.RegisterCreatedObjectUndo(houseGo, "Build Test House");

            var ctrl = houseGo.AddComponent<HouseShuffleController>();
            ctrl.gridWidth = 3;
            ctrl.gridHeight = 3;
            ctrl.cellSize = CellSize;

            for (int i = 0; i < specs.Length; i++)
                BuildRoom(specs[i], houseGo.transform);

            AssetDatabase.SaveAssets();
            Selection.activeGameObject = houseGo;
            Debug.Log("[House] built " + specs.Length + " test rooms. Definitions in " + DefFolder);
        }

        static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects"))
                AssetDatabase.CreateFolder("Assets", "ScriptableObjects");
            if (!AssetDatabase.IsValidFolder(DefFolder))
                AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Rooms");
        }

        static void BuildRoom(Spec sp, Transform parent)
        {
            var def = ScriptableObject.CreateInstance<RoomDefinition>();
            def.roomType = sp.type;
            def.doorNorth = sp.n; def.doorEast = sp.e; def.doorSouth = sp.s; def.doorWest = sp.w;
            def.anchored = sp.anchored;
            def.fixedCell = new Vector2Int(sp.fx, sp.fy);
            def.allowRotation = !sp.anchored;
            def.required = true;
            def.lockNorth = sp.locks[0];
            def.lockEast = sp.locks[1];
            def.lockSouth = sp.locks[2];
            def.lockWest = sp.locks[3];
            def.minKeys = sp.minKeys;
            foreach (var f in sp.forbidden) def.forbiddenNeighbours.Add(f);
            AssetDatabase.CreateAsset(def, DefFolder + "/" + sp.name + ".asset");

            var room = new GameObject(sp.name);
            room.transform.SetParent(parent, false);

            var inst = room.AddComponent<RoomInstance>();
            inst.definition = def;

            var trig = room.GetComponent<BoxCollider>();
            trig.isTrigger = true;
            trig.size = new Vector3(CellSize, WallHeight, CellSize);
            trig.center = new Vector3(0f, WallHeight * 0.5f, 0f);

            // floor
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor";
            floor.transform.SetParent(room.transform, false);
            floor.transform.localPosition = new Vector3(0f, -0.15f, 0f);
            floor.transform.localScale = new Vector3(CellSize, 0.3f, CellSize);

            bool[] hasDoor = { sp.n, sp.e, sp.s, sp.w };
            for (int d = 0; d < 4; d++)
            {
                var side = new GameObject(((Dir)d).ToString());
                side.transform.SetParent(room.transform, false);

                var solid = MakeWall(side.transform, (Dir)d, "Wall", CellSize);
                inst.wallFillers[d] = solid;

                if (hasDoor[d])
                {
                    var gap = new GameObject("Doorway");
                    gap.transform.SetParent(side.transform, false);
                    float sideLen = (CellSize - DoorWidth) * 0.5f;
                    float off = (DoorWidth + sideLen) * 0.5f;
                    MakeWallPiece(gap.transform, (Dir)d, "Left", sideLen, -off);
                    MakeWallPiece(gap.transform, (Dir)d, "Right", sideLen, off);
                    inst.doorways[d] = gap;
                    gap.SetActive(false);

                    // the slab that blocks the gap when locked
                    var locked = MakeWallPiece(side.transform, (Dir)d, "LockedDoor", DoorWidth, 0f);
                    inst.lockedDoors[d] = locked;
                    locked.SetActive(false);
                }
            }
        }

        static GameObject MakeWall(Transform parent, Dir d, string name, float length)
        {
            var holder = new GameObject(name);
            holder.transform.SetParent(parent, false);
            MakeWallPiece(holder.transform, d, "Piece", length, 0f);
            return holder;
        }

        // length runs along the wall, offset slides it sideways
        static GameObject MakeWallPiece(Transform parent, Dir d, string name, float length, float offset)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);

            float half = CellSize * 0.5f;
            Vector3 pos, scale;

            if (d == Dir.North || d == Dir.South)
            {
                float z = (d == Dir.North) ? half : -half;
                pos = new Vector3(offset, WallHeight * 0.5f, z);
                scale = new Vector3(length, WallHeight, WallThick);
            }
            else
            {
                float x = (d == Dir.East) ? half : -half;
                pos = new Vector3(x, WallHeight * 0.5f, offset);
                scale = new Vector3(WallThick, WallHeight, length);
            }

            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            return go;
        }
    }
}