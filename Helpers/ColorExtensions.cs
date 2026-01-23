using System;
using Microsoft.Xna.Framework;

namespace Cornifer.Helpers;

public static class ColorExtensions {
    public static void SetAlpha(ref this Color color, float alpha) {
        color.A = (byte)(Math.Clamp(alpha, 0, 1) * 255);
    }
}