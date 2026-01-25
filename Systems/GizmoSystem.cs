using System.Collections.Generic;
using Arch.Core;
using Arch.Core.Extensions;
using Cornifer.Renderers;
using Microsoft.Xna.Framework;

namespace Cornifer.Systems;

public static class GizmoSystem {
    public static void Draw(Renderer renderer, HashSet<Entity> selectedEntities) {
        foreach (var entity in selectedEntities) {
            if (!entity.IsAlive()) return;
            
            ref var vis = ref entity.Get<Visual>();

            // 1. 计算物体在世界空间中的左上角
            Vector2 worldDrawPos = new(
                vis.WorldPosition.X - vis.LocalPosition.X,
                vis.WorldPosition.Y - (vis.Texture.Height - vis.LocalPosition.Y)
            );

            // 2. 将世界坐标转换为屏幕坐标
            var screenPos = renderer.TransformVector(worldDrawPos);

            // 3. 计算屏幕上的尺寸 (需要乘以缩放倍率 Scale)
            var screenW = vis.Texture.Width * renderer.Scale;
            var screenH = vis.Texture.Height * renderer.Scale;

            // 4. 绘制外框 (使用转换后的屏幕坐标)
            // 注意：这里的厚度 thickness 设为 2，它在屏幕上永远是 2 像素，不会随缩放变粗
            DrawHollowRect(renderer, new Rectangle((int)screenPos.X, (int)screenPos.Y, (int)screenW, (int)screenH),
                Color.Cyan, 2);

            // 5. 绘制原点 (同样转换 WorldPosition)
            var screenOrigin = renderer.TransformVector(vis.WorldPosition);
            if (renderer is ScreenRenderer sr) {
                sr.SpriteBatch.Draw(Content.Tex.Pixel, screenOrigin - new Vector2(3, 3),
                null, Color.Yellow, 0, Vector2.Zero, new Vector2(6, 6), default, 0);
                // === 画原点（世界空间）===
                // sr.SpriteBatch.Draw(
                    // texture: Content.Tex.Pixel,
                    // position: vis.WorldPosition,
                    // color: Color.Yellow
                // );
            }
            
            
        }
    }

    
    public static void DrawHollowRect(Renderer renderer, Rectangle rect, Color color, int thickness) {
        if (renderer is not ScreenRenderer sr) return;

        // 直接在屏幕空间绘制四条边
        sr.SpriteBatch.Draw(Content.Tex.Pixel, new Rectangle(rect.X, rect.Y, rect.Width, thickness), color);
        sr.SpriteBatch.Draw(Content.Tex.Pixel, new Rectangle(rect.X, rect.Y, thickness, rect.Height), color);
        sr.SpriteBatch.Draw(Content.Tex.Pixel,
            new Rectangle(rect.X + rect.Width - thickness, rect.Y, thickness, rect.Height), color);
        sr.SpriteBatch.Draw(Content.Tex.Pixel,
            new Rectangle(rect.X, rect.Y + rect.Height - thickness, rect.Width, thickness), color);
    }
}