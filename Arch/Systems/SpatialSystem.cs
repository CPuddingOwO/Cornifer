using System.Collections.Generic;
using System.Linq;
using Arch.Core;
using Arch.Core.Extensions;
using Cornifer.Helpers;
using Microsoft.Xna.Framework;

namespace Cornifer.Arch.Systems;

public static class SpatialSystem {
    private static SpatialNode? _root;
    private static readonly List<Entity> CandidateBuffer = new(16);

    // 空间内容边界

    // 所有实体列表
    private static readonly List<Entity> _allEntities = new(128);


    public static Rectangle ContentBounds { get; private set; }

    public static IReadOnlyList<Entity> AllEntities => _allEntities;


    /// <summary>
    ///     重建空间索引。应在每一帧更新坐标后调用。
    /// </summary>
    public static void RebuildIndex(World world, Rectangle mapSize) {
        _root ??= new SpatialNode(mapSize);
        _root.Clear();

        _allEntities.Clear();
        int minX = int.MaxValue, minY = int.MaxValue;
        int maxX = int.MinValue, maxY = int.MinValue;
        var hasEntities = false;

        var query = new QueryDescription().WithAll<Visual>();
    
        // 只需要这一趟遍历即可
        world.Query(in query, (Entity entity, ref Visual vis) => {
            // 统一使用 Visual 结构体内部定义的 Bounds
            Rectangle bounds = vis.Bounds;

            // 1. 插入四叉树
            _root.Insert(entity, bounds);

            // 2. 收集所有实体
            _allEntities.Add(entity);

            // 3. 计算 MBR (最小包围矩形)
            if (bounds.Left < minX) minX = bounds.Left;
            if (bounds.Top < minY) minY = bounds.Top;
            if (bounds.Right > maxX) maxX = bounds.Right;
            if (bounds.Bottom > maxY) maxY = bounds.Bottom;
            hasEntities = true;
        });

        if (hasEntities)
            ContentBounds = new Rectangle(minX, minY, maxX - minX, maxY - minY);
        else
            ContentBounds = Rectangle.Empty;
    }

    /// <summary>
    ///     执行像素级点选判定。
    /// </summary>
    /// <param name="worldMousePos">世界空间下的鼠标坐标</param>
    /// <returns>命中的最上层实体</returns>
    public static Entity? GetEntityAtPixel(Vector2 worldMousePos) {
        if (_root == null) return null;

        CandidateBuffer.Clear();
        _root.Query(worldMousePos, CandidateBuffer);

        if (CandidateBuffer.Count == 0) return null;

        // 这里的排序逻辑应遵循 LayerMember 的层级顺序（从前向后）
        var hit = CandidateBuffer
            .OrderByDescending(e => e.Get<LayerMember>().Layer) // 假设枚举值越大越靠前
            .FirstOrDefault(entity => IsPixelHit(entity, worldMousePos));

        if (hit.IsAlive()) return hit;
        return null;
    }

    /// <summary>
    ///     获取矩形区域内所有满足像素级判定的实体。
    /// </summary>
    /// <param name="rect">世界空间下的目标检索矩形</param>
    /// <param name="resultBuffer">用于存储结果的列表</param>
    public static void GetEntitiesInRect(Rectangle rect, List<Entity> resultBuffer) {
        if (_root == null) return;

        // 1. 利用四叉树初步筛选出所有包围盒相交的实体
        CandidateBuffer.Clear();
        _root.Query(rect, CandidateBuffer);

        if (CandidateBuffer.Count == 0) return;

        // 2. 遍历候选者，执行像素精度的相交检查
        foreach (var entity in CandidateBuffer)
            if (IsRectPixelHit(entity, rect))
                resultBuffer.Add(entity);
    }

    /// <summary>
    ///     判定实体的非透明像素是否与指定矩形区域有交集。
    /// </summary>
    private static bool IsRectPixelHit(Entity entity, Rectangle worldRect) {
        ref var vis = ref entity.Get<Visual>();
        var entityBounds = vis.Bounds; // 使用统一的 Bounds

        var intersection = Rectangle.Intersect(entityBounds, worldRect);
        if (intersection.IsEmpty) return false;

        for (var y = intersection.Top; y < intersection.Bottom; y++)
        for (var x = intersection.Left; x < intersection.Right; x++) {
            // 核心：这里的换算逻辑必须和 IsPixelHit 一致
            var texX = x - entityBounds.X;
            var texY = y - entityBounds.Y;

            if (TextureCache.GetPixelAt(vis.Texture, texX, texY).A > 0) return true;
        }
        return false;
    }

    private static bool IsPixelHit(Entity entity, Vector2 pos) {
        ref var vis = ref entity.Get<Visual>();
        var bounds = vis.Bounds;

        // 只要把鼠标点减去矩形的左上角，就是纹理坐标
        var localX = (int)(pos.X - bounds.X);
        var localY = (int)(pos.Y - bounds.Y);

        if (localX < 0 || localX >= bounds.Width || localY < 0 || localY >= bounds.Height)
            return false;

        return TextureCache.GetPixelAt(vis.Texture, localX, localY).A > 0;
    }
}