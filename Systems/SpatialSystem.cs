using Arch.Core;
using Cornifer.Helpers;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Arch.Core.Extensions;

namespace Cornifer.Systems;

public static class SpatialSystem {
    private static SpatialNode? _root;
    private static readonly List<Entity> CandidateBuffer = new(16);
    
    // 空间内容边界
    private static Rectangle _contentBounds;
    // 所有实体列表
    private static readonly List<Entity> _allEntities = new(128);

    
    public static Rectangle ContentBounds => _contentBounds;
    public static IReadOnlyList<Entity> AllEntities => _allEntities;
    

    /// <summary>
    /// 重建空间索引。应在每一帧更新坐标后调用。
    /// </summary>
    public static void RebuildIndex(World world, Rectangle mapSize) {
        _root ??= new SpatialNode(mapSize);
        _root.Clear();
        
        // 重置统计数据
        _allEntities.Clear();
        int minX = int.MaxValue, minY = int.MaxValue;
        int maxX = int.MinValue, maxY = int.MinValue;
        bool hasEntities = false;

        var query = new QueryDescription().WithAll<Visual>();
        world.Query(in query, (Entity entity, ref Visual vis) => {
            Rectangle bounds = new(
                (int)(vis.WorldPosition.X - vis.OriginOffset.X),
                (int)(vis.WorldPosition.Y - (vis.Texture.Height - vis.OriginOffset.Y)),
                vis.Texture.Width,
                vis.Texture.Height
            );

            // 1. 插入四叉树
            _root.Insert(entity, bounds);

            // 2. 顺便收集所有实体
            _allEntities.Add(entity);

            // 3. 顺便计算最小包围矩形 (MBR)
            if (bounds.Left < minX) minX = bounds.Left;
            if (bounds.Top < minY) minY = bounds.Top;
            if (bounds.Right > maxX) maxX = bounds.Right;
            if (bounds.Bottom > maxY) maxY = bounds.Bottom;
            hasEntities = true;
        });

        // 更新最终边界
        if (hasEntities) {
            _contentBounds = new Rectangle(minX, minY, maxX - minX, maxY - minY);
        } else {
            _contentBounds = Rectangle.Empty;
        }
        
        world.Query(in query, (Entity entity, ref Visual vis) => {
            // 计算实体在世界中的实际矩形范围（基于左下角对齐逻辑反推左上角）
            Rectangle bounds = new(
                (int)(vis.WorldPosition.X - vis.OriginOffset.X),
                (int)(vis.WorldPosition.Y - (vis.Texture.Height - vis.OriginOffset.Y)),
                vis.Texture.Width,
                vis.Texture.Height
            );
            _root.Insert(entity, bounds);
        });
    }

    /// <summary>
    /// 执行像素级点选判定。
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
    /// 获取矩形区域内所有满足像素级判定的实体。
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
        foreach (Entity entity in CandidateBuffer) {
            if (IsRectPixelHit(entity, rect)) {
                resultBuffer.Add(entity);
            }
        }
    }

    /// <summary>
    /// 判定实体的非透明像素是否与指定矩形区域有交集。
    /// </summary>
    private static bool IsRectPixelHit(Entity entity, Rectangle worldRect) {
        ref var vis = ref entity.Get<Visual>();

        // 计算实体的世界空间边界矩形
        Rectangle entityBounds = new(
            (int)(vis.WorldPosition.X - vis.OriginOffset.X),
            (int)(vis.WorldPosition.Y - (vis.Texture.Height - vis.OriginOffset.Y)),
            vis.Texture.Width,
            vis.Texture.Height
        );

        // 计算两个矩形的公共交集部分
        var intersection = Rectangle.Intersect(entityBounds, worldRect);
        if (intersection.IsEmpty) return false;

        // 遍历交集区域内的每一个像素
        for (var y = intersection.Top; y < intersection.Bottom; y++) {
            for (var x = intersection.Left; x < intersection.Right; x++) {
                // 将当前遍历的世界坐标转回纹理局部坐标
                // 公式同 IsPixelHit: localX = x - entityLeft
                var texX = x - entityBounds.X;
                var texY = y - entityBounds.Y;

                // 采样像素 Alpha 值
                var pixel = TextureCache.GetPixelAt(vis.Texture, texX, texY);

                // 只要找到一个非透明像素在矩形内，即视为命中
                if (pixel.A > 0) {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool IsPixelHit(Entity entity, Vector2 worldPos) {
        ref var vis = ref entity.Get<Visual>();

        // 1. 将世界坐标转回纹理局部坐标 (Local Space)
        // 计算公式推导：worldPos.X = drawPos.X + localX
        // 所以：localX = worldPos.X - (vis.WorldPos.X - vis.LocalPos.X)
        float localX = worldPos.X - (vis.WorldPosition.X - vis.OriginOffset.X);
        float localY = worldPos.Y - (vis.WorldPosition.Y - (vis.Texture.Height - vis.OriginOffset.Y));

        int texX = (int)localX;
        int texY = (int)localY;

        // 2. 采样像素 Alpha 值
        Color pixel = TextureCache.GetPixelAt(vis.Texture, texX, texY);

        // 如果 Alpha > 0，认为命中了非透明区域
        return pixel.A > 0;
    }
}