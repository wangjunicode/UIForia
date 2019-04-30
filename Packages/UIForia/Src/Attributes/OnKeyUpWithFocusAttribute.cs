using System;
using UIForia.UIInput;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Attributes {
    // don't get key up events for non key code inputs :(
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class OnKeyUpWithFocusAttribute : KeyboardInputBindingAttribute {

        public OnKeyUpWithFocusAttribute(KeyCode key = KeyCodeUtil.AnyKey, KeyboardModifiers modifiers = KeyboardModifiers.None, KeyEventPhase keyEventPhase = KeyEventPhase.Early)
            : base(key, '\0', modifiers, InputEventType.KeyUp, true, keyEventPhase) { }

    }
}