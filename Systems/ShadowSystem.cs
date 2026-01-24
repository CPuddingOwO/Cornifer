using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Arch.Core;
using Cornifer.Renderers;

namespace Cornifer.Systems;

public static class ShadowSystem {
    // 阴影配置参数
    private const int ShadowSize = 1;
    private static Color _shadowColor = Color.DeepPink; 
    private static RenderTarget2D? _buffer;
    // private static RenderTarget2D? _bufferB;

    public static void Draw(World world, CameraRenderer renderer) {
        var device = renderer.SpriteBatch.GraphicsDevice;
        
        // 1. 更新 Buffer
        UpdateBuffers(device);
        
        Content.Ect.Shadow.Parameters["TextureSize"].SetValue(new Vector2(_buffer.Width, _buffer.Height));
        Content.Ect.Shadow.Parameters["ShadowSize"].SetValue(ShadowSize); 
        Content.Ect.Shadow.Parameters["ShadowColor"].SetValue(_shadowColor.ToVector4());
        Content.Ect.Shadow.Parameters["CameraScale"].SetValue(renderer.Scale);
        
        // 2. 渲染剪影 -> Buffer A
        device.SetRenderTarget(_buffer);
        device.Clear(Color.Transparent); 
        renderer.SpriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: renderer.Transform, blendState: BlendState.AlphaBlend);
        var query = new QueryDescription().WithAll<Visual>();
        world.Query(in query, (Entity entity, ref Visual vis) => {
            if (!vis.Visible) return;
    
            Vector2 drawPos = new(
                vis.WorldPosition.X - vis.LocalPosition.X,
                vis.WorldPosition.Y - (vis.Texture.Height - vis.LocalPosition.Y)
            );
            
            renderer.SpriteBatch.Draw(vis.Texture, drawPos, Color.White);
        });
        renderer.SpriteBatch.End();

        // 3. 水平 Pass: Buffer A -> Buffer B
        // device.SetRenderTarget(_bufferB);
        // device.Clear(Color.Transparent);
        //
        // renderer.SpriteBatch.Begin(effect: Content.Ect.Shadow, samplerState: SamplerState.PointClamp, blendState: BlendState.Opaque);
        // Content.Ect.Shadow.CurrentTechnique.Passes["Horizontal"].Apply();
        // renderer.SpriteBatch.Draw(_bufferA, Vector2.Zero, Color.White); 
        // renderer.SpriteBatch.End();

        // 4. 垂直 Pass: Buffer B -> 屏幕
        device.SetRenderTarget(null);
        device.Clear(Color.CornflowerBlue);
        
        renderer.SpriteBatch.Begin(effect: Content.Ect.Shadow, samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);
        // Content.Ect.Shadow.CurrentTechnique.Passes["Vertical"].Apply();
        renderer.SpriteBatch.Draw(_buffer, Vector2.Zero, Color.White); 
        renderer.SpriteBatch.End();
    }

    private static void UpdateBuffers(GraphicsDevice device) {
        var w = device.PresentationParameters.BackBufferWidth;
        var h = device.PresentationParameters.BackBufferHeight;

        // 只要有一个为空或大小不对，就全部重刷
        if (_buffer != null && _buffer.Width == w && _buffer.Height == h) return;
        _buffer?.Dispose();
        // _bufferB?.Dispose();

        _buffer = new RenderTarget2D(device, w, h, false, SurfaceFormat.Color, DepthFormat.None);
        // _bufferB = new RenderTarget2D(device, w, h, false, SurfaceFormat.Color, DepthFormat.None);
    }
}