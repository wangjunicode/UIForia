using System;
using Src.Input;
using Src.Util;
using UnityEngine;

namespace Src {

    [AttributeUsage(AttributeTargets.Method)]
    public abstract class KeyboardInputBindingAttribute : Attribute {

        public readonly KeyCode key;
        public readonly EventModifiers modifiers;
        public readonly InputEventType eventType;
        public readonly bool requireFocus;

        protected KeyboardInputBindingAttribute(KeyCode key, EventModifiers modifiers, InputEventType eventType, bool requireFocus) {
            this.key = key;
            this.modifiers = modifiers;
            this.eventType = eventType;
            this.requireFocus = requireFocus;
        }

    }

    [AttributeUsage(AttributeTargets.Method)]
    public abstract class InputBindingAttribute : Attribute { }


    [AttributeUsage(AttributeTargets.Method)]
    public class OnKeyUpAttribute : KeyboardInputBindingAttribute {

        public OnKeyUpAttribute(KeyCode key = KeyCodeUtil.AnyKey, EventModifiers modifiers = EventModifiers.None)
            : base(key, modifiers, InputEventType.KeyUp, false) { }

    }

    [AttributeUsage(AttributeTargets.Method)]
    public class OnKeyDownAttribute : KeyboardInputBindingAttribute {

        public OnKeyDownAttribute(KeyCode key = KeyCodeUtil.AnyKey, EventModifiers modifiers = EventModifiers.None)
            : base(key, modifiers, InputEventType.KeyDown, false) { }

    }

    [AttributeUsage(AttributeTargets.Method)]
    public class OnKeyUpWithFocusAttribute : KeyboardInputBindingAttribute {

        public OnKeyUpWithFocusAttribute(KeyCode key = KeyCodeUtil.AnyKey, EventModifiers modifiers = EventModifiers.None)
            : base(key, modifiers, InputEventType.KeyUp, true) { }

    }

    [AttributeUsage(AttributeTargets.Method)]
    public class OnKeyDownWithFocusAttribute : KeyboardInputBindingAttribute {

        public OnKeyDownWithFocusAttribute(KeyCode key = KeyCodeUtil.AnyKey, EventModifiers modifiers = EventModifiers.None)
            : base(key, modifiers, InputEventType.KeyDown, true) { }

    }

}