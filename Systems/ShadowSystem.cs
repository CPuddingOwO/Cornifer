using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Arch.Core;
using Cornifer.Renderers;

namespace Cornifer.Systems;

public static class ShadowSystem {
    // 阴影配置参数
    private const int ShadowSize = 2;
    private static Color _shadowColor = Color.DeepPink;
    private static RenderTarget2D? _bufferA;
    // private static RenderTarget2D? _bufferB;
    
    public static void Draw(CameraRenderer renderer) {
        var device = renderer.SpriteBatch.GraphicsDevice;
        
        if (renderer.RenderTarget2D == null) return;
        var w = renderer.RenderTarget2D.Width;
        var h = renderer.RenderTarget2D.Height;
        
        if (_bufferA == null || _bufferA.Width != w || _bufferA.Height != h) {
            _bufferA?.Dispose();
            _bufferA = new RenderTarget2D(device, w, h, false, SurfaceFormat.Color, DepthFormat.None);
        }
        
        // if (_bufferB == null || _bufferB.Width != w || _bufferB.Height != h) {
        //     _bufferB?.Dispose();
        //     _bufferB = new RenderTarget2D(device, w, h, false, SurfaceFormat.Color, DepthFormat.None);
        // }
        
        Content.Ect.Shadow.Parameters["TextureSize"].SetValue(new Vector2(_bufferA.Width, _bufferA.Height));
        Content.Ect.Shadow.Parameters["ShadowSize"].SetValue(ShadowSize); 
        Content.Ect.Shadow.Parameters["ShadowColor"].SetValue(_shadowColor.ToVector4());
        Content.Ect.Shadow.Parameters["CameraScale"].SetValue(renderer.Scale);
        
        // 2. 渲染剪影 -> Buffer
        device.SetRenderTarget(_bufferA);
        device.Clear(Color.Transparent); 
        renderer.SpriteBatch.Begin(
            samplerState: SamplerState.PointClamp, 
            blendState: BlendState.AlphaBlend);
        VisualSystem.Draw(Map.World, renderer);

        renderer.SpriteBatch.End();
        
        // device.SetRenderTarget(_bufferB);
        device.SetRenderTarget(renderer.RenderTarget2D);
        device.Clear(Color.Transparent);
        
        renderer.SpriteBatch.Begin(
            effect: Content.Ect.Shadow, 
            samplerState: SamplerState.PointClamp, 
            blendState: BlendState.AlphaBlend);
        renderer.SpriteBatch.Draw(_bufferA, Vector2.Zero, Color.White); 
        renderer.SpriteBatch.End();   
        
        // device.SetRenderTarget(renderer.RenderTarget2D);
        // renderer.SpriteBatch.Begin(
        //     samplerState: SamplerState.PointClamp, 
        //     blendState: BlendState.AlphaBlend);
        // renderer.SpriteBatch.Draw(_bufferB, Vector2.Zero, Color.White);
        // renderer.SpriteBatch.End();
    }
}