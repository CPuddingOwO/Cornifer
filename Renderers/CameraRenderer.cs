using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Cornifer.Renderers {
    public class CameraRenderer(SpriteBatch spriteBatch) : ScreenRenderer(spriteBatch) {
        public bool Dragging { get; private set; }

        private Vector2 _dragPos;
        private int _wheelValue;
        private float _wheelZoom;

        const float MaxZoom = 300;
        const float MinZoom = -500;

        public void Update() {
            MouseState state = Mouse.GetState();

            Vector2 screenPos = state.Position.ToVector2();

            bool drag = App.Instance.IsActive && !Interface.IsHovered && !App.Dragging && !App.Selecting;

            UpdateDragging(drag, screenPos);

            float wheel = (state.ScrollWheelValue - _wheelValue) / 120;
            _wheelValue = state.ScrollWheelValue;

            if (wheel != 0 && !Interface.IsHovered) {
                // 每次滚轮缩放 25% 或 50%
                float zoomFactor = 1.25f;
                float newScale = Scale;

                if (wheel > 0)
                    newScale *= zoomFactor;
                else
                    newScale /= zoomFactor;

                // 关键点：如果你想要绝对清晰，可以在缩放接近整数时强行取整
                if (newScale > 0.9f && newScale < 1.1f) newScale = 1.0f;

                // 限制缩放范围
                newScale = MathHelper.Clamp(newScale, 0.1f, 32f);

                SetScale(newScale, screenPos);
            }
        }

        void SetScale(float scale, Vector2 at) {
            if (scale == Scale) return;
            Vector2 atWorldBefore = InverseTransformVector(at);
            Scale = scale;
            Vector2 atWorldAfter = InverseTransformVector(at);
            Position -= atWorldAfter - atWorldBefore;
        }

        void UpdateDragging(bool drag, Vector2 screenPos) {
            if (drag && !Dragging) {
                _dragPos = screenPos;
                Dragging = true;
            } else if (drag && Dragging) {
                Position -= (screenPos - _dragPos) / Scale;
                _dragPos = screenPos;
            } else if (!drag && Dragging) {
                Dragging = false;
            }
        }
    }
}