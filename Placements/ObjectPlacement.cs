using Arch.Core;
using Cornifer.Arch;
using Cornifer.Rw;
using Microsoft.Xna.Framework;

namespace Cornifer.Placements;

public record ObjectDescriptor(
    Vector2 Position,
    Mod Mod) : PlacementDescriptor(Position, Mod);

public class ObjectPlacementHandler : PlacementHandler<ObjectDescriptor> {
    public override Entity Place(World world, ObjectDescriptor desc) {
        return world.Create(
            new Metadata { SourceMod = desc.Mod }
        );
    }
}