using System;
using System.Collections.Generic;
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
    public static Entity? SelectedEntity { get; set; }

    public static void Initialize() {
        World = World.Create();
    }

    /// <summary>
    /// 在指定世界坐标放置一个实体
    /// </summary>
    public static Entity Place(string name, Vector2 worldPos, Texture2D tex, Layer layer = Layer.Objects) {
        return World.Create(
            new Identifier { Name = name },
            new Visual {
                Texture = tex,
                Visible = true,
                WorldPosition = worldPos,
                OffsetPosition = worldPos, // 初始时 Offset = World
                LocalPosition = new Vector2(tex.Width / 2, tex.Height / 2) // 默认原点在正中心
            },
            new LayerMember { Layer = layer, Locked = false }
        );
    }

    public static void SpawnTestData() {
        // 放置一些随机物体进行测试
        var tex = Content.Tex.SlugcatIcons; // 替换为你实际的测试贴图
        Random rand = new();

        for (var i = 0; i < 50; i++) {
            var randomPos = new Vector2(rand.Next(0, 2000), rand.Next(0, 2000));
            Place($"Object_{i}", randomPos, tex, (Layer)rand.Next(0, 4));
        }
    }

    public static void Update(GameTime gt) {
        ArchRegister.Update();
        
        if (InputHandler.Select.JustPressed && !Interface.IsHovered) {
            var worldMouse = App.WorldCamera.InverseTransformVector(InputHandler.MouseState.Position.ToVector2());
            SelectedEntity = SpatialSystem.GetEntityAtPixel(worldMouse);
        }
        
    }
}