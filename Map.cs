using System;
using System.Collections.Generic;
using System.Linq;
using Arch.Core;
using Cornifer.Input;
using Cornifer.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Cornifer;

public static class Map {
    // ECS 核心
    public static World World { get; private set; } = null!;
    public static HashSet<Entity> SelectedEntities { get; } = [];
    public static bool IsDragging { get; set; } // 是否正在拖拽实体

    public static void Initialize() {
        World = World.Create();
    }

    /// <summary>
    /// 在指定世界坐标放置一个实体
    /// </summary>
    public static Entity Place(string name, Vector2 worldPos, Texture2D tex, Layer layer = Layer.Objects) {
        Console.WriteLine($"Placing Entity: {name} at {worldPos} in layer {layer}");
        return World.Create(
            new Identifier { Name = name },
            new Visual {
                Texture = tex,
                Visible = true,
                WorldPosition = worldPos,
                LocalPosition = new Vector2(tex.Width / 2f, tex.Height / 2f) // 默认原点在正中心
            },
            new Hierarchy(),
            new LayerMember { Layer = layer, Locked = false },
            new Shadow() {
                CornerRadius = 6,
                Amount = 5
            }
        );
    }

    public static void SpawnTestData() {
        // 放置一些随机物体进行测试
        var tex = Content.Tex.Objects; // 替换为你实际的测试贴图
        Random rand = new();

        for (var i = 0; i < 50; i++) {
            var randomPos = new Vector2(rand.Next(-2000, 2000), rand.Next(-2000, 2000));
            var e = Place($"Object_{i}", randomPos, tex, (Layer)rand.Next(0, 4));
            if (rand.Next(0, 2) != 0) continue;
            HierarchySystem.SetParent(e, Place($"Object_Child_{i}", new Vector2(randomPos.X+100, randomPos.Y+100), Content.Tex.SlugcatIcons, (Layer)rand.Next(0, 4)));
        }
    }

    public static void Update(GameTime gt) { }
}