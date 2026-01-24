using Cornifer.Renderers;
using Cornifer.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cornifer;

public static class ArchRegister {
    public static void Update() {
        InteractionSystem.Update(App.WorldCamera);
        // 重建四叉树
        SpatialSystem.RebuildIndex(Map.World, new Rectangle(-10000, -10000, 20000, 20000));
        HierarchySystem.Update(Map.World);
    }
    
    public static void Draw(CameraRenderer renderer) {
        // --- 1. 阴影 Pass (内部自带切换 RenderTarget 和 Begin/End) ---
        ShadowSystem.Draw(Map.World, renderer);

        // --- 2. 世界空间 Pass (绘制物体主体) ---
        // 使用 renderer.TransformMatrix，坐标受缩放和位移影响
        renderer.SpriteBatch.Begin(samplerState: SamplerState.PointClamp);
    
        VisualSystem.Draw(Map.World, renderer); 
        GizmoSystem.Draw(renderer, Map.SelectedEntities); // 这样就不会报错了
        HierarchySystem.Draw(Map.World, renderer);
        InteractionSystem.DrawSelectionMarquee(renderer);
    
        renderer.SpriteBatch.End();
        
    }
}