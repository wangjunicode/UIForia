using System.Collections.Generic;
using UIForia.Util;
using UnityEngine;

namespace UIForia {
    public class LegacyKeyboardAdapter : KeyboardAdapter {

        private static readonly Event s_Event = new Event();
        
        private Queue<QueuedKeyboardEvent> queue;
        
        public LegacyKeyboardAdapter() {
            this.queue = new Queue<QueuedKeyboardEvent>();
        }

        public void GetKeyEvents() {
            queue.Clear();
        }

        public void GetNewText(StructList<char> text) { }
        
        public void GetInputEvents() {
            
        }

        public Queue<QueuedKeyboardEvent> keyEventQueue { get => queue; }

        public void Update() {

            while (Event.PopEvent(s_Event)) {
                if (!s_Event.isKey) {
                    continue;
                }
                KeyCode keyCode = s_Event.keyCode;
                char character = s_Event.character;

                if (keyCode == KeyCode.None && character == '\0') {
                    continue;
                }

                switch (s_Event.rawType) {
                    case EventType.KeyDown:
                        HandleKeyUp(s_Event);
                        break;
                    case EventType.KeyUp:
                        Debug.Log($"{s_Event.shift} {character}");
                        HandleKeyDown(s_Event);
                        break;
                }
            }
        }

        public void HandleKeyUp(Event current) {

            queue.Enqueue(new QueuedKeyboardEvent {
                eventType = current.type,
                modifiers = current.modifiers,
                commandName = current.commandName,
                character = current.character,
                keyCode = current.keyCode
            });
            
        }

        public void HandleKeyDown(Event current) {
            queue.Enqueue(new QueuedKeyboardEvent {
                eventType = current.type,
                modifiers = current.modifiers,
                commandName = current.commandName,
                character = current.character,
                keyCode = current.keyCode
            });

        }

    }
}