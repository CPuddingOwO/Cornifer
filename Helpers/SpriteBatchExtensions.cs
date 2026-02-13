using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cornifer.Helpers;

public static class SpriteBatchExtensions {
    /// <summary>
    ///     扩展 <see cref="SpriteBatch" /> 以绘制一条实线。
    /// </summary>
    /// <param name="spriteBatch">当前的绘制批处理器。</param>
    /// <param name="p1">线段的起始点坐标。</param>
    /// <param name="p2">线段的结束点坐标。</param>
    /// <param name="color">线条颜色。</param>
    /// <param name="thickness">线条宽度（像素）。默认为 1。</param>
    /// <remarks>
    ///     该方法利用 1x1 像素贴图进行缩放和旋转。
    ///     旋转中心设置为 <c>new Vector2(0, 0.5f)</c>，这确保了线条宽度是围绕中心线向两侧扩展的。
    /// </remarks>
    public static void DrawLine(this SpriteBatch spriteBatch, Vector2 p1, Vector2 p2, Color color,
        float thickness = 1) {
        var diff = p2 - p1;
        var angle = MathF.Atan2(diff.Y, diff.X);
        spriteBatch.Draw(Content.Tex.Pixel, p1, null, color, angle, new Vector2(0, .5f),
            new Vector2(diff.Length(), thickness), SpriteEffects.None, 0);
    }

    /// <summary>
    ///     扩展 <see cref="SpriteBatch" /> 以绘制一条虚线（Dash Line）。
    /// </summary>
    /// <param name="spriteBatch">当前的绘制批处理器。</param>
    /// <param name="p1">线段的起点。</param>
    /// <param name="p2">线段的终点。</param>
    /// <param name="dashColor">实色部分的颜色。</param>
    /// <param name="emptyColor">虚空间隙部分的颜色。如果为 <c>null</c>，则间隙处不绘制任何内容（透明）。</param>
    /// <param name="dashLength">单个虚线段的长度（单位：像素）。</param>
    /// <param name="emptyLength">单个间隙的长度。如果不指定，则默认与 <paramref name="dashLength" /> 相同。</param>
    /// <param name="thickness">线条宽度（像素）。默认为 1。</param>
    /// <param name="startOffset">起始偏移量（像素）。可用于实现虚线滚动的动画效果。</param>
    /// <example>
    ///     绘制一条红白相间的虚线
    ///     <code>
    /// spriteBatch.DrawDashLine(start, end, Color.Red, Color.White, 10f, 5f);
    /// </code>
    /// </example>
    public static void DrawDashLine(this SpriteBatch spriteBatch, Vector2 p1, Vector2 p2, Color dashColor,
        Color? emptyColor, float dashLength, float? emptyLength = null, float thickness = 1,
        float? startOffset = null) {
        emptyLength ??= dashLength;

        var remainingLength = (p1 - p2).Length();
        var dash = true;
        var dir = p2 - p1;
        dir.Normalize();
        var pos = p1;

        // 处理初始偏移
        if (startOffset.HasValue)
            pos += dir * startOffset.Value;

        while (remainingLength > 0) {
            var length = dash ? dashLength : emptyLength.Value;

            // 确保不会绘制超出终点
            if (length > remainingLength)
                length = remainingLength;

            var nextPos = pos + dir * length;
            var color = dash ? dashColor : emptyColor;

            // 只有在颜色不为 null 时才调用绘制，实现透明间隙
            if (color.HasValue) spriteBatch.DrawLine(pos, nextPos, color.Value, thickness);

            dash = !dash;
            pos = nextPos;
            remainingLength -= length;
        }
    }
}