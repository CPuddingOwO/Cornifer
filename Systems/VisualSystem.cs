using System;
using Arch.Core;
using Cornifer.Renderers;
using Microsoft.Xna.Framework;

namespace Cornifer.Systems;

public static class VisualSystem {
    public static void Draw(World world, CameraRenderer renderer) {
        foreach (Layer layer in Enum.GetValues(typeof(Layer))) {
            var query = new QueryDescription().WithAll<Visual, LayerMember, Shadow>();
            world.Query(in query, (ref Visual vis, ref LayerMember lm, ref Shadow sha) => {
                if (lm.Layer != layer || !vis.Visible) return;

                // 左下角为原点 
                // DrawPos.Y = World.Y - (Texture.H - Local.Y)
                Vector2 drawPos = new(
                    vis.WorldPosition.X - vis.LocalPosition.X,
                    vis.WorldPosition.Y - (vis.Texture.Height - vis.LocalPosition.Y)
                );
                
                // App.SpriteBatch.Draw(vis.Texture, drawPos, Color.White);
                renderer.DrawTexture(
                    vis.Texture, 
                    drawPos, 
                    source: null, 
                    worldSize: null, // 如果需要缩放可以传 vis.Size
                    color: Color.White, 
                    origin: Vector2.Zero, 
                    rotation: 0f
                );
            });
        }
    }
}