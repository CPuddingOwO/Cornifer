using System;
using System.Collections.Generic;
using Arch.Core;
using Arch.Core.Extensions;
using Cornifer.Input;
using Cornifer.Renderers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Cornifer.Systems;

public static class InteractionSystem {
    private enum Mode { None, Dragging, Marquee }

    private static Mode _currentMode = Mode.None;
    private static Vector2? _selectionStart;
    public static Rectangle? SelectionRect { get; private set; }
    private static Vector2 _lastMouseWorldPos;
    private static readonly List<Entity> QueryBuffer = new(128);

    public static void Update(Renderer renderer) {
        var ms = InputHandler.MouseState;
        var prevMs = InputHandler.PrevMouseState;
        
        var worldMouse = renderer.InverseTransformVector(ms.Position.ToVector2());
        bool isControlDown = InputHandler.KeyboardState.IsKeyDown(Keys.LeftControl);

        // --- 核心修复：手动判定触发边缘，不依赖 Keybind 内部逻辑 ---
        bool mouseJustPressed = ms.LeftButton == ButtonState.Pressed && prevMs.LeftButton == ButtonState.Released;
        bool mouseHeld = ms.LeftButton == ButtonState.Pressed;
        bool mouseJustReleased = ms.LeftButton == ButtonState.Released && prevMs.LeftButton == ButtonState.Pressed;

        // 1. 按下瞬间：由于有了 mouseJustPressed，这一段代码在一趟点击中只会运行一次！
        if (mouseJustPressed && !Interface.IsHovered) {
            _lastMouseWorldPos = worldMouse;
            var hit = SpatialSystem.GetEntityAtPixel(worldMouse);

            if (hit.HasValue) {
                // 点中了：进入拖拽模式
                _currentMode = Mode.Dragging;
                if (!Map.SelectedEntities.Contains(hit.Value)) {
                    if (!isControlDown) Map.SelectedEntities.Clear();
                    Map.SelectedEntities.Add(hit.Value);
                }
            } else {
                // 点空了：进入框选模式
                _currentMode = Mode.Marquee;
                _selectionStart = worldMouse;
                SelectionRect = null;
                if (!isControlDown) Map.SelectedEntities.Clear();
            }
        }

        // 2. 持续按住：严格根据按下瞬间决定的 currentMode 执行逻辑
        if (mouseHeld) {
            if (_currentMode == Mode.Dragging) {
                Vector2 delta = worldMouse - _lastMouseWorldPos;
                if (delta != Vector2.Zero) {
                    foreach (var entity in Map.SelectedEntities) {
                        if (!entity.IsAlive()) continue;
                        ref var vis = ref entity.Get<Visual>();
                        vis.WorldPosition += delta;
                        vis.OffsetPosition += delta;
                    }
                }
            } else if (_currentMode == Mode.Marquee && _selectionStart.HasValue) {
                // 更新框选矩形
                var start = _selectionStart.Value;
                var x = Math.Min(start.X, worldMouse.X);
                var y = Math.Min(start.Y, worldMouse.Y);
                var w = Math.Max(1, Math.Abs(start.X - worldMouse.X));
                var h = Math.Max(1, Math.Abs(start.Y - worldMouse.Y));
                SelectionRect = new Rectangle((int)x, (int)y, (int)w, (int)h);
            }
        }

        // 3. 释放瞬间
        if (mouseJustReleased) {
            if (_currentMode == Mode.Marquee && SelectionRect.HasValue) {
                QueryBuffer.Clear();
                SpatialSystem.GetEntitiesInRect(SelectionRect.Value, QueryBuffer);
                foreach (var entity in QueryBuffer) Map.SelectedEntities.Add(entity);
            }

            _currentMode = Mode.None;
            _selectionStart = null;
            SelectionRect = null;
        }

        _lastMouseWorldPos = worldMouse;
    }

    public static void DrawSelectionMarquee(Renderer renderer) {
        if (_currentMode != Mode.Marquee || !SelectionRect.HasValue) return;

        var rect = SelectionRect.Value;
        var screenPos = renderer.TransformVector(new(rect.X, rect.Y));
        var screenW = rect.Width * renderer.Scale;
        var screenH = rect.Height * renderer.Scale;

        if (renderer is ScreenRenderer sr) {
            sr.SpriteBatch.Draw(Content.Tex.Pixel, new Rectangle((int)screenPos.X, (int)screenPos.Y, (int)screenW, (int)screenH), Color.Cyan * 0.2f);
            GizmoSystem.DrawHollowRect(renderer, new Rectangle((int)screenPos.X, (int)screenPos.Y, (int)screenW, (int)screenH), Color.Cyan, 1);
        }
    }
}