using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using Rendering;
using Src.Compilers;
using Src.Input;
using Src.InputBindings;
using UnityEngine;

namespace Src.Systems {

    public class KeyboardEventTreeNode : IHierarchical {

        private readonly UIElement element;
        public readonly IReadOnlyList<KeyboardEventHandler> handlers;

        public KeyboardEventTreeNode(UIElement element, List<KeyboardEventHandler> handlers) {
            this.element = element;
            this.handlers = handlers;
        }

        public int UniqueId => element.id;
        public IHierarchical Element => element;
        public IHierarchical Parent => element.parent;

    }

    public class MouseEventTreeNode : IHierarchical {

        private readonly UIElement element;
        public readonly IReadOnlyList<MouseEventHandler> handlers;

        public MouseEventTreeNode(UIElement element, List<MouseEventHandler> handlers) {
            this.element = element;
            this.handlers = handlers;
        }

        public int UniqueId => element.id;
        public IHierarchical Element => element;
        public IHierarchical Parent => element.parent;

    }

    public class AcceptFocus : Attribute { }

    // element.style.CreateBinding<Type>(StyleState.Normal, "property", () => value).Enabled = false;

    public class GOInputSystem : IInputSystem, IInputProvider {

        private const string EventAlias = "$event";

        private readonly IStyleSystem styleSystem;
        private readonly ILayoutSystem layoutSystem;

        private HashSet<int> hoverStylesThisFrame;
        private HashSet<int> hoverStylesLastFrame;
        private HashSet<int> elementsThisFrame;
        private HashSet<int> elementsLastFrame;

        private readonly HashSet<int> hoverStyles;
        private readonly IElementRegistry elementSystem;
        private readonly Dictionary<KeyCode, KeyState> keyStates;
        private readonly Dictionary<int, InputBindingGroup> bindingMap;
        private readonly SkipTree<KeyboardEventTreeNode> keyboardEventTree;
        private readonly SkipTree<MouseEventTreeNode> mouseEventTree;

        private int[] scratchArray;
        private int resultCount;
        private LayoutResult[] queryResults;
        private Vector2 mousePosition;

        private int focusedId;
        private List<int> keyEventHandlers;
        private readonly List<KeyCode> downThisFrame;
        private readonly List<KeyCode> upThisFrame;

        private KeyboardModifiers modifiersThisFrame;
        private KeyboardModifiers modifiersLastFrame;

        private UIElement lastFocused;
        private static readonly Event s_Event = new Event();

        public GOInputSystem(ILayoutSystem layoutSystem, IElementRegistry elementSystem, IStyleSystem styleSystem) {
            this.styleSystem = styleSystem;
            this.layoutSystem = layoutSystem;
            this.elementSystem = elementSystem;

            this.hoverStyles = new HashSet<int>();
            this.hoverStylesThisFrame = new HashSet<int>();
            this.hoverStylesLastFrame = new HashSet<int>();

            this.elementsThisFrame = new HashSet<int>();
            this.elementsLastFrame = new HashSet<int>();

            this.bindingMap = new Dictionary<int, InputBindingGroup>();

            this.styleSystem.onAvailableStatesChanged += HandleStatefulStyle;
            queryResults = new LayoutResult[16];
            scratchArray = new int[16];

            upThisFrame = new List<KeyCode>();
            downThisFrame = new List<KeyCode>();
            keyStates = new Dictionary<KeyCode, KeyState>();
            keyboardEventTree = new SkipTree<KeyboardEventTreeNode>();
            mouseEventTree = new SkipTree<MouseEventTreeNode>();

            focusedId = -1;
        }

        public KeyboardModifiers KeyboardModifiers => modifiersThisFrame;

        public bool IsMouseLeftDown { get; private set; }
        public bool IsMouseLeftDownThisFrame { get; private set; }

        public bool IsMouseRightDown { get; private set; }
        public bool IsMouseRightDownThisFrame { get; private set; }

        public bool IsMouseMiddleDown { get; private set; }
        public bool IsMouseMiddleDownThisFrame { get; private set; }

        public Vector2 ScrollDelta { get; private set; }

        public Vector2 MousePosition { get; private set; }
        public Vector2 MouseDownPosition { get; private set; }

        public bool IsDragging { get; private set; }

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

        public void OnUpdate() {
            Vector2 positionLastFrame = mousePosition;

            // this assumes strictly screen space UI for now
            mousePosition = new Vector2(UnityEngine.Input.mousePosition.x, Screen.height - UnityEngine.Input.mousePosition.y);

            // focusProvider.ReleaseFocus(); 

            ProcessKeyboardEvents();
            ProcessMouseEvents();

            RunMouseEvent(InputEventType.MouseMove);

            Swap(ref elementsLastFrame, ref elementsThisFrame);
            Swap(ref hoverStylesLastFrame, ref hoverStylesThisFrame);
            elementsThisFrame.Clear();
            hoverStylesThisFrame.Clear();
        }

        public void OnDestroy() {
            this.styleSystem.onAvailableStatesChanged -= HandleStatefulStyle;
        }

        public void OnReady() { }

        public void OnInitialize() { }

        public void OnReset() {
            // don't clear key states?
            focusedId = -1;
            resultCount = 0;
            elementsLastFrame.Clear();
            elementsThisFrame.Clear();
            hoverStyles.Clear();
            hoverStylesLastFrame.Clear();
            hoverStylesThisFrame.Clear();
            keyboardEventTree.Clear();
            mouseEventTree.Clear();
            bindingMap.Clear();
        }

        private void ResolveMasks(Vector2 point) { }

        public void OnElementCreated(InitData elementData) {
            InputBinding[] inputBindings = elementData.inputBindings;

            if (inputBindings != null && inputBindings.Length > 0) {
                InputEventType handledEvents = 0;

                for (int i = 0; i < inputBindings.Length; i++) {
                    handledEvents |= inputBindings[i].eventType;
                }

                bindingMap[elementData.elementId] = new InputBindingGroup(elementData.context, inputBindings, handledEvents);
            }

            if (elementData.keyboardEventHandlers != null) {
                keyboardEventTree.AddItem(new KeyboardEventTreeNode(elementData.element, elementData.keyboardEventHandlers));
            }

            // todo -- merge mouseEventHandlers with inputBindings
            if (elementData.mouseEventHandlers != null) {
                mouseEventTree.AddItem(new MouseEventTreeNode(elementData.element, elementData.mouseEventHandlers));
            }

            // need a tree for event handlers for bubble / capture
            // capture if any handlers have capture
            // bubble if any handlers have bubble
            // capture first -> top down until target element
            // bubble seconds -> target element -> to top
            // does not traverse the hierarchy, just the parents of element handling click
            // seems like only one element should handle events (ie deepest)

            // events need to specify a phase 

            // find a single target element -> deepest in hierarchy in element tree, not event tree
            // from that node in the event tree traverse to get list of parents
            // while not reached target & event propagation not canceled 
            //     invoke handler
            //      if handler cancels propagation immediately (ie other handlers on same element)
            //      break out
            //       
            for (int i = 0; i < elementData.children.Count; i++) {
                OnElementCreated(elementData.children[i]);
            }
        }

        // probably wise to use EventSystem.current.SetSelectedGameObject() for interop w/ unity components
        // the target does not need to implement ISelectHandler
        // also will want to forward clicks n stuff if using <Prefab> interop

        public void OnElementEnabled(UIElement element) { }

        public void OnElementDisabled(UIElement element) { }

        public void OnElementDestroyed(UIElement element) {
            elementsLastFrame.Remove(element.id);
            elementsThisFrame.Remove(element.id);
            hoverStyles.Remove(element.id);
            hoverStylesLastFrame.Remove(element.id);
            hoverStylesThisFrame.Remove(element.id);

            mouseEventTree.RemoveHierarchy(element);
            keyboardEventTree.RemoveHierarchy(element);
        }

        public void OnElementShown(UIElement element) { }

        public void OnElementHidden(UIElement element) { }

        public KeyState GetKeyState(KeyCode keyCode) {
            KeyState state;
            if (keyStates.TryGetValue(keyCode, out state)) {
                return state;
            }

            return KeyState.Up;
        }

        private void ProcessKeyboardEvent(KeyCode keyCode, InputEventType eventType, char character, KeyboardModifiers modifiers) {
            KeyboardInputEvent keyEvent = new KeyboardInputEvent(eventType, keyCode, character, modifiers, focusedId != -1);
            if (focusedId == -1) {
                keyboardEventTree.ConditionalTraversePreOrder(keyEvent, (item, evt) => {
                    if (evt.stopPropagation) return false;

                    IReadOnlyList<KeyboardEventHandler> handlers = item.handlers;
                    for (int i = 0; i < handlers.Count; i++) {
                        if (evt.stopPropagationImmediately) break;
                        handlers[i].Invoke(item.Element, evt);
                    }

                    if (evt.stopPropagationImmediately) {
                        evt.StopPropagation();
                    }

                    return !evt.stopPropagation;
                });
            }
            else {
                KeyboardEventTreeNode focusedNode = keyboardEventTree.GetItem(focusedId);
                IReadOnlyList<KeyboardEventHandler> handlers = focusedNode.handlers;
                for (int i = 0; i < handlers.Count; i++) {
                    if (keyEvent.stopPropagationImmediately) break;
                    handlers[i].Invoke(focusedNode.Element, keyEvent);
                }
            }
        }

        private void ProcessKeyboardEvents() {
            for (int i = 0; i < upThisFrame.Count; i++) {
                keyStates[upThisFrame[i]] = KeyState.UpNotThisFrame;
            }

            for (int i = 0; i < downThisFrame.Count; i++) {
                keyStates[downThisFrame[i]] = KeyState.DownNotThisFrame;
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
                        if (keyStates.ContainsKey(keyCode)) {
                            KeyState state = keyStates[keyCode];
                            if ((state & KeyState.Down) == 0) {
                                downThisFrame.Add(keyCode);
                                keyStates[keyCode] = KeyState.DownThisFrame;
                                ProcessKeyboardEvent(keyCode, InputEventType.KeyDown, s_Event.character, modifiersThisFrame);
                            }
                        }
                        else {
                            downThisFrame.Add(keyCode);
                            keyStates[keyCode] = KeyState.DownThisFrame;
                            ProcessKeyboardEvent(keyCode, InputEventType.KeyDown, s_Event.character, modifiersThisFrame);
                        }

                        HandleModifierDown(keyCode);
                        break;

                    case EventType.KeyUp:
                        upThisFrame.Add(keyCode);
                        keyStates[keyCode] = KeyState.UpThisFrame;
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
                keyStates[code] = KeyState.UpThisFrame;
                upThisFrame.Add(code);
                ProcessKeyboardEvent(code, InputEventType.KeyUp, '\0', modifiersThisFrame);
            }
            else if (UnityEngine.Input.GetKeyDown(code)) {
                keyStates[code] = KeyState.DownThisFrame;
                downThisFrame.Add(code);
            }
            else if (isDown) {
                keyStates[code] = KeyState.Down;
            }
            else {
                keyStates[code] = KeyState.Up;
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


        private void HandleStatefulStyle(int elementId, StyleState availableStates) {
            bool hasHoverState = (availableStates & StyleState.Hover) != 0;
            if (hasHoverState) {
                hoverStyles.Add(elementId);
            }
            else {
                hoverStyles.Remove(elementId);
            }
        }

        private void ProcessMouseEvents() {

            IsMouseLeftDown = UnityEngine.Input.GetMouseButton(0);
            IsMouseRightDown = UnityEngine.Input.GetMouseButton(1);
            IsMouseMiddleDown = UnityEngine.Input.GetMouseButton(2);
            
            IsMouseLeftDownThisFrame = UnityEngine.Input.GetMouseButtonDown(0);
            IsMouseRightDownThisFrame = UnityEngine.Input.GetMouseButtonDown(1);
            IsMouseMiddleDownThisFrame = UnityEngine.Input.GetMouseButtonDown(2);

            if (IsMouseLeftDown) {
                if (IsMouseLeftDownThisFrame) {
                    MouseDownPosition = UnityEngine.Input.mousePosition;
                }
            }
            else {
                MouseDownPosition = new Vector2(-1, -1);
            }
            
            MousePosition = UnityEngine.Input.mousePosition;
            ScrollDelta = UnityEngine.Input.mouseScrollDelta;
            
            resultCount = layoutSystem.QueryPoint(mousePosition, ref queryResults);
            List<UIElement> elements = new List<UIElement>();

            for (int i = 0; i < resultCount; i++) {
                int elementId = queryResults[i].elementId;

                elementsThisFrame.Add(elementId);

                elements.Add(elementSystem.GetElement(elementId));

                if (hoverStyles.Contains(elementId)) {
                    hoverStylesThisFrame.Add(elementId);
                    styleSystem.EnterState(elementId, StyleState.Hover);
                }
            }

            hoverStylesLastFrame.ExceptWith(hoverStylesThisFrame);
            foreach (int elementId in hoverStylesLastFrame) {
                styleSystem.ExitState(elementId, StyleState.Hover);
            }

            elements.Sort((a, b) => a.depth < b.depth ? 1 : -1);

            // elements this frame that were not in the set last frame
            // -> if has binding for mouse enter -> mouse enter
            // if was in last frame & not in this frame -> mouse exit

            RunMouseEnter();
            RunMouseExit();

            // get top of stack element for mouse
            // traverse while event propagating

            // todo this is only the bubble phase
            MouseInputEvent mouseEvent = new MouseInputEvent(InputEventType.MouseEnter, mousePosition, KeyboardModifiers, false);
            for (int i = 0; i < elements.Count; i++) {
                RunBindings(elements[i], mouseEvent);
                if (mouseEvent.ShouldStopPropagation) {
                    break;
                }
            }
            
            // todo -- or tab focus!

            if (IsMouseLeftDownThisFrame) {
                elementsThisFrame.CopyTo(scratchArray);
                // todo -- sort by depth
                // todo -- cull masked
                bool didFocus = false;
                for (int i = 0; i < elementsThisFrame.Count; i++) {
                    UIElement element = elementSystem.GetElement(scratchArray[i]);
                    
                    if ((element.flags & UIElementFlags.AcceptFocus) == 0) {
                        continue;
                    }
                    
                    if (element.id != focusedId) {
                        if (focusedId != -1) {
                            IFocusable blurElement = (IFocusable) elementSystem.GetElement(focusedId);
                            blurElement.Blur();
                        }

                        IFocusable focusElement = (IFocusable) element;
                        focusElement.Focus();
                        focusedId = element.id;
                    }

                    didFocus = true;
                    break;
                }

                if (!didFocus && focusedId != -1) {
                    IFocusable blurElement = (IFocusable) elementSystem.GetElement(focusedId);
                    blurElement.Blur();
                    focusedId = -1;
                }
            }
        }

        private void RunMouseEnter() {
            if (elementsThisFrame.Count >= scratchArray.Length) {
                Array.Resize(ref scratchArray, elementsThisFrame.Count * 2);
            }

            elementsThisFrame.CopyTo(scratchArray);
            InputEvent mouseEnter = new MouseInputEvent(InputEventType.MouseEnter, mousePosition, modifiersThisFrame, false);
            for (int i = 0; i < elementsThisFrame.Count; i++) {
                int elementId = scratchArray[i];
                if (!elementsLastFrame.Contains(elementId)) {
                    RunBindings(elementId, mouseEnter);
                }
            }
        }

        private void RunMouseExit() {
            if (elementsLastFrame.Count >= scratchArray.Length) {
                Array.Resize(ref scratchArray, elementsThisFrame.Count * 2);
            }

            elementsLastFrame.CopyTo(scratchArray);
            InputEvent mouseExit = new MouseInputEvent(InputEventType.MouseExit, mousePosition, modifiersThisFrame, false);
            for (int i = 0; i < elementsLastFrame.Count; i++) {
                int elementId = scratchArray[i];
                if (!elementsThisFrame.Contains(elementId)) {
                    RunBindings(elementId, mouseExit);
                }
                else {
                    //RunBindings(elementId, mouseHover);
                }
            }
        }

        private void RunMouseEvent(InputEventType eventType) {
            if (elementsThisFrame.Count >= scratchArray.Length) {
                Array.Resize(ref scratchArray, elementsThisFrame.Count * 2);
            }

            elementsThisFrame.CopyTo(scratchArray);
            InputEvent mouseEvent = new MouseInputEvent(eventType, mousePosition, modifiersThisFrame, false);
            for (int i = 0; i < elementsThisFrame.Count; i++) {
                RunBindings(scratchArray[i], mouseEvent);
            }
        }

        private void RunBindings(int elementId, InputEvent inputEvent) {
            RunBindings(elementSystem.GetElement(elementId), inputEvent);
        }

        private void RunBindings(UIElement element, InputEvent inputEvent) {
            InputBindingGroup bindingGroup;

            if (!bindingMap.TryGetValue(element.id, out bindingGroup)) {
                return;
            }

            if ((bindingGroup.handledEvents & inputEvent.type) == 0) {
                return;
            }

            InputBinding[] bindings = bindingGroup.bindings;
            InputEventType eventType = inputEvent.type;

            bindingGroup.context.SetObjectAlias(EventAlias, inputEvent);

            for (int i = 0; i < bindings.Length; i++) {
                InputBinding binding = bindings[i];
                if (binding.eventType != eventType) {
                    continue;
                }

                binding.Execute(element, bindingGroup.context);
                if (inputEvent.ShouldStopPropagationImmediately) {
                    break;
                }
            }

            bindingGroup.context.RemoveObjectAlias(EventAlias);
        }

        private static void Swap<T>(ref T lhs, ref T rhs) {
            T temp = lhs;
            lhs = rhs;
            rhs = temp;
        }

    }

}