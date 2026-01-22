using Cornifer.Systems;

namespace Cornifer;

public static class ArchRegister {
    public static void Update() {
        HierarchySystem.Update(Map.World);   
    }
    
    public static void Draw() {
        RenderSystem.Draw(Map.World);
    }
}