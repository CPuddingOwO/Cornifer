using Cornifer.Renderers;
using Cornifer.Systems;
using Microsoft.Xna.Framework;

namespace Cornifer;

public static class ArchRegister {
    public static void Update() {
        HierarchySystem.Update(Map.World);
        // 重建四叉树 (假设地图边界为 0,0,5000,5000)
        SpatialSystem.RebuildIndex(Map.World, new Rectangle(0, 0, 5000, 5000));
    }
    
    public static void Draw(Renderer renderer) {
        VisualSystem.Draw(Map.World, renderer);
        GizmoSystem.Draw(renderer, Map.SelectedEntity);
    }
}