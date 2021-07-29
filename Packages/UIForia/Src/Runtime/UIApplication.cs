using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using UIForia.Elements;
using UIForia.Layout;
using UIForia.Rendering;
using UIForia.Style;
using UIForia.Text;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace UIForia {

    public abstract unsafe class UIApplication : IDisposable {

        public const bool DisableJobSafety = false;

        public delegate bool StyleConditionEvaluator(UIView view, DeviceInfo deviceInfo);

        public float dpiScaleFactor = 1;//Mathf.Max(1, Screen.dpi / 96f);

        public static readonly float originalDpiScaleFactor = Mathf.Max(1, Screen.dpi / 96f);

        public float DPIScaleFactor {
            get => dpiScaleFactor;
            set => dpiScaleFactor = value;
        }

        internal LightList<UIView> viewList;
        internal LightList<UIScriptInstance> activeScriptInstances;

        internal DataList<ColorVariable>.Shared colorVariables;
        internal DataList<ValueVariable>.Shared valueVariables;
        internal DataList<TextureVariable>.Shared textureVariables;

        // todo -- meta table can maybe hold style state too
        internal UIElement[] instanceTable;
        internal StyleInfo[] styleInfoTable;
        internal StyleState[] styleStateTable;
        internal SmallListSlice[] attributeTable;
        internal TemplateInfo[] templateInfoTable;
        internal InstancePropertyInfo[] instancePropertyTable;
        internal ushort[] depthTable;
        internal ushort[] elementIdToViewId;
        internal ElementId[] elementIdToParentId;
        internal RuntimeTraversalInfo[] runtimeInfoTable;
        internal int[] layoutIndexByElementId;

        // layout based data which we copy for the user
        internal Size[] layoutSizeByLayoutIndex;
        internal OffsetRect[] layoutBordersByLayoutIndex;
        internal OffsetRect[] layoutPaddingsByLayoutIndex;
        internal float2[] layoutLocalPositionByLayoutIndex;
        internal OrientedBounds[] layoutBoundsByLayoutIndex;
        internal float4x4[] layoutLocalMatrixByLayoutIndex;
        internal float4x4[] layoutWorldMatrixByLayoutIndex;

        internal DataList<StyledAttributeChange> attributeChanges;
        internal ManagedListAllocator<StyleId> styleIdAllocator;
        internal ManagedListAllocator<ElementAttribute> attributeAllocator;
        internal ManagedListAllocator<PropertyContainer> instancePropertyAllocator;

        // assigned from ApplicationSetup but disposed here
        // todo -- when resetting we also need to dispose these
        internal StyleDatabase styleDatabase;

        private int elementIdGenerator;
        private int elementCapacity;
        private int layoutBufferCapacity;
        private int viewIdGenerator; // wants to be ushort w/wrapping 
        internal ApplicationLoop applicationLoop;

        internal TextDataTable* textDataTable;
        internal UIForiaRuntimeSystem runtime;
        internal Queue<int> elementIdFreeList;
        internal MouseAdapter mouseAdapter;
        internal KeyboardAdapter keyboardAdapter;

        internal StructList<StyleConditionEvaluatorEntry> styleConditionEvaluators;
        internal LightList<UIScript> scripts;

        internal TextEditor textEditor;

        // implicitly a EditableText id/
        internal ElementId focusedTextInputElementId;

        internal UIApplication() {
            this.textEditor = new TextEditor();

            this.styleIdAllocator = new ManagedListAllocator<StyleId>("StyleIdAllocator", 2, 32);
            this.attributeAllocator = new ManagedListAllocator<ElementAttribute>("AttributeAllocator", 2, 16);
            this.instancePropertyAllocator = new ManagedListAllocator<PropertyContainer>("InstancePropertyAllocator", 2, 64, 4);
            this.elementIdFreeList = new Queue<int>();
            this.applicationLoop = new ApplicationLoop();

            this.attributeChanges = new DataList<StyledAttributeChange>(32, Allocator.Persistent);
            this.colorVariables = new DataList<ColorVariable>.Shared(8, Allocator.Persistent);
            this.valueVariables = new DataList<ValueVariable>.Shared(8, Allocator.Persistent);
            this.textureVariables = new DataList<TextureVariable>.Shared(8, Allocator.Persistent);
            this.activeScriptInstances = new LightList<UIScriptInstance>();

            this.mouseAdapter = new MouseAdapter();
            this.keyboardAdapter = new LegacyKeyboardAdapter();

            this.runtime = new UIForiaRuntimeSystem(this);
            this.scripts = new LightList<UIScript>();

            styleConditionEvaluators = new StructList<StyleConditionEvaluatorEntry>();
            // todo -- find a better home for this
            s_DebuggedApplication = this;

        }
        internal static UIApplication s_DebuggedApplication;

        internal int MaxElementId => elementIdGenerator;

        public void SetSize(int width, int height) {
            this.width = width;
            this.height = height;
            viewList[0].SetSize(width, height); // hack
        }

        private float width;
        private float height;

        // todo -- dpi?
        public float Width {
            get => width <= 0 ? Screen.width : width;
        }

        // todo -- dpi?
        public float Height {
            get => height <= 0 ? Screen.height : height;
        }

        public void SetMouseAdapter(MouseAdapter mouseAdapter) {
            this.mouseAdapter = mouseAdapter ?? this.mouseAdapter;
        }

        public void SetKeyboardAdapter(KeyboardAdapter keyboardAdapter) {
            this.keyboardAdapter = keyboardAdapter ?? this.keyboardAdapter;
        }

        public void Dispose() {

            applicationLoop.Dispose();

            if (textDataTable != null) {
                textDataTable->Dispose();
                TypedUnsafe.Dispose(textDataTable, Allocator.Persistent);
                textDataTable = default;
            }

            runtime.Dispose();

            attributeChanges.Dispose();
            styleDatabase.Dispose();

            colorVariables.Dispose();
            valueVariables.Dispose();
            textureVariables.Dispose();

            if (s_DebuggedApplication == this) {
                s_DebuggedApplication = null;
            }

        }

        ~UIApplication() {
            Dispose();
        }

        internal void Initialize(ApplicationInfo setup) {

            textDataTable = TypedUnsafe.Malloc(TextDataTable.Create(), Allocator.Persistent);

            colorVariables.size = 0;
            valueVariables.size = 0;
            textureVariables.size = 0;

            styleDatabase = setup.styleDatabase;
            elementIdGenerator = 1; // id 0 is invalid
            elementCapacity = setup.initialElementCapacity < 64 ? 64 : setup.initialElementCapacity;
            viewList = new LightList<UIView>();
            ResizeElementIdBasedBackingBuffer();
            ResizeLayoutIdBasedBuffer(64);

            runtimeInfoTable[0].flags = UIElementFlags.Enabled;

            UIView view = new UIView(viewIdGenerator++, this, "Default", new Rect(0, 0, Screen.width, Screen.height));

            styleDatabase.Initialize(); // todo -- I think this init logic doesn't belong to the style db, move it elsewhere

            view.updateFn = setup.main;
            viewList.Add(view);

            applicationLoop.Initialize(this);
        }

        public void Update(float deltaTime = -1) {

            if (deltaTime < 0) deltaTime = Time.deltaTime;

            bool allTicked = false;

            runtime.prevFrameId = runtime.currentFrameId++;

            if (runtime.prevFrameId == 0) runtime.prevFrameId = -1; // this will let the enable calls trigger on frame 0 

            runtime.StartFrame();

            while (!allTicked) {

                allTicked = true;

                for (int i = 0; i < viewList.size; i++) {
                    UIView uiView = viewList.array[i];

                    if (uiView.lastTickFrame != runtime.currentFrameId) {
                        allTicked = false;
                        uiView.lastTickFrame = runtime.currentFrameId;
                        runtime.currentView = uiView;
                        uiView.updateContext = uiView.updateFn(runtime, uiView.RootElement, uiView.updateContext);
                    }

                }

            }

            TempList<ElementId> invokeDisableList = default;

            fixed (RuntimeTraversalInfo* runtimeInfo = runtimeInfoTable) {
                new DisableElementMaps() {
                    activeElements = runtime.activeElementList,
                    invokeDisableList = &invokeDisableList,
                    runtimeInfo = new CheckedArray<RuntimeTraversalInfo>(runtimeInfo, MaxElementId)
                }.Run();
            }

            for (int i = 0; i < invokeDisableList.size; i++) {
                instanceTable[invokeDisableList[i].index].OnDisable();
            }

            invokeDisableList.Dispose();

            runtime.DiffDestructionScopes();

            applicationLoop.Update(deltaTime, this);

        }

        public void Render(Camera camera) {
            if (camera == null) return;
            applicationLoop.Render(this, camera);
        }

        internal void CreateElement(int viewId, ElementId parentId, UIElement element) {

            int idx;
            if (elementIdFreeList.Count != 0) {
                idx = elementIdFreeList.Dequeue();
            }
            else {
                idx = elementIdGenerator++;
                if (idx >= elementCapacity) {
                    ResizeElementIdBasedBackingBuffer();
                }
            }

            ref RuntimeTraversalInfo meta = ref runtimeInfoTable[idx];

            meta.index = 0;
            meta.flags = default;
            meta.lastChildIndex = 0;
            // note: need to use generation from existing data at idx, do not touch it execpt on destruction!

            element.application = this;

            styleStateTable[idx] = default;

            // maybe I can fold this into meta? depends on if we really want/ need tag name 
            templateInfoTable[idx] = new TemplateInfo() {
                templateHostId = default,
                templateOriginId = default,
                tagNameId = default,
                typeClass = GetTypeClass(element) // todo -- this should be known at compile time 
            };

            instanceTable[idx] = element;
            elementIdToParentId[idx] = parentId;
            elementIdToViewId[idx] = (ushort) viewId;
            depthTable[idx] = (ushort) (depthTable[parentId.id & ElementId.k_IndexMask] + 1);
            element.elementId = new ElementId(idx, meta.generation);

        }

        private ElementTypeClass GetTypeClass(UIElement element) {
            // todo -- for templates / slots / other this needs to figured out 
            if (element is UITextElement) return ElementTypeClass.Text;
            if (element is UIContainerElement) return ElementTypeClass.Container;
            return ElementTypeClass.Template;
        }

        internal void ResizeLayoutIdBasedBuffer(int size) {

            if (size <= layoutBufferCapacity) {
                return;
            }

            layoutBufferCapacity = size;

            // in case this is the first frame we don't want to double the capacity yet
            if (layoutSizeByLayoutIndex != null) {
                layoutBufferCapacity *= 2;
            }

            Array.Resize(ref layoutSizeByLayoutIndex, layoutBufferCapacity);
            Array.Resize(ref layoutBordersByLayoutIndex, layoutBufferCapacity);
            Array.Resize(ref layoutPaddingsByLayoutIndex, layoutBufferCapacity);
            Array.Resize(ref layoutLocalPositionByLayoutIndex, layoutBufferCapacity);
            Array.Resize(ref layoutBoundsByLayoutIndex, layoutBufferCapacity);
            Array.Resize(ref layoutLocalMatrixByLayoutIndex, layoutBufferCapacity);
            Array.Resize(ref layoutWorldMatrixByLayoutIndex, layoutBufferCapacity);
        }

        private void ResizeElementIdBasedBackingBuffer() {

            // in case this is the first frame we don't want to double the capacity yet
            if (styleInfoTable != null) {
                elementCapacity *= 2;
            }

            Array.Resize(ref instanceTable, elementCapacity);
            Array.Resize(ref styleInfoTable, elementCapacity);
            Array.Resize(ref templateInfoTable, elementCapacity);
            Array.Resize(ref styleStateTable, elementCapacity);
            Array.Resize(ref instancePropertyTable, elementCapacity);
            Array.Resize(ref attributeTable, elementCapacity);
            Array.Resize(ref depthTable, elementCapacity);
            Array.Resize(ref elementIdToViewId, elementCapacity);
            Array.Resize(ref elementIdToParentId, elementCapacity);
            Array.Resize(ref runtimeInfoTable, elementCapacity);
            Array.Resize(ref layoutIndexByElementId, elementCapacity);

            runtime.runtimeInfoTable = runtimeInfoTable;
            runtime.styleInfoTable = styleInfoTable;
        }

        internal UIView GetView(ushort viewId) {
            for (int i = 0; i < viewList.size; i++) {
                if (viewList.array[i].viewId == viewId) {
                    return viewList.array[i];
                }
            }

            return null;
        }

        // todo -- needs to be handled in a way that works with split screen, ie multiple focuses per view 
        public bool RequestFocus(UIElement element) {
            throw new NotImplementedException();
        }

        public bool IsElementAlive(ElementId elementId) {
            return runtimeInfoTable[elementId.index].generation == elementId.generation;
        }

        public void RegisterFocusable(UIElement element) {
            throw new NotImplementedException();
        }

        public void UnregisterFocusable(UIElement element) {
            throw new NotImplementedException();
        }

        #region Variables

        public void SetEnumVariable<T>(ElementId elementId, string variableName, T value) where T : unmanaged, Enum {
            throw new NotImplementedException("style vars need a tagger that is threadsafe");

            // int enumTypeId = styleDatabase.GetEnumTypeId(typeof(T));
            // if (enumTypeId <= 0) {
            //     throw new Exception("Invalid enum usage: " + typeof(T));
            // }
            //
            // ushort variableNameId = (ushort) internSystem.AddConstant(FormatVariableName(variableName));
            //
            // unsafe {
            //     ValueVariable variable = new ValueVariable() {
            //         elementId = elementId,
            //         variableNameId = variableNameId,
            //         value = UIValue.FromEnum(*(int*) &value),
            //         enumTypeId = (ushort) enumTypeId
            //     };
            //
            //     for (int i = 0; i < valueVariables.size; i++) {
            //         if (valueVariables[i].elementId != elementId || valueVariables[i].variableNameId != variableNameId) {
            //             continue;
            //         }
            //
            //         valueVariables[i] = variable;
            //         return;
            //     }
            //
            //     valueVariables.Add(variable);
            // }

        }

        public void RemoveEnumVariable<T>(ElementId elementId, string variableName) {
            throw new NotImplementedException("style vars need a tagger that is threadsafe");

            ushort variableNameId = default; // (ushort) styleDatabase.tagger.GetTagId(FormatVariableName(variableName));
            for (int i = 0; i < valueVariables.size; i++) {
                if (valueVariables[i].elementId != elementId || valueVariables[i].variableNameId != variableNameId) {
                    continue;
                }

                valueVariables[i] = valueVariables[valueVariables.size - 1];
                valueVariables.size--;

                return;
            }
        }

        public void SetValueVariable(ElementId elementId, string variableName, UIValue value) {
            int variableNameId = styleDatabase.variableTagger.GetTagId(variableName);
            if (variableNameId == -1) {
                return;
            }

            ValueVariable variable = new ValueVariable() {
                elementId = elementId,
                variableNameId = (ushort) variableNameId,
                value = value
            };

            for (int i = 0; i < valueVariables.size; i++) {
                if (valueVariables[i].elementId != elementId || valueVariables[i].variableNameId != variableNameId) {
                    continue;
                }

                valueVariables[i] = variable;
                return;
            }

            valueVariables.Add(variable);
        }

        public void RemoveValueVariable(ElementId elementId, string variableName) {

            int variableNameId = styleDatabase.variableTagger.GetTagId(variableName);
            if (variableNameId == -1) {
                return;
            }

            for (int i = 0; i < valueVariables.size; i++) {
                if (valueVariables[i].elementId != elementId || valueVariables[i].variableNameId != variableNameId) {
                    continue;
                }

                valueVariables[i] = valueVariables[valueVariables.size - 1];
                valueVariables.size--;

                return;
            }
        }

        public void SetStyleCondition(string conditionName, StyleConditionEvaluator evaluator) {

            if (evaluator == null) {
                // todo beat up user
                return;
            }

            int conditionId = styleDatabase.conditionTagger.GetTagId(conditionName);
            if (conditionId == -1) {
                // todo print nice error message
                return;
            }

            for (var index = 0; index < styleConditionEvaluators.size; index++) {
                if (styleConditionEvaluators.array[index].id == conditionId) {
                    styleConditionEvaluators.array[index].evaluator = evaluator;
                    return;
                }
            }

            styleConditionEvaluators.Add(new StyleConditionEvaluatorEntry() {
                id = conditionId,
                evaluator = evaluator
            });
        }

        public void RemoveStyleCondition(string conditionName) {

            int conditionId = styleDatabase.conditionTagger.GetTagId(conditionName);
            if (conditionId == -1) {
                // todo print nice error message
                return;
            }

            for (var index = 0; index < styleConditionEvaluators.size; index++) {
                if (styleConditionEvaluators.array[index].id == conditionId) {
                    styleConditionEvaluators.RemoveAt(index);
                    return;
                }
            }
        }

        public void SetColorVariable(ElementId elementId, string variableName, UIColor color) {
            int variableNameId = styleDatabase.variableTagger.GetTagId(variableName);
            if (variableNameId == -1) {
                return;
            }

            ColorVariable variable = new ColorVariable() {
                elementId = elementId,
                variableNameId = (ushort) variableNameId,
                color = color
            };

            for (int i = 0; i < colorVariables.size; i++) {
                if (colorVariables[i].elementId != elementId || colorVariables[i].variableNameId != variableNameId) {
                    continue;
                }

                colorVariables[i] = variable;
                return;
            }

            colorVariables.Add(variable);
        }

        public void RemoveColorVariable(ElementId elementId, string variableName) {
            int variableNameId = styleDatabase.variableTagger.GetTagId(variableName);
            if (variableNameId == -1) {
                return;
            }

            for (int i = 0; i < colorVariables.size; i++) {
                if (colorVariables[i].elementId != elementId || colorVariables[i].variableNameId != variableNameId) {
                    continue;
                }

                colorVariables[i] = colorVariables[colorVariables.size - 1];
                colorVariables.size--;

                return;
            }
        }

        internal VariableNameId ResolveVariableName(string variableName) {
            throw new NotImplementedException("style vars need a tagger that is threadsafe");

            if (string.IsNullOrEmpty(variableName)) {
                return default;
            }

            variableName = FormatVariableName(variableName);
//            return new VariableNameId((ushort) internSystem.AddConstant(variableName));
        }

        private static string FormatVariableName(string variableName) {

            // todo -- dont allocate this string concat 
            if (variableName[0] != '$') {
                variableName = '$' + variableName;
            }

            return variableName;
        }

        #endregion

        #region Attributes

        internal void SetAttribute(ElementId elementId, string key, string value) {

            if (string.IsNullOrEmpty(key)) {
                return;
            }

            ref SmallListSlice slice = ref attributeTable[elementId.index];

            if (value == null) {
                RemoveAttribute(elementId, key, ref slice);
                return;
            }

            int end = slice.start + slice.length;

            for (int i = slice.start; i < end; i++) {
                ref ElementAttribute attr = ref attributeAllocator.memory[i];
                if (attr.key == key) {
                    if (attr.value == value) {
                        return;
                    }

                    ReplaceAttribute(elementId, ref attr, key, value);
                    return;
                }
            }

            AddAttribute(elementId, key, value, ref slice);

        }

        private void AddAttribute(ElementId elementId, string key, string value, ref SmallListSlice slice) {

            int keyTagId = styleDatabase.attributeTagger.GetTagId(key);
            if (keyTagId >= 0) {
                attributeChanges.Add(new StyledAttributeChange() {
                    elementId = elementId,
                    operation = StyledAttributeOperation.Add,
                    tagId = keyTagId
                });
            }

            char* cbuffer = stackalloc char[key.Length + value.Length + 1];
            fixed (char* keyBuffer = key)
            fixed (char* valueBuffer = value) {
                TypedUnsafe.MemCpy(cbuffer, keyBuffer, key.Length);
                cbuffer[key.Length] = '=';
                TypedUnsafe.MemCpy(cbuffer + key.Length + 1, valueBuffer, value.Length);
            }

            int valueTagId = styleDatabase.attributeTagger.GetTagId(cbuffer, key.Length + value.Length + 1);

            if (valueTagId >= 0) {
                attributeChanges.Add(new StyledAttributeChange() {
                    elementId = elementId,
                    operation = StyledAttributeOperation.Add,
                    tagId = valueTagId
                });
            }

            if (slice.length + 1 >= slice.capacity) {

                SmallListSlice newSlice = attributeAllocator.AllocateSlice(slice.length + 1);

                for (int i = 0; i < slice.length; i++) {
                    attributeAllocator.memory[newSlice.start + i] = attributeAllocator.memory[slice.start + i];
                }

                attributeAllocator.memory[slice.start + slice.length] = new ElementAttribute() {
                    key = key,
                    value = value,
                    keyId = keyTagId,
                    valueId = valueTagId
                };

                if (slice.capacity > 0) {
                    attributeAllocator.Free(slice.start, slice.capacity);
                }

                newSlice.length = (ushort) (slice.length + 1);
                slice = newSlice;

            }
            else {
                attributeAllocator.memory[slice.length++] = new ElementAttribute() {
                    key = key,
                    value = value,
                    keyId = keyTagId,
                    valueId = valueTagId
                };
            }

        }

        private void RemoveAttribute(ElementId elementId, string key, ref SmallListSlice slice) {

            for (int i = 0; i < slice.length; i++) {

                ref ElementAttribute attr = ref attributeAllocator.memory[slice.start + i];

                if (attr.key == key) {

                    if (attr.keyId >= 0) {
                        attributeChanges.Add(new StyledAttributeChange() {
                            operation = StyledAttributeOperation.Remove,
                            elementId = elementId,
                            tagId = attr.keyId
                        });
                    }

                    if (attr.valueId >= 0) {
                        attributeChanges.Add(new StyledAttributeChange() {
                            operation = StyledAttributeOperation.Remove,
                            elementId = elementId,
                            tagId = attr.valueId
                        });
                    }

                    attr = default;
                    return;
                }
            }

        }

        private void ReplaceAttribute(ElementId elementId, ref ElementAttribute attr, string key, string value) {

            if (attr.valueId < 0) {
                // remove it
                attributeChanges.Add(new StyledAttributeChange() {
                    elementId = elementId,
                    tagId = attr.valueId,
                    operation = StyledAttributeOperation.Remove,
                });
            }

            char* cbuffer = stackalloc char[key.Length + value.Length];

            fixed (char* keyBuffer = key)
            fixed (char* valueBuffer = value) {
                TypedUnsafe.MemCpy(cbuffer, keyBuffer, key.Length);
                cbuffer[key.Length] = '=';
                TypedUnsafe.MemCpy(cbuffer + key.Length + 1, valueBuffer, value.Length);
            }

            int valueTagId = styleDatabase.attributeTagger.GetTagId(cbuffer, key.Length + value.Length + 1);

            if (valueTagId >= 0) {
                attributeChanges.Add(new StyledAttributeChange() {
                    elementId = elementId,
                    operation = StyledAttributeOperation.Add,
                    tagId = valueTagId
                });
            }

        }

        internal string GetAttribute(ElementId elementId, string key) {

            if (string.IsNullOrEmpty(key)) {
                return null;
            }

            ref SmallListSlice slice = ref attributeTable[elementId.index];

            for (int i = 0; i < slice.length; i++) {
                ref ElementAttribute attr = ref attributeAllocator.memory[slice.start + i];
                if (attr.key == key) {
                    return attr.value;
                }
            }

            return null;

        }

        #endregion

        public void RequestTextInputFocus(ElementId elementId) {
            if (!ElementSystem.IsDeadOrDisabled(elementId, runtimeInfoTable)) {
                focusedTextInputElementId = elementId;
            }
            else {
                ReleaseTextInputFocus();
            }
        }

        public void ReleaseTextInputFocus() {
            focusedTextInputElementId = default;
            // todo maybe?
            textEditor.SetTextEditorData(default, null);
        }

        public void ActivateTextEditor(EditableText editableText, TextEditorData textSettings) {
            focusedTextInputElementId = editableText.elementId;
            textEditor.SetTextEditorData(textSettings, editableText);
        }

        public void BlurTextInput() { }

        public bool TryGetAnimationIdByName(string module, string animationName, out AnimationReference reference) {
            return styleDatabase.TryGetAnimationIdByName(module, animationName, out reference);
        }

        public void RunScript(UIScriptInstance scriptInstance) {
            activeScriptInstances.Add(scriptInstance);
        }


        public bool TryFindScript(string scriptName, out UIScript script) {
            script = default;
            for (int i = 0; i < scripts.size; i++) {
                if (scripts.array[i].scriptName == scriptName) {
                    script = scripts.array[i];
                    return true;
                }
            }

            return false;
        }

    }

}