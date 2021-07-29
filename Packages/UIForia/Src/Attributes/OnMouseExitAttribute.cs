using System;
using UIForia.UIInput;

namespace UIForia.Attributes {
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class OnMouseExitAttribute : MouseEventHandlerAttribute {

        public OnMouseExitAttribute(KeyboardModifiers modifiers = KeyboardModifiers.None, EventPhase phase = EventPhase.BeforeUpdate)
            : base(modifiers, InputEventType.MouseExit, phase) { }

        public OnMouseExitAttribute(EventPhase phase)
            : base(KeyboardModifiers.None, InputEventType.MouseExit, phase) { }

    }
}