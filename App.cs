using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Cornifer.Input;
using Cornifer.Renderers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cornifer;

public class App : Game {
    public static App Instance { get; private set; } = null!;
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
            WorldCamera.Size = _graphicsManager.GraphicsDevice.Viewport.Bounds.Size.ToVector2();
        };
    }
    
    protected override void Initialize() {
        Interface.Initialize();
        Map.Initialize();
        _spriteBatch = new SpriteBatch(_graphicsManager.GraphicsDevice);
        WorldCamera = new CameraRenderer(_spriteBatch);
        base.Initialize();
    }
    
    protected override void LoadContent() {
        Cornifer.Content.Initialize(Content);

        InputHandler.Initialize();
        
        base.LoadContent();
    }
    
    protected override void Update(GameTime gt) {
        InputHandler.Update();
        Map.Update(gt);
        ArchRegister.Update();
        WorldCamera.Update();
        
        base.Update(gt);
    }

    protected override void Draw(GameTime gt) {
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