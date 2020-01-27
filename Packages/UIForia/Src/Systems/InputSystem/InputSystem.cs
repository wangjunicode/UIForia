using System;
using System.Collections.Generic;
using UIForia.Elements;
using UIForia.Rendering;
using UIForia.Systems.Input;
using UIForia.UIInput;
using UIForia.Util;
using UnityEngine;
using Debug = System.Diagnostics.Debug;

namespace UIForia.Systems {

    public abstract class InputSystem : IInputSystem {

        private struct PressedKey {

            public readonly KeyCode keyCode;
            public readonly char character;

            public PressedKey(KeyCode keyCode, char character) {
                this.keyCode = keyCode;
                this.character = character;
            }

        }

        public event Action<IFocusable> onFocusChanged;

        private const float k_DragThreshold = 5f;

        private readonly ILayoutSystem m_LayoutSystem;

        protected readonly KeyboardInputManager keyboardInputManager;

        private List<UIElement> m_ElementsThisFrame;

#if UNITY_EDITOR
        public List<UIElement> DebugElementsThisFrame => m_ElementsLastFrame;
        public bool DebugMouseUpThisFrame => mouseState.isLeftMouseUpThisFrame;
#endif

        private List<UIElement> m_ElementsLastFrame;

        // temporary hack for the building system, this should be formalized and use ElementRef instead
        public IReadOnlyList<UIElement> ElementsThisFrame => m_ElementsLastFrame;

        private CursorStyle currentCursor;

        protected UIElement m_FocusedElement;
        protected DragEvent m_CurrentDragEvent;
        private KeyboardModifiers modifiersThisFrame;

        protected MouseState mouseState;
        protected KeyboardInputState m_KeyboardState;

        private readonly List<UIElement> m_ExitedElements;
        private readonly List<UIElement> m_ActiveElements;
        private readonly List<UIElement> m_EnteredElements;
        private readonly LightList<UIElement> m_MouseDownElements;
        private readonly LightList<UIElement> hoveredElements;

        private readonly Dictionary<KeyCode, KeyState> m_KeyStates;
        private readonly Dictionary<int, DragCreatorGroup> m_DragCreatorMap;

        private readonly EventPropagator m_EventPropagator;
        private readonly List<ValueTuple<object, UIElement>> m_MouseEventCaptureList;
        private static readonly Event s_Event = new Event();

        public KeyboardModifiers KeyboardModifiers => modifiersThisFrame;

        private readonly SkipTree<UIElement> m_KeyboardEventTree;

        private LightList<KeyboardEventHandlerInvocation>
            lateHandlers = new LightList<KeyboardEventHandlerInvocation>();

        private LightList<UIEvent> lateTriggers = new LightList<UIEvent>();

        private List<IFocusable> focusables;

        private int focusableIndex;

        protected InputSystem(ILayoutSystem layoutSystem, KeyboardInputManager keyboardInputManager = null) {
            this.m_LayoutSystem = layoutSystem;

            this.m_MouseDownElements = new LightList<UIElement>();
            this.m_ElementsThisFrame = new List<UIElement>();
            this.m_ElementsLastFrame = new List<UIElement>();
            this.m_EnteredElements = new List<UIElement>();
            this.m_ExitedElements = new List<UIElement>();
            this.m_ActiveElements = new List<UIElement>();

            this.m_DragCreatorMap = new Dictionary<int, DragCreatorGroup>();
            //   this.m_DragHandlerMap = new Dictionary<int, DragHandlerGroup>();

            // this.m_PressedKeys = new LightList<PressedKey>(16);
            // this.m_UpThisFrame = new LightList<KeyCode>();
            // this.m_DownThisFrame = new LightList<KeyCode>();
            this.m_KeyStates = new Dictionary<KeyCode, KeyState>();
            this.m_KeyboardEventTree = new SkipTree<UIElement>();
            this.keyboardInputManager = keyboardInputManager ?? new KeyboardInputManager();
            this.m_EventPropagator = new EventPropagator();
            this.m_MouseEventCaptureList = new List<ValueTuple<object, UIElement>>();
            // this.m_DragEventCaptureList = new List<ValueTuple<DragEventHandler, UIElement>>();
            this.m_FocusedElement = null;
            this.focusables = new List<IFocusable>();
            this.hoveredElements = new LightList<UIElement>(16);
        }

        public DragEvent CurrentDragEvent => m_CurrentDragEvent;

        public bool IsMouseLeftDown => mouseState.isLeftMouseDown;
        public bool IsMouseLeftDownThisFrame => mouseState.isLeftMouseDownThisFrame;
        public bool IsMouseLeftUpThisFrame => mouseState.isLeftMouseUpThisFrame;

        public bool IsMouseRightDown => mouseState.isRightMouseDown;
        public bool IsMouseRightDownThisFrame => mouseState.isRightMouseDownThisFrame;
        public bool IsMouseRightUpThisFrame => mouseState.isRightMouseUpThisFrame;

        public bool IsMouseMiddleDown => mouseState.isMiddleMouseDown;
        public bool IsMouseMiddleDownThisFrame => mouseState.isMiddleMouseDownThisFrame;
        public bool IsMouseMiddleUpThisFrame => mouseState.isMiddleMouseUpThisFrame;

        public Vector2 ScrollDelta => mouseState.scrollDelta;

        public Vector2 MousePosition => mouseState.mousePosition;
        public Vector2 MouseDownPosition => mouseState.leftMouseButtonState.downPosition;

        public bool IsDragging { get; protected set; }

        protected abstract MouseState GetMouseState();

        public IFocusable GetFocusedElement() {
            return (IFocusable) m_FocusedElement;
        }

        public void RegisterFocusable(IFocusable focusable) {
            focusables.Add(focusable);
        }

        public void UnRegisterFocusable(IFocusable focusable) {
            focusables.Remove(focusable);
        }

        public void FocusNext() {
            int initialIndex = focusableIndex;
            do {
                focusableIndex = focusableIndex == 0 ? focusables.Count - 1 : focusableIndex - 1;
            } while (!(RequestFocus(focusables[focusableIndex]) || focusableIndex == initialIndex));
        }

        public void FocusPrevious() {
            int initialIndex = focusableIndex;
            do {
                focusableIndex = focusableIndex + 1 == focusables.Count ? 0 : focusableIndex + 1;
            } while (!(RequestFocus(focusables[focusableIndex]) || focusableIndex == initialIndex));
        }

        public bool RequestFocus(IFocusable target) {
            if (!(target is UIElement element && !element.isDisabled)) {
                return false;
            }

            // try focus the element early to see if they accept being focused.
            if (!target.Focus()) {
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
                onFocusChanged?.Invoke(target);
            }

            m_FocusedElement = (UIElement) target;
            m_FocusedElement.style.EnterState(StyleState.Focused);
            onFocusChanged?.Invoke(target);
            focusableIndex = -1;
            for (int i = 0; i < focusables.Count; i++) {
                if (focusables[i] == target) {
                    focusableIndex = i;
                    return true;
                }
            }

            return true;
        }

        public void ReleaseFocus(IFocusable target) {
            if (m_FocusedElement == (UIElement) target) {
                IFocusable focusable = (IFocusable) m_FocusedElement;
                m_FocusedElement.style.ExitState(StyleState.Focused);
                focusable.Blur();
                // todo -- if focus handlers added via template invoke them
                m_FocusedElement = null;
                focusableIndex = -1;
                onFocusChanged?.Invoke(target);
            }
        }

        public virtual void OnUpdate() {
            mouseState = GetMouseState();
            m_KeyboardState = keyboardInputManager.UpdateKeyboardInputState();

            ProcessKeyboardEvents();
            ProcessMouseInput();

            if (!IsDragging) {
                ProcessMouseEvents();
            }
            else {
                RunMouseEvents(m_ExitedElements, InputEventType.MouseExit);
            }

            ProcessDragEvents();

            List<UIElement> temp = m_ElementsLastFrame;
            m_ElementsLastFrame = m_ElementsThisFrame;
            m_ElementsThisFrame = temp;

            for (int i = 0; i < m_ElementsLastFrame.Count; i++) {
                if (m_ElementsLastFrame[i].isDisabled || m_ElementsLastFrame[i].isDestroyed) {
                    m_ElementsLastFrame.RemoveAt(i--);
                }
            }

            m_ElementsThisFrame.Clear();
            m_EnteredElements.Clear();
            m_ExitedElements.Clear();

            if (IsMouseLeftUpThisFrame) {
                m_MouseDownElements.Clear();
            }
        }

        public virtual void DelayEvent(UIElement origin, UIEvent evt) {
            evt.origin = origin;
            lateTriggers.Add(evt);
        }

        public virtual void OnLateUpdate() {
            int lateHandlersCount = lateHandlers.Count;
            KeyboardEventHandlerInvocation[] invocations = lateHandlers.Array;
            for (int index = 0; index < lateHandlersCount; index++) {
                KeyboardEventHandlerInvocation invocation = invocations[index];
                throw new NotImplementedException();
//                invocation.handler.Invoke(invocation.target, invocation.evt);
            }

            lateHandlers.Clear();

            for (int index = 0; index < lateTriggers.Count; index++) {
                UIEvent uiEvent = lateTriggers[index];
                uiEvent.origin.TriggerEvent(uiEvent);
                if (uiEvent is TabNavigationEvent && uiEvent.IsPropagating()) {
                    // If the TabNavigationEvent isn't handled we assume it's ok to tab to the next input element.
                    if (uiEvent.keyboardInputEvent.shift) {
                        FocusPrevious();
                    }
                    else {
                        FocusNext();
                    }
                }
            }

            lateTriggers.Clear();
        }

        private void ProcessKeyboardEvents() {
            StructList<KeyCodeState> keyCodeStates = m_KeyboardState.GetKeyCodeStates();
            for (int i = 0; i < keyCodeStates.size; i++) {
                KeyCodeState keyCodeState = keyCodeStates[i];

                InputEventType inputEventType;
                if (keyCodeState.keyState == KeyState.DownThisFrame) {
                    inputEventType = InputEventType.KeyDown;
                }
                else if (keyCodeState.keyState == KeyState.Down) {
                    inputEventType = InputEventType.KeyHeldDown;
                }
                else {
                    inputEventType = InputEventType.KeyUp;
                }

                ProcessKeyboardEvent(keyCodeState.keyCode, inputEventType, keyCodeState.character, m_KeyboardState.modifiersThisFrame);
            }
        }

        private void ProcessMouseInput() {
            // if element does not have state requested -> hover flag, drag listener, pointer events = none, don't add
            // buckets feel like a lot of overhead
            // for each element, track if has overflowing children 
            // if it does not and element is culled, skip directly to children's children and repeat
            // if aabb yMin is below screen height or aabb ymax is less than 0 -> cull

            // broadphase culling and input querying are related
            // neither uses render bounds, just obb and aabb
            // if dragging only attempt intersections with elements who have drag responders
            // if not dragging only attempt intersections with elements who have hover state (if mouse is present) or drag create or mouse / touch interactions

            LightList<UIElement> queryResults = (LightList<UIElement>) m_LayoutSystem.QueryPoint(mouseState.mousePosition, LightList<UIElement>.Get());

            queryResults.Sort((a, b) => {
                int viewDepthComparison = b.View.Depth - a.View.Depth;
                if (viewDepthComparison != 0) return viewDepthComparison;
                if (b.layoutBox.zIndex != a.layoutBox.zIndex) {
                    return b.layoutBox.zIndex - a.layoutBox.zIndex;
                }

                return b.layoutBox.traversalIndex - a.layoutBox.traversalIndex;
            });

            if (!IsDragging) {
                LightList<UIElement> ancestorElements = LightList<UIElement>.Get();

                if (queryResults.Count > 0) {
                    /*
                     * Every following element must be a parent of the first.
                     * This makes no sense for drag events but a lot for every other.
                     */
                    UIElement firstElement = queryResults[0];
                    ancestorElements.Add(firstElement);

                    for (int index = 1; index < queryResults.size; index++) {
                        UIElement element = queryResults[index];
                        if (IsParentOf(element, firstElement)) {
                            ancestorElements.Add(element);
                        }
                    }

                    LightList<UIElement>.Release(ref queryResults);
                    queryResults = ancestorElements;
                }
            }

            bool didMouseMove = mouseState.DidMove;

            if (didMouseMove) {
                for (int i = 0; i < hoveredElements.size; i++) {
                    UIElement element = hoveredElements.array[i];

                    if ((element.flags & UIElementFlags.EnabledFlagSet) != UIElementFlags.EnabledFlagSet) {
                        hoveredElements.RemoveAt(i--);
                        continue;
                    }

                    if (!queryResults.Contains(element)) {
                        hoveredElements.RemoveAt(i--);
                        element.style.ExitState(StyleState.Hover);
                    }
                }

                for (int i = 0; i < queryResults.Count; i++) {
                    UIElement element = queryResults.array[i];

                    if ((element.style.currentState & StyleState.Hover) == 0) {
                        hoveredElements.Add(element);
                        element.style.EnterState(StyleState.Hover);
                    }
                }
            }

            for (int i = 0; i < queryResults.Count; i++) {
                UIElement element = queryResults[i];

                m_ElementsThisFrame.Add(element);

                if (!m_ElementsLastFrame.Contains(element)) {
                    m_EnteredElements.Add(element);
                }

                if (IsMouseLeftDownThisFrame) {
                    element.style?.EnterState(StyleState.Active);
                    m_ActiveElements.Add(element);
                }
            }

            for (int i = 0; i < m_ElementsLastFrame.Count; i++) {
                if (!m_ElementsThisFrame.Contains(m_ElementsLastFrame[i])) {
                    m_ExitedElements.Add(m_ElementsLastFrame[i]);
                }
            }

            if (IsMouseLeftUpThisFrame) {
                for (int i = 0; i < m_ActiveElements.Count; i++) {
                    m_ActiveElements[i].style?.ExitState(StyleState.Active);
                }

                m_ActiveElements.Clear();
            }

            if (!IsDragging) {
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

                if (mouseState.AnyMouseDownThisFrame) {
                    m_MouseDownElements.AddRange(m_ElementsThisFrame);
                }
            }

            LightList<UIElement>.Release(ref queryResults);
        }

        private static bool IsParentOf(UIElement element, UIElement child) {
            UIElement ptr = child.parent;
            while (ptr != null) {
                if (ptr == element) {
                    return true;
                }

                ptr = ptr.parent;
            }

            return false;
        }

        private void ProcessDragEvents() {
            if (IsDragging) {
                if (mouseState.ReleasedDrag) {
                    EndDrag(InputEventType.DragDrop);
                    m_MouseDownElements.Clear();
                }
                else {
                    UpdateDrag();
                }
            }
            else if (mouseState.AnyMouseDown) {
                if (Vector2.Distance(mouseState.MouseDownPosition, mouseState.mousePosition) >= k_DragThreshold) {
                    BeginDrag();
                }
            }
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
                RunDragEvent(m_ElementsThisFrame,
                    mouseState.DidMove ? InputEventType.DragMove : InputEventType.DragHover);
            }

            if (m_CurrentDragEvent.IsCanceled) {
                EndDrag(InputEventType.DragCancel);
            }

            if (m_CurrentDragEvent.IsDropped) {
                EndDrag(InputEventType.DragDrop);
            }
        }

        private void BeginDrag() {
            if (m_CurrentDragEvent != null) {
                return;
            }

            if (m_MouseDownElements.size == 0) {
                return;
            }

            mouseState.leftMouseButtonState.isDrag = mouseState.isLeftMouseDown;
            mouseState.rightMouseButtonState.isDrag = mouseState.isRightMouseDown;
            mouseState.middleMouseButtonState.isDrag = mouseState.isMiddleMouseDown;

            IsDragging = true;
            m_EventPropagator.Reset(mouseState);
            MouseInputEvent mouseEvent = new MouseInputEvent(m_EventPropagator, InputEventType.DragCreate, modifiersThisFrame);

            m_EventPropagator.origin = m_MouseDownElements.array[0];

            for (int i = 0; i < m_MouseDownElements.Count; i++) {
                UIElement element = m_MouseDownElements[i];

                if (element.isDestroyed || element.isDisabled || element.inputHandlers == null) {
                    continue;
                }

                if ((element.inputHandlers.handledEvents & InputEventType.DragCreate) == 0) {
                    continue;
                }

                for (int creatorIndex = 0; creatorIndex < element.inputHandlers.dragCreators.size; creatorIndex++) {
                    InputHandlerGroup.DragCreatorData data = element.inputHandlers.dragCreators.array[creatorIndex];

                    m_CurrentDragEvent = data.handler.Invoke(mouseEvent);

                    if (m_CurrentDragEvent != null) {
                        m_CurrentDragEvent.StartTime = Time.realtimeSinceStartup;
                        m_CurrentDragEvent.DragStartPosition = MousePosition;

                        UpdateDrag(true);
                        return;
                    }
                }
            }

            // todo -- capture phase
        }

        private void EndDrag(InputEventType evtType) {
            IsDragging = false;

            if (m_CurrentDragEvent == null) {
                return;
            }

            m_CurrentDragEvent.MousePosition = MousePosition;
            m_CurrentDragEvent.Modifiers = modifiersThisFrame;

            bool isOriginElementThisFrame = false;
            for (int i = 0; i < m_ElementsThisFrame.Count; i++) {
                if (m_ElementsThisFrame[i].id == m_CurrentDragEvent.origin.id) {
                    isOriginElementThisFrame = true;
                    break;
                }
            }

            if (!isOriginElementThisFrame) {
                m_ElementsThisFrame.Add(m_CurrentDragEvent.origin);
            }

            if (evtType == InputEventType.DragCancel) {
                RunDragEvent(m_ElementsThisFrame, InputEventType.DragCancel);
            }
            else if (evtType == InputEventType.DragDrop) {
                RunDragEvent(m_ElementsThisFrame, InputEventType.DragDrop);
                m_CurrentDragEvent.Drop(true);
            }

            m_CurrentDragEvent.OnComplete();
            m_CurrentDragEvent = null;
        }

        private void RunDragEvent(List<UIElement> elements, InputEventType eventType) {
            if (m_CurrentDragEvent.IsCanceled && eventType != InputEventType.DragCancel) {
                return;
            }

            m_CurrentDragEvent.CurrentEventType = eventType;
            m_CurrentDragEvent.source = m_EventPropagator;

            m_EventPropagator.Reset(mouseState);

            LightList<Action<DragEvent>> captureList = LightList<Action<DragEvent>>.Get();

            for (int i = 0; i < elements.Count; i++) {
                UIElement element = elements[i];
                if (element.isDestroyed || element.isDisabled) {
                    continue;
                }

                if (element.inputHandlers == null) {
                    continue;
                }

                if ((element.inputHandlers.handledEvents & eventType) == 0) {
                    continue;
                }

                for (int j = 0; j < element.inputHandlers.eventHandlers.size; j++) {
                    ref InputHandlerGroup.HandlerData handler = ref element.inputHandlers.eventHandlers.array[j];

                    if (handler.eventType != eventType) {
                        continue;
                    }

                    Action<DragEvent> castHandler = (Action<DragEvent>) handler.handlerFn;

                    if (handler.eventPhase != EventPhase.Bubble) {
                        captureList.Add(castHandler);
                        continue;
                    }

                    CurrentDragEvent.target = element;
                    castHandler.Invoke(m_CurrentDragEvent);

                    if (m_CurrentDragEvent.IsCanceled || m_EventPropagator.shouldStopPropagation) {
                        break;
                    }
                }

                if (m_CurrentDragEvent.IsCanceled || m_EventPropagator.shouldStopPropagation) {
                    captureList.Release();
                    return;
                }
            }

            for (int i = 0; i < captureList.size; i++) {
                if (m_CurrentDragEvent.IsCanceled || m_EventPropagator.shouldStopPropagation) {
                    break;
                }

                captureList.array[i].Invoke(m_CurrentDragEvent);
            }

            captureList.Release();
        }

        public void OnReset() {
            // don't clear key states
            m_FocusedElement = null;

            focusables.Clear();

            focusableIndex = -1;

            m_ElementsLastFrame.Clear();

            m_ElementsThisFrame.Clear();

            m_MouseDownElements.Clear();

//            m_KeyboardEventTree.Clear();
            m_DragCreatorMap.Clear();

            m_CurrentDragEvent = null;

            IsDragging = false;
        }

        public void OnDestroy() { }

        public void OnViewAdded(UIView view) { }

        public void OnViewRemoved(UIView view) { }

        public void OnElementEnabled(UIElement element) { }

        public void OnElementDisabled(UIElement element) {
            BlurOnDisableOrDestroy();
        }

        public void OnElementDestroyed(UIElement element) {
            BlurOnDisableOrDestroy();

            m_ElementsLastFrame.Remove(element);

            m_ElementsThisFrame.Remove(element);

            m_MouseDownElements.Remove(element);

//            m_KeyboardEventTree.RemoveHierarchy(element);
            // todo -- clear child handlers
            m_DragCreatorMap.Remove(element.id);
        }

        private void BlurOnDisableOrDestroy() {
            if (m_FocusedElement != null && (m_FocusedElement.isDisabled || m_FocusedElement.isDestroyed)) {
                ReleaseFocus((IFocusable) m_FocusedElement);
            }
        }

        public void OnElementCreated(UIElement element) { }

        public void OnAttributeSet(UIElement element, string attributeName, string currentValue,
            string attributeValue) { }

        public virtual bool IsKeyDown(KeyCode keyCode) {
            return (GetKeyState(keyCode) & KeyState.Down) != 0;
        }

        public virtual bool IsKeyDownThisFrame(KeyCode keyCode) {
            return (GetKeyState(keyCode) & KeyState.DownThisFrame) != 0;
        }

        public virtual bool IsKeyUp(KeyCode keyCode) {
            KeyState state = GetKeyState(keyCode);
            return (state == KeyState.Up || (state & KeyState.UpThisFrame) != 0);
        }

        public virtual bool IsKeyUpThisFrame(KeyCode keyCode) {
            return (GetKeyState(keyCode) & KeyState.UpThisFrame) != 0;
        }

        public virtual KeyState GetKeyState(KeyCode keyCode) {
            KeyState state;
            if (m_KeyStates.TryGetValue(keyCode, out state)) {
                return state;
            }

            return KeyState.Up;
        }

        public void RegisterKeyboardHandler(UIElement element) {
            const InputEventType keyEvents = InputEventType.KeyDown | InputEventType.KeyUp | InputEventType.KeyHeldDown;
            if (element.inputHandlers != null && (element.inputHandlers.handledEvents & keyEvents) != 0) {
                m_KeyboardEventTree.AddItem(element);
            }
        }

        protected void ProcessKeyboardEvent(KeyCode keyCode, InputEventType eventType, char character, KeyboardModifiers modifiers) {
            GenericInputEvent keyEvent = new GenericInputEvent(eventType, modifiers, m_EventPropagator, character, keyCode, m_FocusedElement != null);

            if (m_FocusedElement == null) {
                m_KeyboardEventTree.ConditionalTraversePreOrder(keyEvent, (item, evt) => {
                    if (evt.propagator.shouldStopPropagation) return false;

                    UIElement element = (UIElement) item.Element;
                    if (element.isDestroyed || element.isDisabled) {
                        return false;
                    }

                    InputHandlerGroup evtHandlerGroup = item.inputHandlers;

                    for (int i = 0; i < evtHandlerGroup.eventHandlers.size; i++) {
                        if (evt.propagator.shouldStopPropagation) break;
                        ref InputHandlerGroup.HandlerData handler = ref evtHandlerGroup.eventHandlers.array[i];
                        if (handler.eventType != evt.type) {
                            continue;
                        }

                        Action<GenericInputEvent> keyHandler = handler.handlerFn as Action<GenericInputEvent>;
                        Debug.Assert(keyHandler != null, nameof(keyHandler) + " != null");
                        keyHandler.Invoke(evt);
                    }

                    return !evt.propagator.shouldStopPropagation;
                });
            }

            else {
                InputHandlerGroup evtHandlerGroup = m_FocusedElement.inputHandlers;
                for (int i = 0; i < evtHandlerGroup.eventHandlers.size; i++) {
                    if (m_EventPropagator.shouldStopPropagation) break;
                    ref InputHandlerGroup.HandlerData handler = ref evtHandlerGroup.eventHandlers.array[i];
                    if (handler.eventType != keyEvent.type) {
                        continue;
                    }

                    Action<GenericInputEvent> keyHandler = evtHandlerGroup.eventHandlers[i].handlerFn as Action<GenericInputEvent>;
                    Debug.Assert(keyHandler != null, nameof(keyHandler) + " != null");
                    keyHandler.Invoke(keyEvent);
                }
            }
        }

        private void RunMouseEvents(List<UIElement> elements, InputEventType eventType) {
            if (elements.Count == 0) return;

            m_EventPropagator.Reset(mouseState);

            m_EventPropagator.origin = elements[0];
            for (int i = 0;
                i < elements.Count;
                i++) {
                UIElement element = elements[i];
                if (element.isDestroyed || element.isDisabled) {
                    continue;
                }

                if (element.inputHandlers == null || (element.inputHandlers.handledEvents & eventType) == 0) {
                    continue;
                }

                LightList<InputHandlerGroup.HandlerData> handlers = element.inputHandlers.eventHandlers;

                for (int j = 0; j < handlers.size; j++) {
                    InputHandlerGroup.HandlerData handlerData = handlers.array[j];

                    if (handlerData.eventType != eventType) {
                        continue;
                    }

                    if (handlerData.eventPhase != EventPhase.Bubble) {
                        m_MouseEventCaptureList.Add(ValueTuple.Create(handlerData.handlerFn, element));
                        continue;
                    }

                    if ((handlerData.modifiers & modifiersThisFrame) == handlerData.modifiers) {
                        Action<GenericInputEvent> handler = handlerData.handlerFn as Action<GenericInputEvent>;
                        Debug.Assert(handler != null, nameof(handler) + " != null");
                        handler.Invoke(new GenericInputEvent(eventType, modifiersThisFrame, m_EventPropagator, '\0', default, element == m_FocusedElement));
                    }

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
                Action<GenericInputEvent> handler = (Action<GenericInputEvent>) m_MouseEventCaptureList[i].Item1;
                UIElement element = m_MouseEventCaptureList[i].Item2;

                handler.Invoke(new GenericInputEvent(eventType, modifiersThisFrame, m_EventPropagator, '\0', default,
                    element == m_FocusedElement));

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
            if (mouseState.scrollDelta != Vector2.zero) {
                RunMouseEvents(m_ElementsThisFrame, InputEventType.MouseScroll);
            }

            if (mouseState.isLeftMouseDownThisFrame || mouseState.isRightMouseDownThisFrame || mouseState.isMiddleMouseDownThisFrame) {
                HandleBlur();

                if (m_ElementsThisFrame.Count > 0 && m_ElementsThisFrame[0].View.RequestFocus()) {
                    // todo let's see if we have to process the mouse event again
                }

                RunMouseEvents(m_ElementsThisFrame, InputEventType.MouseDown);
            }
            else if (mouseState.isLeftMouseUpThisFrame || mouseState.isRightMouseUpThisFrame || mouseState.isMiddleMouseUpThisFrame) {
                RunMouseEvents(m_ElementsThisFrame, InputEventType.MouseUp);
                if (mouseState.clickCount > 0) {
                    RunMouseEvents(m_ElementsThisFrame, InputEventType.MouseClick);
                }
                else if (!mouseState.isLeftMouseDown && !mouseState.isMiddleMouseDown) {
                    RunMouseEvents(m_ElementsThisFrame, InputEventType.MouseContext);
                }
            }
            else if (mouseState.isLeftMouseDown || mouseState.isRightMouseDown || mouseState.isMiddleMouseDown) {
                RunMouseEvents(m_ElementsThisFrame, InputEventType.MouseHeldDown);
            }

            RunMouseEvents(m_ElementsThisFrame,
                mouseState.DidMove ? InputEventType.MouseMove : InputEventType.MouseHover);
        }

        private void HandleBlur() {
            if (m_FocusedElement == null) {
                return;
            }

            if (m_ElementsThisFrame.Count == 0) {
                ReleaseFocus((IFocusable) m_FocusedElement);
                return;
            }

            UIElement ptr = m_ElementsThisFrame[0];
            while (ptr != null) {
                if (ptr == m_FocusedElement) {
                    return;
                }

                ptr = ptr.parent;
            }

            ReleaseFocus((IFocusable) m_FocusedElement);
        }

    }

    public struct KeyboardEventHandlerInvocation {

        public KeyboardInputEvent evt { get; set; }

    }

}