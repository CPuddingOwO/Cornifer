using Cornifer.Renderers;
using Cornifer.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cornifer;

public static class ArchRegister {
    public static void Update(GameTime gt) {
        InteractionSystem.Update(App.WorldCamera);
        // 重建四叉树
        SpatialSystem.RebuildIndex(Map.World, new Rectangle(-10000, -10000, 20000, 20000));
        HierarchySystem.Update(Map.World);
    }
    
    public static void Draw(CameraRenderer renderer) {
        var device = renderer.SpriteBatch.GraphicsDevice;
        
        // device.SetRenderTarget(renderer.RenderTarget2D);
        // device.Clear(Color.Transparent);
        
        device.SetRenderTarget(null);
        device.Clear(Color.Transparent);
        
        renderer.SpriteBatch.Begin(
            blendState: BlendState.AlphaBlend,
            samplerState: SamplerState.PointClamp,
            effect: Content.Eft.Grid
        );
        GridSystem.Draw(Map.World, renderer);
        renderer.SpriteBatch.End();
        
        renderer.SpriteBatch.Begin(
            blendState: BlendState.AlphaBlend,
            samplerState: SamplerState.PointClamp,
            transformMatrix: renderer.Transform,
            effect: Content.Eft.Shadow);
        ShadowSystem.Draw(Map.World, renderer);
        renderer.SpriteBatch.End();

        renderer.SpriteBatch.Begin(
            samplerState: SamplerState.PointClamp,
            transformMatrix: renderer.Transform);
        VisualSystem.Draw(Map.World, renderer); 
        GizmoSystem.Draw(renderer, Map.SelectedEntities);
        HierarchySystem.Draw(Map.World, renderer);
        InteractionSystem.DrawSelectionMarquee(renderer);
        renderer.SpriteBatch.End();
        
        // device.SetRenderTarget(null);
        // device.Clear(Color.CornflowerBlue);
        //
        // renderer.SpriteBatch.Begin(
        //     samplerState: SamplerState.PointClamp, 
        //     blendState: BlendState.AlphaBlend);
        // renderer.SpriteBatch.Draw(renderer.RenderTarget2D, Vector2.Zero, Color.White);
        // renderer.SpriteBatch.End();
    }
}