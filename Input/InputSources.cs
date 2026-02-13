using Microsoft.Xna.Framework.Input;

namespace Cornifer.Input;

// 键盘输入
public class KeyboardInput(Keys key) : KeybindInput {
    public Keys Key { get; set; } = key;
    public override bool CurrentState => !InputHandler.Disable && InputHandler.KeyboardState.IsKeyDown(Key);
    public override bool PrevState => !InputHandler.Disable && InputHandler.PrevKeyboardState.IsKeyDown(Key);
    public override string KeyName => Key.ToString();
}

// 修饰键输入 (Shift/Ctrl/Alt)
public class ModifierInput(ModifierKeys key) : KeybindInput {
    public ModifierKeys Key { get; set; } = key;
    public override bool CurrentState => !InputHandler.Disable && GetState(InputHandler.KeyboardState);
    public override bool PrevState => !InputHandler.Disable && GetState(InputHandler.PrevKeyboardState);
    public override string KeyName => Key.ToString();

    private bool GetState(KeyboardState state) {
        return Key switch {
            ModifierKeys.Shift => state.IsKeyDown(Keys.LeftShift) || state.IsKeyDown(Keys.RightShift),
            ModifierKeys.Control => state.IsKeyDown(Keys.LeftControl) || state.IsKeyDown(Keys.RightControl),
            ModifierKeys.Alt => state.IsKeyDown(Keys.LeftAlt) || state.IsKeyDown(Keys.RightAlt),
            ModifierKeys.Windows => state.IsKeyDown(Keys.LeftWindows) || state.IsKeyDown(Keys.RightWindows),
            _ => false
        };
    }
}

// 鼠标输入
public class MouseInput(MouseKeys key) : KeybindInput {
    public MouseKeys Key { get; set; } = key;
    public override bool CurrentState => !InputHandler.Disable && GetState(InputHandler.MouseState);
    public override bool PrevState => !InputHandler.Disable && GetState(InputHandler.PrevMouseState);
    public override string KeyName => Key.ToString();

    private bool GetState(MouseState state) {
        return Key switch {
            MouseKeys.LeftButton => state.LeftButton == ButtonState.Pressed,
            MouseKeys.RightButton => state.RightButton == ButtonState.Pressed,
            MouseKeys.MiddleButton => state.MiddleButton == ButtonState.Pressed,
            _ => false
        };
    }
}