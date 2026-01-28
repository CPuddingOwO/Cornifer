using System.Collections.Generic;
using Arch.Core;
using Arch.Core.Extensions;
using Cornifer.Helpers;
using Cornifer.Renderers;
using Microsoft.Xna.Framework;

namespace Cornifer.Systems;

public static class HierarchySystem {
    private static readonly QueryDescription Query = new QueryDescription().WithAll<Visual, Hierarchy>();

    public static void Update(World world) { }

    public static void SetParent(Entity? parent, Entity child, Vector2? source = null, Vector2? target = null) {
        ref var childHier = ref child.Get<Hierarchy>();

        // 1. 如果已有父物体，先从旧父物体的列表里移除自己
        if (childHier.Parent.HasValue && childHier.Parent.Value.IsAlive()) {
            ref var oldParentHier = ref childHier.Parent.Value.Get<Hierarchy>();
            oldParentHier.Children.Remove(child);
        }

        // 2. 设置新父物体
        childHier.Parent = parent;

        // 3. 将自己添加到新父物体的 Children 列表
        if (!parent.HasValue || !parent.Value.IsAlive()) return;
        
        ref var newParentHier = ref parent.Value.Get<Hierarchy>();
        if (!newParentHier.Children.Contains(child))
            newParentHier.Children.Add(child);
        
        // 4. 设置连线位置
        ref var childVisual = ref child.Get<Visual>();
        ref var parentVisual = ref parent.Value.Get<Visual>();
        
        // 默认连线位置为贴图中心
        childHier.SourceOffset = source ?? new Vector2(parentVisual.Texture.Width / 2f, -parentVisual.Texture.Height / 2f);
        childHier.TargetOffset = target ?? new Vector2(childVisual.Texture.Width / 2f, -childVisual.Texture.Height / 2f);
        // childHier.SourceOffset = source ?? ;
        // childHier.TargetOffset = target ?? ;
    }

    public static void Draw(World world, Renderer renderer) {
        if (renderer is not CameraRenderer cameraRenderer) return;
        // 绘制父子关系线条    
        world.Query(in Query, (Entity entity, ref Visual vis, ref Hierarchy hier) => {
            if (!hier.Parent.HasValue || !hier.Parent.Value.IsAlive()) return;

            ref var parentVis = ref hier.Parent.Value.Get<Visual>();
            cameraRenderer.SpriteBatch.DrawLine(parentVis.WorldPosition + hier.SourceOffset, vis.WorldPosition + hier.TargetOffset, Color.Yellow);
        });
    }
}