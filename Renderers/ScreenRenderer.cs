using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cornifer.Renderers {
    public class ScreenRenderer(SpriteBatch spriteBatch) : Renderer {
        public SpriteBatch SpriteBatch { get; } = spriteBatch;
        public override Vector2 Size => SpriteBatch.GraphicsDevice.Viewport.Bounds.Size.ToVector2();

        public RenderTarget2D? RenderTarget2D { get; private set; }
    
    
        [MemberNotNull(nameof(RenderTarget2D))]
        public void UpdateBuffer(GraphicsDevice device, int width, int height) {
            if (RenderTarget2D != null && RenderTarget2D.Width == width && RenderTarget2D.Height == width) return;
            RenderTarget2D?.Dispose();

            RenderTarget2D = new RenderTarget2D(device, width, height, false, SurfaceFormat.Color, DepthFormat.None);
        }
        
        
        public void DrawTexture(Texture2D texture, Vector2 worldPos, Rectangle? source, Vector2? worldSize,
            Color? color, Vector2 origin, float rotation, Vector2? scaleOverride = null) {
            Vector2 texScale;
            if (scaleOverride.HasValue)
                texScale = scaleOverride.Value;
            else {
                var texSize = source?.Size.ToVector2() ?? new Vector2(texture.Width, texture.Height);
                texScale = worldSize.HasValue ? worldSize.Value / texSize : Vector2.One;
                texScale *= Scale;
            }

            SpriteBatch.Draw(texture, TransformVector(worldPos), source, color ?? Color.White, rotation, origin,
                texScale, SpriteEffects.None, 0);
        }

        public override void DrawTexture(Texture2D texture, Vector2 worldPos, Rectangle? source, Vector2? worldSize,
            Color? color, Vector2? scaleOverride = null) {
            DrawTexture(texture, worldPos, source, worldSize, color, Vector2.Zero, 0f, scaleOverride);
        }
    }
}