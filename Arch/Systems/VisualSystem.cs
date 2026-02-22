using System;
using Arch.Core;
using Cornifer.Renderers;
using Microsoft.Xna.Framework;

namespace Cornifer.Arch.Systems;

public static class VisualSystem {
    public static void Draw(World world, ScreenRenderer renderer) {
        foreach (Layer layer in Enum.GetValues(typeof(Layer))) {
            var query = new QueryDescription().WithAll<Visual, LayerMember>();
            world.Query(in query, (ref Visual vis, ref LayerMember lm) => {
                if (lm.Layer != layer || !vis.Visible) return;
                // renderer.SpriteBatch.Draw(vis.Texture, vis.CenterPosition, Color.White);
                renderer.SpriteBatch.Draw(
                    vis.Texture, 
                    vis.AnchorPosition,
                    null, 
                    Color.White, 
                    0f, 
                    vis.TextureOffset,
                    1f, 
                    default,
                    0f
                );
            });
        }
    }
}