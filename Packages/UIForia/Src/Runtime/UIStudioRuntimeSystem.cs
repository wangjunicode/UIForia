using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using UIForia.Elements;
using UIForia.Layout;
using UIForia.Rendering;
using UIForia.Style;
using UIForia.UIInput;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace UIForia {

    public static class ContextPool<T> where T : class {

        private static SizedArray<T> pool;

        public static void Release(T val) {
            if (val == null) return;
            pool.Add(val);
        }

        public static T Get() {
            if (pool.size > 0) {
                return pool.array[--pool.size];
            }

            return (T) FormatterServices.GetUninitializedObject(typeof(T));
        }

    }

    public delegate object TeleportFn(UIForiaRuntimeSystem runtime, UIElement parent, PortalInfo portalInfo, RangeInt closureRange, object context);

    public delegate object SlotDefineFn<in T>(UIForiaRuntimeSystem runtime, object untypedContext, UIElement parent, T args, int closureOffset, object context);

    public delegate object SlotOverrideFn<in T, in U>(UIForiaRuntimeSystem runtime, UIElement parent, T args, RangeInt closureScopeRange, U slotHostElement, object context);

    public delegate object SlotRenderFn<in T>(UIForiaRuntimeSystem runtime, UIElement parent, T args, int closureOffset, object context);

    public delegate object BodyFn<in TRootContext>(UIForiaRuntimeSystem runtime, UIElement parent, TRootContext rootContext, object untypedContext);

    public delegate TFnContext LocalSharedFn<in TArgs, in TRootContext, TFnContext>(UIForiaRuntimeSystem runtime, UIElement parent, TArgs args, TRootContext rootContext, TFnContext fnContext);

    public delegate TFnContext LocalFnWithBody<in TArgs, TRootContext, TFnContext>(UIForiaRuntimeSystem runtime, UIElement parent, TArgs args, TRootContext rootContext, TFnContext fnContext, BodyFn<TRootContext> fn);

    public struct UIRuntime {

        public int frameId;
        public ElementId focusId;
        
        internal UIApplication application;

        public void RequestFocus(ElementId elementId) {
            application.RequestTextInputFocus(elementId);
            focusId = elementId;
        }

        public UIScript LoadScript(string scriptName) {
            application.TryFindScript(scriptName, out UIScript retn);
            return retn;
        }

        public UIScriptInstance RunScript(string scriptName, UIElement element) {
            UIScript script = LoadScript(scriptName);

            UIScriptInstance instance = script.Instantiate(element);

            if (instance != null) {
                instance.Run();
                return instance;
            }

            return default;
        }

    }

    public unsafe class UIForiaRuntimeSystem : IDisposable {

        public int currentFrameId;
        public int prevFrameId;

        internal MouseState mouseState;

        internal InputEventType consumedEvents;
        internal InputEventType stoppedEvents;
        internal InputEventType eventsThisFrame;

        internal KeyboardModifiers modifiersThisFrame;

        private LightStack<object> fnContexts;
        private LightList<TeleportInfo> teleportInfos;
        private LightStack<SlotOverrideInfo> overrideStack;

        private object dummySlotOverride = new object();
        private int portalInstanceGenerator;
        internal UIApplication application;

        internal UIView currentView;

        // internal ElementMetaInfo[] metaTable;
        internal DataList<ElementId> activeElementList;
        internal RuntimeTraversalInfo[] runtimeInfoTable;
        internal StyleInfo[] styleInfoTable;

        private StructList<StyleId> styleBuffer;
        private StructList<DestructionScope> toDestroy;
        private LightList<object> storedClosureScopes;
        private HashSet<DestructionScope> destructionScopeSet;
        private ManagedListAllocator<StyleId> styleIdAllocator;
        private StructList<DestructionScope> destructionScopes;
        private StructList<DestructionScope> prevDestructionScopes;

        public StringMemoizer stringMemoizer = new StringMemoizer();
        
        internal DataList<ElementId> inputResultBuffer;

        internal ElementMap hoveredElements;
        internal ElementMap mouseEnterElements;
        internal ElementMap mouseExitElements;
        internal ElementMap scratch;
        internal int elementMapSize;
        internal int maxElementId;

        public UIRuntime uiRuntime;

        internal UIForiaRuntimeSystem(UIApplication application) {
            this.application = application;
            this.fnContexts = new LightStack<object>();
            this.overrideStack = new LightStack<SlotOverrideInfo>();
            this.teleportInfos = new LightList<TeleportInfo>();
            this.activeElementList = new DataList<ElementId>(64, Allocator.Persistent);
            this.destructionScopes = new StructList<DestructionScope>();
            this.prevDestructionScopes = new StructList<DestructionScope>();
            this.toDestroy = new StructList<DestructionScope>();
            this.destructionScopeSet = new HashSet<DestructionScope>();
            this.styleBuffer = new StructList<StyleId>();
            this.styleIdAllocator = application.styleIdAllocator;
            this.storedClosureScopes = new LightList<object>();
            this.inputResultBuffer = new DataList<ElementId>(32, Allocator.Persistent);

        }

        public void Dispose() {
            activeElementList.Dispose();
            inputResultBuffer.Dispose();
            TypedUnsafe.Dispose(hoveredElements.map, Allocator.Persistent);
        }

        internal void ProcessKeyboardInput() { }

        internal void ProcessMouseInput(in MouseState mouseState, CheckedArray<InputQueryResult> mouseQueryResults, ElementMap enabledElementMap) {
            // note: mouseQueryResults is bump allocated, be sure it doesn't get nuked from one frame to the next

            this.mouseState = mouseState;
            EnsureInputMapSize(enabledElementMap.longCount);

            TypedUnsafe.MemClear(scratch.map, elementMapSize);

            eventsThisFrame = default;
            inputResultBuffer.size = 0;
            inputResultBuffer.SetSize(mouseQueryResults.size);
            maxElementId = application.MaxElementId;

            fixed (StyleState* stateTablePtr = application.styleStateTable) {
                TypedUnsafe.MemClear(stateTablePtr, application.MaxElementId);
            }

            if (uiRuntime.focusId != default && !ElementSystem.IsDeadOrDisabled(uiRuntime.focusId, application.runtimeInfoTable)) {
                application.styleStateTable[uiRuntime.focusId.index] |= StyleState.Focus;
            }

            for (int i = 0; i < mouseQueryResults.size; i++) {
                int elementIdIndex = mouseQueryResults.array[i].elementId.index;
                scratch.Set(elementIdIndex);
                application.styleStateTable[elementIdIndex] |= StyleState.Hover;
                inputResultBuffer.array[i] = mouseQueryResults.array[i].elementId;
            }

            for (int i = 0; i < elementMapSize; i++) {
                mouseEnterElements.map[i] = enabledElementMap.map[i] & ~hoveredElements.map[i] & scratch.map[i]; // enter = enabled & was not hovered & hovered
                mouseExitElements.map[i] = enabledElementMap.map[i] & hoveredElements.map[i] & ~scratch.map[i]; // exit = enabled & was hovered & not hovered
                hoveredElements.map[i] = scratch.map[i];
            }

            if (mouseState.scrollDelta.x != 0 || mouseState.scrollDelta.y != 0) {
                // pump current elements into input buffer as scroll event range
                eventsThisFrame |= InputEventType.MouseScroll;
            }

            if (mouseState.isLeftMouseDownThisFrame || mouseState.isRightMouseDownThisFrame || mouseState.isMiddleMouseDownThisFrame) {
                eventsThisFrame |= InputEventType.MouseDown;
            }
            else if (mouseState.isLeftMouseDown || mouseState.isRightMouseDown || mouseState.isMiddleMouseDown) {
                eventsThisFrame |= InputEventType.MouseHeldDown;
            }

            if (mouseState.isLeftMouseUpThisFrame || mouseState.isMiddleMouseUpThisFrame) {
                eventsThisFrame |= InputEventType.MouseUp;

                if (mouseState.clickCount > 0) {
                    eventsThisFrame |= InputEventType.MouseClick;
                }
            }
            else if (mouseState.isRightMouseUpThisFrame) {
                eventsThisFrame |= InputEventType.MouseUp;
                if (!mouseState.isLeftMouseDown && !mouseState.isMiddleMouseDown) {
                    eventsThisFrame |= InputEventType.MouseUp;
                }
            }

            eventsThisFrame |= (mouseState.DidMove ? InputEventType.MouseMove : InputEventType.MouseHover);
        }

        private void EnsureInputMapSize(int newMapSize) {
            if (elementMapSize >= newMapSize) {
                return;
            }

            ulong* maps = TypedUnsafe.Malloc<ulong>(4 * newMapSize, Allocator.Persistent);
            ElementMap newHoverMap = new ElementMap(maps + newMapSize * 0, newMapSize);
            ElementMap newMouseEnterElements = new ElementMap(maps + newMapSize * 1, newMapSize);
            ElementMap newMouseExitElements = new ElementMap(maps + newMapSize * 2, newMapSize);
            scratch = new ElementMap(maps + newMapSize * 3, newMapSize);

            TypedUnsafe.MemCpy(newHoverMap.map, hoveredElements.map, elementMapSize);
            TypedUnsafe.MemCpy(newMouseEnterElements.map, mouseEnterElements.map, elementMapSize);
            TypedUnsafe.MemCpy(newMouseExitElements.map, mouseExitElements.map, elementMapSize);

            TypedUnsafe.Dispose(hoveredElements.map, Allocator.Persistent);

            hoveredElements = newHoverMap;
            mouseEnterElements = newMouseEnterElements;
            mouseExitElements = newMouseExitElements;
            elementMapSize = newMapSize;

        }

        public object PeekFunctionStack(int diff) {
            return fnContexts.array[fnContexts.size - diff];
        }

        public void PushFunctionStack(object context) {
            fnContexts.Push(context);
        }

        public void PopFunctionStack() {
            fnContexts.size--;
        }

        private bool TryFindSlotOverride(int slotId, ushort depth, out SlotOverrideInfo overrideInfo) {

            for (int i = overrideStack.size - 1; i >= 0; i--) {

                if (overrideStack.array[i].depth < depth) break;

                if (overrideStack.array[i].slotId == slotId) {
                    overrideInfo = overrideStack.array[i];
                    return true;
                }

            }

            overrideInfo = default;
            return false;
        }

        public object RenderSlotDefault<T>(object untypedRootContext, UIElement parent, T arguments, object context) {
            // return defaultFn.Invoke(this, untypedRootContext, parent, arguments, 0, context);
            return null;
        }

        public object RenderSlot<T, U>(int slotId, object hostContext, U hostElement, UIElement parent, T arguments, object context, SlotDefineFn<T> defaultFn) where U : UIElement {
            ushort depthLimit = (ushort) (application.depthTable[hostElement.elementId.index]);
            if (TryFindSlotOverride(slotId, depthLimit, out SlotOverrideInfo overrider)) {
                SlotOverrideFn<T, U> fn = (SlotOverrideFn<T, U>) overrider.overrideFn;
                return fn.Invoke(this, parent, arguments, overrider.closureRange, hostElement, context);
            }

            return defaultFn.Invoke(this, hostContext, parent, arguments, 0, context);
        }

        public void PushSlotOverride(int slotId, UIElement hostElement, int captureCount, object factoryFunction) {
            overrideStack.Push(new SlotOverrideInfo() {
                overrideFn = factoryFunction,
                slotId = slotId,
                depth = application.depthTable[hostElement.elementId.index],
                closureRange = new RangeInt(storedClosureScopes.size - captureCount, captureCount)
            });
        }

        public bool MouseEnter(UIElement element, KeyboardModifiers modifiers, out MouseInputEvent evt) {
            evt = default;

            if ((stoppedEvents & InputEventType.MouseEnter) != 0 || (modifiers & modifiersThisFrame) != modifiers) {
                return false;
            }

            ElementId elementId = element.elementId;

            if (elementId.index >= maxElementId || !mouseEnterElements.Get(elementId)) {
                return false;
            }

            // todo -- look over the interface we're exposing here

            bool isFocused = false;
            evt = new MouseInputEvent(this, InputEventType.MouseEnter, modifiers, isFocused, elementId);

            return true;
        }

        public bool MouseExit(UIElement element, KeyboardModifiers modifiers, out MouseInputEvent evt) {
            evt = default;

            if ((stoppedEvents & InputEventType.MouseExit) != 0 || (modifiers & modifiersThisFrame) != modifiers) {
                return false;
            }

            ElementId elementId = element.elementId;

            if (elementId.index >= maxElementId || !mouseExitElements.Get(elementId)) {
                return false;
            }

            // todo -- look over the interface we're exposing here

            bool isFocused = false;
            evt = new MouseInputEvent(this, InputEventType.MouseExit, modifiers, isFocused, elementId);

            return true;
        }

        public bool MouseEvent(UIElement element, InputEventType eventType, KeyboardModifiers modifiers, out MouseInputEvent evt) {
            evt = default;

            if ((eventsThisFrame & eventType) == 0 || (stoppedEvents & eventType) != 0 || (modifiers & modifiersThisFrame) != modifiers) {
                return false;
            }

            ElementId elementId = element.elementId;

            for (int i = 0; i < inputResultBuffer.size; i++) {
                if (inputResultBuffer.array[i].id == elementId.id) {
                    bool isFocused = false;
                    evt = new MouseInputEvent(this, eventType, modifiers, isFocused, elementId);

                    return true;
                }
            }

            return false;
        }

        public bool TextInputEvent(UIElement element, out TextInputEvent evt) {

            ElementId elementId = element.elementId;

            if (application.focusedTextInputElementId == elementId && application.keyboardAdapter.keyEventQueue.Count > 0) {
                string newText = application.textEditor.ProcessTextInputEvent(application.keyboardAdapter.keyEventQueue);
                evt = new TextInputEvent(newText);
                return true;
            }

            evt = default;
            return false;
        }

        public void AddStoredClosureContext(object context) {
            storedClosureScopes.Add(context);
        }

        public void AddStoredClosureContextFromStack(int depthDiff) {
            storedClosureScopes.Add(fnContexts.array[fnContexts.size - depthDiff]);
        }

        public object GetStoredClosureContext(int index) {
            return storedClosureScopes[index];
        }

        public void PushStoredClosureContexts(RangeInt range) {
            //    object head = fnContexts.PeekUnchecked();
            for (int i = range.start; i < range.end; i++) {
                fnContexts.Push(storedClosureScopes.array[i]);
            }

            //fnContexts.Push(head); // account for skipped teleport -- todo confirm this correct
        }

        public void PopStoredClosureContexts(int count) {
            fnContexts.size -= count; // + 1; // account for skipped teleport -- todo confirm this correct
        }

        public void AddDestructiveScope(object context, int destroyId, Action<UIForiaRuntimeSystem, object, int, int> destroyFn) {
            destructionScopes.Add(new DestructionScope() {
                context = context,
                destroyFn = destroyFn,
                destroyId = destroyId,
                aux = -1
            });
        }

        internal void DiffDestructionScopes() {

            for (int i = 0; i < destructionScopes.size; i++) {
                destructionScopeSet.Add(destructionScopes.array[i]);
            }

            for (int i = 0; i < prevDestructionScopes.size; i++) {
                if (!destructionScopeSet.Contains(prevDestructionScopes.array[i])) {
                    toDestroy.Add(prevDestructionScopes.array[i]);
                }
            }

            for (int i = 0; i < toDestroy.size; i++) {
                toDestroy.array[i].destroyFn.Invoke(this, toDestroy.array[i].context, toDestroy.array[i].destroyId, toDestroy.array[i].aux);
            }

            destructionScopeSet.Clear();
            toDestroy.Clear();

            StructList<DestructionScope> tmp = prevDestructionScopes;
            prevDestructionScopes = destructionScopes;
            destructionScopes = tmp;
            destructionScopes.Clear();

        }

        public T DestroyElement<T>(ref T element) where T : UIElement {

            if (element == null) return null;

            Debug.Log("Destroying " + element.elementId);

            // do we invoke destroy now or wait? 
            // I think we wait & try/catch it 
            // or try/catch all code then destroy the elements here
            try {
                int index = element.elementId.index;
                runtimeInfoTable[index].generation++;

                ref InstancePropertyInfo instanceStyleInfo = ref application.instancePropertyTable[index];
                ref StyleInfo styleInfo = ref styleInfoTable[index];

                if (instanceStyleInfo.listSlice.capacity != 0) {
                    application.instancePropertyAllocator.Free(instanceStyleInfo.listSlice.start, instanceStyleInfo.listSlice.capacity);
                    instanceStyleInfo = default;
                }

                if (styleInfo.listSlice.capacity != 0) {
                    styleIdAllocator.Free(styleInfo.listSlice.start, styleInfo.listSlice.capacity);
                    styleInfo = default;
                }

                application.elementIdFreeList.Enqueue(index);

                // todo -- attributes too? 

                application.instanceTable[index] = default;

                element = default;
            }
            catch (Exception e) {
                Debug.LogException(e);
            }

            return element;

        }

        public object GetSlotOverrideMarker(int slotId) {
            for (int i = overrideStack.size - 1; i >= 0; i++) {
                if (overrideStack.array[i].slotId == slotId) {
                    return dummySlotOverride;
                }
            }

            return null;
        }

        public void PopSlotOverride(int count) {
            overrideStack.size -= count;
        }

        public static StyleId MakeStyleId(ulong val) {
            return new StyleId(val);
        }

        public StructList<TeleportRenderInfo> AddTeleport(string portalTarget, int closureScopeCount, StructList<TeleportRenderInfo> renderInfos, TeleportFn teleportFn) {

            renderInfos ??= StructList<TeleportRenderInfo>.Get();

            teleportInfos.Add(new TeleportInfo() {
                closureRange = new RangeInt(storedClosureScopes.size - closureScopeCount, closureScopeCount),
                portalName = portalTarget,
                renderFn = teleportFn,
                renderInfos = renderInfos,
            });

            return renderInfos;
        }

        public int AllocatePortalInstanceId() {
            return ++portalInstanceGenerator;
        }

        public void RenderPortalDefault(int portalInstanceId, UIElement parent, string portalName) {

            if (string.IsNullOrEmpty(portalName) || teleportInfos.size == 0) {
                return;
            }

            int renderIdx = 0;

            for (int i = 0; i < teleportInfos.size; i++) {
                TeleportInfo teleportInfo = teleportInfos.array[i];
                if (teleportInfo.portalName != portalName) {
                    continue;
                }

                PortalInfo portalInfo = new PortalInfo() {
                    portalName = portalName,
                    renderIndex = renderIdx++
                };

                bool foundMatch = false;
                for (int instIter = 0; instIter < teleportInfo.renderInfos.size; instIter++) {
                    ref TeleportRenderInfo instance = ref teleportInfo.renderInfos.array[instIter];

                    if (instance.portalId == portalInstanceId) {
                        foundMatch = true;
                        if (instance.didRender) {
                            continue;
                        }

                        teleportInfo.renderFn(this, parent, portalInfo, teleportInfo.closureRange, instance.context);
                        instance.didRender = true;
                        break;
                    }

                }

                if (!foundMatch) {
                    object context = teleportInfo.renderFn(this, parent, portalInfo, teleportInfo.closureRange, null);
                    teleportInfo.renderInfos.Add(new TeleportRenderInfo() {
                        context = context,
                        didRender = true,
                        portalId = portalInstanceId
                    });
                }

            }

        }

        private void CleanupTeleports() {

            for (int tpIndex = 0; tpIndex < teleportInfos.size; tpIndex++) {

                TeleportInfo teleportInfo = teleportInfos.array[tpIndex];

                // todo -- release render lists that didnt render
                for (int instIter = 0; instIter < teleportInfo.renderInfos.size; instIter++) {
                    teleportInfo.renderInfos.array[instIter].didRender = false;
                }

            }

            teleportInfos.Clear();

        }

        public void SetStyleListFromLongs1(UIElement element, ulong encoded0) {
            StyleId[] styleIdMemory = styleIdAllocator.memory;

            SmallListSlice styleIds = styleIdAllocator.AllocateSlice(1);

            styleIds.length = 1;

            styleIdMemory[styleIds.start] = *(StyleId*) (&encoded0);

            application.styleInfoTable[element.elementId.id & ElementId.k_IndexMask].listSlice = styleIds;
        }

        public void AddStylesFromLongs1(UIElement element, ulong encoded0) {
            styleBuffer.Add(*(StyleId*) (&encoded0));
        }

        public void AddStylesFromLongs2(UIElement element, ulong encoded0, ulong encoded1) {
            styleBuffer.Add(*(StyleId*) (&encoded0));
            styleBuffer.Add(*(StyleId*) (&encoded1));
        }

        public void AddStylesFromLongs3(UIElement element, ulong encoded0, ulong encoded1, ulong encoded2) {
            styleBuffer.Add(*(StyleId*) (&encoded0));
            styleBuffer.Add(*(StyleId*) (&encoded1));
            styleBuffer.Add(*(StyleId*) (&encoded2));
        }

        public void AddStylesFromLongs4(UIElement element, ulong encoded0, ulong encoded1, ulong encoded2, ulong encoded3) {
            styleBuffer.Add(*(StyleId*) (&encoded0));
            styleBuffer.Add(*(StyleId*) (&encoded1));
            styleBuffer.Add(*(StyleId*) (&encoded2));
            styleBuffer.Add(*(StyleId*) (&encoded3));
        }

        public void SetStyleListFromLongs2(UIElement element, ulong encoded0, ulong encoded1) {
            StyleId[] styleIdMemory = styleIdAllocator.memory;

            SmallListSlice styleIds = styleIdAllocator.AllocateSlice(2);

            styleIds.length = 2;

            styleIdMemory[styleIds.start + 0] = *(StyleId*) (&encoded0);
            styleIdMemory[styleIds.start + 1] = *(StyleId*) (&encoded1);

            application.styleInfoTable[element.elementId.id & ElementId.k_IndexMask].listSlice = styleIds;
        }

        public void SetStyleListFromLongs3(UIElement element, ulong encoded0, ulong encoded1, ulong encoded2) {
            StyleId[] styleIdMemory = styleIdAllocator.memory;

            SmallListSlice styleIds = styleIdAllocator.AllocateSlice(3);

            styleIds.length = 3;

            styleIdMemory[styleIds.start + 0] = *(StyleId*) (&encoded0);
            styleIdMemory[styleIds.start + 1] = *(StyleId*) (&encoded1);
            styleIdMemory[styleIds.start + 2] = *(StyleId*) (&encoded2);

            application.styleInfoTable[element.elementId.id & ElementId.k_IndexMask].listSlice = styleIds;
        }

        public void SetStyleListFromLongs4(UIElement element, ulong encoded0, ulong encoded1, ulong encoded2, ulong encoded3) {
            StyleId[] styleIdMemory = styleIdAllocator.memory;

            SmallListSlice styleIds = styleIdAllocator.AllocateSlice(4);

            styleIds.length = 4;

            styleIdMemory[styleIds.start + 0] = *(StyleId*) (&encoded0);
            styleIdMemory[styleIds.start + 1] = *(StyleId*) (&encoded1);
            styleIdMemory[styleIds.start + 2] = *(StyleId*) (&encoded2);
            styleIdMemory[styleIds.start + 3] = *(StyleId*) (&encoded3);

            application.styleInfoTable[element.elementId.id & ElementId.k_IndexMask].listSlice = styleIds;
        }

        public void AddStyleId(StyleId styleId) {
            styleBuffer.Add(styleId);
        }

        public void AddStyleIdAsLong(bool condition, ulong styleId) {
            if (!condition) return;
            styleBuffer.Add(*(StyleId*) &styleId);
        }

        public void AddSelectStyleIdAsLong(bool condition, ulong a, ulong b) {
            styleBuffer.Add(*(StyleId*) (condition ? &a : &b));
        }

        public void AddStyleAsLong(ulong encodedStyle) {
            styleBuffer.Add(*(StyleId*) (&encodedStyle));
        }

        public void AddStyleAsLongWithCondition(bool condition, ulong encodedStyle) {
            if (condition) {
                styleBuffer.Add(*(StyleId*) (&encodedStyle));
            }
        }

        public void ApplyStyleBuffer(UIElement element) {

            ref StyleInfo styleInfo = ref application.styleInfoTable[element.elementId.index];

            StyleId[] styleIdMemory = styleIdAllocator.memory;

            int start = styleInfo.listSlice.start;

            if (styleInfo.listSlice.length == styleBuffer.size) {
                bool different = false;

                for (int i = 0; i < styleBuffer.size; i++) {
                    if (styleIdMemory[start + i] != styleBuffer.array[i]) {
                        different = true;
                    }
                }

                if (!different) {
                    styleBuffer.size = 0;
                    return;
                }

            }

            if (styleInfo.listSlice.capacity < styleBuffer.size) {
                styleIdAllocator.Free(styleInfo.listSlice.start, styleInfo.listSlice.capacity);
                styleInfo.listSlice = styleIdAllocator.AllocateSlice(styleBuffer.size);
            }

            for (int i = 0; i < styleBuffer.size; i++) {
                styleIdMemory[styleInfo.listSlice.start + i] = styleBuffer.array[i];
            }

            styleInfo.listSlice.length = (ushort) styleBuffer.size;
            styleBuffer.size = 0;

            ref RuntimeTraversalInfo info = ref runtimeInfoTable[element.elementId.id & ElementId.k_IndexMask];

            info.flags |= UIElementFlags.StyleListChanged;
        }

        public void SetAttribute(UIElement element, string key, string value) { }

        public void AddEntryPoint(UIElement entry) {
            currentView.RootElement = entry;
            application.CreateElement(currentView.viewId, default, entry);
            application.depthTable[entry.elementId.index] = 0;
        }

        public UIElement AddChild(UIElement parent, UIElement child) {
            application.CreateElement(currentView.viewId, parent.elementId, child);
            return child;
        }

        public void SetEnabledThisFrame(UIElement element) {

            ref RuntimeTraversalInfo runtimeInfo = ref runtimeInfoTable[element.elementId.id & ElementId.k_IndexMask];
            // if enabled is not currently set, set initThisFrame. avoid branching
            UIElementFlags initThisFrame = (UIElementFlags) ((int) (runtimeInfo.flags & UIElementFlags.Enabled ^ UIElementFlags.Enabled) << 1);
            runtimeInfo.flags |= (UIElementFlags.Enabled | initThisFrame);
            runtimeInfo.index = activeElementList.size;
            runtimeInfo.lastChildIndex = 0;

            // inlined list.Add 
            if (activeElementList.size + 1 >= (1 << activeElementList.capacityShiftBits)) {
                activeElementList.EnsureCapacity(activeElementList.size + 1);
            }

            activeElementList.array[activeElementList.size++] = element.elementId;

        }

        public void EndChildRange(UIElement element) {
            runtimeInfoTable[element.elementId.id & ElementId.k_IndexMask].lastChildIndex = activeElementList.size;
        }

        // context is going to be a generated type unless i separate template data from context

        public static void UpdateForeachEnumerable<T, U>(IEnumerable<T> list, ref SizedArray<T> foreachValues, ref SizedArray<U> foreachContexts) {
            if (list == null) {
                if (foreachContexts.size != 0) {
                    for (int i = 0; i < foreachContexts.size; i++) {
                        foreachContexts.array[i] = default;
                        foreachValues.array[i] = default;
                    }

                    foreachValues.size = 0;
                    foreachContexts.size = 0;
                }

                return;
            }

            int count = list.Count();
            if (count > foreachContexts.size) {
                foreachContexts.EnsureCapacity(count);
                foreachContexts.size = count; // growing the list will naturally set the values to empty, which the loop checks will pick up
            }
            else if (count < foreachContexts.size) {
                // if we don't do this we can cause stuff not to be collected
                // foreach always creates children as destructive 
                for (int i = count; i < foreachContexts.size; i++) {
                    foreachContexts.array[i] = default;
                    foreachValues.array[i] = default;

                }

                foreachContexts.size = count;
            }

            // todo -- could skip this with an immutable flag
            // meaning we get values from the source list because user promises source list wont change during iteration

            int idx = 0;
            foreach (T item in list) {
                foreachValues.array[idx++] = item;
            }

        }

        public int UpdateForeachList<T, U>(IList<T> list, ref SizedArray<T> foreachValues, ref SizedArray<U> foreachContexts, Action<UIForiaRuntimeSystem, object, int, int> destroyFn) where U : class {
            if (list == null) {
                if (foreachContexts.size != 0) {
                    for (int i = 0; i < foreachContexts.size; i++) {
                        foreachValues.array[i] = default;
                        // actual destruction code is responsible for returning the context to our pool 
                        prevDestructionScopes.Add(new DestructionScope() {
                            context = foreachContexts.array,
                            destroyId = i,
                            destroyFn = destroyFn
                        });
                    }

                    foreachValues.size = 0;
                    foreachContexts.size = 0;
                }

                return 0;
            }

            int count = list.Count;

            if (count > foreachContexts.size) {
                foreachContexts.EnsureCapacity(count);
                foreachValues.EnsureCapacity(count);

                for (int i = foreachContexts.size; i < count; i++) {
                    foreachContexts.array[i] = ContextPool<U>.Get();
                }

                foreachValues.size = count;
                foreachContexts.size = count; // growing the list will naturally set the values to empty, which the loop checks will pick up
            }
            else if (count < foreachContexts.size) {

                // if we don't do this we can cause stuff not to be collected
                for (int i = count; i < foreachContexts.size; i++) {
                    foreachValues.array[i] = default;
                    // actual destruction code is responsible for returning the context to our pool 
                    prevDestructionScopes.Add(new DestructionScope() {
                        context = foreachContexts.array,
                        destroyId = 0,
                        aux = i,
                        destroyFn = destroyFn
                    });
                }

                foreachValues.size = count;
                foreachContexts.size = count;
            }

            // todo -- could skip this with an immutable flag
            // meaning we get values from the source list because user promises source list wont change during iteration

            for (int i = 0; i < count; i++) {
                foreachValues.array[i] = list[i];
            }

            return count;

        }

        public void StartFrame() {
            activeElementList.size = 0;
            Array.Clear(fnContexts.array, 0, fnContexts.array.Length);
            Array.Clear(storedClosureScopes.array, 0, storedClosureScopes.array.Length);
            fnContexts.size = 0;
            storedClosureScopes.size = 0;
            CleanupTeleports();
            consumedEvents = default;
            stoppedEvents = default;
            modifiersThisFrame = default; // todo -- read from input 

            uiRuntime.frameId = currentFrameId;
            uiRuntime.application = application;
        }

        public void DestroyForeach<T>(ref SizedArray<T> foreachCtx, Action<UIForiaRuntimeSystem, object, int> destroy) {
            int size = foreachCtx.size;
            for (int i = 0; i < size; i++) {
                destroy.Invoke(this, foreachCtx.array, i);
                foreachCtx[i] = default;
            }

            foreachCtx.size = 0;
        }

        private struct SlotOverrideInfo {

            public int slotId;
            public object overrideFn;
            public RangeInt closureRange;
            public ushort depth;

        }

        private struct DestructionScope : IEquatable<DestructionScope> {

            public object context;
            public int destroyId;
            public int aux;
            public Action<UIForiaRuntimeSystem, object, int, int> destroyFn;

            public bool Equals(DestructionScope other) {
                return context == other.context && destroyId == other.destroyId && aux == other.aux;
            }

            public override bool Equals(object obj) {
                return obj is DestructionScope other && Equals(other);
            }

            public override int GetHashCode() {
                unchecked {
                    return (context.GetHashCode() * 397) ^ destroyId * 397 ^ aux;
                }
            }

        }

    }

}