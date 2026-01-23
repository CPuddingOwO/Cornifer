using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Cornifer.Helpers;

/// <summary>
/// 提供贴图像素数据的快速访问缓存，避免频繁的 GPU 内存回读。
/// </summary>
public static class TextureCache {
    private static readonly Dictionary<Texture2D, Color[]> Data = new();

    public static Color[] GetPixels(Texture2D texture) {
        if (Data.TryGetValue(texture, out var colors)) return colors;

        colors = new Color[texture.Width * texture.Height];
        texture.GetData(colors);
        Data[texture] = colors;
        return colors;
    }

    public static Color GetPixelAt(Texture2D texture, int x, int y) {
        if (x < 0 || y < 0 || x >= texture.Width || y >= texture.Height)
            return Color.Transparent;

        var pixels = GetPixels(texture);
        return pixels[y * texture.Width + x];
    }
}