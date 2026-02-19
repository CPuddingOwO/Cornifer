using System;
using System.Collections.Generic;
using Arch.Core;
using Cornifer.Arch;
using Cornifer.Arch.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cornifer;

public static class Map {
    // ECS 核心
    public static World World { get; private set; } = null!;
    public static HashSet<Entity> SelectedEntities { get; } = [];

    public static void Initialize() {
        World = World.Create();
    }

    /// <summary>
    ///     在指定世界坐标放置一个实体
    /// </summary>
    public static Entity Place(
        string name,
        Vector2 worldPos,
        Texture2D tex,
        Layer layer
    ) {
        Console.WriteLine($"Placing Entity: {name} at {worldPos} in layer {layer}");

        const int shadowAmount = 2; // 阴影扩展量

        var sdf = ShadowSystem.GetOrCreateSdf(tex, shadowAmount + 1);

        return World.Create(
            new Identifier { Name = name },
            new Visual {
                Texture = tex,
                Visible = true,
                AnchorPoint = worldPos,
                // TextureCenterOffset = new Vector2(MathF.Round(tex.Width / 2f), MathF.Round(tex.Height / 2f))
                TextureCenterOffset = Vector2.Zero
            },
            new Hierarchy(),
            new LayerMember { Layer = layer, Locked = false },
            new Shadow {
                SdfTexture = sdf,
                Amount = shadowAmount,
                Color = Color.Black,
                Offset = new Vector2(-shadowAmount - 1, -shadowAmount - 1)
            }
        );
    }


    public static void SpawnTestData() {
        // 放置一些随机物体进行测试
        var tex = Content.Tex.Objects;
        Random rand = new();

        for (var i = 0; i < 100; i++) {
            var randomPos = new Vector2(rand.Next(-1000, 1000), rand.Next(-1000, 1000));
            var e = Place($"Object_{i}", randomPos, tex, (Layer)rand.Next(0, 4));
            if (rand.Next(0, 2) != 0) continue;
            HierarchySystem.SetParent(
                e,
                Place($"Object_Child_{i}", new Vector2(randomPos.X + 100, randomPos.Y + 100), Content.Tex.SlugcatIcons,
                    (Layer)rand.Next(0, 4)),
                Vector2.Zero,
                Vector2.Zero
            );
        }

        // 在原点放置一个中心物体
        Place("Object_Center", Vector2.Zero, Content.Tex.MiscSprites, Layer.Object);
    }
}