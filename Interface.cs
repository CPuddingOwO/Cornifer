using System;
using System.IO;
using Arch.Core.Extensions;
using Cornifer.Input;
using MonoGame.ImGuiNet;
using ImGuiNET;
using Microsoft.Xna.Framework;

namespace Cornifer;

public static class Interface {
    private static ImGuiRenderer _renderer = null!;
    public static bool IsHovered { get; private set; }

    public static void BeginLayout(GameTime gt) => _renderer.BeginLayout(gt);
    public static void EndLayout() => _renderer.EndLayout();


    public static void Initialize() {
        _renderer = new ImGuiRenderer(App.Instance);
        var io = ImGui.GetIO();
        io.IniSavingRate = 0f; // 禁用自动保存 imgui.ini 文件

        string fontPath = Path.Combine(App.AppLocation, "Content/Font/MapleMonoNormalNL-NF-CN-Regular.ttf");

        if (File.Exists(fontPath)) {
            // 开启不安全上下文以处理指针
            unsafe {
                // 创建 ImFontConfig 实例
                ImFontConfig* configPtr = ImGuiNative.ImFontConfig_ImFontConfig();

                // 转换为包装器指针以便方便操作
                ImFontConfigPtr fontConfig = new ImFontConfigPtr(configPtr);

                // --- 棱角分明的关键设置 ---
                fontConfig.PixelSnapH = true; // 像素对齐
                fontConfig.OversampleH = 1; // 禁用水平过采样（减少模糊）
                fontConfig.OversampleV = 1; // 禁用垂直过采样（减少模糊）

                io.Fonts.Clear();

                // 传入配置进行加载
                io.Fonts.AddFontFromFileTTF(fontPath, 32, fontConfig, io.Fonts.GetGlyphRangesChineseFull());

                // 重建图集
                _renderer.RebuildFontAtlas();

                // 销毁原生配置对象，防止内存泄漏
                ImGuiNative.ImFontConfig_destroy(configPtr);
            }
        }
    }

    public static void Draw() {
        // --- 获取窗口尺寸 ---
        var viewport = ImGui.GetMainViewport();
        float windowHeight = viewport.Size.Y;
        float windowWidth = viewport.Size.X;

        // --- 1. 设置窗口的位置和大小 ---
        // 位置：X = 总宽 - 面板宽, Y = 0 (右上角)
        ImGui.SetNextWindowPos(new System.Numerics.Vector2(windowWidth - 400f - 8, 8));
        // 大小：宽 = SidePanelWidth, 高 = 窗口总高
        ImGui.SetNextWindowSize(new System.Numerics.Vector2(400f, windowHeight - 16));

        // --- 2. 窗口配置标志 ---
        // NoMove: 禁止用户拖动面板位置
        // NoCollapse: 禁止折叠
        // NoTitleBar: 如果你想要一个纯净的侧边栏，可以去掉标题栏
        // NoResize: 关键！我们要禁用原生的右下角缩放，改为自己实现的左侧缩放
        ImGuiWindowFlags flags = ImGuiWindowFlags.NoMove |
                                 ImGuiWindowFlags.NoCollapse |
                                 ImGuiWindowFlags.NoResize;

        if (ImGui.Begin("SidePanel", flags)) {
            // 这里的 IsHovered 会更新，CameraRenderer 就能据此停止地图操作
            IsHovered = ImGui.IsWindowHovered() || ImGui.IsAnyItemHovered();
            
            ImGui.Separator();
            
            ImGui.BeginGroup();
            ImGui.Text($"相机位置 ({App.WorldCamera.Position.X:F2}, {App.WorldCamera.Position.Y:F2})");
            ImGui.Text($"相机缩放 {App.WorldCamera.Scale:F2}");
            ImGui.EndGroup();
            
            if (ImGui.CollapsingHeader("视图控制", ImGuiTreeNodeFlags.DefaultOpen)) {
                if (ImGui.Button("重置相机")) {
                    App.WorldCamera.Position = new Vector2( -viewport.Size.X / 2f, -viewport.Size.Y / 2f );
                    App.WorldCamera.Scale = 1.0f;
                }
            }
            
            ImGui.Separator();
            
            if (ImGui.Button("放置测试对象")) { Map.SpawnTestData(); }
            ImGui.SameLine();
            if (ImGui.Button("Capture")) { 
                var texture = Systems.CaptureSystem.Capture();
                var directory = Path.GetDirectoryName(App.AppLocation);
                if (!string.IsNullOrEmpty(directory)) {
                    Directory.CreateDirectory(directory);
                }

                // 使用 FileStream 创建文件
                using (Stream stream = File.Create(App.AppLocation + "/Map.png"))
                {
                    // MonoGame 内置方法，会自动处理像素转换和 PNG 编码
                    texture.SaveAsPng(stream, texture.Width, texture.Height);
                }
                texture.Dispose();
            }

            ImGui.Separator();
            if (Map.SelectedEntities.Count != 0) {
                if (ImGui.Button("删除选中的对象") || InputHandler.DeleteEntity.Pressed) {
                    foreach (var entity in Map.SelectedEntities) {
                        Console.WriteLine("Remove Entity: " + (entity.TryGet<Identifier>( out var id) ? id.Name : "Unknown"));
                        Map.SelectedEntities.Remove(entity);
                        Map.World.Destroy(entity);   
                    }
                }
                
                foreach (var entity in Map.SelectedEntities) {
                    var v = entity.Get<Visual>();
                    ArchInspector.Draw(entity);
                    ImGui.BeginGroup();
                    ImGui.Separator();
                    ImGui.Text(entity.TryGet<Identifier>( out var id) ? $"对象 ID: {id.Name}" : "对象 ID: 未知");
                    ImGui.Text(entity.TryGet<LayerMember>( out var layer) ? $"图层: {layer.Layer}" : "图层: 未知");
                    ImGui.Text($"WPos: ({v.WorldPosition.X:F2}, {v.WorldPosition.Y:F2})");
                    ImGui.Text($"LPos: {v.OriginOffset.X:F2}, {v.OriginOffset.Y:F2})");
                    ImGui.EndGroup();
                }
                
            } else {
                ImGui.TextDisabled("未选中任何对象");
            }

            ImGui.End();
        }

        // ImGui.ShowStyleEditor();
        ImGui.ShowDemoWindow();
    }
}