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
        var worldMouse = renderer.InverseTransformVector(InputHandler.MouseState.Position.ToVector2());
        var isControlDown = InputHandler.KeyboardState.IsKeyDown(Keys.LeftControl);
        // 1. 按下瞬间：一趟点击中只会运行一次
        if (InputHandler.SelectEntity.JustPressed && !Interface.IsHovered) {
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
        if (InputHandler.SelectEntity.Pressed) {
            if (_currentMode == Mode.Dragging) {
                var delta = worldMouse - _lastMouseWorldPos;
                // var delta = snappedWorldMouse - _lastMouseWorldPos;
                if (delta != Vector2.Zero) {
                    foreach (var entity in Map.SelectedEntities) {
                        if (!entity.IsAlive()) continue;
                        ref var vis = ref entity.Get<Visual>();

                        if (entity.Has<Hierarchy>()) {
                            var parent = entity.Get<Hierarchy>().Parent;
                            if (parent.HasValue && Map.SelectedEntities.Contains(parent.Value))
                                continue;
                        }

                        // 2. 递归移动该实体及其所有子孙
                        MoveRecursive(entity, delta);
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
        if (InputHandler.SelectEntity.JustReleased) {
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

    private static void MoveRecursive(Entity entity, Vector2 delta) {
        if (!entity.IsAlive()) return;

        // 移动自己
        if (entity.Has<Visual>()) {
            ref var vis = ref entity.Get<Visual>();
            vis.WorldPosition += delta;
        }

        // 移动子物体
        if (!entity.Has<Hierarchy>()) return;
        ref var hier = ref entity.Get<Hierarchy>();
        foreach (var child in hier.Children) {
            MoveRecursive(child, delta);
        }
    }

    public static void DrawSelectionMarquee(Renderer renderer) {
        if (_currentMode != Mode.Marquee || !SelectionRect.HasValue) return;

        var rect = SelectionRect.Value;
        var screenPos = renderer.TransformVector(new Vector2(rect.X, rect.Y));
        var screenW = rect.Width * renderer.Scale;
        var screenH = rect.Height * renderer.Scale;

        if (renderer is not ScreenRenderer sr) return;
        sr.SpriteBatch.Draw(Content.Tex.Pixel,
            new Rectangle((int)screenPos.X, (int)screenPos.Y, (int)screenW, (int)screenH), Color.Cyan * 0.2f);
        GizmoSystem.DrawHollowRect(renderer,
            new Rectangle((int)screenPos.X, (int)screenPos.Y, (int)screenW, (int)screenH), Color.Cyan, 1);
    }
}