using Cornifer.MapObjects;
using Cornifer.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cornifer.Renderers
{
    public class ShadeRenderer : ScreenRenderer
    {
        public bool TargetNeedsClear;

        public override float Scale => 1;
        public override Vector2 Size => MapObject.ShadeRenderTarget?.Size() ?? Vector2.One;

        public ShadeRenderer(SpriteBatch spriteBatch) : base(spriteBatch) { }

        public override void DrawTexture(Texture2D texture, Vector2 worldPos, Rectangle? source, Vector2? worldSize, Color? color, Vector2? scaleOverride = null)
        {
            UI.Structures.SpriteBatchState state = App.SpriteBatch?.GetState();

            App.SpriteBatch?.End();
            RenderTargetBinding[] targets = App.Instance.GraphicsDevice.GetRenderTargets();
            App.Instance.GraphicsDevice.SetRenderTarget(MapObject.ShadeRenderTarget);

            if (TargetNeedsClear)
                App.Instance?.GraphicsDevice.Clear(Color.Transparent);

            App.SpriteBatch?.Begin(samplerState: SamplerState.PointClamp);

            base.DrawTexture(texture, worldPos, source, worldSize, color, scaleOverride);

            App.SpriteBatch?.End();
            App.Instance?.GraphicsDevice.SetRenderTargets(targets);
            App.SpriteBatch?.Begin(state);

            TargetNeedsClear = false;

        }
    }
}
