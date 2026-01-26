using Arch.Core;
using Cornifer.Renderers;
using Microsoft.Xna.Framework;

namespace Cornifer.Systems;

public static class GridSystem {
    public static void Draw(World world, ScreenRenderer renderer) {
        var device = renderer.SpriteBatch.GraphicsDevice;
        var effect = Content.Eft.Grid;
        
        float w = device.Viewport.Width;
        float h = device.Viewport.Height;
        
        // Matrix basicProjection = Matrix.CreateOrthographicOffCenter(
            // 0, device.Viewport.Width, device.Viewport.Height, 0, 0, 1);
        
        effect.Parameters["MatrixTransform"]?.SetValue(renderer.Projection);
        effect.Parameters["ViewportSize"]?.SetValue(new Vector2(w, h));
        effect.Parameters["CameraWorldPos"]?.SetValue(renderer.Position);
        effect.Parameters["GridUnit"]?.SetValue(renderer.Scale); // 1世界单位=多少像素
        effect.Parameters["AxisThickness"]?.SetValue(2f);
        effect.Parameters["MajorThickness"]?.SetValue(1f);
        effect.Parameters["MinorDotSize"]?.SetValue(1f);

        effect.Parameters["AxisColor"]?.SetValue(Color.White.ToVector4());
        effect.Parameters["MajorColor"]?.SetValue(new Color(120,120,120).ToVector4());
        effect.Parameters["MinorColor"]?.SetValue(new Color(80,80,80).ToVector4());
        
        renderer.SpriteBatch.Draw(
            Content.Tex.Pixel,
            new Rectangle(0, 0, (int)w, (int)h),
            Color.White
        );
    }
}