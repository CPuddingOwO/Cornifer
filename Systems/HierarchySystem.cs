using Arch.Core;
using Arch.Core.Extensions;

namespace Cornifer.Systems;

public static class HierarchySystem {
    private static readonly QueryDescription Query = new QueryDescription().WithAll<Visual, Hierarchy>();

    public static void Update(World world) {
        // 简单起见，这里先做一级处理。如果需要深度递归，建议使用递归函数
        world.Query(in Query, (Entity entity, ref Visual vis, ref Hierarchy hier) => {
            if (hier.Parent.HasValue && hier.Parent.Value.IsAlive()) {
                ref var parentVis = ref hier.Parent.Value.Get<Visual>();
                vis.WorldPosition = parentVis.WorldPosition + vis.OffsetPosition;
                return;
            }

            // 没有父物体，世界坐标就是偏移坐标
            vis.WorldPosition = vis.OffsetPosition;
        });
    }
}