using UIForia.UIInput;

namespace UIForia.Systems {

    public interface IInputSystem : ISystem, IInputProvider {

        MouseInputEvent CurrentMouseEvent { get; }
        KeyboardInputEvent CurrentKeyboardEvent { get; }

    }

}