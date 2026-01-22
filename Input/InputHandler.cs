using System;
using Microsoft.Xna.Framework.Input;

namespace Cornifer.Input;

public static class InputHandler {
    
    public static KeyboardState KeyboardState, PrevKeyboardState;
    public static MouseState MouseState, PrevMouseState;
    
    public static Keys[] AllKeys = Enum.GetValues<Keys>();
    public static MouseKeys[] AllMouseKeys = Enum.GetValues<MouseKeys>();
    
    
    
    public static void Initialize() { }

    public static void Update() {
        PrevMouseState = MouseState;
        MouseState = Mouse.GetState();

        PrevKeyboardState = KeyboardState;
        KeyboardState = Keyboard.GetState();   
    }
}