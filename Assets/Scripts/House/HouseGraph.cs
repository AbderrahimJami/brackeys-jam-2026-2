using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TrustNoOne.Shuffle
{
    public enum Dir { North = 0, East = 1, South = 2, West = 3 }

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

        public HashSet<string> ForbiddenNeighbours = new HashSet<string>();

        public bool Anchored;          // never moves (safe room)
        public bool Required = true;   // must stay reachable
        public bool AllowRotation = true;

        public int FixedX = -1, FixedY = -1; // only used when anchored

        public bool HasDoor(Dir facing, int rotation)
        {
            int idx = (((int)facing - rotation) % 4 + 4) % 4;
            return Doors[idx];
        }
    }

    public class PlacedRoom
    {
        public RoomTemplate Template;
        public int X, Y;
        public int Rotation; // 0-3, clockwise 90deg steps

        public bool HasDoor(Dir facing) { return Template.HasDoor(facing, Rotation); }
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
            if (!a.HasDoor(d)) return false;
            int dx, dy; DirUtil.Offset(d, out dx, out dy);
            PlacedRoom b = At(a.X + dx, a.Y + dy);
            if (b == null) return false;
            return b.HasDoor(DirUtil.Opposite(d));
        }
    }

    public class ShuffleConfig
    {
        public int Width = 4, Height = 4;
        public int MaxAttempts = 500;
        public bool AllowRotation = true;
        public bool RequireAllRoomsReachable = true;
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

        HouseLayout BuildCandidate(List<RoomTemplate> rooms, HouseLayout previous, string playerRoomId, ShuffleConfig cfg)
        {
            var layout = new HouseLayout(cfg.Width, cfg.Height);
            var takenCells = new HashSet<int>();
            var movable = new List<RoomTemplate>();

            foreach (var t in rooms)
            {
                if (t.Anchored)
                {
                    layout.Rooms.Add(new PlacedRoom { Template = t, X = t.FixedX, Y = t.FixedY, Rotation = 0 });
                    takenCells.Add(t.FixedY * cfg.Width + t.FixedX);
                }
                else if (previous != null && t.Id == playerRoomId)
                {
                    var old = previous.ById(t.Id);
                    layout.Rooms.Add(new PlacedRoom { Template = t, X = old.X, Y = old.Y, Rotation = old.Rotation });
                    takenCells.Add(old.Y * cfg.Width + old.X);
                }
                else movable.Add(t);
            }

            // free cells, shuffled
            var free = new List<int>();
            for (int i = 0; i < cfg.Width * cfg.Height; i++)
                if (!takenCells.Contains(i)) free.Add(i);
            Shuffle(free);

            for (int i = 0; i < movable.Count; i++)
            {
                int cell = free[i];
                var t = movable[i];
                int rot = (cfg.AllowRotation && t.AllowRotation) ? rng.Next(4) : 0;
                layout.Rooms.Add(new PlacedRoom { Template = t, X = cell % cfg.Width, Y = cell / cfg.Width, Rotation = rot });
            }

            return layout;
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
                    if (!layout.Connected(cur, (Dir)d)) continue;
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
                bool mustReach = cfg.RequireAllRoomsReachable || r.Template.Required;
                if (mustReach && !seen.Contains(r.Template.Id))
                {
                    reason = r.Template.Type + " unreachable";
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
                    top.Append("+" + (r.HasDoor(Dir.North) ? " . " : "---") + "+");
                    mid.Append((r.HasDoor(Dir.West) ? "." : "|") + label + (r.HasDoor(Dir.East) ? "." : "|"));
                    bot.Append("+" + (r.HasDoor(Dir.South) ? " . " : "---") + "+");
                }
                sb.AppendLine(top.ToString());
                sb.AppendLine(mid.ToString());
                sb.AppendLine(bot.ToString());
            }
            return sb.ToString();
        }
    }
}