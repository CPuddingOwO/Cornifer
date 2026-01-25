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
        
        device.SetRenderTarget(buffer);
        device.Clear(Color.Transparent);
        
        _renderer.SpriteBatch.Begin(
            blendState: BlendState.AlphaBlend,
            samplerState: SamplerState.PointClamp,
            transformMatrix: _renderer.Transform,
            effect: Content.Ect.Shadow);
        ShadowSystem.Draw(Map.World, _renderer);
        _renderer.SpriteBatch.End();
        
        
        _renderer.SpriteBatch.Begin(samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);
        VisualSystem.Draw(Map.World, _renderer);
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