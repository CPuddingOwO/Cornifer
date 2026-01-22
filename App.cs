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
    public static GraphicsDeviceManager GraphicsManager = null!;
    public static SpriteBatch SpriteBatch = null!;
    
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
        GraphicsManager = new GraphicsDeviceManager(this);
        IsMouseVisible = true; 
        
        // --- 开启窗口缩放 ---
        Window.AllowUserResizing = true; 
        Window.ClientSizeChanged += (s, e) => {
            // 更新摄像机的投影矩阵
            WorldCamera.Size = GraphicsDevice.Viewport.Bounds.Size.ToVector2();
        };
    }
    
    protected override void Initialize() {
        Interface.Initialize();
        Map.Initialize();
        base.Initialize();
    }
    
    protected override void LoadContent() {
        SpriteBatch = new SpriteBatch(GraphicsDevice);
        WorldCamera = new CameraRenderer(SpriteBatch);
        Cornifer.Content.Initialize(Content);
        
        base.LoadContent();
    }
    
    protected override void Update(GameTime gt) {
        Map.Update(gt);
        InputHandler.Update();
        WorldCamera.Update();
        
        base.Update(gt);
    }

    protected override void Draw(GameTime gt) {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        // --- MonoGame 绘制 ---
        SpriteBatch.Begin(transformMatrix: WorldCamera.Transform, samplerState: SamplerState.PointClamp);
        ArchRegister.Draw();
        SpriteBatch.End();

        // --- 绘制 ImGui ---
        Interface.BeginLayout(gt);
        Interface.Draw();
        Interface.EndLayout();
        
        base.Draw(gt);
    }
}