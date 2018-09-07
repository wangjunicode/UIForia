using System;
using Src.Input;
using Src.Util;
using UnityEngine;

namespace Src {

    [AttributeUsage(AttributeTargets.Method)]
    public abstract class KeyboardInputBindingAttribute : Attribute {

        public readonly char character;
        public readonly KeyCode key;
        public readonly KeyboardModifiers modifiers;
        public readonly InputEventType eventType;
        public readonly bool requireFocus;

        protected KeyboardInputBindingAttribute(KeyCode key, char character, KeyboardModifiers modifiers, InputEventType eventType, bool requireFocus) {
            this.key = key;
            this.character = character;
            this.modifiers = modifiers;
            this.eventType = eventType;
            this.requireFocus = requireFocus;
        }

    }

    public class OnFocusAttribute : Attribute { }

    public class OnBlurAttribute : Attribute { }

    public class OnMouseDownAttribute : Attribute { }
    
    // don't get key up events for non key code inputs :(
    [AttributeUsage(AttributeTargets.Method)]
    public class OnKeyUpAttribute : KeyboardInputBindingAttribute {

        public OnKeyUpAttribute(KeyCode key = KeyCodeUtil.AnyKey, KeyboardModifiers modifiers = KeyboardModifiers.None)
            : base(key, '\0', modifiers, InputEventType.KeyUp, false) { }

    }

    [AttributeUsage(AttributeTargets.Method)]
    public class OnKeyDownAttribute : KeyboardInputBindingAttribute {

        public OnKeyDownAttribute(KeyCode key = KeyCodeUtil.AnyKey, KeyboardModifiers modifiers = KeyboardModifiers.None)
            : base(key, '\0', modifiers, InputEventType.KeyDown, false) { }

        public OnKeyDownAttribute(char character, KeyboardModifiers modifiers = KeyboardModifiers.None)
            : base(KeyCodeUtil.AnyKey, character, modifiers, InputEventType.KeyDown, false) { }

    }

    [AttributeUsage(AttributeTargets.Method)]
    public class OnKeyUpWithFocusAttribute : KeyboardInputBindingAttribute {

        public OnKeyUpWithFocusAttribute(KeyCode key = KeyCodeUtil.AnyKey, KeyboardModifiers modifiers = KeyboardModifiers.None)
            : base(key, '\0', modifiers, InputEventType.KeyUp, true) { }

    }

    [AttributeUsage(AttributeTargets.Method)]
    public class OnKeyDownWithFocusAttribute : KeyboardInputBindingAttribute {

        public OnKeyDownWithFocusAttribute(KeyCode key = KeyCodeUtil.AnyKey, KeyboardModifiers modifiers = KeyboardModifiers.None)
            : base(key, '\0', modifiers, InputEventType.KeyDown, true) { }

        public OnKeyDownWithFocusAttribute(char character, KeyboardModifiers modifiers = KeyboardModifiers.None)
            : base(KeyCodeUtil.AnyKey, character, modifiers, InputEventType.KeyDown, true) { }

    }

}