using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Rectangle = System.Drawing.Rectangle;

namespace Cornifer;

public static class Platform {
    private const int RegistryDataVersion = 1;
    private const string OpenWebMapProtocol = "cornifer://openweb/";
    private static readonly HttpClient HttpClient = new();

    private static readonly WindowsInteractionTaskScheduler Scheduler = new();
    private static IWin32Window? _gameWindow;
    private static readonly bool _detachedWindow = false;

    private static Task<Stream?>? _startupStateStream;
    private static string? _startupStatePath;

    /// <summary>
    ///     初始化并处理启动参数与注册表
    /// </summary>
    public static void Start(string[] args) {
        if (args.Length >= 1) {
            var arg = args[0];
            if (arg.StartsWith(OpenWebMapProtocol)) {
                _startupStateStream = DownloadWebMapAsync(arg);
            } else if (File.Exists(arg)) {
                _startupStateStream = Task.FromResult<Stream?>(File.OpenRead(arg));
                _startupStatePath = arg;
            }
        }

        EnsureRegistryRegistered();
    }

    private static async Task<Stream?> DownloadWebMapAsync(string protocolUrl) {
        try {
            var url = $"https://{protocolUrl[OpenWebMapProtocol.Length..]}";
            var response = await HttpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            var ms = new MemoryStream();
            await response.Content.CopyToAsync(ms);
            ms.Position = 0;
            return ms;
        } catch (Exception ex) {
            Debug.WriteLine($"Download failed: {ex.Message}");
            return null;
        }
    }

    private static void EnsureRegistryRegistered() {
        // 尝试写入。如果失败，可能是权限问题
        try {
            var exePath = Process.GetCurrentProcess().MainModule?.FileName;
            if (exePath is null) return;

            // 优先检查 HKLM (或 ClassesRoot)，如果没权限则尝试写入当前用户 (HKCU)
            // 现代 Windows 推荐将关联写入 HKCU\Software\Classes 以免除管理员权限
            using var root = Registry.CurrentUser.OpenSubKey(@"Software\Classes", true);
            if (root is null) return;

            // 检查版本，避免重复写入
            using var existingKey = root.OpenSubKey(".cornimap");
            if (existingKey?.GetValue("RegistryDataVersion") is int v && v == RegistryDataVersion) return;

            // 注册文件关联
            using var extKey = root.CreateSubKey(".cornimap");
            extKey.SetValue("", "cornimapFile");
            extKey.SetValue("RegistryDataVersion", RegistryDataVersion);

            using var fileKey = root.CreateSubKey("cornimapFile");
            fileKey.SetValue("", "Cornifer map");
            using var cmdKey = fileKey.CreateSubKey(@"shell\open\command");
            cmdKey.SetValue("", $"\"{exePath}\" \"%1\"");

            // 注册协议
            using var protocolKey = root.CreateSubKey("cornifer");
            protocolKey.SetValue("", "Cornifer Protocol");
            protocolKey.SetValue("URL Protocol", "");
            using var protCmdKey = protocolKey.CreateSubKey(@"shell\open\command");
            protCmdKey.SetValue("", $"\"{exePath}\" \"%1\"");
        } catch (UnauthorizedAccessException) {
            // 如果 HKCU 都没权限（极少见），再提示用户
            if (!File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "noAdminWarning.txt")))
                _ = MessageBox("无法更新注册表关联，请尝试以管理员身份运行。", "权限提醒");
        }
    }

    public static async Task<(Stream? stream, string? saveFileName)> GetStartupStateFileStream() {
        if (_startupStateStream is null) return (null, null);
        return (await _startupStateStream, _startupStatePath);
    }

    private static IWin32Window? GetGameWindow() {
        if (_detachedWindow) return null;
        var handle = Process.GetCurrentProcess().MainWindowHandle;
        return handle == 0 ? null : _gameWindow ??= new WindowHandle(handle);
    }

    public static void Stop() {
        Scheduler.Dispose();
    }

    // 使用 C# 12 语法简化内部类
    private sealed class WindowHandle(nint handle) : IWin32Window {
        public IntPtr Handle => handle;
    }

    /// <summary>
    ///     专用的 STA 线程调度器，处理 WinForms 所有交互
    /// </summary>
    private sealed class WindowsInteractionTaskScheduler : TaskScheduler, IDisposable {
        private readonly BlockingCollection<Task> _tasks = new();
        private readonly Thread _thread;

        public WindowsInteractionTaskScheduler() {
            _thread = new Thread(Loop) { IsBackground = true, Name = "STA_WinForms_Thread" };
            _thread.SetApartmentState(ApartmentState.STA);
            _thread.Start();
        }

        public void Dispose() {
            _tasks.CompleteAdding();
            _thread.Join(500);
        }

        private void Loop() {
            foreach (var task in _tasks.GetConsumingEnumerable()) TryExecuteTask(task);
        }

        public Task<T> Schedule<T>(Func<T> func) {
            return Task.Factory.StartNew(func, CancellationToken.None, TaskCreationOptions.None, this);
        }

        public Task Schedule(Action action) {
            return Task.Factory.StartNew(action, CancellationToken.None, TaskCreationOptions.None, this);
        }

        protected override void QueueTask(Task task) {
            _tasks.Add(task);
        }

        protected override bool TryExecuteTaskInline(Task task, bool wasQueued) {
            return false;
        }

        protected override IEnumerable<Task> GetScheduledTasks() {
            return _tasks;
        }
    }

    #region WinForms Interactions

    public static Task<MessageBoxResult> MessageBox(string text, string caption,
        MessageBoxButtons buttons = MessageBoxButtons.Ok) {
        return Scheduler.Schedule(() => {
            var winButtons = buttons == MessageBoxButtons.OkCancel
                ? System.Windows.Forms.MessageBoxButtons.OKCancel
                : System.Windows.Forms.MessageBoxButtons.OK;
            var result = System.Windows.Forms.MessageBox.Show(GetGameWindow(), text, caption, winButtons);
            return result == DialogResult.OK ? MessageBoxResult.Ok : MessageBoxResult.Cancel;
        });
    }

    public static Task<string?> OpenFileDialog(string title, string filter, string? filename = null,
        string? startDir = null) {
        return Scheduler.Schedule(() => {
            using OpenFileDialog dialog = new()
                { Title = title, Filter = filter, FileName = filename, InitialDirectory = startDir };
            return dialog.ShowDialog(GetGameWindow()) == DialogResult.OK ? dialog.FileName : null;
        });
    }

    #endregion

    #region Clipboard

    public static void SetClipboardImage(Image<Rgba32> image) {
        // 1. 准备 PNG 数据
        using var pngStream = new MemoryStream();
        image.SaveAsPng(pngStream);

        // 2. 准备 DIB 数据 (Device Independent Bitmap)
        // 使用高效的内存构造方式
        var dibStream = CreateDibV5Stream(image);

        // 3. 准备 System.Drawing.Bitmap (用于兼容老程序)
        using var bitmap = ConvertToBitmap(image);

        Scheduler.Schedule(() => {
            var data = new DataObject();
            data.SetData(DataFormats.Bitmap, true, bitmap);
            data.SetData("PNG", true, pngStream);
            data.SetData(DataFormats.Dib, true, dibStream);
            Clipboard.SetDataObject(data, true);
        });
    }

    private static MemoryStream CreateDibV5Stream(Image<Rgba32> image) {
        var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);

        var headerSize = 40; // BITMAPINFOHEADER
        var dataSize = image.Width * image.Height * 4;

        writer.Write(headerSize);
        writer.Write(image.Width);
        writer.Write(image.Height);
        writer.Write((short)1); // Planes
        writer.Write((short)32); // BitCount
        writer.Write(3); // BI_BITFIELDS
        writer.Write(dataSize);
        writer.Write(0); // XPelsPerMeter
        writer.Write(0); // YPelsPerMeter
        writer.Write(0); // ClrUsed
        writer.Write(0); // ClrImportant

        // Bitmasks for ARGB
        writer.Write(0x00FF0000);
        writer.Write(0x0000FF00);
        writer.Write(0x000000FF);

        // 像素转换
        var pixelData = new byte[dataSize];
        image.CopyPixelDataTo(pixelData);
        writer.Write(pixelData);

        ms.Position = 0;
        return ms;
    }

    private static unsafe Bitmap ConvertToBitmap(Image<Rgba32> image) {
        var bmp = new Bitmap(image.Width, image.Height, PixelFormat.Format32bppArgb);
        var rect = new Rectangle(0, 0, image.Width, image.Height);
        var data = bmp.LockBits(rect, ImageLockMode.WriteOnly, bmp.PixelFormat);

        try {
            // 计算总字节数
            var totalBytes = image.Width * image.Height * 4;

            // 使用 Span 直接指向 Bitmap 的内存地址
            // data.Scan0.ToPointer() 获取 void* 指针
            var destination = new Span<byte>(data.Scan0.ToPointer(), totalBytes);

            // ImageSharp 内置的高效拷贝
            image.CopyPixelDataTo(destination);
        } finally {
            // 必须在 finally 中解锁，防止内存被锁定导致崩溃
            bmp.UnlockBits(data);
        }

        return bmp;
    }

    #endregion
}

public enum MessageBoxButtons {
    Ok,
    OkCancel
}

public enum MessageBoxResult {
    Ok,
    Cancel
}