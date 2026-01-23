using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cornifer.Helpers;

public static class Texture2DExtensions {
    public static Vector2 Size(this Texture2D texture) {
        return new Vector2(texture.Width, texture.Height);
    }
}