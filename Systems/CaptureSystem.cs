using System;
using Arch.Core;
using Cornifer.Renderers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cornifer.Systems;

public static class CaptureSystem {
    private static ScreenRenderer _renderer = null!;
    
    public static void Initialize(SpriteBatch spriteBatch) {
        _renderer = new ScreenRenderer(spriteBatch);
    }
    
    public static Texture2D Capture() {
        var device = _renderer.SpriteBatch.GraphicsDevice;
        var width = SpatialSystem.ContentBounds.Width;
        var height = SpatialSystem.ContentBounds.Height;
        var buffer = new RenderTarget2D(device, width, height);
        
        // --- 绘制Shadow---
        var shadowBuffer = new RenderTarget2D(device, width, height);
        Content.Ect.Shadow.Parameters["TextureSize"].SetValue(new Vector2(shadowBuffer.Width, shadowBuffer.Height));
        Content.Ect.Shadow.Parameters["ShadowSize"].SetValue(1); 
        Content.Ect.Shadow.Parameters["ShadowColor"].SetValue(Color.DeepPink.ToVector4());
        Content.Ect.Shadow.Parameters["CameraScale"].SetValue(1);
        
        // 2. 渲染剪影 -> Buffer
        device.SetRenderTarget(shadowBuffer);
        device.Clear(Color.Transparent); 
        _renderer.SpriteBatch.Begin(samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);
        foreach (var e in SpatialSystem.AllEntities) {
            var vis = Map.World.Get<Visual>(e);
            if (!vis.Visible) continue;
        
            Vector2 drawPos = new(
                MathF.Round(vis.WorldPosition.X - vis.LocalPosition.X),
                MathF.Round(vis.WorldPosition.Y - (vis.Texture.Height - vis.LocalPosition.Y))
            );
            
            _renderer.SpriteBatch.Draw(vis.Texture, drawPos, Color.White);
        }
        _renderer.SpriteBatch.End();
        
        device.SetRenderTarget(buffer);
        device.Clear(Color.Transparent);
        
        _renderer.SpriteBatch.Begin(effect: Content.Ect.Shadow, samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);
        _renderer.SpriteBatch.Draw(shadowBuffer, Vector2.Zero, Color.White); 
        _renderer.SpriteBatch.End();
        
        // -----------------
        // 绘制所有Entity到 _buffer
        _renderer.SpriteBatch.Begin(samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);
        
        foreach (var e in SpatialSystem.AllEntities) {
            var vis = Map.World.Get<Visual>(e);
            if (!vis.Visible) continue;
        
            Vector2 drawPos = new(
                MathF.Round(vis.WorldPosition.X - vis.LocalPosition.X),
                MathF.Round(vis.WorldPosition.Y - (vis.Texture.Height - vis.LocalPosition.Y))
            );
            
            _renderer.SpriteBatch.Draw(vis.Texture, drawPos, Color.White);
        }
        
        _renderer.SpriteBatch.End();

        // 重置渲染目标回默认
        device.SetRenderTarget(null);

        // 将 _buffer 的内容复制到一个新的 Texture2D
        Texture2D capturedTexture = new(device, width, height);
        var data = new Color[width * height];
        buffer.GetData(data);
        capturedTexture.SetData(data);
        buffer.Dispose();

        return capturedTexture;
    }
}