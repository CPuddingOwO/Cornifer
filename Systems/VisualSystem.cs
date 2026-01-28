using System;
using Arch.Core;
using Cornifer.Renderers;
using Microsoft.Xna.Framework;

namespace Cornifer.Systems;

public static class VisualSystem {
    public static void Draw(World world, ScreenRenderer renderer) {
        foreach (Layer layer in Enum.GetValues(typeof(Layer))) {
            var query = new QueryDescription().WithAll<Visual, LayerMember>();
            world.Query(in query, (ref Visual vis, ref LayerMember lm) => {
                if (lm.Layer != layer || !vis.Visible) return;

                // 左下角为原点 
                // DrawPos.Y = World.Y - (Texture.H - Local.Y)
                var drawPos = new Vector2(
                    vis.WorldPosition.X - vis.OriginOffset.X,
                    vis.WorldPosition.Y - (vis.Texture.Height - vis.OriginOffset.Y)
                );
                
                renderer.SpriteBatch.Draw(vis.Texture, drawPos, Color.White);
            });
        }
    }
}