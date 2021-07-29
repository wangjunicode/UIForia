using System;
using UIForia.UIInput;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Attributes {
    // don't get key up events for non key code inputs :(
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class OnKeyUpWithFocusAttribute : KeyboardInputBindingAttribute {

        public OnKeyUpWithFocusAttribute(KeyCode key = KeyCodeUtil.AnyKey, KeyboardModifiers modifiers = KeyboardModifiers.None, EventPhase keyEventPhase = EventPhase.BeforeUpdate)
            : base(key, '\0', modifiers, InputEventType.KeyUp, true, keyEventPhase) { }

    }
}