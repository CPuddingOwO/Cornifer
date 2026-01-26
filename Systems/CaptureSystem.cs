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
        var bounds = SpatialSystem.ContentBounds;
        const int padding = 16; // 为捕获区域添加一些填充
        var width = bounds.Width + padding * 2;
        var height = bounds.Height + padding * 2;
        var buffer = new RenderTarget2D(device, width, height);
        _renderer.Position = new Vector2(bounds.Left - padding, bounds.Top -padding);
        _renderer.Scale = 1.0001f;
        
        _renderer.UpdateBuffer(device, width, height);
        // var pixelPerfect = Matrix.CreateTranslation(-0.5f, -0.5f, 0) * _renderer.Transform;
        
        device.SetRenderTarget(buffer);
        device.Clear(Color.Transparent);
        
        _renderer.SpriteBatch.Begin(
            blendState: BlendState.AlphaBlend,
            samplerState: SamplerState.PointClamp,
            // transformMatrix: pixelPerfect,
            transformMatrix: _renderer.Transform,
            // transformMatrix: Matrix.Identity,
            effect: Content.Eft.Shadow);
        ShadowSystem.Draw(Map.World, _renderer);
        _renderer.SpriteBatch.End();
        
        
        _renderer.SpriteBatch.Begin(
            samplerState: SamplerState.PointClamp, 
            // transformMatrix: pixelPerfect,
            transformMatrix: _renderer.Transform,
            blendState: BlendState.AlphaBlend);
        VisualSystem.Draw(Map.World, _renderer);
        
        _renderer.SpriteBatch.End();

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