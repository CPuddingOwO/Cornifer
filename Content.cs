using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Cornifer;

public static class Content {
    // --- 总入口 ---
    public static void Initialize(ContentManager content) {
        var root = Path.Combine(App.AppLocation, "Content");
        content.RootDirectory = root;
        // 分类加载
        Fnt.Load(content);
        Tex.Load(content);
        Snd.Load(content);
        Cfg.Load(content);
        Eft.Load(content);
    }

    private static SpriteFont LoadManualSpritefont(string path) {
        Texture2D texture = null!;

        Dictionary<char, (Rectangle Bounds, Rectangle Cropping, Vector3 Kerning)> glyphs = new();

        var lineSpacing = 0;
        float spacing = 1;
        char? defaultCharacter = null;

        var glyphSpacing = 0;
        var ignoreCase = false;

        Point readerPos = new();

        foreach (var line in File.ReadLines(path)) {
            if (line.StartsWith("//"))
                continue;

            var split = line.Split(' ', 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            if (split.Length < 2)
                continue;

            switch (split[0]) {
                case "texture":
                    texture = Texture2D.FromFile(App.Instance?.GraphicsDevice,
                        Path.Combine(Path.GetDirectoryName(path)!, split[1]));
                    break;

                case "defaultChar" when split[1].Length == 1:
                    defaultCharacter = split[1][0];
                    break;

                case "ignoreCase" when bool.TryParse(split[1], out var ignoreCaseValue):
                    ignoreCase = ignoreCaseValue;
                    break;

                case "lineHeight" when int.TryParse(split[1], out var lineHeight):
                    lineSpacing = lineHeight;
                    break;

                case "glyphSpacing" when int.TryParse(split[1], out var glyphSpacingValue):
                    glyphSpacing = glyphSpacingValue;
                    break;

                case "spacing" when float.TryParse(split[1], NumberStyles.Float, CultureInfo.InvariantCulture,
                    out var spacingValue):
                    spacing = spacingValue;
                    break;

                case "spaceWidth" when float.TryParse(split[1], NumberStyles.Float, CultureInfo.InvariantCulture,
                    out var spaceWidth):
                    glyphs[' '] = (new Rectangle(), new Rectangle(), new Vector3(0, spaceWidth, 0));
                    break;

                case "pos":
                    var pos = split[1].Split(' ', 2,
                        StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                    if (pos.Length < 2 || !int.TryParse(pos[0], out var posX) || !int.TryParse(pos[1], out var posY))
                        break;

                    readerPos = new Point(posX, posY);
                    break;

                case "chars":
                    var chars = split[1].Split(' ',
                        StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

                    for (var i = 0; i < chars.Length; i += 2) {
                        if (i >= chars.Length - 1)
                            break;

                        var charStr = chars[i];
                        if (charStr.Length != 1 || !int.TryParse(chars[i + 1], out var charWidth))
                            continue;

                        var chr = charStr[0];

                        if (ignoreCase && (char.IsLower(chr) || char.IsUpper(chr))) {
                            glyphs[char.ToLower(chr)] = (
                                new Rectangle(readerPos.X, readerPos.Y, charWidth, lineSpacing), new Rectangle(),
                                new Vector3(0, charWidth, 0));
                            glyphs[char.ToUpper(chr)] = (
                                new Rectangle(readerPos.X, readerPos.Y, charWidth, lineSpacing), new Rectangle(),
                                new Vector3(0, charWidth, 0));
                        } else {
                            glyphs[chr] = (new Rectangle(readerPos.X, readerPos.Y, charWidth, lineSpacing),
                                new Rectangle(), new Vector3(0, charWidth, 0));
                        }


                        readerPos.X += charWidth + glyphSpacing;
                    }

                    break;
            }
        }

        List<char> characters = new();
        List<Rectangle> glyphBounds = new();
        List<Rectangle> cropping = new();
        List<Vector3> kerning = new();

        foreach (var kvp in glyphs.OrderBy(kvp => kvp.Key)) {
            characters.Add(kvp.Key);
            glyphBounds.Add(kvp.Value.Bounds);
            cropping.Add(kvp.Value.Cropping);
            kerning.Add(kvp.Value.Kerning);
        }

        return new SpriteFont(texture, glyphBounds, cropping, characters, lineSpacing, spacing, kerning,
            defaultCharacter);
    }

    // --- 字体管理 ---
    public static class Fnt {
        public static SpriteFont RodondoExt20M { get; internal set; } = null!;
        public static SpriteFont RodondoExt30M { get; internal set; } = null!;

        internal static void Load(ContentManager cm) {
            RodondoExt20M = LoadManualSpritefont(Path.Combine(cm.RootDirectory, "Font/RodondoExt20M.txt"));
            RodondoExt30M = LoadManualSpritefont(Path.Combine(cm.RootDirectory, "Font/RodondoExt30M.txt"));

            RodondoExt20M.LineSpacing -= 2;
            RodondoExt30M.LineSpacing -= 5;
        }
    }

    // --- 图片管理 ---
    public static class Tex {
        public static Texture2D Objects { get; internal set; } = default!;
        public static Texture2D SlugcatIcons { get; internal set; } = default!;
        public static Texture2D Pixel { get; internal set; } = default!;
        public static Texture2D MiscSprites { get; internal set; } = default!;

        internal static void Load(ContentManager cm) {
            Objects = cm.Load<Texture2D>("Texture/Objects");
            SlugcatIcons = cm.Load<Texture2D>("Texture/SlugcatIcons");
            MiscSprites = cm.Load<Texture2D>("Texture/MiscSprites");

            Pixel = new Texture2D(App.WorldCamera.SpriteBatch.GraphicsDevice, 1, 1);
            Pixel.SetData([Color.White]);
        }
    }

    // --- 音效管理 ---
    public static class Snd {
        public static SoundEffect Idle { get; internal set; } = default!;

        internal static void Load(ContentManager cm) {
            Idle = cm.Load<SoundEffect>("Audio/Idle");
        }
    }

    // --- 配置文件/散装 JSON 管理 ---
    public static class Cfg {
        private static readonly Dictionary<string, string> _jsonCache = new();

        public static string GetJson(string name) {
            return _jsonCache.GetValueOrDefault(name, "{}");
        }

        internal static void Load(ContentManager cm) {
            var configDir = Path.Combine(cm.RootDirectory, "Config");
            if (!Directory.Exists(configDir)) return;

            foreach (var file in Directory.GetFiles(configDir, "*.json"))
                _jsonCache[Path.GetFileNameWithoutExtension(file)] = File.ReadAllText(file);
        }
    }

    public static class Eft {
        public static Effect Shadow { get; internal set; } = null!;
        public static Effect Grid { get; internal set; } = null!;

        internal static void Load(ContentManager cm) {
            Shadow = cm.Load<Effect>("Effect/ShadowShader");
            Grid = cm.Load<Effect>("Effect/GridShader");
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    private class ManualSpriteFontAttribute : Attribute {
    }
}