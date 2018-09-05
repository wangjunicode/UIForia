using System;
using System.Reflection;
using Src.Input;
using Src.Util;
using UnityEngine;

namespace Src.Compilers {

    public abstract class KeyboardEventHandler {

        public InputEventType eventType;
        public bool requiresFocus;
        public EventModifiers requiredModifiers;
        public KeyCode keyCode;
#if DEBUG
        public MethodInfo methodInfo;
#endif
        public abstract void Invoke(object element, KeyboardInputEvent evt);

    }

    public class KeyboardEventHandlerIgnoreEvent<T> : KeyboardEventHandler {

        private readonly Action<T> handler;

        public KeyboardEventHandlerIgnoreEvent(Action<T> handler) {
            this.handler = handler;
        }

        // can probably merge a bunch of flags & just do 1 check
        public override void Invoke(object element, KeyboardInputEvent evt) {
            if (evt.type != eventType) return;
            
            if (requiresFocus && !evt.isFocused) return;
            
            if (evt.keyCode != keyCode && keyCode != KeyCodeUtil.AnyKey) {
                return;
            }
            
            // if all required modifiers are present these should be equal
            if ((requiredModifiers & evt.modifiers) != requiredModifiers) {
                return;
            }
            
            handler((T) element);
            
        }        

    }

    public class KeyboardEventHandler<T> : KeyboardEventHandler {

        private readonly Action<T, KeyboardInputEvent> handler;

        public KeyboardEventHandler(Action<T, KeyboardInputEvent> handler) {
            this.handler = handler;
        }

        public override void Invoke(object element, KeyboardInputEvent evt) {
            if (evt.type != eventType) return;
            
            if (requiresFocus && !evt.isFocused) return;
            
            if (evt.keyCode != keyCode && keyCode != KeyCodeUtil.AnyKey) {
                return;
            }
            
            // if all required modifiers are present these should be equal
            if ((requiredModifiers & evt.modifiers) != requiredModifiers) {
                return;
            }
            handler((T) element, evt);
        }

    }

}