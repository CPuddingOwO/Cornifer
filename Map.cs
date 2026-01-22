using System.Collections.Generic;
using Arch.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cornifer;

public static class Map {
    // ECS 核心
    public static World World { get; private set; } = null!;
    public static Entity? SelectedEntity { get; set; }
    
    public static void Initialize() {
        World = World.Create();
    }

    // 放置一个简单实体
    public static Entity Place(string name, Vector2 pos) {
        return World.Create(
            new Identifier { Name = name },
            new Visual {
                Visible = true,
                Texture = Content.Tex.Objects,
                Size = new Vector2(2, 1),
                WorldPosition = pos,
                LocalPosition = Vector2.Zero,
                OffsetPosition = Vector2.Zero
            },
            new LayerMember {
                Layer = Layer.Objects
            }
            
        );
    }
    
    public static void Update(GameTime gt) {
        ArchRegister.Update();
        
    }
}