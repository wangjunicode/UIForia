using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UIForia.Compilers;
using UIForia.Elements;
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
        private UIView view;

        private TemplateData currentTemplateData;
        private Application application;

        private Action<UIElement> onElementRegistered;

        private ElementSystem elementSystem;

        // public TemplateSystem(ElementSystem elementSystem) {
        //     this.elementSystem = elementSystem;
        //     this.contextStack = new LightStack<ContextEntry>();
        // }

        // private StyleSystem2 styleSystem;
        // private AttributeSystem attributeSystem;

        // internal TemplateSystem(VertigoApplication application) {
        // this.application = application;
        // this.contextStack = new LightStack<ContextEntry>(16);
        // }

        private ApplicationSetup applicationSetup;

        internal TemplateSystem(Application application) {
            this.application = application;
            this.elementSystem = application.elementSystem;
        }

        internal void Initialize(ApplicationSetup applicationSetup) {
            this.applicationSetup = applicationSetup;
            this.contextStack = new LightStack<ContextEntry>();
            this.currentTemplateData = default;
            this.root = default;
            this.parent = default;
            this.element = default;
        }

        // todo -- prooobably need to some stack of contexts so we can create templates while creating templates
        public UIElement CreateEntryPoint(UIView view, Type currentType) {
            this.view = view;
            int idx = applicationSetup.typeTemplateMap[currentType];
            currentTemplateData = applicationSetup.templateData.array[idx];
            this.root = default;
            this.parent = default;
            this.element = default;
            UIElement retn = currentTemplateData.entry.Invoke(this);
            retn.style = new UIStyleSet(element); // todo -- new style system
            retn.application = application;
            return retn;
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

        // leaving type here for documentation reasons, the real data is taken from templateIndex
        public void HydrateElement(Type type, int templateIndex) {
            ref ContextEntry entry = ref contextStack.array[contextStack.size - 1];

            TemplateData oldTemplateData = currentTemplateData;

            currentTemplateData = applicationSetup.templateData.array[templateIndex];

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

        public void CreateChildrenIfEnabled(int childrenTemplateIndex) {
            if (element.isEnabled) {
                currentTemplateData.elements[childrenTemplateIndex](this);
            }
            else {
                element.flags |= UIElementFlags.HasDeferredChildrenCreation;
                // todo -- need to store this template id / reference somewhere to hydrate it. 
                // todo -- also need to actually invoke it somewhere 
            }
        }

        public void SetText(string value) {
            ((UITextElement) element).SetText(value);
        }

        public void SetBindings(int updateBindingId, int lateUpdateBindingId, int constBindingId, int enableBindingId, int bindingVariableCount) {
            // element.bindingNode.variables = bindingVariableCount != 0 ? new BindingVariable[bindingVariableCount] : default;
            // if (updateBindingId != -1) {
            //     element.bindingNode.updateBindings = currentTemplateData.bindings[updateBindingId];
            // }
            //
            // if (constBindingId != -1) {
            //     currentTemplateData.bindings[constBindingId].Invoke(element.bindingNode);
            // }
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
            
            elementSystem.AddChild(parent.id, child.id);

            parent = lastParent;
            element = lastElement;
        }

        public void InitializeEntryPoint(UIElement entry, int attrCount) {
            element = entry;
            root = entry;

            element.style = new UIStyleSet(element);
            element.bindingNode = new LinqBindingNode();
            element.bindingNode.root = element;
            element.bindingNode.parent = null;
            element.bindingNode.element = entry;

            element.flags = UIElementFlags.Alive | UIElementFlags.Enabled | UIElementFlags.AncestorEnabled;

            // todo -- template id / origin id / lexical id or whatever
            // todo -- entry point needs some love
            element.id = elementSystem.CreateElement(element, 0, -9999, -99999, element.flags);

            // attributeSystem.InitializeAttributes(element.id, attrCount);
            if (attrCount > 0) {
                element.attributes = new StructList<ElementAttribute>(attrCount);
            }

            onElementRegistered?.Invoke(element); // do this later in batches maybe? depends on when it must be called
        }

        public unsafe void InitializeHydratedElementDisabled(int attrCount) {
            InitializeHydratedElement(attrCount);
            ref ElementMetaInfo metaInfo = ref elementSystem.metaTable.array[element.id.id & ElementId.ENTITY_INDEX_MASK];
            element.flags &= ~UIElementFlags.Enabled;
            metaInfo.flags &= ~UIElementFlags.Enabled;
        }

        public unsafe void InitializeHydratedElement(int attrCount) {

            ref ElementMetaInfo metaInfo = ref elementSystem.metaTable.array[element.id.id & ElementId.ENTITY_INDEX_MASK];
            metaInfo.flags |= UIElementFlags.TemplateRoot;
            element.flags |= UIElementFlags.TemplateRoot;

            InitializeElement(attrCount);

            contextStack.Push(new ContextEntry() {
                contextRoot = element,
                templateData = currentTemplateData
            });
        }

        public unsafe void InitializeElementDisabled(int attrCount) {
            InitializeElement(attrCount);
            ref ElementMetaInfo metaInfo = ref elementSystem.metaTable.array[element.id.id & ElementId.ENTITY_INDEX_MASK];
            element.flags &= ~UIElementFlags.Enabled;
            metaInfo.flags &= ~UIElementFlags.Enabled;
        }

        public unsafe void InitializeElement(int attrCount) {
            const UIElementFlags defaultFlags = UIElementFlags.Enabled | UIElementFlags.Alive; // todo -- handle needs update, accept enabled as input | UIElementFlags.NeedsUpdate;

            // <Element create:disabled="true" create:children="lazy | eager" disable:destroy="children | self | descendents"/>

            element.application = application;
            element.View = view;
            element.parent = parent;

            int templateId = -1;
            element.id = elementSystem.CreateElement(element, parent.hierarchyDepth + 1, templateId, currentTemplateData.index, defaultFlags);
            element.flags = defaultFlags;
            element.hierarchyDepth = parent.hierarchyDepth + 1;

            ref ElementMetaInfo metaInfo = ref elementSystem.metaTable.array[element.id.id & ElementId.ENTITY_INDEX_MASK];
            element.isAncestorEnabled = parent.isEnabled;
            if (element.parent.isEnabled) {
                metaInfo.flags |= UIElementFlags.AncestorEnabled;
            }
            else {
                metaInfo.flags &= ~UIElementFlags.AncestorEnabled;
            }

            // styleSystem.CreateElement(element.id);

            if (attrCount > 0) {
                // todo -- attribute system
                element.attributes = new StructList<ElementAttribute>(attrCount);
                // attributeSystem.InitializeAttributes(element.id, attrCount);
            }

            // todo -- dont create if no bindings, oooor make it a struct table lookup
            element.style = new UIStyleSet(element); // todo -- new style system, make it a struct
            element.bindingNode = new LinqBindingNode();
            element.bindingNode.element = element;
            element.bindingNode.root = root;

            onElementRegistered?.Invoke(element); // do this later in batches maybe? depends on when it must be called, could also generate as part of template?

            // if element overrides create or needs create bindings -> generate call to create in template generation
        }

        public void InitializeSlotElement(int attrCount, int contextDepth) {
            InitializeElement(attrCount);
            element.bindingNode.referencedContexts = new UIElement[contextDepth];
            UIElement ptr = root;
            // inverted?

            // assign context by walking back up the root hierarchies
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
            // element.bindingNode.variables[idx] = new BindingVariable<T>(name);
        }

        public void ReferenceBindingVariable<T>(int idx, string name) {
            // element.bindingNode.variables[idx] = null; //new BindingVariable<T>(name);
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

    public struct SlotOverride {

        public readonly string slotName;
        public readonly int templateId;
        public readonly TemplateData templateData;
        public readonly SlotType slotType;
        public readonly UIElement root;

        public SlotOverride(string slotName, UIElement root, TemplateData templateData, int templateId, SlotType slotType) {
            this.slotName = slotName;
            this.root = root;
            this.templateData = templateData;
            this.templateId = templateId;
            this.slotType = slotType;
        }

    }

}