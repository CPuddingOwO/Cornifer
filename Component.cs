using System.Collections.Generic;
using Arch.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cornifer;

// 身份
public struct Identifier {
    public string Name;
}

// 渲染信息
public struct Visual {
    public Vector2 Size;
    public Texture2D Texture;
    public bool Visible;

    public Vector2 WorldPosition; // 绝对坐标
    public Vector2 OffsetPosition; // 相对父物体的偏移
    public Vector2 LocalPosition; // 纹理内相对于左下角的原点 (逻辑锚点)
}

// 阴影数据
public struct Shadow {
    public int Size;
    public Texture2D? Texture;
    public bool IsDirty;
}

// 层级关系
public struct Hierarchy {
    public Entity? Parent;
    public List<Entity> Children;
}

public struct LayerMember {
    public Layer Layer;
    public bool Locked;
}