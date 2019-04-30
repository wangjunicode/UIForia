using System;
using UIForia.UIInput;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Attributes {
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class OnKeyUpAttribute : KeyboardInputBindingAttribute {

        public OnKeyUpAttribute(KeyCode key = KeyCodeUtil.AnyKey, KeyboardModifiers modifiers = KeyboardModifiers.None, KeyEventPhase keyEventPhase = KeyEventPhase.Early)
            : base(key, '\0', modifiers, InputEventType.KeyUp, false, keyEventPhase) { }

    }
}