using System;
using System.Collections.Generic;
using UIForia.Rendering;
using UIForia.Elements;
using UIForia.Expressions;
using UIForia.Systems;
using UIForia.Templates;
using UIForia.UIInput;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Stystems.InputSystem {

    public abstract class InputSystem : IInputSystem {

        private const float k_DragThreshold = 5f;

        private readonly ILayoutSystem m_LayoutSystem;

        private List<UIElement> m_ElementsThisFrame;
        private List<UIElement> m_ElementsLastFrame;

        private CursorStyle currentCursor;

        protected UIElement m_FocusedElement;
        protected DragEvent m_CurrentDragEvent;
        protected MouseInputEvent m_CurrentMouseEvent;
        protected KeyboardInputEvent m_CurrentKeyboardEvent;

        private KeyboardModifiers modifiersThisFrame;

        protected MouseState m_MouseState;

        private readonly List<UIElement> m_ExitedElements;
        private readonly List<UIElement> m_EnteredElements;
        private readonly List<UIElement> m_MouseDownElements;
        private readonly Dictionary<KeyCode, KeyState> m_KeyStates;
        private readonly Dictionary<int, MouseHandlerGroup> m_MouseHandlerMap;
        private readonly Dictionary<int, DragHandlerGroup> m_DragHandlerMap;
        private readonly Dictionary<int, DragCreatorGroup> m_DragCreatorMap;
        private readonly SkipTree<KeyboardEventTreeNode> m_KeyboardEventTree;

        private readonly List<KeyCode> m_DownThisFrame;
        private readonly List<KeyCode> m_UpThisFrame;

        private readonly EventPropagator m_EventPropagator;
        private readonly List<ValueTuple<MouseEventHandler, UIElement, ExpressionContext>> m_MouseEventCaptureList;
        private readonly List<ValueTuple<DragEventHandler, UIElement, ExpressionContext>> m_DragEventCaptureList;
        protected static readonly UIElement.RenderLayerComparerAscending s_DepthComparer = new UIElement.RenderLayerComparerAscending();
        private static readonly Event s_Event = new Event();

        public KeyboardModifiers KeyboardModifiers => modifiersThisFrame;

        private static readonly ExpressionContext s_DummyContext = new ExpressionContext(null);

        protected InputSystem(ILayoutSystem layoutSystem) {
            this.m_LayoutSystem = layoutSystem;

            this.m_MouseDownElements = new List<UIElement>();
            this.m_ElementsThisFrame = new List<UIElement>();
            this.m_ElementsLastFrame = new List<UIElement>();
            this.m_EnteredElements = new List<UIElement>();
            this.m_ExitedElements = new List<UIElement>();

            this.m_MouseHandlerMap = new Dictionary<int, MouseHandlerGroup>();
            this.m_DragCreatorMap = new Dictionary<int, DragCreatorGroup>();
            this.m_DragHandlerMap = new Dictionary<int, DragHandlerGroup>();

            this.m_UpThisFrame = new List<KeyCode>();
            this.m_DownThisFrame = new List<KeyCode>();
            this.m_KeyStates = new Dictionary<KeyCode, KeyState>();
            this.m_KeyboardEventTree = new SkipTree<KeyboardEventTreeNode>();
            this.m_EventPropagator = new EventPropagator();
            this.m_MouseEventCaptureList = new List<ValueTuple<MouseEventHandler, UIElement, ExpressionContext>>();
            this.m_DragEventCaptureList = new List<ValueTuple<DragEventHandler, UIElement, ExpressionContext>>();
            this.m_FocusedElement = null;
        }

        public DragEvent CurrentDragEvent => m_CurrentDragEvent;
        public MouseInputEvent CurrentMouseEvent => m_CurrentMouseEvent;
        public KeyboardInputEvent CurrentKeyboardEvent => m_CurrentKeyboardEvent;

        public bool IsMouseLeftDown => m_MouseState.isLeftMouseDown;
        public bool IsMouseLeftDownThisFrame => m_MouseState.isLeftMouseDownThisFrame;
        public bool IsMouseLeftUpThisFrame => m_MouseState.isLeftMouseUpThisFrame;

        public bool IsMouseRightDown => m_MouseState.isRightMouseDown;
        public bool IsMouseRightDownThisFrame => m_MouseState.isRightMouseDownThisFrame;
        public bool IsMouseRightUpThisFrame => m_MouseState.isRightMouseUpThisFrame;

        public bool IsMouseMiddleDown => m_MouseState.isMiddleMouseDown;
        public bool IsMouseMiddleDownThisFrame => m_MouseState.isMiddleMouseDownThisFrame;
        public bool IsMouseMiddleUpThisFrame => m_MouseState.isMiddleMouseUpThisFrame;

        public Vector2 ScrollDelta => m_MouseState.scrollDelta;

        public Vector2 MousePosition => m_MouseState.mousePosition;
        public Vector2 MouseDownPosition => m_MouseState.mouseDownPosition;

        public bool IsDragging { get; protected set; }

        protected abstract MouseState GetMouseState();

        // todo -- make this work
        private void HandleCreateScrollbar(VirtualScrollbar scrollbar) {
            m_DragCreatorMap.Add(scrollbar.id, new DragCreatorGroup(s_DummyContext, new DragEventCreator[] {
                new DragEventCreator_WithEvent<VirtualScrollbar>(KeyboardModifiers.None, EventPhase.Bubble, (instance, evt) => instance.CreateDragEvent(evt)),
            }));

            m_MouseHandlerMap.Add(scrollbar.id, new MouseHandlerGroup(s_DummyContext, new MouseEventHandler[] {
                    new MouseEventHandler_IgnoreEvent<VirtualScrollbar>(InputEventType.MouseEnter, KeyboardModifiers.None, EventPhase.Bubble, (instance) => instance.OnMouseEnter()),
                    new MouseEventHandler_IgnoreEvent<VirtualScrollbar>(InputEventType.MouseHover, KeyboardModifiers.None, EventPhase.Bubble, (instance) => instance.OnMouseMoveOrHover()),
                    new MouseEventHandler_IgnoreEvent<VirtualScrollbar>(InputEventType.MouseMove, KeyboardModifiers.None, EventPhase.Bubble, (instance) => instance.OnMouseMoveOrHover()),
                    new MouseEventHandler_IgnoreEvent<VirtualScrollbar>(InputEventType.MouseExit, KeyboardModifiers.None, EventPhase.Bubble, (instance) => instance.OnMouseExit()),
                }, InputEventType.MouseEnter | InputEventType.MouseHover | InputEventType.MouseMove | InputEventType.MouseExit));
        }

        public bool RequestFocus(IFocusable target) {
            if (!(target is UIElement)) {
                return false;
            }

            // todo -- if focus handlers added via template invoke them
            if (m_FocusedElement != null) {
                if (m_FocusedElement == (UIElement) target) {
                    return true;
                }

                IFocusable focusable = (IFocusable) m_FocusedElement;
                m_FocusedElement.style.ExitState(StyleState.Focused);
                focusable.Blur();
            }

            m_FocusedElement = (UIElement) target;
            target.Focus();
            m_FocusedElement.style.EnterState(StyleState.Focused);
            return true;
        }

        public void ReleaseFocus(IFocusable target) {
            if (m_FocusedElement == (UIElement) target) {
                IFocusable focusable = (IFocusable) m_FocusedElement;
                m_FocusedElement.style.ExitState(StyleState.Focused);
                focusable.Blur();
                // todo -- if focus handlers added via template invoke them
                m_FocusedElement = null;
            }
        }

        public virtual void OnUpdate() {
            m_MouseState = GetMouseState();

            ProcessKeyboardEvents();
            ProcessMouseInput();

            if (!IsDragging) {
                ProcessMouseEvents();
            }

            ProcessDragEvents();


            List<UIElement> temp = m_ElementsLastFrame;
            m_ElementsLastFrame = m_ElementsThisFrame;
            m_ElementsThisFrame = temp;

            m_ElementsThisFrame.Clear();
            m_EnteredElements.Clear();
            m_ExitedElements.Clear();

            if (IsMouseLeftUpThisFrame) {
                m_MouseDownElements.Clear();
            }
        }

        private void ProcessMouseInput() {
            List<UIElement> queryResults = m_LayoutSystem.QueryPoint(m_MouseState.mousePosition, ListPool<UIElement>.Get());

            for (int i = 0; i < queryResults.Count; i++) {
                UIElement element = queryResults[i];

                // todo -- handle masking here
                m_ElementsThisFrame.Add(element);

                if (!m_ElementsLastFrame.Contains(element)) {
                    m_EnteredElements.Add(element);
                    element.style?.EnterState(StyleState.Hover);
                }
            }

            for (int i = 0; i < m_ElementsLastFrame.Count; i++) {
                if (!m_ElementsThisFrame.Contains(m_ElementsLastFrame[i])) {
                    m_ExitedElements.Add(m_ElementsLastFrame[i]);
                    m_ElementsLastFrame[i].style?.ExitState(StyleState.Hover);
                }
            }

            m_EnteredElements.Sort(s_DepthComparer);
            m_ElementsThisFrame.Sort(s_DepthComparer);

            CursorStyle newCursor = null;
            if (m_ElementsThisFrame.Count > 0) {
                for (int i = 0; i < m_ElementsThisFrame.Count; i++) {
                    UIElement element = m_ElementsThisFrame[i];

                    if (element.style.IsDefined(StylePropertyId.Cursor)) {
                        newCursor = element.style.Cursor;
                        if (!newCursor.Equals(currentCursor)) {
                            Cursor.SetCursor(newCursor.texture, newCursor.hotSpot, CursorMode.Auto);
                        }

                        break;
                    }
                }
            }

            if (currentCursor != null && newCursor == null) {
                Cursor.SetCursor(null, new Vector2(0, 0), CursorMode.Auto);
            }

            currentCursor = newCursor;

            if (m_MouseState.isLeftMouseDownThisFrame) {
                m_MouseDownElements.AddRange(m_ElementsThisFrame);
            }

            ListPool<UIElement>.Release(ref queryResults);
        }

        private void ProcessDragEvents() {
            if (IsDragging) {
                if (m_MouseState.isLeftMouseUpThisFrame) {
                    EndDrag(InputEventType.DragDrop);
                    m_MouseDownElements.Clear();
                }
                else {
                    UpdateDrag();
                }

                return;
            }

            if (m_MouseState.isLeftMouseDown) {
                if (Vector2.Distance(m_MouseState.mouseDownPosition, m_MouseState.mousePosition) >= k_DragThreshold) {
                    BeginDrag();
                }

                return;
            }

            IsDragging = false;
        }

        private void UpdateDrag(bool firstFrame = false) {
            if (m_CurrentDragEvent == null) {
                return;
            }

            m_CurrentDragEvent.MousePosition = MousePosition;
            m_CurrentDragEvent.Modifiers = modifiersThisFrame;

            if (firstFrame) {
                RunDragEvent(m_ElementsThisFrame, InputEventType.DragEnter);
                m_CurrentDragEvent.Update();
            }
            else {
                RunDragEvent(m_ExitedElements, InputEventType.DragExit);
                RunDragEvent(m_EnteredElements, InputEventType.DragEnter);
                m_CurrentDragEvent.Update();
                RunDragEvent(m_ElementsThisFrame, m_MouseState.DidMove ? InputEventType.DragMove : InputEventType.DragHover);
            }

            if (m_CurrentDragEvent.IsCanceled) {
                EndDrag(InputEventType.DragCancel);
            }

            if (m_CurrentDragEvent.IsDropped) {
                EndDrag(InputEventType.DragDrop);
            }
        }

        private void BeginDrag() {
            IsDragging = true;
            m_EventPropagator.Reset(m_MouseState);
            MouseInputEvent mouseEvent = new MouseInputEvent(m_EventPropagator, InputEventType.DragCreate, modifiersThisFrame);
            m_CurrentMouseEvent = mouseEvent;

            for (int i = 0; i < m_MouseDownElements.Count; i++) {
                UIElement element = m_MouseDownElements[i];

                if (element.layoutResult.HasScrollbarVertical || element.layoutResult.HasScrollbarHorizontal) {
                    Scrollbar scrollbar = Application.GetCustomScrollbar(element.style.Scrollbar);
                    m_CurrentDragEvent = scrollbar.CreateDragEvent(element, mouseEvent);
                }

                if (m_CurrentDragEvent == null) {
                    DragCreatorGroup dragCreatorGroup;
                    if (!m_DragCreatorMap.TryGetValue(element.id, out dragCreatorGroup)) {
                        continue;
                    }

                    // todo -- figure out if these should respect propagation

                    m_CurrentDragEvent = dragCreatorGroup.TryCreateEvent(element, mouseEvent);
                    if (m_CurrentDragEvent == null) {
                        continue;
                    }
                }

                m_CurrentDragEvent.StartTime = Time.realtimeSinceStartup;
                m_CurrentDragEvent.DragStartPosition = MousePosition;

                UpdateDrag(true);
                return;
            }
        }

        private void EndDrag(InputEventType evtType) {
            IsDragging = false;

            if (m_CurrentDragEvent == null) {
                return;
            }

            m_CurrentDragEvent.MousePosition = MousePosition;
            m_CurrentDragEvent.Modifiers = modifiersThisFrame;

            if (evtType == InputEventType.DragCancel) {
                RunDragEvent(m_ElementsThisFrame, InputEventType.DragCancel);
            }
            else if (evtType == InputEventType.DragDrop) {
                RunDragEvent(m_ElementsThisFrame, InputEventType.DragDrop);
                m_CurrentDragEvent.Drop(true);
            }

            m_CurrentDragEvent = null;
        }

        private void RunDragEvent(List<UIElement> elements, InputEventType eventType) {
            if (m_CurrentDragEvent.IsCanceled && eventType != InputEventType.DragCancel) {
                return;
            }

            m_CurrentDragEvent.CurrentEventType = eventType;
            m_CurrentDragEvent.source = m_EventPropagator;

            m_EventPropagator.Reset(m_MouseState);

            for (int i = 0; i < elements.Count; i++) {
                UIElement element = elements[i];
                DragHandlerGroup dragHandlerGroup;

                if (!m_DragHandlerMap.TryGetValue(element.id, out dragHandlerGroup)) {
                    continue;
                }

                if ((dragHandlerGroup.handledEvents & eventType) == 0) {
                    continue;
                }

                DragEventHandler[] handlers = dragHandlerGroup.handlers;

                for (int j = 0; j < handlers.Length; j++) {
                    DragEventHandler handler = handlers[j];
                    if (handler.eventType != eventType) {
                        continue;
                    }

                    if (handler.eventPhase != EventPhase.Bubble) {
                        m_DragEventCaptureList.Add(ValueTuple.Create(handler, element, dragHandlerGroup.context));
                        continue;
                    }

                    handler.Invoke(element, dragHandlerGroup.context, m_CurrentDragEvent);

                    if (m_CurrentDragEvent.IsCanceled || m_EventPropagator.shouldStopPropagation) {
                        break;
                    }
                }

                if (m_EventPropagator.shouldStopPropagation) {
                    break;
                }

                if (m_CurrentDragEvent.IsCanceled || m_EventPropagator.shouldStopPropagation) {
                    m_DragEventCaptureList.Clear();
                    return;
                }
            }

            for (int i = 0; i < m_DragEventCaptureList.Count; i++) {
                DragEventHandler handler = m_DragEventCaptureList[i].Item1;
                UIElement element = m_DragEventCaptureList[i].Item2;
                ExpressionContext context = m_DragEventCaptureList[i].Item3;

                handler.Invoke(element, context, m_CurrentDragEvent);

                if (m_EventPropagator.shouldStopPropagation) {
                    m_DragEventCaptureList.Clear();
                    return;
                }
            }

            m_DragEventCaptureList.Clear();
        }

        public void OnReset() {
            // don't clear key states
            m_FocusedElement = null;
            m_ElementsLastFrame.Clear();
            m_ElementsThisFrame.Clear();
            m_MouseDownElements.Clear();
            m_KeyboardEventTree.Clear();
            m_MouseHandlerMap.Clear();
            m_DragCreatorMap.Clear();
            m_DragHandlerMap.Clear();
        }

        public void OnDestroy() { }

        public void OnViewAdded(UIView view) { }

        public void OnViewRemoved(UIView view) { }

        public void OnElementEnabled(UIElement element) { }

        public void OnElementDisabled(UIElement element) { }

        public void OnElementDestroyed(UIElement element) {
            m_ElementsLastFrame.Remove(element);
            m_ElementsThisFrame.Remove(element);
            m_MouseDownElements.Remove(element);
            m_KeyboardEventTree.RemoveHierarchy(element);
            // todo -- clear child handlers
            m_MouseHandlerMap.Remove(element.id);
            m_DragCreatorMap.Remove(element.id);
            m_DragHandlerMap.Remove(element.id);
        }

        public void OnElementCreated(UIElement element) {
            UITemplate template = element.OriginTemplate;
            MouseEventHandler[] mouseHandlers = template.mouseEventHandlers;
            DragEventCreator[] dragEventCreators = template.dragEventCreators;
            DragEventHandler[] dragEventHandlers = template.dragEventHandlers;
            KeyboardEventHandler[] keyboardHandlers = template.keyboardEventHandlers;

            if (mouseHandlers != null && mouseHandlers.Length > 0) {
                InputEventType handledEvents = 0;

                for (int i = 0; i < mouseHandlers.Length; i++) {
                    handledEvents |= mouseHandlers[i].eventType;
                }

                m_MouseHandlerMap[element.id] = new MouseHandlerGroup(element.templateContext, mouseHandlers, handledEvents);
            }

            if (dragEventHandlers != null && dragEventHandlers.Length > 0) {
                InputEventType handledEvents = 0;

                for (int i = 0; i < dragEventHandlers.Length; i++) {
                    handledEvents |= dragEventHandlers[i].eventType;
                }

                m_DragHandlerMap[element.id] = new DragHandlerGroup(element.templateContext, dragEventHandlers, handledEvents);
            }

            if (keyboardHandlers != null && keyboardHandlers.Length > 0) {
                m_KeyboardEventTree.AddItem(new KeyboardEventTreeNode(element, keyboardHandlers));
            }

            if (dragEventCreators != null && dragEventCreators.Length > 0) {
                m_DragCreatorMap[element.id] = new DragCreatorGroup(element.templateContext, dragEventCreators);
            }

            if (element.children == null) {
                return;
            }

            for (int i = 0; i < element.children.Count; i++) {
                OnElementCreated(element.children[i]);
            }
        }

        public void OnAttributeSet(UIElement element, string attributeName, string currentValue, string attributeValue) { }

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
            KeyboardInputEvent keyEvent = new KeyboardInputEvent(eventType, keyCode, character, modifiers, m_FocusedElement != null);
            m_CurrentKeyboardEvent = keyEvent;

            if (m_FocusedElement == null) {
                m_KeyboardEventTree.ConditionalTraversePreOrder(keyEvent, (item, evt) => {
                    if (evt.stopPropagation) return false;

                    IReadOnlyList<KeyboardEventHandler> handlers = item.handlers;
                    for (int i = 0; i < handlers.Count; i++) {
                        if (evt.stopPropagationImmediately) break;
                        handlers[i].Invoke(item.Element, default, evt);
                    }

                    if (evt.stopPropagationImmediately) {
                        evt.StopPropagation();
                    }

                    return !evt.stopPropagation;
                });
            }
            else {
                KeyboardEventTreeNode focusedNode = m_KeyboardEventTree.GetItem(m_FocusedElement);
                IReadOnlyList<KeyboardEventHandler> handlers = focusedNode.handlers;
                for (int i = 0; i < handlers.Count; i++) {
                    if (keyEvent.stopPropagationImmediately) break;
                    handlers[i].Invoke(focusedNode.Element, default, keyEvent);
                }
            }
        }

        private void ProcessKeyboardEvents() {
            for (int i = 0; i < m_UpThisFrame.Count; i++) {
                m_KeyStates[m_UpThisFrame[i]] = KeyState.UpNotThisFrame;
            }

            for (int i = 0; i < m_DownThisFrame.Count; i++) {
                m_KeyStates[m_DownThisFrame[i]] = KeyState.DownNotThisFrame;
            }

            m_DownThisFrame.Clear();
            m_UpThisFrame.Clear();

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

                if (s_Event.rawType == EventType.ExecuteCommand || s_Event.rawType == EventType.ValidateCommand) {
                    switch (s_Event.commandName) {
                        case "SelectAll":
                            ProcessKeyEvent(EventType.KeyDown, KeyCode.A, 'a');
                            continue;
                        case "Copy":
                            ProcessKeyEvent(EventType.KeyDown, KeyCode.C, 'c');
                            continue;
                        case "Cut":
                            ProcessKeyEvent(EventType.KeyDown, KeyCode.X, 'x');
                            continue;
                        case "Paste":
                            ProcessKeyEvent(EventType.KeyDown, KeyCode.V, 'v');
                            continue;
                        case "SoftDelete":
                            Debug.Log("Delete");
                            continue;
                        case "Duplicate":
                            Debug.Log("Duplicate");
                            continue;
                        case "Find":
                            continue;
//                            "Copy", "Cut", "Paste", "Delete", "SoftDelete", "Duplicate", "FrameSelected", "FrameSelectedWithLock", "SelectAll", "Find"
                    }
                }

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

                ProcessKeyEvent(s_Event.rawType, s_Event.keyCode, s_Event.character);
            }
        }

        private void ProcessKeyEvent(EventType evtType, KeyCode keyCode, char character) {
            switch (evtType) {
                case EventType.KeyDown:
                    if (m_KeyStates.ContainsKey(keyCode)) {
                        KeyState state = m_KeyStates[keyCode];
                        if ((state & KeyState.Down) == 0) {
                            m_DownThisFrame.Add(keyCode);
                            m_KeyStates[keyCode] = KeyState.DownThisFrame;
                            ProcessKeyboardEvent(keyCode, InputEventType.KeyDown, character, modifiersThisFrame);
                        }
                    }
                    else {
                        m_DownThisFrame.Add(keyCode);
                        m_KeyStates[keyCode] = KeyState.DownThisFrame;
                        ProcessKeyboardEvent(keyCode, InputEventType.KeyDown, character, modifiersThisFrame);
                    }

                    HandleModifierDown(keyCode);
                    break;

                case EventType.KeyUp:
                    m_UpThisFrame.Add(keyCode);
                    m_KeyStates[keyCode] = KeyState.UpThisFrame;
                    ProcessKeyboardEvent(keyCode, InputEventType.KeyUp, character, modifiersThisFrame);
                    HandleModifierUp(keyCode);
                    break;
            }
        }

        private void HandleShiftKey(KeyCode code) {
            bool wasDown = IsKeyDown(code);
            bool isDown = Input.GetKey(code);
            if ((wasDown && !isDown) || Input.GetKeyUp(code)) {
                m_KeyStates[code] = KeyState.UpThisFrame;
                m_UpThisFrame.Add(code);
                ProcessKeyboardEvent(code, InputEventType.KeyUp, '\0', modifiersThisFrame);
            }
            else if (Input.GetKeyDown(code)) {
                m_KeyStates[code] = KeyState.DownThisFrame;
                m_DownThisFrame.Add(code);
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
                    if (!Input.GetKey(KeyCode.RightAlt)) {
                        modifiersThisFrame &= ~KeyboardModifiers.Alt;
                    }

                    break;
                case KeyCode.RightAlt:
                    if (!Input.GetKey(KeyCode.LeftAlt)) {
                        modifiersThisFrame &= ~KeyboardModifiers.Alt;
                    }

                    break;
                case KeyCode.LeftControl:
                    if (!Input.GetKey(KeyCode.RightControl)) {
                        modifiersThisFrame &= ~KeyboardModifiers.Control;
                    }

                    break;
                case KeyCode.RightControl:
                    if (!Input.GetKey(KeyCode.LeftControl)) {
                        modifiersThisFrame &= ~KeyboardModifiers.Control;
                    }

                    break;
                case KeyCode.LeftCommand:
                    if (!Input.GetKey(KeyCode.RightCommand)) {
                        modifiersThisFrame &= ~KeyboardModifiers.Command;
                    }

                    break;
                case KeyCode.RightCommand:
                    if (!Input.GetKey(KeyCode.LeftCommand)) {
                        modifiersThisFrame &= ~KeyboardModifiers.Command;
                    }

                    break;
                case KeyCode.LeftWindows:
                    if (!Input.GetKey(KeyCode.RightWindows)) {
                        modifiersThisFrame &= ~KeyboardModifiers.Windows;
                    }

                    break;
                case KeyCode.RightWindows:
                    if (!Input.GetKey(KeyCode.LeftWindows)) {
                        modifiersThisFrame &= ~KeyboardModifiers.Windows;
                    }

                    break;
                case KeyCode.LeftShift:
                    if (!Input.GetKey(KeyCode.RightShift)) {
                        modifiersThisFrame &= ~KeyboardModifiers.Shift;
                    }

                    break;
                case KeyCode.RightShift:
                    if (!Input.GetKey(KeyCode.LeftShift)) {
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

        private void RunMouseEvents(List<UIElement> elements, InputEventType eventType) {
            m_EventPropagator.Reset(m_MouseState);
            MouseInputEvent mouseEvent = new MouseInputEvent(m_EventPropagator, eventType, modifiersThisFrame);
            m_CurrentMouseEvent = mouseEvent;

            for (int i = 0; i < elements.Count; i++) {
                UIElement element = elements[i];
                MouseHandlerGroup mouseHandlerGroup;

                if (element.layoutResult.HasScrollbarVertical) {
                    Scrollbar scrollbar = Application.GetCustomScrollbar(null);
                    scrollbar.HandleMouseInputEvent(element, mouseEvent);
                    if (m_EventPropagator.shouldStopPropagation) {
                        return;
                    }
                }
                
                if (!m_MouseHandlerMap.TryGetValue(element.id, out mouseHandlerGroup)) {
                    continue;
                }

                if ((mouseHandlerGroup.handledEvents & eventType) == 0) {
                    continue;
                }

                MouseEventHandler[] handlers = mouseHandlerGroup.handlers;

                for (int j = 0; j < handlers.Length; j++) {
                    MouseEventHandler handler = handlers[j];
                    if (handler.eventType != eventType) {
                        continue;
                    }

                    if (handler.eventPhase != EventPhase.Bubble) {
                        m_MouseEventCaptureList.Add(ValueTuple.Create(handler, element, mouseHandlerGroup.context));
                        continue;
                    }

                    handler.Invoke(element, mouseHandlerGroup.context, mouseEvent);

                    if (m_EventPropagator.shouldStopPropagation) {
                        break;
                    }
                }

                if (m_EventPropagator.shouldStopPropagation) {
                    m_MouseEventCaptureList.Clear();
                    return;
                }
            }

            for (int i = 0; i < m_MouseEventCaptureList.Count; i++) {
                MouseEventHandler handler = m_MouseEventCaptureList[i].Item1;
                UIElement element = m_MouseEventCaptureList[i].Item2;
                ExpressionContext context = m_MouseEventCaptureList[i].Item3;

                handler.Invoke(element, context, mouseEvent);

                if (m_EventPropagator.shouldStopPropagation) {
                    m_MouseEventCaptureList.Clear();
                    return;
                }
            }

            m_MouseEventCaptureList.Clear();
        }

        private void ProcessMouseEvents() {
            RunMouseEvents(m_ExitedElements, InputEventType.MouseExit);
            RunMouseEvents(m_EnteredElements, InputEventType.MouseEnter);

            if (m_MouseState.isLeftMouseDownThisFrame || m_MouseState.isRightMouseDownThisFrame) {
                RunMouseEvents(m_ElementsThisFrame, InputEventType.MouseDown);
            }
            else if (m_MouseState.isLeftMouseUpThisFrame || m_MouseState.isRightMouseUpThisFrame) {
                RunMouseEvents(m_ElementsThisFrame, InputEventType.MouseUp);
                if (m_MouseState.isSingleClick) {
                    RunMouseEvents(m_ElementsThisFrame, InputEventType.MouseClick);
                }
            }

            RunMouseEvents(m_ElementsThisFrame, m_MouseState.DidMove ? InputEventType.MouseMove : InputEventType.MouseHover);
        }

    }

}