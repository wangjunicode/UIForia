using UIForia.Input;
using UIForia.Rendering;

namespace UIForia.Systems {

    public interface IInputSystem : ISystem, IInputProvider {

        MouseInputEvent CurrentMouseEvent { get; }
        KeyboardInputEvent CurrentKeyboardEvent { get; }

    }

}