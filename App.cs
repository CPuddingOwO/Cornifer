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
     private static GraphicsDeviceManager _graphicsManager = null!;
    private static SpriteBatch _spriteBatch = null!;
    
    public static CameraRenderer WorldCamera = null!;
    
    public static bool Dragging = false;
    public static bool Selecting = false;
    
    public static readonly string AppLocation;
    
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
        base.Initialize();
    }
    
    protected override void LoadContent() {
        _spriteBatch = new SpriteBatch(_graphicsManager.GraphicsDevice);
        WorldCamera = new CameraRenderer(_spriteBatch);
        Cornifer.Content.Initialize(Content);

        InputHandler.Initialize();
        
        base.LoadContent();
    }
    
    protected override void Update(GameTime gt) {
        Map.Update(gt);
        InputHandler.Update();
        WorldCamera.Update();
        
        base.Update(gt);
    }

    protected override void Draw(GameTime gt) {
        _graphicsManager.GraphicsDevice.Clear(Color.CornflowerBlue);

        // --- MonoGame 绘制 ---
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        ArchRegister.Draw(WorldCamera);
        _spriteBatch.End();

        // --- 绘制 ImGui ---
        Interface.BeginLayout(gt);
        Interface.Draw();
        Interface.EndLayout();
        
        base.Draw(gt);
    }
}