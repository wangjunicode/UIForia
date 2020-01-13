using System;
using UIForia.UIInput;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Elements {

    public class InputHandlerGroup {

        public struct HandlerData {

            public InputEventType eventType;
            public KeyboardModifiers modifiers;
            public KeyCode keyCode;
            public char character;
            public bool requireFocus;
            public EventPhase eventPhase;
            public Action<GenericInputEvent> handler;

        }

        public InputEventType handledEvents;
        public LightList<HandlerData> eventHandlers;

        public InputHandlerGroup() {
            eventHandlers = new LightList<HandlerData>(2);
        }

        public void AddMouseEvent(InputEventType eventType, KeyboardModifiers modifiers, bool requiresFocus, EventPhase phase, Action<GenericInputEvent> handler) {
            handledEvents |= eventType;
            eventHandlers.Add(new HandlerData() {
                eventType = eventType,
                eventPhase = phase,
                keyCode = 0,
                character = '\0',
                requireFocus = requiresFocus,
                modifiers = modifiers,
                handler = handler
            });
        }

        public void AddKeyboardEvent(InputEventType eventType, KeyboardModifiers modifiers, bool requiresFocus, EventPhase phase, KeyCode keyCode, char character, Action<GenericInputEvent> handler) {
            handledEvents |= eventType;
            eventHandlers.Add(new HandlerData() {
                eventType = eventType,
                eventPhase = phase,
                keyCode = keyCode,
                character = character,
                requireFocus = requiresFocus,
                modifiers = modifiers,
                handler = handler
            });
        }

    }

}