using System;
using System.Reflection;
using UnityEngine;

namespace Src.Input {

    public abstract class MouseEventHandler : IComparable<MouseEventHandler> {

        public InputEventType eventType;
        public bool requiresFocus;
        public KeyboardModifiers requiredModifiers;
#if DEBUG
        public MethodInfo methodInfo;
#endif
        public abstract void Invoke(object target, MouseInputEvent evt);

        public int CompareTo(MouseEventHandler other) {
            int focusResult = CompareFocus(other);
            if (focusResult != 0) return focusResult;

            int modifierResult = CompareModifiers(other);
            if (modifierResult != 0) return modifierResult;

            return 1;
        }

        protected bool ShouldRun(MouseInputEvent evt) {
            if (evt.type != eventType) return false;

            if (requiresFocus && !evt.isFocused) return false;

            // if all required modifiers are present these should be equal
            if ((requiredModifiers & evt.modifiers) != requiredModifiers) {
                return false;
            }

            return true;
        }     

        private int CompareFocus(MouseEventHandler other) {
            if (other.requiresFocus == requiresFocus) return 0;
            return requiresFocus ? 1 : -1;
        }

        private int CompareModifiers(MouseEventHandler other) {
            if (requiredModifiers == KeyboardModifiers.None && other.requiredModifiers == KeyboardModifiers.None) return 0;
            if (requiredModifiers != KeyboardModifiers.None && other.requiredModifiers != KeyboardModifiers.None) return 0;
            return (requiredModifiers != KeyboardModifiers.None) ? 1 : -1;
        }

    }

    public class MouseEventHandlerIgnoreEvent<T> : MouseEventHandler {

        private readonly Action<T> handler;

        public MouseEventHandlerIgnoreEvent(Action<T> handler) {
            this.handler = handler;
        }

        // can probably merge a bunch of flags & just do 1 check
        public override void Invoke(object target, MouseInputEvent evt) {
            if (ShouldRun(evt)) {
                handler((T) target);
            }
        }

    }

    public class MouseEventHandler<T> : MouseEventHandler {

        private readonly Action<T, MouseInputEvent> handler;

        public MouseEventHandler(Action<T, MouseInputEvent> handler) {
            this.handler = handler;
        }

        public override void Invoke(object target, MouseInputEvent evt) {
            if (ShouldRun(evt)) {
                handler((T) target, evt);
            }
        }

    }

}