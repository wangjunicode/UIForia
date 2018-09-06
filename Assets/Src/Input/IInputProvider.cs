using Src.Input;
using UnityEngine;

namespace Src.Systems {

    public interface IInputProvider {

        KeyState GetKeyState(KeyCode keyCode);
        
        KeyboardModifiers KeyboardModifiers { get; }
        
        bool IsKeyDown(KeyCode keyCode);
        bool IsKeyDownThisFrame(KeyCode keyCode);
        bool IsKeyUp(KeyCode keyCode);
        bool IsKeyUpThisFrame(KeyCode keyCode);

        bool IsMouseLeftDown { get; }
        bool IsMouseLeftDownThisFrame { get; }

        bool IsMouseRightDown { get; }
        bool IsMouseRightDownThisFrame { get; }

        bool IsMouseMiddleDown { get; }
        bool IsMouseMiddleDownThisFrame { get; }

        Vector2 ScrollDelta { get; }
        Vector2 MousePosition { get; }

        Vector2 MouseDownPosition { get; }

        bool IsDragging { get; }

    }

}