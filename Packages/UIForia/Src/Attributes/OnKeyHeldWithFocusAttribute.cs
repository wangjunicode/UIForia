using System;
using UIForia.UIInput;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Attributes {

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class OnKeyHeldWithFocusAttribute : KeyboardInputBindingAttribute {

        public OnKeyHeldWithFocusAttribute(KeyCode key = KeyCodeUtil.AnyKey, KeyboardModifiers modifiers = KeyboardModifiers.None)
            : base(key, '\0', modifiers, InputEventType.KeyHeldDown, true) { }

        public OnKeyHeldWithFocusAttribute(char character, KeyboardModifiers modifiers = KeyboardModifiers.None)
            : base(KeyCodeUtil.AnyKey, character, modifiers, InputEventType.KeyHeldDown, true) { }

    }

}