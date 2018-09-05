using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using Rendering;
using Src.Compilers;
using Src.Input;
using Src.InputBindings;
using UnityEngine;
using UnityEngine.EventSystems;

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


    [StructLayout(LayoutKind.Explicit)]
    public struct KeyState {

        [FieldOffset(0)] public readonly bool isDownThisFrame;
        [FieldOffset(1)] public readonly bool isDown;
        [FieldOffset(2)] public readonly bool isUpThisFrame;

        public KeyState(bool isDown, bool isDownThisFrame, bool isUpThisFrame) {
            this.isDownThisFrame = isDownThisFrame;
            this.isDown = isDown;
            this.isUpThisFrame = isUpThisFrame;
        }

        public bool isUp => !isDown;

    }

    public class AcceptFocus : Attribute { }

    public class GOInputSystem : IInputSystem {

        public const string EventAlias = "$event";

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

        private int[] scratchArray;
        private int resultCount;
        private LayoutResult[] queryResults;
        private Vector2 mousePosition;

        private int focusedId;
        private List<int> keyEventHandlers;

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

            keyStates = new Dictionary<KeyCode, KeyState>();
            foreach (KeyCode keyCode in Enum.GetValues(typeof(KeyCode))) {
                int intVal = (int) keyCode;
                if (intVal == 0) continue;

                // ignore mouse and joystick keys
                if (intVal > (int) KeyCode.Mouse0) {
                    break;
                }

                keyStates.Add(keyCode, new KeyState(false, false, false));
            }
        }

        public void OnReset() {
            resultCount = 0;
            elementsLastFrame.Clear();
            elementsThisFrame.Clear();
            hoverStyles.Clear();
            hoverStylesLastFrame.Clear();
            hoverStylesThisFrame.Clear();
            bindingMap.Clear();
        }

        private UIElement lastFocused;

        private static Event s_Event = new Event();

        public class KeyCommand {

            

        }

        public enum KeyCommandType {

            None,
            Cut,
            Copy,
            Paste,
            SelectAll,
            
        }
        
        public void OnUpdate() {
            Vector2 positionLastFrame = mousePosition;
            mousePosition = new Vector2(UnityEngine.Input.mousePosition.x, Screen.height - UnityEngine.Input.mousePosition.y);
            QueryLayout();

            List<KeyCode> keysThisFrame = new List<KeyCode>();
            EventModifiers modifiers = 0;
            
            // expose last frame modifiers also?
            
            foreach (KeyValuePair<KeyCode, KeyState> kvp in keyStates) {
                KeyCode keyCode = kvp.Key;
                bool wasDown = kvp.Value.isDown;
                bool isDown = UnityEngine.Input.GetKey(keyCode);
                bool downThisFrame = !wasDown && isDown;
                bool upThisFrame = wasDown && !isDown;

                keyStates[keyCode] = new KeyState(isDown, downThisFrame, upThisFrame);

                if (isDown) {
                    if (downThisFrame) {
                        keysThisFrame.Add(kvp.Key);
                    }

                    switch (keyCode) {
                        case KeyCode.LeftAlt:
                        case KeyCode.RightAlt:
                            modifiers |= EventModifiers.Alt;
                            break;
                        case KeyCode.LeftControl:
                        case KeyCode.RightControl:
                            modifiers |= EventModifiers.Control;
                            break;
                        case KeyCode.LeftCommand:
                        case KeyCode.RightCommand:
                            modifiers |= EventModifiers.Command;
                            break;
                        case KeyCode.LeftWindows:
                        case KeyCode.RightWindows:
                            modifiers |= EventModifiers.FunctionKey;
                            break;
                        case KeyCode.LeftShift:
                        case KeyCode.RightShift:
                            modifiers |= EventModifiers.Shift;
                            break;
                        case KeyCode.Numlock:
                            modifiers |= EventModifiers.Numeric;
                            break;
                        case KeyCode.CapsLock:
                            modifiers |= EventModifiers.CapsLock;
                            break;
                    }
                }
            }

            for (int i = 0; i < keysThisFrame.Count; i++) {
                KeyCode keyCode = keysThisFrame[i];
                if((modifiers & EventModifiers.Control) != 0) {
                    if (keyCode == KeyCode.A) {
                        
                    }
                }
            }
            
            KeyboardInputEvent keyboardEvent = null;//new KeyboardInputEvent();

            // keyboard event handlers should be sorted by require focus
            // then by require specific key
            // then the rest
            // focus + specific key is top priority
            // then focus w/o specific key
            // then specific key w/o focus
            // then any key w/o focus
            
            // different attr for copy, paste, cut, select all, undo?
            // [OnKeyCommand(KeyCommand.Copy)]
            // just a shortcut for ctrl + c?
            // if command comes in, 2 options
            // 1. run command handlers first
            //    if still propagating, run key handlers
            // 2. run command handlers
            //    drop key handlers for that frame
            
            // event should have a list of all keys pressed this frame
            // it should also give an index of which is currently being processed
            
            if (focusedId != -1) {
                keyboardEventTree.ConditionalTraversePreOrder((item) => {
                    IReadOnlyList<KeyboardEventHandler> handlers = item.handlers;
                    for (int i = 0; i < handlers.Count; i++) {
                        if (keyboardEvent.stopPropagationImmediately) break;
                        handlers[i].Invoke(item.Element, keyboardEvent);
                    }
                    return keyboardEvent.stopPropagation;
                });
                
            }
            else {
                KeyboardEventTreeNode focusedNode = keyboardEventTree.GetItem(focusedId);
                IReadOnlyList<KeyboardEventHandler> handlers = focusedNode.handlers;
                for (int i = 0; i < handlers.Count; i++) {
                    if (keyboardEvent.stopPropagationImmediately) break;
                    handlers[i].Invoke(focusedNode.Element, keyboardEvent);
                }
            }
            
            // DetectKeyCommands();
            // DetectKeySequences();

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

        private readonly SkipTree<KeyboardEventTreeNode> keyboardEventTree;


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

        // event type [KeyDown | KeyUp | KeyCommand | RequireFocus]
        // handler -> needs to get reflected for arg types if takes event or not, if arg is not event, error
        // mask -> Tuple {KeyCode, Modifier} only invoke if mask matches
        // masked fns called first if any mask matches don't call others
        // 1 event per frame w/ all keys or multiple events?
        // multiple events w/ cascaded commands 'ctrl-c'

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

        private static void Swap<T>(ref T lhs, ref T rhs) {
            T temp = lhs;
            lhs = rhs;
            rhs = temp;
        }

    }

}