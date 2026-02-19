using System.Collections.Generic;
using Arch.Core;
using Cornifer.Rw;
using Microsoft.Xna.Framework;

namespace Cornifer.Placements;

public abstract record PlacementDescriptor(Vector2 Position, Mod Mod) {
    public List<Entity>? Children = null;

    // 父子关系
    public Entity? Parent = null;
}

public abstract class PlacementHandler<T> where T : PlacementDescriptor {
    // 负责将 Descriptor 转换为 Arch 的 Entity
    public abstract Entity Place(World world, T descriptor);

    // 负责清理（通常 Arch 只需要 Destroy，但如果有外部资源如 RenderTarget 则需在此释放）
    public virtual void Remove(World world, Entity entity) {
        if (world.IsAlive(entity)) world.Destroy(entity);
    }
}