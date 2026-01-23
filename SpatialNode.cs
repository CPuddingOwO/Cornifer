using System.Collections.Generic;
using Arch.Core;
using Microsoft.Xna.Framework;

namespace Cornifer;

/// <summary>
/// 工业级四叉树实现，用于快速检索空间内的实体。
/// </summary>
public class SpatialNode(Rectangle bounds, int depth = 0) {
    private const int MaxEntitiesPerNode = 8;
    private const int MaxDepth = 6;

    private readonly Rectangle _bounds = bounds;
    private readonly List<SpatialEntry> _entries = new();
    private SpatialNode[]? _children;

    private struct SpatialEntry {
        public Entity Entity;
        public Rectangle Bounds;
    }

    /// <summary>
    /// 将实体插入四叉树。
    /// </summary>
    public void Insert(Entity entity, Rectangle bounds) {
        if (!_bounds.Intersects(bounds)) return;

        if (_children != null) {
            foreach (var child in _children) child.Insert(entity, bounds);
            return;
        }

        _entries.Add(new SpatialEntry { Entity = entity, Bounds = bounds });

        if (_entries.Count > MaxEntitiesPerNode && depth < MaxDepth) {
            Split();
        }
    }

    /// <summary>
    /// 检索包含特定点的所有实体。
    /// </summary>
    public void Query(Vector2 point, List<Entity> results) {
        if (!_bounds.Contains(point)) return;

        if (_children != null) {
            foreach (var child in _children) child.Query(point, results);
        } else {
            foreach (var entry in _entries) {
                if (entry.Bounds.Contains(point)) results.Add(entry.Entity);
            }
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

        foreach (var entry in _entries) {
            foreach (var child in _children) child.Insert(entry.Entity, entry.Bounds);
        }

        _entries.Clear();
    }

    public void Clear() {
        _entries.Clear();
        _children = null;
    }
}