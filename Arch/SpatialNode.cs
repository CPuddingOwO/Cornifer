using System.Collections.Generic;
using Arch.Core;
using Microsoft.Xna.Framework;

namespace Cornifer.Arch;

/// <summary>
///     工业级四叉树实现，用于快速检索空间内的实体。
/// </summary>
public class SpatialNode(Rectangle bounds, int depth = 0) {
    private const int MaxEntitiesPerNode = 8;
    private const int MaxDepth = 6;

    private readonly Rectangle _bounds = bounds;
    private readonly List<SpatialEntry> _entries = new();
    private SpatialNode[]? _children;

    /// <summary>
    ///     将实体插入四叉树。
    /// </summary>
    public void Insert(Entity entity, Rectangle bounds) {
        if (!_bounds.Intersects(bounds)) return;

        if (_children != null) {
            foreach (var child in _children) child.Insert(entity, bounds);
            return;
        }

        _entries.Add(new SpatialEntry { Entity = entity, Bounds = bounds });

        if (_entries.Count > MaxEntitiesPerNode && depth < MaxDepth) Split();
    }

    /// <summary>
    ///     检索包含特定点的所有实体。
    /// </summary>
    public void Query(Vector2 point, List<Entity> results) {
        if (!_bounds.Contains(point)) return;

        if (_children != null)
            foreach (var child in _children)
                child.Query(point, results);
        else
            foreach (var entry in _entries)
                if (entry.Bounds.Contains(point))
                    results.Add(entry.Entity);
    }

    /// <summary>
    ///     检索与指定矩形区域相交的所有实体。
    /// </summary>
    /// <param name="range">世界空间下的检索矩形</param>
    /// <param name="results">用于存储结果的列表（建议由调用方预分配内存）</param>
    public void Query(Rectangle range, List<Entity> results) {
        // 快速排斥：如果查询区域与当前节点完全不相交，直接跳过
        if (!_bounds.Intersects(range)) return;

        // 如果存在子节点，则递归向下查找
        if (_children != null)
            // 使用普通循环代替 foreach 减少迭代器开销
            for (var i = 0; i < 4; i++)
                _children[i].Query(range, results);
        // 如果是叶子节点，进行碰撞判定
        else
            for (var i = 0; i < _entries.Count; i++) {
                var entry = _entries[i];

                // 判定实体的包围盒是否与检索区域相交
                if (entry.Bounds.Intersects(range)) results.Add(entry.Entity);
            }
    }

    private void Split() {
        var subWidth = _bounds.Width / 2;
        var subHeight = _bounds.Height / 2;
        var x = _bounds.X;
        var y = _bounds.Y;

        _children = [
            new SpatialNode(new Rectangle(x, y, subWidth, subHeight), depth + 1),
            new SpatialNode(new Rectangle(x + subWidth, y, subWidth, subHeight), depth + 1),
            new SpatialNode(new Rectangle(x, y + subHeight, subWidth, subHeight), depth + 1),
            new SpatialNode(new Rectangle(x + subWidth, y + subHeight, subWidth, subHeight), depth + 1)
        ];

        foreach (var entry in _entries)
        foreach (var child in _children)
            child.Insert(entry.Entity, entry.Bounds);

        _entries.Clear();
    }

    public void Clear() {
        _entries.Clear();
        _children = null; // TODO: 递归清理子节点以减少GC压力
    }

    private struct SpatialEntry {
        public Entity Entity;
        public Rectangle Bounds;
    }
}