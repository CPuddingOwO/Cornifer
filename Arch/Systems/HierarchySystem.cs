using Arch.Core;
using Arch.Core.Extensions;
using Cornifer.Helpers;
using Cornifer.Renderers;
using Microsoft.Xna.Framework;

namespace Cornifer.Arch.Systems;

public static class HierarchySystem {
    private static readonly QueryDescription Query = new QueryDescription().WithAll<Visual, Hierarchy>();

    public static void Update(World world) {
    }

    /// <summary>
    /// 设置父子关系 并可选地指定连线的起点和终点偏移  
    /// [!]无组件检查 必须存在 Hierarchy 和 Visual 组件
    /// </summary>
    /// <param name="child">子节点Entity</param>
    /// <param name="parent">父节点Entity</param>
    /// <param name="sourceOffset">父节点侧端点偏移 左上角为0,0 为null时为锚点坐标</param>
    /// <param name="targetOffset">子节点侧端点偏移 左上角为0,0 为null时为锚点坐标</param>
    public static Entity SetParent(this Entity child, Entity parent, Vector2? sourceOffset = null, Vector2? targetOffset = null) {
        ref var childHier = ref child.Get<Hierarchy>();

        // 1. 移除旧父子关系
        if (childHier.Parent.HasValue && childHier.Parent.Value.IsAlive()) {
            ref var oldParentHier = ref childHier.Parent.Value.Get<Hierarchy>();
            oldParentHier.Children.Remove(child);
        }

        childHier.Parent = parent;

        // 2. 建立新父子关系
        // if (!parent.HasValue || !parent.Value.IsAlive()) return ;
        if (!parent.IsAlive()) return child;

        ref var newParentHier = ref parent.Get<Hierarchy>();
        if (!newParentHier.Children.Contains(child))
            newParentHier.Children.Add(child);

        // 3. 设置连线位置偏移
        // 如果不传入，默认为 Vector2.Zero，即连向 锚点坐标
        // TODO: 目前为从左上角开始
        
        var parentVis = parent.Get<Visual>();
        var childVis = child.Get<Visual>();
        childHier.SourceOffset = sourceOffset - parentVis.TextureOffset ?? Vector2.Zero;
        childHier.TargetOffset = targetOffset - childVis.TextureOffset ?? Vector2.Zero;

        return child;
    }

    public static void Draw(World world, Renderer renderer) {
        if (renderer is not CameraRenderer cameraRenderer) return;

        // 计算物理 2 像素在当前缩放下的宽度，保证线条在屏幕上看起来粗细不变
        // 假设 cameraRenderer 有个 Zoom 属性
        var thickness = 2f / cameraRenderer.Scale; 

        world.Query(in Query, (ref Visual vis, ref Hierarchy hier) => {
            // 条件检查：是否有父物体，且是否开启了连线显示
            if (!hier.Parent.HasValue || !hier.Parent.Value.IsAlive() || !hier.Visible) return;

            ref var parentVis = ref hier.Parent.Value.Get<Visual>();

            // 起点：父物体中心 + 偏移
            var start = parentVis.AnchorPosition + hier.SourceOffset;
            // 终点：子物体中心 + 偏移
            var end = vis.AnchorPosition + hier.TargetOffset;

            cameraRenderer.SpriteBatch.Line(start, end, Color.Yellow * 0.8f, thickness);
        });
    }
}