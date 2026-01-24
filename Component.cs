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
    public Texture2D Texture;
    public bool Visible;

    public Vector2 WorldPosition; // Entity原点的世界坐标
    public Vector2 LocalPosition; // Entity原点在贴图上的本地坐标（偏移）
}

public struct Shadow {
    // --- 已弃用 CPU 生成阴影方案 ShadowBaker ---
    // --- DEPRECATED CPU Shadow Baking Scheme ShadowBaker ---
    public Texture2D? Texture;  // 阴影贴图: 自动生成 根据对应的 Visual.Texture
    public Vector2 Offset;      // 阴影通常比原图大，所以渲染位置要偏移 -(shadowAmount)
    // --- END ---
    
    public int Amount;          // 阴影扩展量
    public int CornerRadius;    // 圆角半径
}

// 父子关系
public struct Hierarchy() {
    public Entity? Parent = null;
    public List<Entity> Children = [];
}

public struct LayerMember {
    public Layer Layer;
    public bool Locked;
}