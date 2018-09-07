using System;
using System.Reflection;
using Src.Input;
using Src.Util;
using UnityEngine;

namespace Src.Compilers {

    public abstract class KeyboardEventHandler : IComparable<KeyboardEventHandler> {

        public InputEventType eventType;
        public bool requiresFocus;
        public KeyboardModifiers requiredModifiers;
        public KeyCode keyCode;
        public char character;
#if DEBUG
        public MethodInfo methodInfo;
#endif
        public abstract void Invoke(object target, KeyboardInputEvent evt);

        // keyboard event handlers should be sorted by require focus
        // then by require specific key
        // then the rest
        // focus + specific key is top priority
        // then focus w/o specific key
        // then specific key w/o focus
        // then any key w/o focus
        
        public int CompareTo(KeyboardEventHandler other) {

            int focusResult = CompareFocus(other);
            if (focusResult != 0) return focusResult;

            int keyCodeResult = CompareKeys(other);
            if (keyCodeResult != 0) return keyCodeResult;
            
            int modifierResult = CompareModifiers(other);
            if (modifierResult != 0) return modifierResult;

            return 1;
        }

        protected bool ShouldRun(KeyboardInputEvent evt) {
            if (evt.type != eventType) return false;
            
            if (requiresFocus && !evt.isFocused) return false;

            if (character != '\0' && (character != evt.character)) return false;
            
            if (evt.keyCode != keyCode && keyCode != KeyCodeUtil.AnyKey) {
                return false;
            }
            
            // if all required modifiers are present these should be equal
            if ((requiredModifiers & evt.modifiers) != requiredModifiers) {
                return false;
            }

            return true;
        }

        private int CompareKeys(KeyboardEventHandler other) {
            if (keyCode == other.keyCode) return 0;
            return keyCode != KeyCodeUtil.AnyKey ? 1 : -1;
        }

        private int CompareFocus(KeyboardEventHandler other) {
            if (other.requiresFocus == requiresFocus) return 0;
            return requiresFocus ? 1 : -1;
        }

        private int CompareModifiers(KeyboardEventHandler other) {
            if (requiredModifiers == KeyboardModifiers.None && other.requiredModifiers == KeyboardModifiers.None) return 0;
            if (requiredModifiers != KeyboardModifiers.None && other.requiredModifiers != KeyboardModifiers.None) return 0;
            return (requiredModifiers != KeyboardModifiers.None) ? 1 : -1;
        }
    }
    

    public class KeyboardEventHandlerIgnoreEvent<T> : KeyboardEventHandler {

        private readonly Action<T> handler;

        public KeyboardEventHandlerIgnoreEvent(Action<T> handler) {
            this.handler = handler;
        }

        // can probably merge a bunch of flags & just do 1 check
        public override void Invoke(object target, KeyboardInputEvent evt) {

            if (ShouldRun(evt)) {
                handler((T) target);
            }

        }        

    }

    public class KeyboardEventHandler<T> : KeyboardEventHandler {

        private readonly Action<T, KeyboardInputEvent> handler;

        public KeyboardEventHandler(Action<T, KeyboardInputEvent> handler) {
            this.handler = handler;
        }

        public override void Invoke(object target, KeyboardInputEvent evt) {
            if (ShouldRun(evt)) {
                handler((T) target, evt);
            }
        }

    }

}