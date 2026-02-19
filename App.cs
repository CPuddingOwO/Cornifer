using System;
using Cornifer.Arch;
using Cornifer.Arch.Systems;
using Cornifer.Input;
using Cornifer.Renderers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cornifer;

public class App : Game {
    public static readonly string AppLocation;
    public static CameraRenderer WorldCamera = null!;

    private static GraphicsDeviceManager _graphicsManager = null!;
    private static SpriteBatch _spriteBatch = null!;


    static App() {
        AppLocation = AppDomain.CurrentDomain.BaseDirectory;
        Console.WriteLine("App Location: " + AppLocation);
    }

    public App() {
        Instance = this;
        _graphicsManager = new GraphicsDeviceManager(this);
        IsMouseVisible = true;

        // --- 开启窗口缩放 ---
        Window.AllowUserResizing = true;
        Window.ClientSizeChanged += (s, e) => {
            // 更新摄像机的投影矩阵
            // WorldCamera.UpdateBuffer(WorldCamera.SpriteBatch.GraphicsDevice,
            //     _graphicsManager.GraphicsDevice.Viewport.Bounds.Size.X,
            //     _graphicsManager.GraphicsDevice.Viewport.Bounds.Size.Y);
            WorldCamera.Size = _graphicsManager.GraphicsDevice.Viewport.Bounds.Size.ToVector2();
        };
    }

    public static App Instance { get; private set; } = null!;

    protected override void Initialize() {
        Interface.Initialize();
        Map.Initialize();
        _spriteBatch = new SpriteBatch(_graphicsManager.GraphicsDevice);
        WorldCamera = new CameraRenderer(_spriteBatch);
        var vp = _graphicsManager.GraphicsDevice.Viewport;
        var vpSize = vp.Bounds.Size;
        // WorldCamera.UpdateBuffer(_graphicsManager.GraphicsDevice, vpSize.X, vpSize.Y);

        WorldCamera.Position = new Vector2(-vpSize.X / 2f, -vpSize.Y / 2f);
        CaptureSystem.Initialize(_spriteBatch);

        GitDescriptor.Load();
        base.Initialize();
    }

    protected override void LoadContent() {
        Cornifer.Content.Initialize(Content);

        InputHandler.Initialize();

        base.LoadContent();
    }

    protected override void Update(GameTime gt) {
        InputHandler.Update();
        ArchRegister.Update(gt);
        WorldCamera.Update();

        base.Update(gt);
    }

    protected override void Draw(GameTime gt) {
        // _graphicsManager.GraphicsDevice.SetRenderTarget(null);
        // _graphicsManager.GraphicsDevice.Clear(Color.CornflowerBlue);

        // --- MonoGame 绘制 ---
        ArchRegister.Draw(WorldCamera);

        // --- 绘制 ImGui ---
        Interface.BeginLayout(gt);
        Interface.Draw();
        Interface.EndLayout();

        base.Draw(gt);
    }
}