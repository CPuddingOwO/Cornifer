using Cornifer.Json;
using Cornifer.MapObjects;
using Cornifer.UI.Elements;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows.Forms;

namespace Cornifer.Connections
{
    public class Connection
    {
        static List<Point> ShortcutTracingCache = new();

        public Room Source;
        public Room Destination;

        public Point SourcePoint;
        public Point DestinationPoint;

        public bool Invalid;
        public bool IsInRoomShortcut = false;

        public bool Active => Source.Active && Destination.Active && (!IsInRoomShortcut || Source.DrawInRoomShortcuts.Value);
        public Color Color => IsInRoomShortcut ? Color.Lerp(Color.White, Source.Subregion.Value.BackgroundColor.Color, .3f) : Color.White;
        public string JsonKey => IsInRoomShortcut ? $"#{Source.Name}~{SourcePoint.X}~{SourcePoint.Y}" : $"{Source.Name}~{Destination.Name}";

        public Color GuideColor = Color.Magenta;

        public ObjectProperty<bool> AllowWhiteToRedPixel = new("whiteToRed", true);

        public List<ConnectionPoint> Points = new();

        public Connection(Room room, Room.Shortcut shortcut)
        {
            Source = Destination = room;
            IsInRoomShortcut = true;

            SourcePoint = shortcut.Entrance;
            DestinationPoint = shortcut.Target;

            ShortcutTracingCache.Clear();

            room.TraceShortcut(SourcePoint, ShortcutTracingCache);

            foreach (Point point in ShortcutTracingCache)
            {
                Points.Add(new(this)
                {
                    Parent = Source,
                    ParentPosition = point.ToVector2()
                });
            }
        }

        public bool IsRegionLink = false;

        public Connection(Room source, Room destination)
        {
            Source = source;
            Destination = destination;
            Invalid = false;
            IsRegionLink = true;

            Points.Add(new(this) { Parent = Source, ParentPosition = GetRegionExitPosition(Source) });
            Points.Add(new(this) { Parent = Destination, ParentPosition = GetRegionExitPosition(Destination) });
        }

        public static Vector2 GetRegionExitPosition(Room room)
        {
            if (room.IsGate && room.Exits.Length >= 2)
            {
                // For a standard Gate, exits are usually [0] Left, [1] Right.
                // Try to find the exit that is NOT connected to any room in the same region.
                for (int i = 0; i < room.Connections.Length; i++)
                {
                    if (room.Connections[i] is null)
                    {
                        // This exit is not connected to a room in this region. 
                        // It's likely the exit to the other region.
                        if (i < room.Exits.Length) return room.Exits[i].ToVector2();
                    }
                }
            }
            return room.WorldPosition + room.Size / 2f;
        }

        public Connection(Room source, Room.Connection connection)
        {
            Invalid = true;
            if (source is null || connection.Target is null)
            {
                Main.LoadErrors.Add($"Tried mapping connection from {source?.Name ?? "NONE"} to {connection.Target?.Name ?? "NONE"}");
            }
            else if (connection.Exit < 0 || connection.Exit >= source.Exits.Length)
            {
                //if (source.Active)
                Main.LoadErrors.Add($"Tried mapping connection from nonexistent shortcut {connection.Exit} in {source?.Name ?? "NONE"}");
            }
            else if (connection.TargetExit < 0 || connection.TargetExit >= connection.Target.Exits.Length)
            {
                //if (connection.Target.Active)
                Main.LoadErrors.Add($"Tried mapping connection from nonexistent shortcut {connection.TargetExit} in {connection.Target?.Name ?? "NONE"}");
            }
            else
            {
                Invalid = false;
            }

            if (Invalid)
            {
                Source = null!;
                Destination = null!;
                return;
            }

            Source = source!;
            Destination = connection.Target!;

            SourcePoint = source!.Exits[connection.Exit];
            DestinationPoint = connection.Target!.Exits[connection.TargetExit];
        }

        internal void BuildConfig(UIList list)
        {
            list.Elements.Add(new UILabel
            {
                Text = "Connection config",
                Height = 20,
                TextAlign = new(.5f)
            });

            list.Elements.Add(new UIButton
            {
                Text = "Allow white-red ending",
                Height = 20,

                Selectable = true,
                Selected = AllowWhiteToRedPixel.Value,

                SelectedTextColor = Color.Black,
                SelectedBackColor = Color.White,

            }.OnEvent(UIElement.ClickEvent, (btn, _) => AllowWhiteToRedPixel.Value = btn.Selected));
        }

        public void LoadJson(JsonNode node)
        {
            if (node is JsonValue value)
            {
                int pointCount = value.Deserialize<int>();
                if (pointCount == 0)
                    return;

                Vector2 start = Source.WorldPosition + SourcePoint.ToVector2();
                Vector2 end = Destination.WorldPosition + DestinationPoint.ToVector2();

                Points.Clear();
                float tpp = 1 / (pointCount + 1);
                float t = tpp;
                for (int i = 0; i < pointCount; i++)
                {
                    ConnectionPoint newPoint = new(this)
                    {
                        ParentPosition = Vector2.Lerp(start, end, t),
                    };
                    Points.Add(newPoint);
                    t += tpp;
                }
            }
            else if (node is JsonArray pointsArray)
                LoadPointArray(pointsArray);
            else if (node is JsonObject obj)
            {
                if (obj.TryGet("points", out JsonArray? points))
                    LoadPointArray(points);
                AllowWhiteToRedPixel.LoadFromJson(obj);
            }
        }

        public bool MatchesPredicate(Predicate<MapObject> predicate) 
        {
            if (predicate(Source) || predicate(Destination))
                return true;

            foreach (ConnectionPoint point in Points)
                if (predicate(point))
                    return true;

            return false;
        }

        public JsonNode SaveJson()
        {
            EnsurePointParents();
            return new JsonObject
            {
                ["points"] = new JsonArray(Points.Select(p => p.SaveJson()).ToArray())
            }.SaveProperty(AllowWhiteToRedPixel);
        }
        Room? GetExpectedParent(int index, int totalPoints)
        {
            if (IsInRoomShortcut)
                return Source;

            if (IsRegionLink)
                return index == totalPoints - 1 ? Destination : Source;

            return Source;
        }

        void EnsurePointParents()
        {
            for (int i = 0; i < Points.Count; i++)
            {
                Room? expected = GetExpectedParent(i, Points.Count);
                if (expected is null)
                    continue;

                ConnectionPoint point = Points[i];
                if (point.Parent == expected)
                    continue;

                Vector2 worldPos = point.WorldPosition;
                point.Parent = expected;
                point.WorldPosition = worldPos;
            }
        }
        

        void LoadPointArray(JsonArray points)
        {
            Points.Clear();
            
            List<JsonNode> validNodes = new(points.Count);
            foreach (JsonNode? node in points)
                if (node is not null)
                    validNodes.Add(node);
            
            for (int i = 0; i < validNodes.Count; i++)
            {
                JsonNode node = validNodes[i];
                ConnectionPoint newPoint = new(this);
            
                Room? parentRoom = GetExpectedParent(i, validNodes.Count);
                if (parentRoom is not null)
                    newPoint.Parent = parentRoom;
            
                newPoint.LoadJson(node);
                Points.Add(newPoint);

                
            }

            EnsurePointParents();
        }

        public override string ToString()
        {
            return JsonKey;
        }
    }

}
