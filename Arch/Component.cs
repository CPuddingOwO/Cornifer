using System.Collections.Generic;
using Arch.Core;
using Cornifer.Rw;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cornifer.Arch;

public struct Identifier {
    public string Name;
}

/// <summary>
/// 视觉组件
/// </summary>
public struct Visual {
    /// <summary>
    ///  纹理
    /// </summary>
    public Texture2D Texture;

    /// <summary>
    /// 是否可见
    /// </summary>
    public bool Visible;

    /// <summary>
    /// 锚点坐标
    /// </summary>
    public Vector2 AnchorPoint;

    /// <summary>
    /// 纹理中心相对于锚点的偏移
    /// </summary>
    public Vector2 TextureCenterOffset; // Entity中心相对于锚点的偏移

    /// <summary>
    /// *Readonly 锚点坐标 MonoGame默认左上角
    /// </summary>
    public Vector2 AnchorPosition => AnchorPoint;

    /// <summary>
    /// *Readonly 中心坐标 纹理正中心
    /// </summary>
    public Vector2 CenterPosition => AnchorPoint + TextureCenterOffset;
}

/// <summary>
/// 阴影组件
/// </summary>
public struct Shadow {
    /// <summary>
    /// SDF 纹理 Signed Distance Field
    /// </summary>
    public Texture2D SdfTexture;

    /// <summary>
    /// 阴影扩展量 单位像素 值越大阴影越大 但性能开销也越大
    /// </summary>
    public int Amount;

    /// <summary>
    /// 阴影颜色
    /// </summary>
    public Color Color;

    /// <summary>
    /// 阴影偏移 单位像素 (-Amount - 1, -Amount - 1)
    /// </summary>
    public Vector2 Offset;
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

public struct Metadata {
    public Mod SourceMod;
}