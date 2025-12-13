using Cornifer.MapObjects;
using Cornifer.Renderers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cornifer
{
    public class Layer
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public virtual bool Special { get; set; }
        public bool Visible = true;
        public bool DefaultVisibility = true;

        public Layer(string id, string name, bool special, bool defaultVisibility)
        {
            Id = id;
            Name = name;
            Special = special;
            Visible = defaultVisibility;
            DefaultVisibility = defaultVisibility;
        }

        public virtual void Update() { }

        public virtual void DrawShade(Renderer renderer, Predicate<MapObject>? predicate = null)
        {
            foreach (MapObject obj in Main.WorldObjectLists)
                if (predicate?.Invoke(obj) is null or true)
                    obj.DrawShade(renderer, this);
        }

        public virtual void Draw(Renderer renderer, Predicate<MapObject>? predicate = null) 
        {
            foreach (MapObject obj in Main.WorldObjectLists)
                if (predicate?.Invoke(obj) is null or true)
                    obj.Draw(renderer, this);
        }

        public virtual void DrawGuides(Renderer renderer) { }
    }

    public class ConnectionsLayer : Layer 
    {
        public bool InRoomConnections { get; }

        public override bool Special => true;

        public ConnectionsLayer(bool inRoomConnections, bool defaultVisibility) : base(
            inRoomConnections ? "inroomconnections" : "connections",
            inRoomConnections ? "In-Room Connections" : "Connections",
            true, defaultVisibility)
        {
            InRoomConnections = inRoomConnections;
        }

        public override void DrawShade(Renderer renderer, Predicate<MapObject>? predicate = null)
        {
            foreach (var region in Main.Regions)
                region.Connections?.DrawShadows(renderer, !InRoomConnections, InRoomConnections, predicate);

            if (!InRoomConnections)
                Main.GlobalConnections.DrawShadows(renderer, true, false, predicate);
        }

        public override void Draw(Renderer renderer, Predicate<MapObject>? predicate = null)
        {
            foreach (var region in Main.Regions)
            {
                region.Connections?.DrawConnections(renderer, true, !InRoomConnections, InRoomConnections, predicate);
                region.Connections?.DrawConnections(renderer, false, !InRoomConnections, InRoomConnections, predicate);
            }

            if (!InRoomConnections)
            {
                Main.GlobalConnections.DrawConnections(renderer, true, true, false, predicate);
                Main.GlobalConnections.DrawConnections(renderer, false, true, false, predicate);
            }
        }

        public override void DrawGuides(Renderer renderer)
        {
            foreach (var region in Main.Regions)
                region.Connections?.DrawGuideLines(renderer, !InRoomConnections, InRoomConnections);

            if (!InRoomConnections)
                Main.GlobalConnections.DrawGuideLines(renderer, true, false);
        }
    }

    public class GridLayer : Layer
    {
        private const int GridSize = 4; // 4px minimal unit
        private const int ThickLineInterval = 4; // Every 4th line is thick
        private const int ThickLineWidth = 3; // 3px thick lines
        private const int ThinLineWidth = 1; // 1px thin lines

        public GridLayer() : base("grid", "Grid", true, true)
        {
        }

        public override void Draw(Renderer renderer, Predicate<MapObject>? predicate = null)
        {
            if (!Visible) return;

            try
            {
                // Get screen dimensions safely
                Vector2 screenSize = renderer.Size;
                if (screenSize.X <= 0 || screenSize.Y <= 0) return;

                // Calculate world bounds from screen coordinates
                Vector2 worldTopLeft = renderer.InverseTransformVector(Vector2.Zero);
                Vector2 worldBottomRight = renderer.InverseTransformVector(screenSize);

                // Add some padding to ensure grid lines extend beyond screen edges
                int padding = GridSize * 4;
                worldTopLeft.X -= padding;
                worldTopLeft.Y -= padding;
                worldBottomRight.X += padding;
                worldBottomRight.Y += padding;

                // Calculate grid bounds aligned to grid size
                int startX = (int)(worldTopLeft.X / GridSize) * GridSize;
                int endX = (int)(worldBottomRight.X / GridSize + 1) * GridSize;
                int startY = (int)(worldTopLeft.Y / GridSize) * GridSize;
                int endY = (int)(worldBottomRight.Y / GridSize + 1) * GridSize;

                // Limit grid bounds to reasonable values to prevent performance issues
                const int maxGridLines = 500;
                if ((endX - startX) / GridSize > maxGridLines || (endY - startY) / GridSize > maxGridLines)
                    return;

                // Debug output to verify grid is being drawn
                // Console.WriteLine($"Drawing grid: Camera pos=({renderer.Position.X:F1}, {renderer.Position.Y:F1}), Scale={renderer.Scale:F2}, Screen={screenSize.X}x{screenSize.Y}");
                // Console.WriteLine($"Grid bounds: X=[{startX}, {endX}], Y=[{startY}, {endY}], Lines={((endX-startX)+(endY-startY))/GridSize}");

                // Draw vertical lines
                int verticalLinesDrawn = 0;
                for (int x = startX; x <= endX; x += GridSize)
                {
                    Vector2 screenStart = renderer.TransformVector(new Vector2(x, startY));
                    Vector2 screenEnd = renderer.TransformVector(new Vector2(x, endY));
                    
                    bool isThickLine = (x / GridSize) % ThickLineInterval == 0;
                    int lineWidth = isThickLine ? ThickLineWidth : ThinLineWidth;
                    
                    // Draw line if it's within screen bounds
                    if (screenStart.X >= -50 && screenStart.X <= screenSize.X + 50)
                    {
                        Main.SpriteBatch.DrawLine(
                            new Vector2(screenStart.X, Math.Max(-50, screenStart.Y)),
                            new Vector2(screenStart.X, Math.Min(screenSize.Y + 50, screenEnd.Y)),
                            isThickLine ? Color.Gray * 0.4f : Color.Gray * 0.15f,
                            lineWidth
                        );
                        verticalLinesDrawn++;
                    }
                }

                // Draw horizontal lines
                int horizontalLinesDrawn = 0;
                for (int y = startY; y <= endY; y += GridSize)
                {
                    Vector2 screenStart = renderer.TransformVector(new Vector2(startX, y));
                    Vector2 screenEnd = renderer.TransformVector(new Vector2(endX, y));
                    
                    bool isThickLine = (y / GridSize) % ThickLineInterval == 0;
                    int lineWidth = isThickLine ? ThickLineWidth : ThinLineWidth;
                    
                    // Draw line if it's within screen bounds
                    if (screenStart.Y >= -50 && screenStart.Y <= screenSize.Y + 50)
                    {
                        Main.SpriteBatch.DrawLine(
                            new Vector2(Math.Max(-50, screenStart.X), screenStart.Y),
                            new Vector2(Math.Min(screenSize.X + 50, screenEnd.X), screenStart.Y),
                            isThickLine ? Color.Gray * 0.4f : Color.Gray * 0.15f,
                            lineWidth
                        );
                        horizontalLinesDrawn++;
                    }
                }

                // Console.WriteLine($"Grid drawn: {verticalLinesDrawn} vertical lines, {horizontalLinesDrawn} horizontal lines");
            }
            catch (Exception ex)
            {
                // Log error but don't crash the application
                Console.WriteLine($"Error drawing grid: {ex.Message}");
            }
        }
    }
}
