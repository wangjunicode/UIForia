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

    public class AcceptFocus : Attribute { }

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
            QueryLayout();

            // focusProvider.ReleaseFocus(); 
            
            ProcessKeyboardEvents();

            // EventSystem.current.currentSelectedGameObject;

            if (UnityEngine.Input.GetMouseButtonDown(0)) {
                RunMouseEvent(InputEventType.MouseDown);

                // for each element this frame sorted by depth

                // if accept focus
                // element.focus();
                // break;

                // if last focused element is not current focused element
                // last focused element.blur()

                List<UIElement> focusAcceptors = new List<UIElement>();
                for (int i = 0; i < elementsThisFrame.Count; i++) {
                    UIElement element = elementSystem.GetElement(scratchArray[i]);
                    if ((element.flags & UIElementFlags.AcceptFocus) != 0) {
                        focusAcceptors.Add(element);
                    }
                }

                if (focusAcceptors.Count > 0) {
                    // ask current focus to relinquish?
                    focusAcceptors.Sort((a, b) => a.depth > b.depth ? -1 : 1);

                    // focus should probably cascade
                    UIElement focused = focusAcceptors[0];
                    if (focused != lastFocused) {
                        if (lastFocused != null) {
                            //view.BlurElement(lastFocused);
                        }

                        lastFocused = focused;
                        // view.FocusElement(focused);
                    }
                }
            }

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
            bindingMap.Clear();
        }

        public void OnElementCreated(InitData elementData) {
            InputBinding[] inputBindings = elementData.inputBindings;

            if (inputBindings != null && inputBindings.Length > 0) {
                InputEventType handledEvents = 0;

                for (int i = 0; i < inputBindings.Length; i++) {
                    handledEvents |= inputBindings[i].eventType;
                }

                bindingMap[elementData.elementId] = new InputBindingGroup(elementData.context, inputBindings, handledEvents);
            }

            // should maybe be implicit based on handlers
            if (elementData.element.GetType().GetCustomAttribute(typeof(AcceptFocus)) != null) {
                //focusAcceptors.Add(elementData.element.id);
            }

            if (elementData.keyboardEventHandlers != null) {
                keyboardEventTree.AddItem(new KeyboardEventTreeNode(elementData.element, elementData.keyboardEventHandlers));
            }

            // if(elementData.keyboardBindings.Count > 0) {
            //    keyboardBindMap.Add(elementData.elementId, elementData.keyboardBindings) 
            //    keyboardBindTree.AddItem(elementData.element);
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


        private void ProcessKeyboardEvent(KeyCode keyCode, InputEventType eventType, KeyboardModifiers modifiers) {
            KeyboardInputEvent keyEvent = new KeyboardInputEvent(eventType, keyCode, modifiers, focusedId != -1);
            if (focusedId != -1) {
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

            modifiersThisFrame = modifiersLastFrame;

            while (Event.PopEvent(s_Event)) {
                KeyCode pressedKeyCode = s_Event.keyCode;
                if (pressedKeyCode == KeyCode.None || (int) pressedKeyCode > (int) KeyCode.Mouse0) {
                    continue;
                }

                switch (s_Event.rawType) {
                    case EventType.KeyDown:
                        downThisFrame.Add(pressedKeyCode);
                        keyStates[pressedKeyCode] = KeyState.DownThisFrame;
                        ProcessKeyboardEvent(pressedKeyCode, InputEventType.KeyDown, modifiersThisFrame);
                        HandleModifierDown(pressedKeyCode);

                        break;
                    case EventType.KeyUp:
                        upThisFrame.Add(pressedKeyCode);
                        keyStates[pressedKeyCode] = KeyState.UpThisFrame;
                        ProcessKeyboardEvent(pressedKeyCode, InputEventType.KeyUp, modifiersThisFrame);
                        HandleModifierUp(pressedKeyCode);
                        break;
                }
            }

            modifiersThisFrame = KeyboardModifiers.None;
        }

        private void HandleModifierDown(KeyCode keyCode) {
            switch (keyCode) {
                case KeyCode.LeftAlt:
                    modifiersThisFrame |= KeyboardModifiers.LeftAlt;
                    break;
                case KeyCode.RightAlt:
                    modifiersThisFrame |= KeyboardModifiers.RightAlt;
                    break;
                case KeyCode.LeftControl:
                    modifiersThisFrame |= KeyboardModifiers.LeftControl;
                    break;
                case KeyCode.RightControl:
                    modifiersThisFrame |= KeyboardModifiers.RightControl;
                    break;
                case KeyCode.LeftCommand:
                    modifiersThisFrame |= KeyboardModifiers.LeftCommand;
                    break;
                case KeyCode.RightCommand:
                    modifiersThisFrame |= KeyboardModifiers.RightCommand;
                    break;
                case KeyCode.LeftWindows:
                    modifiersThisFrame |= KeyboardModifiers.LeftWindows;
                    break;
                case KeyCode.RightWindows:
                    modifiersThisFrame |= KeyboardModifiers.RightWindows;
                    break;
                case KeyCode.LeftShift:
                    modifiersThisFrame |= KeyboardModifiers.LeftShift;
                    break;
                case KeyCode.RightShift:
                    modifiersThisFrame |= KeyboardModifiers.RightShift;
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
                    modifiersThisFrame &= ~KeyboardModifiers.LeftAlt;
                    break;
                case KeyCode.RightAlt:
                    modifiersThisFrame &= ~KeyboardModifiers.RightAlt;
                    break;
                case KeyCode.LeftControl:
                    modifiersThisFrame &= ~KeyboardModifiers.LeftControl;
                    break;
                case KeyCode.RightControl:
                    modifiersThisFrame &= ~KeyboardModifiers.RightControl;
                    break;
                case KeyCode.LeftCommand:
                    modifiersThisFrame &= ~KeyboardModifiers.LeftCommand;
                    break;
                case KeyCode.RightCommand:
                    modifiersThisFrame &= ~KeyboardModifiers.RightCommand;
                    break;
                case KeyCode.LeftWindows:
                    modifiersThisFrame &= ~KeyboardModifiers.LeftWindows;
                    break;
                case KeyCode.RightWindows:
                    modifiersThisFrame &= ~KeyboardModifiers.RightWindows;
                    break;
                case KeyCode.LeftShift:
                    modifiersThisFrame &= ~KeyboardModifiers.LeftShift;
                    break;
                case KeyCode.RightShift:
                    modifiersThisFrame &= ~KeyboardModifiers.RightShift;
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

        private void QueryLayout() {
            resultCount = layoutSystem.QueryPoint(mousePosition, ref queryResults);

            for (int i = 0; i < resultCount; i++) {
                int elementId = queryResults[i].elementId;
                elementsThisFrame.Add(elementId);

                if (hoverStyles.Contains(elementId)) {
                    hoverStylesThisFrame.Add(elementId);
                    styleSystem.EnterState(elementId, StyleState.Hover);
                }
            }

            hoverStylesLastFrame.ExceptWith(hoverStylesThisFrame);
            foreach (int elementId in hoverStylesLastFrame) {
                styleSystem.ExitState(elementId, StyleState.Hover);
            }

            // elements this frame that were not in the set last frame
            // -> if has binding for mouse enter -> mouse enter
            // if was in last frame & not in this frame -> mouse exit

            RunMouseEnter();
            RunMouseExit();
        }

        private void RunMouseEnter() {
            if (elementsThisFrame.Count >= scratchArray.Length) {
                Array.Resize(ref scratchArray, elementsThisFrame.Count * 2);
            }

            elementsThisFrame.CopyTo(scratchArray);
            InputEvent mouseEnter = new MouseInputEvent(InputEventType.MouseEnter, mousePosition);
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
            InputEvent mouseExit = new MouseInputEvent(InputEventType.MouseExit, mousePosition);
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
            InputEvent mouseEvent = new MouseInputEvent(eventType, mousePosition);
            for (int i = 0; i < elementsThisFrame.Count; i++) {
                RunBindings(scratchArray[i], mouseEvent);
            }
        }

        private void RunBindings(int elementId, InputEvent inputEvent) {
            InputBindingGroup bindingGroup;
            if (!bindingMap.TryGetValue(elementId, out bindingGroup)) {
                return;
            }

            if ((bindingGroup.handledEvents & inputEvent.type) == 0) {
                return;
            }

            InputBinding[] bindings = bindingGroup.bindings;
            UIElement element = elementSystem.GetElement(elementId);
            InputEventType eventType = inputEvent.type;

            bindingGroup.context.SetObjectAlias(EventAlias, inputEvent);
            for (int i = 0; i < bindings.Length; i++) {
                InputBinding binding = bindings[i];
                if (binding.eventType == eventType) {
                    binding.Execute(element, bindingGroup.context);
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