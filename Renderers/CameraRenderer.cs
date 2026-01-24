using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Cornifer.Input;

namespace Cornifer.Renderers;

public class CameraRenderer(SpriteBatch spriteBatch) : ScreenRenderer(spriteBatch) {
    private bool Dragging { get; set; }

    private Vector2 _dragPos;
    private int _wheelValue;

    public void Update() {
        var state = InputHandler.MouseState;
        var screenPos = state.Position.ToVector2();

        var drag = App.Instance.IsActive && !Interface.IsHovered && InputHandler.MoveCamera.Pressed;

        UpdateDragging(drag, screenPos);

        var wheel = (state.ScrollWheelValue - _wheelValue) / 120f;
        _wheelValue = state.ScrollWheelValue;

        if (wheel == 0 || Interface.IsHovered) return;
        // 每次滚轮缩放 25% 或 50%
        const float zoomFactor = 1.25f;
        var newScale = Scale;

        if (wheel > 0) newScale *= zoomFactor;
        else newScale /= zoomFactor;

        // 绝对清晰 在缩放接近整数时强行取整
        if (newScale is > 0.9f and < 1.1f) newScale = 1.0f;

        // 限制缩放范围
        newScale = MathHelper.Clamp(newScale, 0.2f, 32f);

        SetScale(newScale, screenPos);
    }

    private void SetScale(float scale, Vector2 at) {
        if (Math.Abs(scale - Scale) < 0.1) return;
        var atWorldBefore = InverseTransformVector(at);
        Scale = scale;
        var atWorldAfter = InverseTransformVector(at);
        Position -= atWorldAfter - atWorldBefore;
    }

    private void UpdateDragging(bool drag, Vector2 screenPos) {
        switch (drag) {
            case true when !Dragging:
                _dragPos = screenPos;
                Dragging = true;
                break;
            case true when Dragging:
                Position -= (screenPos - _dragPos) / Scale;
                _dragPos = screenPos;
                break;
            case false when Dragging:
                Dragging = false;
                break;
        }
    }
}