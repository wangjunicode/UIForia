using System;
using UIForia.UIInput;

namespace UIForia.Systems {

    public interface IInputSystem : ISystem, IInputProvider {

        event Action<IFocusable> onFocusChanged;
        MouseInputEvent CurrentMouseEvent { get; }
        DragEvent CurrentDragEvent { get; }
        KeyboardInputEvent CurrentKeyboardEvent { get; }
        void OnLateUpdate();
    }

}