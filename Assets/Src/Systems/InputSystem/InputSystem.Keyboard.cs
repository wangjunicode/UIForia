using System.Collections.Generic;
using Src.Input;
using Src.Systems;
using UnityEngine;

public abstract partial class InputSystem {

    private static readonly Event s_Event = new Event();
    
    public KeyboardModifiers KeyboardModifiers => modifiersThisFrame;

    public bool IsKeyDown(KeyCode keyCode) {
        return (GetKeyState(keyCode) & KeyState.Down) != 0;
    }

    public bool IsKeyDownThisFrame(KeyCode keyCode) {
        return (GetKeyState(keyCode) & KeyState.DownThisFrame) != 0;
    }

    public bool IsKeyUp(KeyCode keyCode) {
        KeyState state = GetKeyState(keyCode);
        return (state == KeyState.Up || (state & KeyState.UpThisFrame) != 0);
    }

    public bool IsKeyUpThisFrame(KeyCode keyCode) {
        return (GetKeyState(keyCode) & KeyState.UpThisFrame) != 0;
    }

    public KeyState GetKeyState(KeyCode keyCode) {
        KeyState state;
        if (m_KeyStates.TryGetValue(keyCode, out state)) {
            return state;
        }

        return KeyState.Up;
    }

    protected void ProcessKeyboardEvent(KeyCode keyCode, InputEventType eventType, char character, KeyboardModifiers modifiers) {
        KeyboardInputEvent keyEvent = new KeyboardInputEvent(eventType, keyCode, character, modifiers, m_FocusedId != -1);
        if (m_FocusedId == -1) {
            keyboardEventTree.ConditionalTraversePreOrder(keyEvent, (item, evt) => {
                if (evt.stopPropagation) return false;

                IReadOnlyList<KeyboardEventHandler> handlers = item.handlers;
                for (int i = 0; i < handlers.Count; i++) {
                    if (evt.stopPropagationImmediately) break;
                    handlers[i].Invoke(item.Element, null, evt);
                }

                if (evt.stopPropagationImmediately) {
                    evt.StopPropagation();
                }

                return !evt.stopPropagation;
            });
        }
        else {
            KeyboardEventTreeNode focusedNode = keyboardEventTree.GetItem(m_FocusedId);
            IReadOnlyList<KeyboardEventHandler> handlers = focusedNode.handlers;
            for (int i = 0; i < handlers.Count; i++) {
                if (keyEvent.stopPropagationImmediately) break;
                handlers[i].Invoke(focusedNode.Element, null, keyEvent);
            }
        }
    }

    private void ProcessKeyboardEvents() {
        for (int i = 0; i < upThisFrame.Count; i++) {
            m_KeyStates[upThisFrame[i]] = KeyState.UpNotThisFrame;
        }

        for (int i = 0; i < downThisFrame.Count; i++) {
            m_KeyStates[downThisFrame[i]] = KeyState.DownNotThisFrame;
        }

        downThisFrame.Clear();
        upThisFrame.Clear();

        HandleShiftKey(KeyCode.LeftShift);
        HandleShiftKey(KeyCode.RightShift);

        if (IsKeyDown(KeyCode.LeftShift) || IsKeyDown(KeyCode.RightShift)) {
            modifiersThisFrame |= KeyboardModifiers.Shift;
        }
        else {
            modifiersThisFrame &= ~KeyboardModifiers.Shift;
        }

        while (Event.PopEvent(s_Event)) {
            KeyCode keyCode = s_Event.keyCode;
            char character = s_Event.character;

            // need to check this on osx, according to stackoverflow OSX and Windows might handle
            // sending key events differently

            if (keyCode == KeyCode.None && character != '\0') {
                if (s_Event.rawType == EventType.KeyDown) {
                    ProcessKeyboardEvent(keyCode, InputEventType.KeyDown, character, modifiersThisFrame);
                    continue;
                }

                if (s_Event.rawType == EventType.KeyUp) {
                    ProcessKeyboardEvent(keyCode, InputEventType.KeyUp, character, modifiersThisFrame);
                    continue;
                }
            }

            switch (s_Event.rawType) {
                case EventType.KeyDown:
                    if (m_KeyStates.ContainsKey(keyCode)) {
                        KeyState state = m_KeyStates[keyCode];
                        if ((state & KeyState.Down) == 0) {
                            downThisFrame.Add(keyCode);
                            m_KeyStates[keyCode] = KeyState.DownThisFrame;
                            ProcessKeyboardEvent(keyCode, InputEventType.KeyDown, s_Event.character, modifiersThisFrame);
                        }
                    }
                    else {
                        downThisFrame.Add(keyCode);
                        m_KeyStates[keyCode] = KeyState.DownThisFrame;
                        ProcessKeyboardEvent(keyCode, InputEventType.KeyDown, s_Event.character, modifiersThisFrame);
                    }

                    HandleModifierDown(keyCode);
                    break;

                case EventType.KeyUp:
                    upThisFrame.Add(keyCode);
                    m_KeyStates[keyCode] = KeyState.UpThisFrame;
                    ProcessKeyboardEvent(keyCode, InputEventType.KeyUp, s_Event.character, modifiersThisFrame);
                    HandleModifierUp(keyCode);
                    break;
            }
        }
    }

    private void HandleShiftKey(KeyCode code) {
        bool wasDown = IsKeyDown(code);
        bool isDown = UnityEngine.Input.GetKey(code);
        if ((wasDown && !isDown) || UnityEngine.Input.GetKeyUp(code)) {
            m_KeyStates[code] = KeyState.UpThisFrame;
            upThisFrame.Add(code);
            ProcessKeyboardEvent(code, InputEventType.KeyUp, '\0', modifiersThisFrame);
        }
        else if (UnityEngine.Input.GetKeyDown(code)) {
            m_KeyStates[code] = KeyState.DownThisFrame;
            downThisFrame.Add(code);
        }
        else if (isDown) {
            m_KeyStates[code] = KeyState.Down;
        }
        else {
            m_KeyStates[code] = KeyState.Up;
        }
    }

    private void HandleModifierDown(KeyCode keyCode) {
        switch (keyCode) {
            case KeyCode.LeftAlt:
            case KeyCode.RightAlt:
                modifiersThisFrame |= KeyboardModifiers.Alt;
                break;
            case KeyCode.LeftControl:
            case KeyCode.RightControl:
                modifiersThisFrame |= KeyboardModifiers.Control;
                break;
            case KeyCode.LeftCommand:
            case KeyCode.RightCommand:
                modifiersThisFrame |= KeyboardModifiers.Command;
                break;
            case KeyCode.LeftWindows:
            case KeyCode.RightWindows:
                modifiersThisFrame |= KeyboardModifiers.Windows;
                break;
            case KeyCode.LeftShift:
            case KeyCode.RightShift:
                modifiersThisFrame |= KeyboardModifiers.Shift;
                break;
            case KeyCode.Numlock:
                modifiersThisFrame |= KeyboardModifiers.NumLock;
                break;
            case KeyCode.CapsLock:
                modifiersThisFrame |= KeyboardModifiers.CapsLock;
                break;
        }
    }

    private void HandleModifierUp(KeyCode keyCode) {
        switch (keyCode) {
            case KeyCode.LeftAlt:
                if (!UnityEngine.Input.GetKey(KeyCode.RightAlt)) {
                    modifiersThisFrame &= ~KeyboardModifiers.Alt;
                }

                break;
            case KeyCode.RightAlt:
                if (!UnityEngine.Input.GetKey(KeyCode.LeftAlt)) {
                    modifiersThisFrame &= ~KeyboardModifiers.Alt;
                }

                break;
            case KeyCode.LeftControl:
                if (!UnityEngine.Input.GetKey(KeyCode.RightControl)) {
                    modifiersThisFrame &= ~KeyboardModifiers.Control;
                }

                break;
            case KeyCode.RightControl:
                if (!UnityEngine.Input.GetKey(KeyCode.LeftControl)) {
                    modifiersThisFrame &= ~KeyboardModifiers.Control;
                }

                break;
            case KeyCode.LeftCommand:
                if (!UnityEngine.Input.GetKey(KeyCode.RightCommand)) {
                    modifiersThisFrame &= ~KeyboardModifiers.Command;
                }

                break;
            case KeyCode.RightCommand:
                if (!UnityEngine.Input.GetKey(KeyCode.LeftCommand)) {
                    modifiersThisFrame &= ~KeyboardModifiers.Command;
                }

                break;
            case KeyCode.LeftWindows:
                if (!UnityEngine.Input.GetKey(KeyCode.RightWindows)) {
                    modifiersThisFrame &= ~KeyboardModifiers.Windows;
                }

                break;
            case KeyCode.RightWindows:
                if (!UnityEngine.Input.GetKey(KeyCode.LeftWindows)) {
                    modifiersThisFrame &= ~KeyboardModifiers.Windows;
                }

                break;
            case KeyCode.LeftShift:
                if (!UnityEngine.Input.GetKey(KeyCode.RightShift)) {
                    modifiersThisFrame &= ~KeyboardModifiers.Shift;
                }

                break;
            case KeyCode.RightShift:
                if (!UnityEngine.Input.GetKey(KeyCode.LeftShift)) {
                    modifiersThisFrame &= ~KeyboardModifiers.Shift;
                }

                break;
            case KeyCode.Numlock:
                modifiersThisFrame &= ~KeyboardModifiers.NumLock;
                break;
            case KeyCode.CapsLock:
                modifiersThisFrame &= ~KeyboardModifiers.CapsLock;
                break;
        }
    }

}