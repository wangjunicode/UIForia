using System;
using System.Collections.Generic;
using UIForia.Util;
using UnityEngine;

namespace UIForia {

    public interface KeyboardAdapter {

        void GetNewText(StructList<char> text);

        void GetInputEvents();

        Queue<QueuedKeyboardEvent> keyEventQueue { get; }

    }

    public struct QueuedKeyboardEvent : IEquatable<QueuedKeyboardEvent> {

        public char character;
        public EventModifiers modifiers;
        public string commandName;
        public EventType eventType;
        public KeyCode keyCode;

        /// <summary>
        ///   <para>Is Shift held down? (Read Only)</para>
        /// </summary>
        public bool shift {
            get => (uint) (modifiers & EventModifiers.Shift) > 0U;
            set {
                if (!value)
                    modifiers &= ~EventModifiers.Shift;
                else
                    modifiers |= EventModifiers.Shift;
            }
        }

        /// <summary>
        ///   <para>Is Control key held down? (Read Only)</para>
        /// </summary>
        public bool control {
            get => (uint) (modifiers & EventModifiers.Control) > 0U;
            set {
                if (!value)
                    modifiers &= ~EventModifiers.Control;
                else
                    modifiers |= EventModifiers.Control;
            }
        }

        /// <summary>
        ///   <para>Is Alt/Option key held down? (Read Only)</para>
        /// </summary>
        public bool alt {
            get => (uint) (modifiers & EventModifiers.Alt) > 0U;
            set {
                if (!value)
                    modifiers &= ~EventModifiers.Alt;
                else
                    modifiers |= EventModifiers.Alt;
            }
        }

        /// <summary>
        ///   <para>Is Command/Windows key held down? (Read Only)</para>
        /// </summary>
        public bool command {
            get => (uint) (modifiers & EventModifiers.Command) > 0U;
            set {
                if (!value)
                    modifiers &= ~EventModifiers.Command;
                else
                    modifiers |= EventModifiers.Command;
            }
        }

        /// <summary>
        ///   <para>Is Caps Lock on? (Read Only)</para>
        /// </summary>
        public bool capsLock {
            get => (uint) (modifiers & EventModifiers.CapsLock) > 0U;
            set {
                if (!value)
                    modifiers &= ~EventModifiers.CapsLock;
                else
                    modifiers |= EventModifiers.CapsLock;
            }
        }

        /// <summary>
        ///   <para>Is the current keypress on the numeric keyboard? (Read Only)</para>
        /// </summary>
        public bool numeric {
            get => (uint) (modifiers & EventModifiers.Numeric) > 0U;
            set {
                if (!value)
                    modifiers &= ~EventModifiers.Numeric;
                else
                    modifiers |= EventModifiers.Numeric;
            }
        }

        /// <summary>
        ///   <para>Is the current keypress a function key? (Read Only)</para>
        /// </summary>
        public bool functionKey => (uint) (modifiers & EventModifiers.FunctionKey) > 0U;

        public bool Equals(QueuedKeyboardEvent obj) {
            if (eventType != obj.eventType || (modifiers & ~EventModifiers.CapsLock) != (obj.modifiers & ~EventModifiers.CapsLock)) {
                return false;
            }

            return keyCode == obj.keyCode;
        }

        public override bool Equals(object obj) {
            return obj is QueuedKeyboardEvent other && Equals(other);
        }

        public override int GetHashCode() {
            int num = (int) (ushort) keyCode;
            return (int) ((EventModifiers) (num * 37) | modifiers);
        }

    }
}