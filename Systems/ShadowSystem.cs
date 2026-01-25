using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Arch.Core;
using Cornifer.Renderers;

namespace Cornifer.Systems;

public static class ShadowSystem {
    private static readonly Dictionary<(Texture2D tex, int pad), Texture2D> SdfCache = new();

    
    public static void Draw(World world, ScreenRenderer renderer) {
        foreach (Layer layer in Enum.GetValues(typeof(Layer))) {
            var query = new QueryDescription().WithAll<Visual, LayerMember, Shadow>();
            world.Query(in query, (ref Visual vis, ref LayerMember lm, ref Shadow sha) => {
                if (lm.Layer != layer || !vis.Visible) return;

                // 左下角为原点 
                // DrawPos.Y = World.Y - (Texture.H - Local.Y)
                Vector2 drawPos = new(
                    vis.WorldPosition.X - vis.LocalPosition.X,
                    vis.WorldPosition.Y - (vis.Texture.Height - vis.LocalPosition.Y)
                );

                drawPos += sha.Offset;

                // 设置 Shader 参数
                Content.Ect.Shadow.Parameters["SdfTexture"]?.SetValue(sha.SdfTexture);
                Content.Ect.Shadow.Parameters["ShadowAmount"]?.SetValue((float)sha.Amount);
                Content.Ect.Shadow.Parameters["ShadowColor"]?.SetValue(sha.Color.ToVector4());

                Content.Ect.Shadow.Parameters["TextureSize"]?.SetValue(
                    new Vector2(
                        sha.SdfTexture.Width,
                        sha.SdfTexture.Height
                    )
                );
                renderer.SpriteBatch.Draw(
                    sha.SdfTexture,
                    drawPos,
                    null,
                    Color.White,
                    0f,
                    Vector2.Zero,
                    1f,
                    SpriteEffects.None,
                    0f
                );
            });
        }
    }

    /// <summary>
    /// 从 Alpha 贴图生成 SDF（Signed Distance Field）
    /// </summary>
    private static void GenerateSdf(Color[] pixels, int width, int height, float padding) {
        var length = width * height;

        // 距离缓存
        var distance = new float[length];

        // 1. 初始化
        for (var i = 0; i < length; i++) {
            // 内部：负距离
            distance[i] = pixels[i].A > 0 ? -padding : padding;
        }

        var radius = (int)MathF.Ceiling(padding);

        // 2. 对每个像素，寻找最近的“反相像素”
        for (var y = 0; y < height; y++) {
            for (var x = 0; x < width; x++) {
                var index = y * width + x;
                var inside = pixels[index].A > 0;

                var best = MathF.Abs(distance[index]);

                for (var oy = -radius; oy <= radius; oy++) {
                    for (var ox = -radius; ox <= radius; ox++) {
                        var nx = x + ox;
                        var ny = y + oy;

                        if (nx < 0 || ny < 0 || nx >= width || ny >= height) continue;

                        var nIndex = ny * width + nx;
                        var nInside = pixels[nIndex].A > 0;

                        if (inside == nInside) continue;

                        var d = MathF.Sqrt(ox * ox + oy * oy);
                        if (d < best)
                            best = d;
                    }
                }

                distance[index] = inside ? -best : best;
            }
        }

        // 3. 归一化并写回 pixels
        for (var i = 0; i < length; i++) {
            var d = distance[i];

            // clamp 到 [-maxDistance, +maxDistance]
            d = Math.Clamp(d, -padding, padding);

            // 映射到 [0,1]
            var sdf = d / padding * 0.5f + 0.5f;

            var v = (byte)(sdf * 255);

            pixels[i] = new Color((int)v, v, v, 255);
        }
    }

    private static Texture2D BuildSdfTexture(Texture2D source, int padding) {
        var srcW = source.Width;
        var srcH = source.Height;

        var w = srcW + padding * 2;
        var h = srcH + padding * 2;

        var pixels = new Color[w * h];
        Array.Fill(pixels, Color.Transparent);

        var srcPixels = new Color[srcW * srcH];
        source.GetData(srcPixels);

        // 把原图拷贝到中间
        for (var y = 0; y < srcH; y++)
        for (var x = 0; x < srcW; x++)
            pixels[(y + padding) * w + (x + padding)] =
                srcPixels[y * srcW + x];

        // 在 padded 图上生成 SDF
        GenerateSdf(pixels, w, h, padding);

        var sdfTex = new Texture2D(
            source.GraphicsDevice,
            w, h, false,
            SurfaceFormat.Color
        );
        sdfTex.SetData(pixels);

        return sdfTex;
    }
    
    public static Texture2D GetOrCreateSdf(Texture2D source, int padding) {
        var key = (source, padding);

        if (SdfCache.TryGetValue(key, out var sdf))
            return sdf;

        sdf = BuildSdfTexture(source, padding);
        SdfCache[key] = sdf;
        return sdf;
    }

}