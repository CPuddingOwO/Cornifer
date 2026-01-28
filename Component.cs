using System.Collections.Generic;
using Arch.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cornifer;

public struct Identifier {
    public string Name;
}

public struct Visual {
    public Texture2D Texture;
    public bool Visible;

    public Vector2 OriginOffset;  // Entity原点偏移量
    public Vector2 WorldPosition; // Entity的世界坐标
}

public struct Shadow {
    public Texture2D SdfTexture;            // SDF纹理
    public int Amount;                      // 阴影扩展量
    public Color Color;                     // 阴影颜色
    public Vector2 Offset;                  // 阴影偏移
}

public struct Hierarchy() {
    public Entity? Parent = null;
    public readonly List<Entity> Children = [];
    
    public Vector2 SourceOffset = Vector2.Zero; // 父物体上连线的本地位置
    public Vector2 TargetOffset = Vector2.Zero; // 子物体上连线的本地位置
}

public struct LayerMember {
    public Layer Layer;
    public bool Locked;
}