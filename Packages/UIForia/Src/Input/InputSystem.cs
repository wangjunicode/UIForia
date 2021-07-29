// using System;
// using System.Collections.Generic;
// using System.Diagnostics;
// using UIForia.Compilers;
// using UIForia.Elements;
// using UIForia.Rendering;
// using UIForia.Style;
// using UIForia.UIInput;
// using UIForia.Util;
// using UnityEngine;
// using Debug = System.Diagnostics.Debug;
//
// namespace UIForia.Systems {
//
//     public abstract class InputSystem : IInputSystem {
//
//         public event Action<IFocusable> onFocusChanged;
//
//         private const float k_DragThreshold = 5f;
//         private ElementComparer elementComp;
//         
//         protected readonly KeyboardInputManager keyboardInputManager;
//
//         private List<UIElement> elementsThisFrame;
//
// #if UNITY_EDITOR
//         public List<UIElement> DebugElementsThisFrame => elementsLastFrame;
//         public bool DebugMouseUpThisFrame => mouseState.isLeftMouseUpThisFrame;
// #endif
//
//         private List<UIElement> elementsLastFrame;
//
//         // temporary hack for the building system, this should be formalized and use ElementRef instead
//         public IReadOnlyList<UIElement> ElementsThisFrame => elementsLastFrame;
//
//         private CursorStyle currentCursor;
//
//         protected UIElement focusedElement;
//         protected DragEvent currentDragEvent;
//
//         protected MouseState mouseState;
//         protected KeyboardInputState keyboardState;
//
//         private readonly List<UIElement> exitedElements;
//         private readonly List<UIElement> activeElements;
//         private readonly List<UIElement> enteredElements;
//         private readonly LightList<UIElement> mouseDownElements;
//         private readonly LightList<UIElement> hoveredElements;
//
//         private readonly EventPropagator eventPropagator;
//         private readonly List<ValueTuple<object, UIElement>> mouseEventCaptureList;
//
//         public KeyboardModifiers KeyboardModifiers => keyboardState.modifiersThisFrame;
//
//         // private readonly SkipTree<UIElement> keyboardEventTree;
//
//         private List<IFocusable> focusables;
//
//         private int focusableIndex;
//
//         private readonly InputEventHolder eventHolder;
//
//         protected InputSystem(KeyboardInputManager keyboardInputManager = null) {
//             this.eventHolder = new InputEventHolder();
//             this.mouseDownElements = new LightList<UIElement>();
//             this.elementsThisFrame = new List<UIElement>();
//             this.elementsLastFrame = new List<UIElement>();
//             this.enteredElements = new List<UIElement>();
//             this.exitedElements = new List<UIElement>();
//             this.activeElements = new List<UIElement>();
//             this.elementComp = new ElementComparer();
//
//             // this.keyboardEventTree = new SkipTree<UIElement>();
//             this.keyboardInputManager = keyboardInputManager ?? new KeyboardInputManager();
//             this.eventPropagator = new EventPropagator();
//             this.mouseEventCaptureList = new List<ValueTuple<object, UIElement>>();
//             // this.m_DragEventCaptureList = new List<ValueTuple<DragEventHandler, UIElement>>();
//             this.focusedElement = null;
//             this.focusables = new List<IFocusable>();
//             this.hoveredElements = new LightList<UIElement>(16);
//         }
//
//         public DragEvent CurrentDragEvent => currentDragEvent;
//
//         public bool IsMouseLeftDown => mouseState.isLeftMouseDown;
//         public bool IsMouseLeftDownThisFrame => mouseState.isLeftMouseDownThisFrame;
//         public bool IsMouseLeftUpThisFrame => mouseState.isLeftMouseUpThisFrame;
//
//         public bool IsMouseRightDown => mouseState.isRightMouseDown;
//         public bool IsMouseRightDownThisFrame => mouseState.isRightMouseDownThisFrame;
//         public bool IsMouseRightUpThisFrame => mouseState.isRightMouseUpThisFrame;
//
//         public bool IsMouseMiddleDown => mouseState.isMiddleMouseDown;
//         public bool IsMouseMiddleDownThisFrame => mouseState.isMiddleMouseDownThisFrame;
//         public bool IsMouseMiddleUpThisFrame => mouseState.isMiddleMouseUpThisFrame;
//
//         public Vector2 ScrollDelta => mouseState.scrollDelta;
//
//         public Vector2 MousePosition => mouseState.mousePosition;
//         public Vector2 MouseDownPosition => mouseState.leftMouseButtonState.downPosition;
//
//         public bool IsDragging { get; protected set; }
//
//         protected abstract MouseState GetMouseState();
//
//         public IFocusable GetFocusedElement() {
//             return (IFocusable) focusedElement;
//         }
//
//         public void RegisterFocusable(IFocusable focusable) {
//             focusables.Add(focusable);
//         }
//
//         public void UnRegisterFocusable(IFocusable focusable) {
//             focusables.Remove(focusable);
//         }
//
//         public void FocusNext() {
//             int initialIndex = focusableIndex;
//             do {
//                 focusableIndex = focusableIndex == 0 ? focusables.Count - 1 : focusableIndex - 1;
//             } while (!(RequestFocus(focusables[focusableIndex]) || focusableIndex == initialIndex));
//         }
//
//         public void FocusPrevious() {
//             int initialIndex = focusableIndex;
//             do {
//                 focusableIndex = focusableIndex + 1 == focusables.Count ? 0 : focusableIndex + 1;
//             } while (!(RequestFocus(focusables[focusableIndex]) || focusableIndex == initialIndex));
//         }
//
//         public bool RequestFocus(IFocusable target) {
//             if (!(target is UIElement element && !element.isDisabled)) {
//                 return false;
//             }
//
//             // try focus the element early to see if they accept being focused.
//             if (!target.Focus()) {
//                 return false;
//             }
//
//             // todo -- if focus handlers added via template invoke them
//             if (focusedElement != null) {
//                 if (focusedElement == (UIElement) target) {
//                     return true;
//                 }
//
//                 IFocusable focusable = (IFocusable) focusedElement;
//               //  focusedElement.styleOld.ExitState(StyleState.Focus);
//                 focusable.Blur();
//                 onFocusChanged?.Invoke(target);
//             }
//
//             focusedElement = (UIElement) target;
//          //   focusedElement.styleOld.EnterStateInternal(StyleState.Focus);
//             onFocusChanged?.Invoke(target);
//             focusableIndex = -1;
//             for (int i = 0; i < focusables.Count; i++) {
//                 if (focusables[i] == target) {
//                     focusableIndex = i;
//                     return true;
//                 }
//             }
//
//             return true;
//         }
//
//         public void ReleaseFocus(IFocusable target) {
//             if (focusedElement.isDisabled || focusedElement.isDestroyed) {
//                 focusedElement = null;
//                 focusableIndex = -1;
//                 onFocusChanged?.Invoke(target);
//                 return;
//             }
//
//             if (focusedElement == (UIElement) target) {
//                 IFocusable focusable = (IFocusable) focusedElement;
//               //  focusedElement.styleOld.ExitState(StyleState.Focus);
//                 focusable.Blur();
//                 // todo -- if focus handlers added via template invoke them
//                 focusedElement = null;
//                 focusableIndex = -1;
//                 onFocusChanged?.Invoke(target);
//             }
//         }
//
//         public void Read() {
//             mouseState = GetMouseState();
//         }
//
//      
//         public virtual void OnUpdate() {
//             keyboardState = keyboardInputManager.UpdateKeyboardInputState();
//
//             ProcessKeyboardEvents();
//             ProcessMouseInput();
//             UIElement firstElement = null;
//             if (elementsThisFrame.Count != 0) {
//                 firstElement = elementsThisFrame[0];
//             }
//
//             if (!IsDragging) {
//                 ProcessMouseEvents();
//             }
//             else {
//                 RunMouseEvents(exitedElements, InputEventType.MouseExit);
//             }
//
//             ProcessDragEvents();
//
//
//             List<UIElement> temp = elementsLastFrame;
//             elementsLastFrame = elementsThisFrame;
//             elementsThisFrame = temp;
//
//             for (int i = 0; i < elementsLastFrame.Count; i++) {
//                 if (elementsLastFrame[i].isDisabled || elementsLastFrame[i].isDestroyed) {
//                     elementsLastFrame.RemoveAt(i--);
//                 }
//             }
//
//             elementsThisFrame.Clear();
//             enteredElements.Clear();
//             exitedElements.Clear();
//
//             if (IsMouseLeftUpThisFrame) {
//                 mouseDownElements.Clear();
//             }
//         }
//
//         private void ProcessKeyboardEvents() {
//             StructList<KeyCodeState> keyCodeStates = keyboardState.GetKeyCodeStates();
//             for (int i = 0; i < keyCodeStates.size; i++) {
//                 KeyCodeState keyCodeState = keyCodeStates[i];
//
//                 InputEventType inputEventType;
//                 if (keyCodeState.keyState == KeyState.DownThisFrame) {
//                     inputEventType = InputEventType.KeyDown;
//                 }
//                 else if (keyCodeState.keyState == KeyState.Down) {
//                     inputEventType = InputEventType.KeyHeldDown;
//                 }
//                 else {
//                     inputEventType = InputEventType.KeyUp;
//                 }
//
//                 ProcessKeyboardEvent(keyCodeState.keyCode, inputEventType, keyCodeState.character, keyboardState.modifiersThisFrame);
//             }
//         }
//
//         private LightList<UIElement> ancestorBuffer = new LightList<UIElement>();
//         private LightList<UIElement> queryResults = new LightList<UIElement>();
//
//         private void ProcessMouseInput() {
//             // if element does not have state requested -> hover flag, drag listener, pointer events = none, don't add
//             // buckets feel like a lot of overhead
//             // for each element, track if has overflowing children 
//             // if it does not and element is culled, skip directly to children's children and repeat
//             // if aabb yMin is below screen height or aabb ymax is less than 0 -> cull
//
//             // broadphase culling and input querying are related
//             // neither uses render bounds, just obb and aabb
//             // if dragging only attempt intersections with elements who have drag responders
//             // if not dragging only attempt intersections with elements who have hover state (if mouse is present) or drag create or mouse / touch interactions
//
//             queryResults.QuickClear();
//
//             // .QueryPoint(mouseState.mousePosition, queryResults);
//
//             if (UnityEngine.Input.GetKeyDown(KeyCode.A)) {
//                 Debugger.Break();
//             }
//
//             ElementTable<TraversalInfo> traversalTable = default; // layoutSystem.elementSystem.traversalTable;
//
//             // queryResults.Sort((a, b) => traversalTable[b.id].ftbIndex - traversalTable[a.id].ftbIndex);
//             elementComp.traversalTable = traversalTable;
//             queryResults.Sort(elementComp);
//
//             if (!IsDragging) {
//                 ancestorBuffer.QuickClear();
//
//                 if (queryResults.size > 0) {
//                     /*
//                      * Every following element must be a parent of the first.
//                      * This makes no sense for drag events but a lot for every other.
//                      */
//                     UIElement firstElement = queryResults[0];
//                     ancestorBuffer.Add(firstElement);
//
//                     for (int index = 1; index < queryResults.size; index++) {
//                         UIElement element = queryResults[index];
//                         if (traversalTable[element.elementId].IsAncestorOf(traversalTable[firstElement.elementId])) {
//                             ancestorBuffer.Add(element);
//                         }
//                     }
//
//                     queryResults.size = 0;
//                     queryResults.AddRange(ancestorBuffer);
//                 }
//             }
//
//             bool didMouseMove = mouseState.DidMove;
//
//             if (didMouseMove) {
//                 for (int i = 0; i < hoveredElements.size; i++) {
//                     UIElement element = hoveredElements.array[i];
//
//                     if (!element.isEnabled) {
//                         hoveredElements.RemoveAt(i--);
//                         continue;
//                     }
//
//                     // if (!queryResults.Contains(element)) {
//                     //     hoveredElements.RemoveAt(i--);
//                     //     element.styleOld.styleSystem.ExitState(element.elementId, StyleState.Hover);
//                     // }
//                 }
//
//                 for (int i = 0; i < queryResults.Count; i++) {
//                     UIElement element = queryResults.array[i];
//
//                     // if ((element.styleOld.currentState & StyleState.Hover) == 0) {
//                     //     hoveredElements.Add(element);
//                     //     element.styleOld.styleSystem.EnterState(element.elementId, StyleState.Hover);
//                     // }
//                 }
//             }
//
//             for (int i = 0; i < queryResults.Count; i++) {
//                 UIElement element = queryResults[i];
//
//                 elementsThisFrame.Add(element);
//
//                 if (!elementsLastFrame.Contains(element)) {
//                     enteredElements.Add(element);
//                 }
//
//                 // if (IsMouseLeftDownThisFrame) {
//                 //     element.styleOld.EnterStateInternal(StyleState.Active);
//                 //     activeElements.Add(element);
//                 // }
//             }
//
//             for (int i = 0; i < elementsLastFrame.Count; i++) {
//                 if (!elementsThisFrame.Contains(elementsLastFrame[i])) {
//                     exitedElements.Add(elementsLastFrame[i]);
//                 }
//             }
//
//             if (IsMouseLeftUpThisFrame) {
//                 // for (int i = 0; i < activeElements.Count; i++) {
//                 //     activeElements[i].styleOld.ExitState(StyleState.Active);
//                 // }
//
//                 activeElements.Clear();
//             }
//
//             if (!IsDragging) {
//                 CursorStyle newCursor = null;
//                 if (elementsThisFrame.Count > 0) {
//                     for (int i = 0; i < elementsThisFrame.Count; i++) {
//                         UIElement element = elementsThisFrame[i];
//
//                         // todo -- bring back cursor!!!
//                         // if (element.style.IsDefined(StylePropertyId.Cursor)) {
//                         //     newCursor = element.style.Cursor;
//                         //     if (!newCursor.Equals(currentCursor)) {
//                         //         Cursor.SetCursor(newCursor.texture, newCursor.hotSpot, CursorMode.Auto);
//                         //     }
//                         //
//                         //     break;
//                         // }
//                     }
//                 }
//
//                 if (currentCursor != null && newCursor == null) {
//                     Cursor.SetCursor(null, new Vector2(0, 0), CursorMode.Auto);
//                 }
//
//                 currentCursor = newCursor;
//
//                 if (mouseState.AnyMouseDownThisFrame) {
//                     mouseDownElements.AddRange(elementsThisFrame);
//                 }
//             }
//         }
//
//         private static bool IsParentOf(UIElement element, UIElement child) {
//             // UIElement ptr = child.parent;
//             // while (ptr != null) {
//             //     if (ptr == element) {
//             //         return true;
//             //     }
//             //
//             //     ptr = ptr.parent;
//             // }
//
//             return false;
//         }
//
//         private void ProcessDragEvents() {
//             if (IsDragging) {
//                 if (mouseState.ReleasedDrag) {
//                     EndDrag(InputEventType.DragDrop);
//                     mouseDownElements.Clear();
//                 }
//                 else {
//                     UpdateDrag();
//                 }
//             }
//             else if (mouseState.AnyMouseDown) {
//                 // if (Vector2.Distance(mouseState.MouseDownPosition, mouseState.mousePosition) >= k_DragThreshold / Application.dpiScaleFactor) {
//                 //     BeginDrag();
//                 // }
//             }
//         }
//
//         private void UpdateDrag(bool firstFrame = false) {
//             if (currentDragEvent == null) {
//                 return;
//             }
//
//             if (currentDragEvent.lockCursor && currentDragEvent.cursor != null) {
//                 Cursor.SetCursor(currentDragEvent.cursor.texture, currentDragEvent.cursor.hotSpot, CursorMode.Auto);
//                 currentCursor = currentDragEvent.cursor;
//             }
//
//             currentDragEvent.MousePosition = MousePosition;
//             currentDragEvent.Modifiers = keyboardState.modifiersThisFrame;
//
//             if (firstFrame) {
//                 RunDragEvent(elementsThisFrame, InputEventType.DragEnter);
//                 currentDragEvent.Update();
//             }
//             else {
//                 RunDragEvent(exitedElements, InputEventType.DragExit);
//                 RunDragEvent(enteredElements, InputEventType.DragEnter);
//                 currentDragEvent.Update();
//                 RunDragEvent(elementsThisFrame,
//                     mouseState.DidMove ? InputEventType.DragMove : InputEventType.DragHover);
//             }
//
//             if (currentDragEvent.IsCanceled) {
//                 EndDrag(InputEventType.DragCancel);
//             }
//
//             if (currentDragEvent.IsDropped) {
//                 EndDrag(InputEventType.DragDrop);
//             }
//         }
//
//         private void BeginDrag() {
//             if (currentDragEvent != null) {
//                 return;
//             }
//
//             if (mouseDownElements.size == 0) {
//                 return;
//             }
//
//             mouseState.leftMouseButtonState.isDrag = mouseState.isLeftMouseDown;
//             mouseState.rightMouseButtonState.isDrag = mouseState.isRightMouseDown;
//             mouseState.middleMouseButtonState.isDrag = mouseState.isMiddleMouseDown;
//
//             IsDragging = true;
//             eventPropagator.Reset(mouseState);
//
//             eventPropagator.origin = mouseDownElements.array[0];
//
//             for (int i = 0; i < mouseDownElements.Count; i++) {
//                 UIElement element = mouseDownElements[i];
//
//                 // if (element.isDestroyed || element.isDisabled || element.inputHandlers == null) {
//                 //     continue;
//                 // }
//                 //
//                 // if ((element.inputHandlers.handledEvents & InputEventType.DragCreate) == 0) {
//                 //     continue;
//                 // }
//                 //
//                 // for (int creatorIndex = 0; creatorIndex < element.inputHandlers.dragCreators.size; creatorIndex++) {
//                 //     InputHandlerGroup.DragCreatorData data = element.inputHandlers.dragCreators.array[creatorIndex];
//                 //
//                 //     throw new NotImplementedException("Todo -- fix up drag events with new compiler");
//                 //     currentDragEvent = null; //data.handler.Invoke(new MouseInputEvent(m_EventPropagator, InputEventType.DragCreate, m_KeyboardState.modifiersThisFrame, false, element));
//                 //
//                 //     if (currentDragEvent != null) {
//                 //         currentDragEvent.StartTime = Time.realtimeSinceStartup;
//                 //         currentDragEvent.DragStartPosition = MousePosition;
//                 //         currentDragEvent.origin = element;
//                 //         currentDragEvent.Begin();
//                 //         UpdateDrag(true);
//                 //         return;
//                 //     }
//                 // }
//             }
//
//             if (currentDragEvent == null) {
//                 IsDragging = false;
//             }
//
//             // todo -- capture phase
//         }
//
//         private void EndDrag(InputEventType evtType) {
//             IsDragging = false;
//
//             if (currentDragEvent == null) {
//                 return;
//             }
//
//             currentDragEvent.MousePosition = MousePosition;
//             currentDragEvent.Modifiers = keyboardState.modifiersThisFrame;
//
//             bool isOriginElementThisFrame = false;
//             for (int i = 0; i < elementsThisFrame.Count; i++) {
//                 if (elementsThisFrame[i].elementId == currentDragEvent.origin.elementId) {
//                     isOriginElementThisFrame = true;
//                     break;
//                 }
//             }
//
//             if (!isOriginElementThisFrame) {
//                 elementsThisFrame.Add(currentDragEvent.origin);
//             }
//
//             if (evtType == InputEventType.DragCancel) {
//                 RunDragEvent(elementsThisFrame, InputEventType.DragCancel);
//                 currentDragEvent.Cancel();
//             }
//             else if (evtType == InputEventType.DragDrop) {
//                 RunDragEvent(elementsThisFrame, InputEventType.DragDrop);
//                 currentDragEvent.Drop(true);
//             }
//
//             currentDragEvent.OnComplete();
//             currentDragEvent = null;
//         }
//
//         private void RunDragEvent(List<UIElement> elements, InputEventType eventType) {
//             if (currentDragEvent.IsCanceled && eventType != InputEventType.DragCancel) {
//                 return;
//             }
//
//             currentDragEvent.CurrentEventType = eventType;
//             currentDragEvent.source = eventPropagator;
//
//             eventPropagator.Reset(mouseState);
//
//             LightList<Action<DragEvent>> captureList = LightList<Action<DragEvent>>.Get();
//
//             for (int i = 0; i < elements.Count; i++) {
//                 UIElement element = elements[i];
//
//                 if (element.isDestroyed || element.isDisabled) {
//                     continue;
//                 }
//
//                 // if (element.inputHandlers == null) {
//                 //     continue;
//                 // }
//                 //
//                 // if ((element.inputHandlers.handledEvents & eventType) == 0) {
//                 //     continue;
//                 // }
//                 //
//                 // for (int j = 0; j < element.inputHandlers.eventHandlers.size; j++) {
//                 //     ref InputHandlerGroup.HandlerData handler = ref element.inputHandlers.eventHandlers.array[j];
//                 //
//                 //     if ((handler.eventType & eventType) == 0) {
//                 //         continue;
//                 //     }
//                 //
//                 //     throw new NotImplementedException("Todo -- fixup input system");
//                 //     Action<DragEvent> castHandler = null; //(Action<DragEvent>) handler.handlerFn;
//                 //
//                 //     if (handler.eventPhase != EventPhase.Bubble) {
//                 //         captureList.Add(castHandler);
//                 //         continue;
//                 //     }
//                 //
//                 //     CurrentDragEvent.element = element;
//                 //     castHandler.Invoke(currentDragEvent);
//                 //
//                 //     if (currentDragEvent.IsCanceled || eventPropagator.shouldStopPropagation) {
//                 //         break;
//                 //     }
//                 // }
//                 //
//                 // if (currentDragEvent.IsCanceled || eventPropagator.shouldStopPropagation) {
//                 //     captureList.Release();
//                 //     return;
//                 // }
//             }
//
//             for (int i = 0; i < captureList.size; i++) {
//                 if (currentDragEvent.IsCanceled || eventPropagator.shouldStopPropagation) {
//                     break;
//                 }
//
//                 captureList.array[i].Invoke(currentDragEvent);
//             }
//
//             captureList.Release();
//         }
//
//         public void OnReset() {
//             // don't clear key states
//             focusedElement = null;
//
//             focusables.Clear();
//
//             focusableIndex = -1;
//
//             elementsLastFrame.Clear();
//
//             elementsThisFrame.Clear();
//
//             mouseDownElements.Clear();
//
//             // keyboardEventTree.Clear();
//
//             currentDragEvent = null;
//
//             IsDragging = false;
//         }
//
//         public void OnDestroy() { }
//
//         public void OnViewAdded(UIView view) { }
//
//         public void OnViewRemoved(UIView view) { }
//
//         public void OnElementEnabled(UIElement element) { }
//
//         public void OnElementDisabled(UIElement element) {
//             BlurOnDisableOrDestroy();
//         }
//
//         public void OnElementDestroyed(UIElement element) {
//             BlurOnDisableOrDestroy();
//
//             elementsLastFrame.Remove(element);
//
//             elementsThisFrame.Remove(element);
//
//             mouseDownElements.Remove(element);
//
//             // keyboardEventTree.RemoveHierarchy(element);
//         }
//
//         internal void BlurOnDisableOrDestroy() {
//             if (focusedElement != null && (focusedElement.isDisabled || focusedElement.isDestroyed)) {
//                 try {
//                     ReleaseFocus((IFocusable) focusedElement);
//                 }
//                 catch (Exception e) {
//                     UnityEngine.Debug.LogException(e);
//                 }
//             }
//         }
//
//         public void OnElementCreated(UIElement element) { }
//
//         public void OnAttributeSet(UIElement element, string attributeName, string currentValue,
//             string attributeValue) { }
//
//         public bool IsKeyDown(KeyCode keyCode) {
//             return keyboardState.IsKeyDown(keyCode);
//         }
//
//         public bool IsKeyDownThisFrame(KeyCode keyCode) {
//             return keyboardState.IsKeyDownThisFrame(keyCode);
//         }
//
//         public bool IsKeyUp(KeyCode keyCode) {
//             return keyboardState.IsKeyUp(keyCode);
//         }
//
//         public bool IsKeyUpThisFrame(KeyCode keyCode) {
//             return keyboardState.IsKeyUpThisFrame(keyCode);
//         }
//
//         public KeyState GetKeyState(KeyCode keyCode) {
//             return keyboardState.GetKeyState(keyCode);
//         }
//
//         public void RegisterKeyboardHandler(UIElement element) {
//             const InputEventType keyEvents = InputEventType.KeyDown | InputEventType.KeyUp | InputEventType.KeyHeldDown;
//             // if (element.inputHandlers != null && (element.inputHandlers.handledEvents & keyEvents) != 0) {
//             //     // keyboardEventTree.AddItem(element);
//             // }
//         }
//
//         protected void ProcessKeyboardEvent(KeyCode keyCode, InputEventType eventType, char character, KeyboardModifiers modifiers) {
//             return;
//             // GenericInputEvent keyEvent = new GenericInputEvent(eventType, modifiers, m_EventPropagator, character, keyCode, m_FocusedElement != null);
//             KeyboardInputEvent keyInputEvent = new KeyboardInputEvent(eventType, keyCode, character, modifiers, focusedElement != null);
//             if (focusedElement == null) {
//                 // keyboardEventTree.ConditionalTraversePreOrder(keyInputEvent, (item, evt) => {
//                 //     if (evt.stopPropagation) return false;
//                 //
//                 //     UIElement element = default; // (UIElement) item.Element;
//                 //     if (element.isDestroyed || element.isDisabled) {
//                 //         return false;
//                 //     }
//                 //
//                 //     InputHandlerGroup evtHandlerGroup = item.inputHandlers;
//                 //
//                 //     bool ran = false;
//                 //     for (int i = 0; i < evtHandlerGroup.eventHandlers.size; i++) {
//                 //         if (evt.stopPropagation) break;
//                 //         ref InputHandlerGroup.HandlerData handler = ref evtHandlerGroup.eventHandlers.array[i];
//                 //         if (!ShouldRun(handler, evt)) {
//                 //             continue;
//                 //         }
//                 //
//                 //         if (!ran) {
//                 //             ran = true;
//                 //             RunBindings(element);
//                 //         }
//                 //
//                 //         throw new NotImplementedException("Fix input system");
//                 //         Action<KeyboardInputEvent> keyHandler = null; //handler.handlerFn as Action<KeyboardInputEvent>;
//                 //         Debug.Assert(keyHandler != null, nameof(keyHandler) + " != null");
//                 //         keyHandler.Invoke(evt);
//                 //     }
//                 //
//                 //     if (ran) {
//                 //         RunWriteBindings(element);
//                 //     }
//                 //
//                 //     return !evt.stopPropagation;
//                 // });
//             }
//
//             else {
//                 // UIElement element = focusedElement;
//                 // InputHandlerGroup evtHandlerGroup = focusedElement.inputHandlers;
//                 // if (evtHandlerGroup == null) {
//                 //     return;
//                 // }
//                 //
//                 // RunBindings(element);
//                 //
//                 // for (int i = 0; i < evtHandlerGroup.eventHandlers.size; i++) {
//                 //     if (eventPropagator.shouldStopPropagation) break;
//                 //     ref InputHandlerGroup.HandlerData handler = ref evtHandlerGroup.eventHandlers.array[i];
//                 //     if (!ShouldRun(handler, keyInputEvent)) {
//                 //         continue;
//                 //     }
//                 //
//                 //     throw new NotImplementedException("todo -- fix input system");
//                 //     Action<KeyboardInputEvent> keyHandler = null; //evtHandlerGroup.eventHandlers[i].handlerFn as Action<KeyboardInputEvent>;
//                 //     Debug.Assert(keyHandler != null, nameof(keyHandler) + " != null");
//                 //     keyHandler.Invoke(keyInputEvent);
//                 // }
//                 //
//                 // RunWriteBindings(element);
//             }
//         }
//
//         // private static bool ShouldRun(in InputHandlerGroup.HandlerData handlerData, in KeyboardInputEvent evt) {
//         //     if (evt.eventType != handlerData.eventType) return false;
//         //
//         //     if (handlerData.requireFocus && !evt.isFocused) return false;
//         //
//         //     if (handlerData.character != '\0' && (handlerData.character != evt.character)) return false;
//         //
//         //     if (evt.keyCode != handlerData.keyCode && handlerData.keyCode != KeyCodeUtil.AnyKey) {
//         //         return false;
//         //     }
//         //
//         //     // if all required modifiers are present these should be equal
//         //     return (handlerData.modifiers & evt.modifiers) == handlerData.modifiers;
//         // }
//
//         private void RunMouseEvents(List<UIElement> elements, InputEventType eventType) {
//             if (elements.Count == 0) return;
//
//             eventPropagator.Reset(mouseState);
//
//             eventPropagator.origin = elements[0];
//             for (int i = 0; i < elements.Count; i++) {
//                 UIElement element = elements[i];
//                 if (element.isDestroyed || element.isDisabled) {
//                     continue;
//                 }
//
//                 // if (element.inputHandlers == null || (element.inputHandlers.handledEvents & eventType) == 0) {
//                 //     continue;
//                 // }
//                 //
//                 // LightList<InputHandlerGroup.HandlerData> handlers = element.inputHandlers.eventHandlers;
//                 //
//                 // for (int j = 0; j < handlers.size; j++) {
//                 //     InputHandlerGroup.HandlerData handlerData = handlers.array[j];
//                 //
//                 //     if (handlerData.eventType != eventType) {
//                 //         continue;
//                 //     }
//                 //
//                 //     if (handlerData.eventPhase != EventPhase.Bubble) {
//                 //         mouseEventCaptureList.Add(ValueTuple.Create(handlerData.handlerFn, element));
//                 //         continue;
//                 //     }
//                 //
//                 //     if ((handlerData.modifiers & keyboardState.modifiersThisFrame) == handlerData.modifiers) {
//                 //         eventHolder.mouseEvent = new MouseInputEvent(eventPropagator, eventType, keyboardState.modifiersThisFrame, element == focusedElement, element);
//                 //         handlerData.handlerFn.Invoke(element.bindingNode, eventHolder);
//                 //     }
//                 //
//                 //     if (eventPropagator.shouldStopPropagation) {
//                 //         break;
//                 //     }
//                 // }
//
//                 if (eventPropagator.shouldStopPropagation) {
//                     mouseEventCaptureList.Clear();
//                     return;
//                 }
//             }
//
//             for (int i = 0; i < mouseEventCaptureList.Count; i++) {
//                 Action<MouseInputEvent> handler = (Action<MouseInputEvent>) mouseEventCaptureList[i].Item1;
//                 UIElement element = mouseEventCaptureList[i].Item2;
//
//                // handler.Invoke(new MouseInputEvent(eventPropagator, eventType, keyboardState.modifiersThisFrame, element == focusedElement, element));
//
//                 if (eventPropagator.shouldStopPropagation) {
//                     mouseEventCaptureList.Clear();
//                     return;
//                 }
//             }
//
//             mouseEventCaptureList.Clear();
//         }
//
//         private void ProcessMouseEvents() {
//             RunMouseEvents(exitedElements, InputEventType.MouseExit);
//
//             RunMouseEvents(enteredElements, InputEventType.MouseEnter);
//             // if (mouseState.scrollDelta != Vector2.zero) {
//             //     RunMouseEvents(elementsThisFrame, InputEventType.MouseScroll);
//             // }
//
//             if (mouseState.isLeftMouseDownThisFrame || mouseState.isRightMouseDownThisFrame || mouseState.isMiddleMouseDownThisFrame) {
//                 HandleBlur();
//
//                 if (elementsThisFrame.Count > 0 && elementsThisFrame[0].View.RequestFocus()) {
//                     // todo let's see if we have to process the mouse event again
//                 }
//
//                 RunMouseEvents(elementsThisFrame, InputEventType.MouseDown);
//             }
//             else if (mouseState.isLeftMouseUpThisFrame || mouseState.isMiddleMouseUpThisFrame) {
//                 RunMouseEvents(elementsThisFrame, InputEventType.MouseUp);
//                 if (mouseState.clickCount > 0) {
//                     RunMouseEvents(elementsThisFrame, InputEventType.MouseClick);
//                 }
//             }
//             else if (mouseState.isRightMouseUpThisFrame) {
//                 RunMouseEvents(elementsThisFrame, InputEventType.MouseUp);
//                 if (!mouseState.isLeftMouseDown && !mouseState.isMiddleMouseDown) {
//                     RunMouseEvents(elementsThisFrame, InputEventType.MouseContext);
//                 }
//             }
//             else if (mouseState.isLeftMouseDown || mouseState.isRightMouseDown || mouseState.isMiddleMouseDown) {
//                 RunMouseEvents(elementsThisFrame, InputEventType.MouseHeldDown);
//             }
//
//             RunMouseEvents(elementsThisFrame,
//                 mouseState.DidMove ? InputEventType.MouseMove : InputEventType.MouseHover);
//         }
//
//         private void HandleBlur() {
//             if (focusedElement == null) {
//                 return;
//             }
//
//             if (elementsThisFrame.Count == 0) {
//                 ReleaseFocus((IFocusable) focusedElement);
//                 return;
//             }
//
//             UIElement ptr = elementsThisFrame[0];
//             // while (ptr != null) {
//             //     if (ptr == focusedElement) {
//             //         return;
//             //     }
//             //
//             //     ptr = ptr.parent;
//             // }
//
//             ReleaseFocus((IFocusable) focusedElement);
//         }
//
//     }
//
//     public struct KeyboardEventHandlerInvocation {
//
//         public KeyboardInputEvent evt { get; set; }
//
//     }
//
//   
//
// }