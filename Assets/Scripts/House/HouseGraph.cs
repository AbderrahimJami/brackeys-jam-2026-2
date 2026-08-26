using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TrustNoOne.Shuffle
{
    public enum Dir { North = 0, East = 1, South = 2, West = 3 }

    public enum DoorState { Wall, Locked, Open }

    public static class DirUtil
    {
        public static Dir Opposite(Dir d) { return (Dir)(((int)d + 2) % 4); }

        public static void Offset(Dir d, out int dx, out int dy)
        {
            switch (d)
            {
                case Dir.North: dx = 0; dy = 1; break;
                case Dir.East: dx = 1; dy = 0; break;
                case Dir.South: dx = 0; dy = -1; break;
                default: dx = -1; dy = 0; break;
            }
        }
    }

    // one room's rules. no unity types, this mirrors the ScriptableObject later
    public class RoomTemplate
    {
        public string Id;
        public string Type;

        // doors at rotation 0, indexed by Dir
        public bool[] Doors = new bool[4];

        // 0 = unlocked, 1-3 = needs that many keys
        public int[] DoorLocks = new int[4];

        // room isn't expected to be reachable until the player has this many keys
        public int MinKeys;

        public HashSet<string> ForbiddenNeighbours = new HashSet<string>();

        public bool Anchored;          // never moves (safe room)
        public bool Required = true;   // must stay reachable
        public bool AllowRotation = true;

        public int FixedX = -1, FixedY = -1; // only used when anchored

        public bool HasDoor(Dir facing, int rotation)
        {
            return Doors[LocalIndex(facing, rotation)];
        }

        public int DoorLock(Dir facing, int rotation)
        {
            return DoorLocks[LocalIndex(facing, rotation)];
        }

        public static int LocalIndex(Dir facing, int rotation)
        {
            return (((int)facing - rotation) % 4 + 4) % 4;
        }
    }

    public class PlacedRoom
    {
        public RoomTemplate Template;
        public int X, Y;
        public int Rotation; // 0-3, clockwise 90deg steps

        public bool HasDoor(Dir facing) { return Template.HasDoor(facing, Rotation); }
        public int DoorLock(Dir facing) { return Template.DoorLock(facing, Rotation); }
    }

    public class HouseLayout
    {
        public int Width, Height;
        public List<PlacedRoom> Rooms = new List<PlacedRoom>();

        public HouseLayout(int w, int h) { Width = w; Height = h; }

        public PlacedRoom At(int x, int y)
        {
            for (int i = 0; i < Rooms.Count; i++)
                if (Rooms[i].X == x && Rooms[i].Y == y) return Rooms[i];
            return null;
        }

        public PlacedRoom ById(string id)
        {
            for (int i = 0; i < Rooms.Count; i++)
                if (Rooms[i].Template.Id == id) return Rooms[i];
            return null;
        }

        // two rooms are connected only if BOTH have a door facing each other
        public bool Connected(PlacedRoom a, Dir d)
        {
            return ConnectionLock(a, d) >= 0;
        }

        // -1 means no doorway at all. otherwise the keys needed to pass
        public int ConnectionLock(PlacedRoom a, Dir d)
        {
            if (!a.HasDoor(d)) return -1;
            int dx, dy; DirUtil.Offset(d, out dx, out dy);
            PlacedRoom b = At(a.X + dx, a.Y + dy);
            if (b == null) return -1;
            Dir back = DirUtil.Opposite(d);
            if (!b.HasDoor(back)) return -1;
            int la = a.DoorLock(d);
            int lb = b.DoorLock(back);
            return la > lb ? la : lb;
        }
    }

    public class ShuffleConfig
    {
        public int Width = 4, Height = 4;
        public int MaxAttempts = 500;
        public bool AllowRotation = true;
        public bool RequireAllRoomsReachable = true;

        // how many keys the player currently holds
        public int PlayerKeys = 0;
    }

    public class ShuffleResult
    {
        public HouseLayout Layout;
        public bool Success;
        public int Attempts;
        public string FailReason;
    }

    public class HouseGenerator
    {
        Random rng;
        public HouseGenerator(int seed) { rng = new Random(seed); }
        public HouseGenerator() { rng = new Random(); }

        // playerRoomId stays exactly where it was. pass null for the first build
        public ShuffleResult Generate(List<RoomTemplate> rooms, HouseLayout previous, string playerRoomId, ShuffleConfig cfg)
        {
            var result = new ShuffleResult();
            string lastFail = "no attempts";

            for (int attempt = 1; attempt <= cfg.MaxAttempts; attempt++)
            {
                HouseLayout candidate = BuildCandidate(rooms, previous, playerRoomId, cfg);
                string why;
                if (Validate(candidate, cfg, playerRoomId, out why))
                {
                    result.Layout = candidate;
                    result.Success = true;
                    result.Attempts = attempt;
                    return result;
                }
                lastFail = why;
            }

            // couldn't find one, keep the old house rather than break the game
            result.Layout = previous;
            result.Success = false;
            result.Attempts = cfg.MaxAttempts;
            result.FailReason = lastFail;
            return result;
        }

        // grow the house outward from the pinned rooms so it's connected by construction.
        // random scatter + reject almost never lands a valid layout once the grid has gaps
        HouseLayout BuildCandidate(List<RoomTemplate> rooms, HouseLayout previous, string playerRoomId, ShuffleConfig cfg)
        {
            var layout = new HouseLayout(cfg.Width, cfg.Height);
            var taken = new HashSet<int>();
            var remaining = new List<RoomTemplate>();

            foreach (var t in rooms)
            {
                if (t.Anchored)
                {
                    layout.Rooms.Add(new PlacedRoom { Template = t, X = t.FixedX, Y = t.FixedY, Rotation = 0 });
                    taken.Add(t.FixedY * cfg.Width + t.FixedX);
                }
                else if (previous != null && t.Id == playerRoomId)
                {
                    var old = previous.ById(t.Id);
                    layout.Rooms.Add(new PlacedRoom { Template = t, X = old.X, Y = old.Y, Rotation = old.Rotation });
                    taken.Add(old.Y * cfg.Width + old.X);
                }
                else remaining.Add(t);
            }

            Shuffle(remaining);

            // repeatedly hang an unplaced room off an open doorway of a placed one
            while (remaining.Count > 0)
            {
                var openings = FindOpenings(layout, cfg, taken);
                if (openings.Count == 0) break;
                Shuffle(openings);

                if (!PlaceOne(layout, cfg, taken, remaining, openings)) break;
            }

            // anything we couldn't attach goes in a random hole. validation will reject it
            if (remaining.Count > 0)
            {
                var free = new List<int>();
                for (int i = 0; i < cfg.Width * cfg.Height; i++)
                    if (!taken.Contains(i)) free.Add(i);
                Shuffle(free);

                for (int i = 0; i < remaining.Count && i < free.Count; i++)
                {
                    var t = remaining[i];
                    int rot = (cfg.AllowRotation && t.AllowRotation) ? rng.Next(4) : 0;
                    layout.Rooms.Add(new PlacedRoom { Template = t, X = free[i] % cfg.Width, Y = free[i] / cfg.Width, Rotation = rot });
                }
            }

            return layout;
        }

        struct Opening
        {
            public PlacedRoom From;
            public Dir Facing;
            public int X, Y;
        }

        // every doorway on a placed room that points at an empty in-bounds cell
        List<Opening> FindOpenings(HouseLayout layout, ShuffleConfig cfg, HashSet<int> taken)
        {
            var list = new List<Opening>();
            foreach (var placed in layout.Rooms)
            {
                for (int d = 0; d < 4; d++)
                {
                    if (!placed.HasDoor((Dir)d)) continue;
                    int dx, dy; DirUtil.Offset((Dir)d, out dx, out dy);
                    int nx = placed.X + dx, ny = placed.Y + dy;
                    if (nx < 0 || ny < 0 || nx >= cfg.Width || ny >= cfg.Height) continue;
                    if (taken.Contains(ny * cfg.Width + nx)) continue;
                    list.Add(new Opening { From = placed, Facing = (Dir)d, X = nx, Y = ny });
                }
            }
            return list;
        }

        bool PlaceOne(HouseLayout layout, ShuffleConfig cfg, HashSet<int> taken, List<RoomTemplate> remaining, List<Opening> openings)
        {
            var rotations = new List<int> { 0, 1, 2, 3 };

            foreach (var op in openings)
            {
                Dir back = DirUtil.Opposite(op.Facing);

                for (int i = 0; i < remaining.Count; i++)
                {
                    var t = remaining[i];
                    Shuffle(rotations);

                    foreach (int rot in rotations)
                    {
                        if (rot != 0 && (!cfg.AllowRotation || !t.AllowRotation)) continue;
                        if (!t.HasDoor(back, rot)) continue;
                        if (!AdjacencyOk(layout, t, op.X, op.Y)) continue;

                        layout.Rooms.Add(new PlacedRoom { Template = t, X = op.X, Y = op.Y, Rotation = rot });
                        taken.Add(op.Y * cfg.Width + op.X);
                        remaining.RemoveAt(i);
                        return true;
                    }
                }
            }
            return false;
        }

        static bool AdjacencyOk(HouseLayout layout, RoomTemplate t, int x, int y)
        {
            for (int d = 0; d < 4; d++)
            {
                int dx, dy; DirUtil.Offset((Dir)d, out dx, out dy);
                var n = layout.At(x + dx, y + dy);
                if (n == null) continue;
                if (t.ForbiddenNeighbours.Contains(n.Template.Type)) return false;
                if (n.Template.ForbiddenNeighbours.Contains(t.Type)) return false;
            }
            return true;
        }

        void Shuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                T tmp = list[i]; list[i] = list[j]; list[j] = tmp;
            }
        }

        public static bool Validate(HouseLayout layout, ShuffleConfig cfg, string playerRoomId, out string reason)
        {
            reason = null;
            if (layout == null) { reason = "null layout"; return false; }

            // 1. adjacency rules
            foreach (var r in layout.Rooms)
            {
                for (int d = 0; d < 4; d++)
                {
                    int dx, dy; DirUtil.Offset((Dir)d, out dx, out dy);
                    var n = layout.At(r.X + dx, r.Y + dy);
                    if (n == null) continue;
                    if (r.Template.ForbiddenNeighbours.Contains(n.Template.Type))
                    {
                        reason = r.Template.Type + " next to " + n.Template.Type;
                        return false;
                    }
                }
            }

            // 2. connectivity, flood fill from the player's room
            PlacedRoom start = playerRoomId != null ? layout.ById(playerRoomId) : null;
            if (start == null) start = layout.Rooms.FirstOrDefault();
            if (start == null) { reason = "empty house"; return false; }

            var seen = new HashSet<string>();
            var queue = new Queue<PlacedRoom>();
            queue.Enqueue(start);
            seen.Add(start.Template.Id);

            while (queue.Count > 0)
            {
                var cur = queue.Dequeue();
                for (int d = 0; d < 4; d++)
                {
                    int lockLevel = layout.ConnectionLock(cur, (Dir)d);
                    // no doorway, or locked beyond what the player can open
                    if (lockLevel < 0 || lockLevel > cfg.PlayerKeys) continue;
                    int dx, dy; DirUtil.Offset((Dir)d, out dx, out dy);
                    var n = layout.At(cur.X + dx, cur.Y + dy);
                    if (n != null && !seen.Contains(n.Template.Id))
                    {
                        seen.Add(n.Template.Id);
                        queue.Enqueue(n);
                    }
                }
            }

            foreach (var r in layout.Rooms)
            {
                // rooms gated behind more keys than we have aren't expected to be reachable yet
                if (r.Template.MinKeys > cfg.PlayerKeys) continue;

                bool mustReach = cfg.RequireAllRoomsReachable || r.Template.Required;
                if (mustReach && !seen.Contains(r.Template.Id))
                {
                    reason = r.Template.Type + " unreachable with " + cfg.PlayerKeys + " keys";
                    return false;
                }
            }

            return true;
        }

        // ascii dump so you can eyeball layouts without unity
        public static string Print(HouseLayout l)
        {
            if (l == null) return "(null layout)";
            var sb = new StringBuilder();
            for (int y = l.Height - 1; y >= 0; y--)
            {
                var top = new StringBuilder();
                var mid = new StringBuilder();
                var bot = new StringBuilder();
                for (int x = 0; x < l.Width; x++)
                {
                    var r = l.At(x, y);
                    if (r == null) { top.Append("     "); mid.Append("     "); bot.Append("     "); continue; }
                    string label = r.Template.Type.Length >= 3 ? r.Template.Type.Substring(0, 3) : r.Template.Type.PadRight(3);
                    top.Append("+" + (r.HasDoor(Dir.North) ? " " + Mark(r, Dir.North) + " " : "---") + "+");
                    mid.Append((r.HasDoor(Dir.West) ? Mark(r, Dir.West) : "|") + label + (r.HasDoor(Dir.East) ? Mark(r, Dir.East) : "|"));
                    bot.Append("+" + (r.HasDoor(Dir.South) ? " " + Mark(r, Dir.South) + " " : "---") + "+");
                }
                sb.AppendLine(top.ToString());
                sb.AppendLine(mid.ToString());
                sb.AppendLine(bot.ToString());
            }
            return sb.ToString();
        }

        static string Mark(PlacedRoom r, Dir d)
        {
            int l = r.DoorLock(d);
            return l <= 0 ? "." : l.ToString();
        }
    }
}