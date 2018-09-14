using System;
using Src.Input;
using Src.Util;
using UnityEngine;

namespace Src {

    [AttributeUsage(AttributeTargets.Class)]
    public class AcceptFocus : Attribute { }

    [AttributeUsage(AttributeTargets.Method)]
    public abstract class MouseEventHandlerAttribute : Attribute {

        public readonly KeyboardModifiers modifiers;
        public readonly InputEventType eventType;
        public readonly EventPhase phase;

        protected MouseEventHandlerAttribute(KeyboardModifiers modifiers, InputEventType eventType, EventPhase phase) {
            this.modifiers = modifiers;
            this.eventType = eventType;
            this.phase = phase;
        }

    }

    [AttributeUsage(AttributeTargets.Method)]
    public class OnMouseDownAttribute : MouseEventHandlerAttribute {

        public OnMouseDownAttribute(KeyboardModifiers modifiers = KeyboardModifiers.None, EventPhase phase = EventPhase.Bubble)
            : base(modifiers, InputEventType.MouseDown, phase) { }


        public OnMouseDownAttribute(EventPhase phase)
            : base(KeyboardModifiers.None, InputEventType.MouseDown, phase) { }

    }

    [AttributeUsage(AttributeTargets.Method)]
    public class OnMouseUpAttribute : MouseEventHandlerAttribute {

        public OnMouseUpAttribute(KeyboardModifiers modifiers = KeyboardModifiers.None, EventPhase phase = EventPhase.Bubble)
            : base(modifiers, InputEventType.MouseUp, phase) { }

        public OnMouseUpAttribute(EventPhase phase)
            : base(KeyboardModifiers.None, InputEventType.MouseUp, phase) { }

    }

    [AttributeUsage(AttributeTargets.Method)]
    public class OnMouseEnterAttribute : MouseEventHandlerAttribute {

        public OnMouseEnterAttribute(KeyboardModifiers modifiers = KeyboardModifiers.None, EventPhase phase = EventPhase.Bubble)
            : base(modifiers, InputEventType.MouseEnter, phase) { }

        public OnMouseEnterAttribute(EventPhase phase)
            : base(KeyboardModifiers.None, InputEventType.MouseEnter, phase) { }

    }

    [AttributeUsage(AttributeTargets.Method)]
    public class OnMouseExitAttribute : MouseEventHandlerAttribute {

        public OnMouseExitAttribute(KeyboardModifiers modifiers = KeyboardModifiers.None, EventPhase phase = EventPhase.Bubble)
            : base(modifiers, InputEventType.MouseExit, phase) { }

        public OnMouseExitAttribute(EventPhase phase)
            : base(KeyboardModifiers.None, InputEventType.MouseExit, phase) { }

    }

    [AttributeUsage(AttributeTargets.Method)]
    public class OnMouseMoveAttribute : MouseEventHandlerAttribute {

        public OnMouseMoveAttribute(KeyboardModifiers modifiers = KeyboardModifiers.None, EventPhase phase = EventPhase.Bubble)
            : base(modifiers, InputEventType.MouseMove, phase) { }

        public OnMouseMoveAttribute(EventPhase phase)
            : base(KeyboardModifiers.None, InputEventType.MouseMove, phase) { }

    }

    [AttributeUsage(AttributeTargets.Method)]
    public class OnMouseHoverAttribute : MouseEventHandlerAttribute {

        public OnMouseHoverAttribute(KeyboardModifiers modifiers = KeyboardModifiers.None, EventPhase phase = EventPhase.Bubble)
            : base(modifiers, InputEventType.MouseHover, phase) { }

        public OnMouseHoverAttribute(EventPhase phase)
            : base(KeyboardModifiers.None, InputEventType.MouseHover, phase) { }

    }


    [AttributeUsage(AttributeTargets.Method)]
    public class OnMouseContextAttribute : MouseEventHandlerAttribute {

        public OnMouseContextAttribute(KeyboardModifiers modifiers = KeyboardModifiers.None, EventPhase phase = EventPhase.Bubble)
            : base(modifiers, InputEventType.MouseContext, phase) { }

        public OnMouseContextAttribute(EventPhase phase)
            : base(KeyboardModifiers.None, InputEventType.MouseContext, phase) { }

    }

    [AttributeUsage(AttributeTargets.Method)]
    public abstract class KeyboardInputBindingAttribute : Attribute {

        public readonly char character;
        public readonly KeyCode key;
        public readonly KeyboardModifiers modifiers;
        public readonly InputEventType eventType;
        public readonly bool requiresFocus;

        protected KeyboardInputBindingAttribute(KeyCode key, char character, KeyboardModifiers modifiers, InputEventType eventType, bool requiresFocus) {
            this.key = key;
            this.character = character;
            this.modifiers = modifiers;
            this.eventType = eventType;
            this.requiresFocus = requiresFocus;
        }

    }


    // don't get key up events for non key code inputs :(
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class OnKeyUpAttribute : KeyboardInputBindingAttribute {

        public OnKeyUpAttribute(KeyCode key = KeyCodeUtil.AnyKey, KeyboardModifiers modifiers = KeyboardModifiers.None)
            : base(key, '\0', modifiers, InputEventType.KeyUp, false) { }

    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class OnKeyDownAttribute : KeyboardInputBindingAttribute {

        public OnKeyDownAttribute(KeyCode key = KeyCodeUtil.AnyKey, KeyboardModifiers modifiers = KeyboardModifiers.None)
            : base(key, '\0', modifiers, InputEventType.KeyDown, false) { }

        public OnKeyDownAttribute(char character, KeyboardModifiers modifiers = KeyboardModifiers.None)
            : base(KeyCodeUtil.AnyKey, character, modifiers, InputEventType.KeyDown, false) { }

    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class OnKeyUpWithFocusAttribute : KeyboardInputBindingAttribute {

        public OnKeyUpWithFocusAttribute(KeyCode key = KeyCodeUtil.AnyKey, KeyboardModifiers modifiers = KeyboardModifiers.None)
            : base(key, '\0', modifiers, InputEventType.KeyUp, true) { }

    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class OnKeyDownWithFocusAttribute : KeyboardInputBindingAttribute {

        public OnKeyDownWithFocusAttribute(KeyCode key = KeyCodeUtil.AnyKey, KeyboardModifiers modifiers = KeyboardModifiers.None)
            : base(key, '\0', modifiers, InputEventType.KeyDown, true) { }

        public OnKeyDownWithFocusAttribute(char character, KeyboardModifiers modifiers = KeyboardModifiers.None)
            : base(KeyCodeUtil.AnyKey, character, modifiers, InputEventType.KeyDown, true) { }

    }


   
}