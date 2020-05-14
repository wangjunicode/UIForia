using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UIForia.Compilers;
using UIForia.Elements;
using UIForia.Layout;
using UIForia.Parsing;
using UIForia.Rendering;
using UIForia.Systems;
using UIForia.UIInput;
using UIForia.Util;
using UnityEngine;
using UnityEngine.Assertions;

namespace UIForia {

    public class TemplateSystem {

        private int idGenerator;

        private LightStack<ContextEntry> contextStack;

        private UIElement root;
        private UIElement parent;
        private UIElement element;
        private TemplateData currentTemplateData;
        private Dictionary<Type, TemplateData> templateDataMap;

        private Action<UIElement> onElementRegistered;

        // internal readonly StructList<int> freeListIndex;
        // internal readonly LightList<UIElement> elementMap;
        // private int indexGenerator;
        private ElementSystem elementSystem;
        private StyleSystem2 styleSystem;
        private AttributeSystem attributeSystem;

        internal TemplateSystem(Dictionary<Type, TemplateData> templateDataMap) {
            this.templateDataMap = templateDataMap;
            this.contextStack = new LightStack<ContextEntry>(16);
            //       this.freeListIndex = new StructList<int>(64);
            //      this.elementMap = new LightList<UIElement>(64);
        }

        internal UIElement CreateEntryPoint<T>() where T : UIElement, new() {
            throw new NotImplementedException();
        }

        internal UIElement CreateEntryPoint(UIView window, TemplateData data) {
            currentTemplateData = data;

            UIElement retn = data.entry(this);

            window.dummyRoot.children.Add(retn);

            return retn;
        }

        internal UIElement CreateAndInsertChild() {
            throw new NotImplementedException();
        }

        internal UIElement CreateAndAppendChild() {
            throw new NotImplementedException();
        }

        public void HydrateEntryPoint() {

            contextStack.array[contextStack.size++] = new ContextEntry() {
                contextRoot = element,
                templateData = currentTemplateData
            };

            parent = element;
            currentTemplateData.hydrate(this);
            parent = null;

            Assert.IsTrue(contextStack.size == 1);

            contextStack.array[--contextStack.size] = default;

        }

        public void HydrateElement(Type type) {

            ref ContextEntry entry = ref contextStack.array[contextStack.size - 1];

            TemplateData oldTemplateData = currentTemplateData;

            templateDataMap.TryGetValue(type, out currentTemplateData);

            UIElement oldRoot = root;
            root = element;

            // ReSharper disable once PossibleNullReferenceException
            currentTemplateData.hydrate(this);

            root = oldRoot;
            currentTemplateData = oldTemplateData;

            entry.overrides?.Release();
            entry = default;
            contextStack.size--;
        }

        public void SetText(string value) {
            ((UITextElement) element).SetText(value);
        }

        public void SetBindings(int updateBindingId, int lateUpdateBindingId, int constBindingId, int enableBindingId, int bindingVariableCount) {
            element.bindingNode.variables = bindingVariableCount != 0 ? new BindingVariable[bindingVariableCount] : default;
            if (updateBindingId != -1) {
                element.bindingNode.updateBindings = currentTemplateData.bindings[updateBindingId];
            }

            if (constBindingId != -1) {
                currentTemplateData.bindings[constBindingId].Invoke(element.bindingNode);
            }
        }

        public void OverrideSlot(string slotName, int slotTemplateId) {
            ref ContextEntry entry = ref contextStack.array[contextStack.size - 1];
            entry.overrides = entry.overrides ?? StructList<SlotOverride>.Get();
            entry.overrides.Add(new SlotOverride(slotName, root, currentTemplateData, slotTemplateId, SlotType.Override));
        }

        public void ForwardSlot(string slotName, int slotTemplateId) {
            ref ContextEntry entry = ref contextStack.array[contextStack.size - 1];
            entry.overrides = entry.overrides ?? StructList<SlotOverride>.Get();
            entry.overrides.Add(new SlotOverride(slotName, root, currentTemplateData, slotTemplateId, SlotType.Forward));
        }

        public void AddChild(UIElement child, int templateIndex) {
            UIElement lastElement = element;
            UIElement lastParent = parent;
            parent = element;
            element = child;
            currentTemplateData.elements[templateIndex](this);
            parent.children[parent.children.size++] = child;
            elementSystem.AddChild(parent.id, child.id);
            parent = lastParent;
            element = lastElement;
        }

        public void AddSlotChild(UIElement child, string slotName, int slotIndex) {
            ref ContextEntry entry = ref contextStack.array[contextStack.size - 1];
            UIElement lastElement = element;
            UIElement lastParent = parent;
            parent = element;
            element = child;

            parent.children[parent.children.size++] = child;

            bool found = false;
            if (entry.overrides != null) {
                for (int i = 0; i < entry.overrides.size; i++) {
                    ref SlotOverride slotOverride = ref entry.overrides.array[i];
                    if (slotOverride.slotName != slotName) {
                        continue;
                    }

                    found = true;
                    UIElement oldRoot = root;
                    TemplateData oldTemplateData = currentTemplateData;
                    currentTemplateData = slotOverride.templateData;
                    root = slotOverride.root;
                    ((UISlotDefinition) child).slotType = slotOverride.slotType;
                    currentTemplateData.elements[slotOverride.templateId](this);
                    root = oldRoot;
                    currentTemplateData = oldTemplateData;
                    break;
                }
            }

            if (!found) {
                currentTemplateData.elements[slotIndex](this);
                ((UISlotDefinition) child).slotType = SlotType.Define;
            }

            parent = lastParent;
            element = lastElement;

        }

        public void InitializeEntryPoint(UIElement entry, int attrCount, int childCount) {
            element = entry;
            root = entry;
            // todo -- elementSystem.CreateElement()
            element.attributes = new SizedArray<ElementAttribute>(attrCount);
            element.children = new LightList<UIElement>(childCount);
            element.bindingNode = new LinqBindingNode();
            element.bindingNode.root = element;
            element.bindingNode.parent = null;
            element.bindingNode.element = entry;
            element.flags = UIElementFlags.Alive | UIElementFlags.Enabled | UIElementFlags.AncestorEnabled;
            element.style = new UIStyleSet(element); // todo -- remove
        }

        // todo -- template origin info / id
        public void InitializeHydratedElement(int attrCount, int childCount) {
            element.flags |= UIElementFlags.TemplateRoot;
            InitializeElement(attrCount, childCount);

            contextStack.Push(new ContextEntry() {
                contextRoot = element,
                templateData = currentTemplateData
            });

        }

        public void InitializeElement(int attrCount, int childCount) {
            element.flags |= UIElementFlags.Alive;

            if ((parent.flags & UIElementFlags.EnabledFlagSet) == (UIElementFlags.EnabledFlagSet)) {
                element.flags |= UIElementFlags.Enabled | UIElementFlags.AncestorEnabled;
            }

            element.parent = parent;
            // todo -- template id / origin id / lexical id or whatever
            element.id = elementSystem.CreateElement(parent.hierarchyDepth + 1, -9999, -99999, element.flags);
            styleSystem.CreateElement(element.id);
            
            attributeSystem.InitializeAttributes(element.id, attrCount);
            
            // todo -- other systems will need to know that this element was created as well
            // would be REALLY nice to do this in bulk when we have all the data, explore this!
            // could do 2 passes, 1 to generate data, relationships and sizes, and a second one to fill with data?
            // element.attributes = new SizedArray<ElementAttribute>(attrCount);
            element.children = new LightList<UIElement>(childCount); // todo to sized array or block allocated range list
            element.layoutResult = new LayoutResult(element);
            element.bindingNode = new LinqBindingNode();
            element.bindingNode.element = element;
            element.bindingNode.root = root;
            // element.style = new UIStyleSet(element); // todo -- remove!

            onElementRegistered?.Invoke(element); // do this later in batches maybe? depends on when it must be called

            element.hierarchyDepth = parent.hierarchyDepth + 1;

        }

        public void InitializeSlotElement(int attrCount, int childCount, int contextDepth) {
            InitializeElement(attrCount, childCount);
            element.bindingNode.referencedContexts = new UIElement[contextDepth];
            UIElement ptr = root;
            // inverted?
            for (int i = 0; i < contextDepth; i++) {
                element.bindingNode.referencedContexts[i] = ptr;
                ptr = ptr.bindingNode.root;
            }
        }

        public void InvokeOnCreate() {
            try {
                element.OnCreate();
            }
            catch (Exception e) {
                Debug.Log(e); // todo -- diagnostics
            }
        }

        public void InvokeOnReady() {
            try {
                element.OnReady();
            }
            catch (Exception e) {
                Debug.Log(e); // todo -- diagnostics
            }
        }

        public void InvokeOnEnable() {
            try {
                element.OnEnable();
            }
            catch (Exception e) {
                Debug.Log(e); // todo -- diagnostics
            }
        }

        public void InitializeStaticAttribute(string key, string value) {
            element.attributes[element.attributes.size++] = new ElementAttribute(key, value);
        }

        // might do work for selector indexing later
        public void InitializeDynamicAttribute(string key) {
            element.attributes[element.attributes.size++] = new ElementAttribute(key, string.Empty);
        }

        public void AddMouseEventHandler(InputEventType eventType, KeyboardModifiers modifiers, bool requiresFocus, EventPhase phase, int index) {
            element.inputHandlers = element.inputHandlers ?? new InputHandlerGroup();
            element.inputHandlers.AddMouseEvent(eventType, modifiers, requiresFocus, phase, currentTemplateData.inputEventHandlers[index]);
        }

        public void AddKeyboardEventHandler(InputEventType eventType, KeyboardModifiers modifiers, bool requiresFocus, EventPhase phase, KeyCode keyCode, char character, int index) {
            element.inputHandlers = element.inputHandlers ?? new InputHandlerGroup();
            element.inputHandlers.AddKeyboardEvent(eventType, modifiers, requiresFocus, phase, keyCode, character, currentTemplateData.inputEventHandlers[index]);
        }

        public void AddDragEventHandler(InputEventType eventType, KeyboardModifiers modifiers, bool requiresFocus, EventPhase phase, int index) {
            element.inputHandlers = element.inputHandlers ?? new InputHandlerGroup();
            element.inputHandlers.AddDragEvent(eventType, modifiers, requiresFocus, phase, currentTemplateData.inputEventHandlers[index]);
        }

        public void AddDragCreateHandler(KeyboardModifiers modifiers, bool requiresFocus, EventPhase phase, int index) {
            element.inputHandlers = element.inputHandlers ?? new InputHandlerGroup();
            element.inputHandlers.AddDragCreator(modifiers, requiresFocus, phase, currentTemplateData.inputEventHandlers[index]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CreateBindingVariable<T>(int idx, string name) {
            element.bindingNode.variables[idx] = new BindingVariable<T>(name);
        }

        public void ReferenceBindingVariable<T>(int idx, string name) {
            element.bindingNode.variables[idx] = null; //new BindingVariable<T>(name);
            throw new NotImplementedException();
        }

        public void RegisterForKeyboardEvents() {
            // todo! application.InputSystem.RegisterKeyboardEvents(element);
        }

        private struct ContextEntry {

            public UIElement contextRoot;
            public TemplateData templateData;
            public StructList<SlotOverride> overrides;

        }

    }

}