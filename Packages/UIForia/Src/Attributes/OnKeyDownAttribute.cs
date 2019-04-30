using System;
using UIForia.UIInput;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Attributes {
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class OnKeyDownAttribute : KeyboardInputBindingAttribute {

        public OnKeyDownAttribute(KeyCode key = KeyCodeUtil.AnyKey, KeyboardModifiers modifiers = KeyboardModifiers.None, KeyEventPhase keyEventPhase = KeyEventPhase.Early)
            : base(key, '\0', modifiers, InputEventType.KeyDown, false, keyEventPhase) { }

        public OnKeyDownAttribute(char character, KeyboardModifiers modifiers = KeyboardModifiers.None, KeyEventPhase keyEventPhase = KeyEventPhase.Early)
            : base(KeyCodeUtil.AnyKey, character, modifiers, InputEventType.KeyDown, false, keyEventPhase) { }

    }
}