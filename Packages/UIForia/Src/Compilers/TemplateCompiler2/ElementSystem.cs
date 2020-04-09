using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UIForia.Compilers;
using UIForia.Elements;
using UIForia.Layout;
using UIForia.Systems;
using UIForia.Util;
using UnityEngine;
using UnityEngine.Assertions;

namespace UIForia {

    public class ElementSystem {

        private int idGenerator;

        private LightStack<ContextEntry> contextStack;

        private UIElement root;
        private UIElement parent;
        private UIElement element;
        private TemplateData currentTemplateData;
        private Dictionary<Type, TemplateData> templateDataMap;
        private Action<UIElement> onElementRegistered;
        internal readonly StructList<int> freeListIndex;
        internal readonly LightList<UIElement> elementMap;
        private int indexGenerator;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CreateBindingVariable<T>(int idx, string name) {
            element.bindingNode.variables[idx] = new BindingVariable<T>(name);
        }

        internal ElementSystem(Dictionary<Type, TemplateData> templateDataMap) {
            this.templateDataMap = templateDataMap;
            this.contextStack = new LightStack<ContextEntry>(16);
            this.freeListIndex = new StructList<int>(64);
            this.elementMap = new LightList<UIElement>(64);
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

            TemplateData oldTemplateData = currentTemplateData;

            templateDataMap.TryGetValue(type, out currentTemplateData);

            // ReSharper disable once PossibleNullReferenceException
            currentTemplateData.hydrate(this);

            currentTemplateData = oldTemplateData;

            // ContextEntry entry = contextStack.PopUnchecked();
            // if (entry.overrides != null) {
            // StructList<SlotOverride>.Release(ref entry.overrides);
            // }
        }

        public void SetText(string value) {
            ((UITextElement) element).SetText(value);
        }

        public void SetBindings(int updateBindingId, int lateUpdateBindingId, int bindingVariableCount) {
            element.bindingNode.variables = bindingVariableCount != 0 ? new BindingVariable[bindingVariableCount] : default;
        }

        public void OverrideSlot(string slotName, int slotTemplateId) {
            // contextStack.array[contextStack.size - 1].overrides.array[slotIndex] = new SlotOverride(slotName, currentTemplateData.slots[slotTemplateId]);
        }

        public void ForwardSlot(string slotName, int slotTemplateId) {
            // technically I shouldn't have to null or index check here since the code that is generated is always valid 
            // StructList<SlotOverride> parentOverrides = contextStack.array[contextStack.size - 2].overrides;
            //
            // for (int i = 0; i < parentOverrides.size; i++) {
            //     if (parentOverrides.array[i].slotName == slotName) {
            //         contextStack.array[contextStack.size - 1].overrides.array[slotIndex] = parentOverrides.array[i];
            //         return;
            //     }
            // }
            //
            // contextStack.array[contextStack.size - 1].overrides.array[slotIndex] = new SlotOverride(slotName, data.slots[slotTemplateId]);

        }

        public void AddChild(UIElement child, int templateIndex) {
            UIElement lastElement = element;
            UIElement lastParent = parent;
            parent = element;
            element = child;
            currentTemplateData.elements[templateIndex](this);
            parent.children[parent.children.size++] = child;
            parent = lastParent;
            element = lastElement;
        }

        public void AddSlotChild(UIElement child, string slotName, int slotIndex) {
            ContextEntry entry = contextStack.array[contextStack.size - 1];
            UIElement lastElement = element;
            UIElement lastParent = parent;
            parent = element;
            element = child;

            parent.children[parent.children.size++] = child;

            bool found = false;
            for (int i = 0; i < entry.overrides.size; i++) {
                ref SlotOverride slotOverride = ref entry.overrides.array[i];
                if (slotOverride.slotName == slotName) {
                    found = true;
                    slotOverride.template(this);
                    break;
                }
            }

            if (!found) {
                currentTemplateData.elements[slotIndex](this);
            }

            parent = lastParent;
            element = lastElement;

        }

        public void InitializeEntryPoint(UIElement entry, int attrCount, int childCount) {
            element = entry;
            element.attributes = new StructList<ElementAttribute>(attrCount);
            element.children = new LightList<UIElement>(childCount);
            element.bindingNode = new LinqBindingNode();
            element.bindingNode.root = element;
            element.bindingNode.parent = null;
        }
        
        public void InitializeElement(int attrCount, int childCount) {
            element.parent = parent;
            element.id = idGenerator++;
            element.index = freeListIndex.size > 0 ? freeListIndex.array[--freeListIndex.size] : indexGenerator++;
            element.attributes = new StructList<ElementAttribute>(attrCount); // todo to sized array
            element.children = new LightList<UIElement>(childCount);          // todo to sized array
            element.layoutResult = new LayoutResult(element);
            element.bindingNode = new LinqBindingNode();
            element.bindingNode.root = root;
            
            // todo -- template origin info / id
            if (element.index >= elementMap.array.Length) {
                elementMap.EnsureAdditionalCapacity(32);
            }
            
            elementMap.array[element.index] = element;
            
            onElementRegistered?.Invoke(element);
            
            if((parent.flags & UIElementFlags.EnabledFlagSet) == (UIElementFlags.EnabledFlagSet)) {
                element.flags |= UIElementFlags.AncestorEnabled;
            }
            else {
                element.flags &= ~UIElementFlags.AncestorEnabled;
            }
            
            element.hierarchyDepth = parent.hierarchyDepth + 1;

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
        
        private struct ContextEntry {

            public UIElement contextRoot;
            public TemplateData templateData;
            public SizedArray<SlotOverride> overrides;

        }

        public void ReferenceBindingVariable(int i, string name) {
            // search current template scope's context stack for variable with 'name'
            element.bindingNode.variables[i] = null;
        }

    }

}