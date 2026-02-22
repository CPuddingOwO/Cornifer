using System.Collections.Generic;
using Arch.Core;
using Arch.Core.Extensions;
using Cornifer.Renderers;
using Microsoft.Xna.Framework;

namespace Cornifer.Arch.Systems;

public static class GizmoSystem {
    /// <summary>
    /// 高亮传入的实体集合
    /// </summary>
    /// <param name="renderer"></param>
    /// <param name="entities"></param>
    public static void HighlightEntity(ScreenRenderer renderer, HashSet<Entity> entities) {
        foreach (var entity in entities) {
            if (!entity.IsAlive()) return;

            ref var vis = ref entity.Get<Visual>();
            var texSize = new Vector2(vis.Texture.Width, vis.Texture.Height);

            // 1. 绘制高亮矩形 (从左上角开始)
            Rect(renderer, vis.VisualTopLeftPosition, texSize, Color.Cyan, 1);
        
            // 2. 绘制锚点 (在物体中心)
            renderer.SpriteBatch.Draw(
                Content.Tex.Pixel, 
                vis.AnchorPosition, 
                null, 
                Color.Yellow, 
                0f, 
                new Vector2(0.5f, 0.5f), // 让 1x1 的像素点也居中显示在锚点上
                1f, 
                default, 
                0f
            );
        }
    }

    public static void RectFilled(ScreenRenderer renderer, Vector2 position, Vector2 size, Color color, float thickness) {
        renderer.SpriteBatch.Draw(Content.Tex.Pixel,
            position,
            // position + new Vector2(size.X - thickness, size.Y - thickness),
            null,
            color,
            0,
            Vector2.Zero,
            new Vector2(size.X + thickness, size.Y + thickness),
            default,
            0);
    }

    public static void Rect(ScreenRenderer renderer, Vector2 position, Vector2 size, Color color, float thickness) {
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