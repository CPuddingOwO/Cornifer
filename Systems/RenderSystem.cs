using System;
using Arch.Core;
using Microsoft.Xna.Framework;

namespace Cornifer.Systems;

public static class RenderSystem {
    public static void Draw(World world) {
        foreach (Layer layer in Enum.GetValues(typeof(Layer))) {
            var query = new QueryDescription().WithAll<Visual, LayerMember>();
            world.Query(in query, (ref Visual vis, ref LayerMember lm) => {
                if (lm.Layer != layer || !vis.Visible) return;

                // 重点：实现你要求的左下角逻辑
                // DrawPos.Y = World.Y - (Texture.H - Local.Y)
                Vector2 drawPos = new(
                    vis.WorldPosition.X - vis.LocalPosition.X,
                    vis.WorldPosition.Y - (vis.Texture.Height - vis.LocalPosition.Y)
                );

                App.SpriteBatch.Draw(vis.Texture, drawPos, Color.White);
            });
        }
    }
}