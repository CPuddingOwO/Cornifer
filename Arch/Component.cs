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
    /// 锚点坐标 中心
    /// </summary>
    public Vector2 AnchorPoint;

    /// <summary>
    /// 锚点相对于纹理左上角的偏移 *四舍五入 后的 Width/2, Height/2
    /// </summary>
    public Vector2 TextureOffset;

    /// <summary>
    /// *Readonly 锚点坐标
    /// </summary>
    public Vector2 AnchorPosition => AnchorPoint;
    
    /// <summary>
    /// 获取纹理在世界/屏幕中实际渲染的左上角坐标
    /// </summary>
    public Vector2 VisualTopLeftPosition => AnchorPosition - TextureOffset;
    
    /// <summary>
    /// 获取该纹理在世界坐标中实际占用的矩形区域 (左上角起始)
    /// </summary>
    public Rectangle Bounds => new Rectangle(
        (int)(AnchorPosition.X - TextureOffset.X),
        (int)(AnchorPosition.Y - TextureOffset.Y),
        Texture.Width,
        Texture.Height
    );
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

    public Vector2 SourceOffset = Vector2.Zero; // 相对于父物体 CenterPosition 的偏移
    public Vector2 TargetOffset = Vector2.Zero; // 相对于子物体 CenterPosition 的偏移
    
    public bool Visible = true; // 是否显示连线
}

public struct LayerMember {
    public Layer Layer;
    public bool Locked;
}

public struct Metadata {
    public Mod SourceMod;
}