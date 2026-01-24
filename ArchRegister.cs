using Cornifer.Renderers;
using Cornifer.Systems;
using Microsoft.Xna.Framework;

namespace Cornifer;

public static class ArchRegister {
    public static void Update() {
        InteractionSystem.Update(App.WorldCamera);
        // 重建四叉树 (假设地图边界为 0, 0,10000,10000)
        SpatialSystem.RebuildIndex(Map.World, new Rectangle(0, 0, 10000, 10000));
        HierarchySystem.Update(Map.World);
    }
    
    public static void Draw(Renderer renderer) {
        VisualSystem.Draw(Map.World, renderer);
        GizmoSystem.Draw(renderer, Map.SelectedEntities);
        InteractionSystem.DrawSelectionMarquee(renderer);
    }
}