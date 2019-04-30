using System;
using UIForia.UIInput;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Attributes {
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class OnKeyDownWithFocusAttribute : KeyboardInputBindingAttribute {

        public OnKeyDownWithFocusAttribute(KeyCode key = KeyCodeUtil.AnyKey, KeyboardModifiers modifiers = KeyboardModifiers.None, KeyEventPhase keyEventPhase = KeyEventPhase.Early)
            : base(key, '\0', modifiers, InputEventType.KeyDown, true, keyEventPhase) {
        }

        public OnKeyDownWithFocusAttribute(char character, KeyboardModifiers modifiers = KeyboardModifiers.None, KeyEventPhase keyEventPhase = KeyEventPhase.Early)
            : base(KeyCodeUtil.AnyKey, character, modifiers, InputEventType.KeyDown, true, keyEventPhase) {
        }

    }
}
