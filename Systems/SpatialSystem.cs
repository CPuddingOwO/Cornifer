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

    /// <summary>
    /// 重建空间索引。应在每一帧更新坐标后调用。
    /// </summary>
    public static void RebuildIndex(World world, Rectangle mapSize) {
        _root ??= new SpatialNode(mapSize);
        _root.Clear();

        var query = new QueryDescription().WithAll<Visual>();
        world.Query(in query, (Entity entity, ref Visual vis) => {
            // 计算实体在世界中的实际矩形范围（基于左下角对齐逻辑反推左上角）
            Rectangle bounds = new(
                (int)(vis.WorldPosition.X - vis.LocalPosition.X),
                (int)(vis.WorldPosition.Y - (vis.Texture.Height - vis.LocalPosition.Y)),
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
        return CandidateBuffer
            .OrderByDescending(e => e.Get<LayerMember>().Layer) // 假设枚举值越大越靠前
            .FirstOrDefault(entity => IsPixelHit(entity, worldMousePos));
    }

    private static bool IsPixelHit(Entity entity, Vector2 worldPos) {
        
        ref var vis = ref entity.Get<Visual>();

        // 1. 将世界坐标转回纹理局部坐标 (Local Space)
        // 计算公式推导：worldPos.X = drawPos.X + localX
        // 所以：localX = worldPos.X - (vis.WorldPos.X - vis.LocalPos.X)
        float localX = worldPos.X - (vis.WorldPosition.X - vis.LocalPosition.X);
        float localY = worldPos.Y - (vis.WorldPosition.Y - (vis.Texture.Height - vis.LocalPosition.Y));

        int texX = (int)localX;
        int texY = (int)localY;

        // 2. 采样像素 Alpha 值
        Color pixel = TextureCache.GetPixelAt(vis.Texture, texX, texY);

        // 如果 Alpha > 0，认为命中了非透明区域
        return pixel.A > 0;
    }
}