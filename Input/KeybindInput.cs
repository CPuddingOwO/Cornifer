using Microsoft.Xna.Framework.Input;

namespace Cornifer.Input
{
    public enum KeybindState { Released = 0, JustReleased = 1, JustPressed = 2, Pressed = 3 }
    public enum ModifierKeys { Shift, Control, Alt, Windows }
    public enum MouseKeys { LeftButton, RightButton, MiddleButton, XButton1, XButton2 }

    public abstract class KeybindInput
    {
        public abstract bool CurrentState { get; }
        public abstract bool PrevState { get; }
        public abstract string KeyName { get; }

        // 使用位运算计算状态：Current << 1 | Old
        public KeybindState State => (KeybindState)((CurrentState ? 2 : 0) | (PrevState ? 1 : 0));

        public static implicit operator KeybindInput(Keys key) => new KeyboardInput(key);
        public static implicit operator KeybindInput(ModifierKeys key) => new ModifierInput(key);
        public static implicit operator KeybindInput(MouseKeys key) => new MouseInput(key);

        public virtual bool InputEquality(KeybindInput other) => KeyName == other.KeyName;
    }
}