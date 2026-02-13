using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Cornifer.Input;

public static class InputHandler {
    public static KeyboardState KeyboardState, PrevKeyboardState;
    public static MouseState MouseState, PrevMouseState;

    public static bool Disable;
    public static Dictionary<string, Keybind> Keybinds = new();

    // Keybinds
    public static readonly Keybind Undo = new("Undo", ModifierKeys.Control, Keys.Z);
    public static readonly Keybind Redo = new("Redo", ModifierKeys.Control, Keys.Y);

    public static readonly Keybind SelectEntity = new("SingleSelect", MouseKeys.LeftButton);
    public static readonly Keybind MoveEntity = new("MoveEntity", MouseKeys.LeftButton);
    public static readonly Keybind MoveCamera = new("MoveCamera", MouseKeys.RightButton);
    public static readonly Keybind DeleteEntity = new("Delete", Keys.Delete);

    // 快捷获取坐标属性
    public static Point MousePoint => MouseState.Position;
    public static Vector2 MousePosition => new(MouseState.X, MouseState.Y);
    public static Vector2 MouseDelta => (MouseState.Position - PrevMouseState.Position).ToVector2();


    public static void Initialize() {
        // 自动注册静态字段中的 Keybind
        var fields = typeof(InputHandler).GetFields(BindingFlags.Public | BindingFlags.Static);
        foreach (var f in fields.Where(f => f.FieldType == typeof(Keybind)))
            Keybinds[f.Name] = (Keybind)f.GetValue(null)!;
        // LoadKeybinds();
    }

    public static void Update() {
        Disable = !App.Instance.IsActive;

        PrevMouseState = MouseState;
        MouseState = Mouse.GetState();

        PrevKeyboardState = KeyboardState;
        KeyboardState = Keyboard.GetState();
    }

    public static void RefreshEncapsulatedBinds() {
        var allCombos = Keybinds.Values.SelectMany(k => k.Inputs).ToList();
        foreach (var combo in allCombos) {
            combo.EncapsulatingCombos.Clear();
            // 如果 comboB 包含 comboA 的所有键且比 comboA 更长，则 comboB 封装 comboA
            var superiors = allCombos.Where(other => other.ComboEncapsulates(combo));
            foreach (var s in superiors)
                if (!combo.EncapsulatingCombos.Any(existing => existing.InputEquality(s)))
                    combo.EncapsulatingCombos.Add(s);
        }
    }
}