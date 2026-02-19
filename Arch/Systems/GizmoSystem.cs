using System.Collections.Generic;
using Arch.Core;
using Arch.Core.Extensions;
using Cornifer.Helpers;
using Cornifer.Renderers;
using Microsoft.Xna.Framework;

namespace Cornifer.Arch.Systems;

public static class GizmoSystem {
    public static void Draw(ScreenRenderer renderer, HashSet<Entity> selectedEntities) {
        foreach (var entity in selectedEntities) {
            if (!entity.IsAlive()) return;

            ref var vis = ref entity.Get<Visual>();

            // 1. 计算物体在世界空间中的左上角
            Vector2 worldDrawPos = new(
                vis.WorldPosition.X - vis.OriginOffset.X,
                vis.WorldPosition.Y - (vis.Texture.Height - vis.OriginOffset.Y)
            );

            // 注意：这里的厚度 thickness 设为 2，它在屏幕上永远是 2 像素，不会随缩放变粗
            DrawHollowRect(renderer, worldDrawPos, vis.Texture.Size(), Color.Cyan, 1);

            renderer.SpriteBatch.Draw(Content.Tex.Pixel, vis.WorldPosition - new Vector2(3, 3),
                null, Color.Yellow, 0, Vector2.Zero, new Vector2(6, 6), default, 0);
        }
    }


    public static void DrawHollowRect(ScreenRenderer renderer, Vector2 position, Vector2 size, Color color,
        int thickness) {
        renderer.SpriteBatch.Draw(Content.Tex.Pixel,
            position,
            null,
            color,
            0,
            Vector2.Zero,
            new Vector2(size.X, thickness),
            default,
            0);

        renderer.SpriteBatch.Draw(Content.Tex.Pixel,
            position,
            null,
            color,
            0,
            Vector2.Zero,
            new Vector2(thickness, size.Y),
            default,
            0);

        renderer.SpriteBatch.Draw(Content.Tex.Pixel,
            position + new Vector2(size.X - thickness, 0),
            null,
            color,
            0,
            Vector2.Zero,
            new Vector2(thickness, size.Y),
            default,
            0);

        renderer.SpriteBatch.Draw(Content.Tex.Pixel,
            position + new Vector2(0, size.Y - thickness),
            null,
            color,
            0,
            Vector2.Zero,
            new Vector2(size.X, thickness),
            default,
            0);
    }
}