using System;
using UIForia.Compilers;
using UIForia.Elements;
using UIForia.Systems;
using UIForia.Util;
using UnityEngine.Assertions;

namespace UIForia {

    public class ElementSystem {

        private int idGenerator;

        private LightStack<ContextEntry> contextStack;
        private LightStack<UIElement> elementStack;
        
        private UIElement parent;
        private UIElement element;
        private TemplateData currentTemplateData;
        private UIElement root;

        public struct ContextEntry {

            public UIElement contextRoot;
            public TemplateData templateData;
            public StructList<SlotOverride> overrides;

        }

        internal ElementSystem() {
            this.contextStack = new LightStack<ContextEntry>(16);
            this.elementStack = new LightStack<UIElement>(32);
        }

        internal UIElement CreateEntryPoint<T>() where T : UIElement, new() {
            return null;
        }
        
        internal UIElement CreateEntryPoint(UIView window, TemplateData data) {
            currentTemplateData = data;
            
            UIElement retn = data.entry(this);

            window.dummyRoot.children.Add(retn);
            
            return retn;
        }

        internal UIElement CreateAndInsertChild() {
            return null;
        }

        internal UIElement CreateAndAppendChild() {
            return null;
        }

        public void InitializeHydratedElement(UIElement element, int slotCount, int referenceArraySize) {

            if (slotCount > 0) {
                contextStack.Push(new ContextEntry() {
                    contextRoot = element
                });
            }
            else {

                // todo -- maybe off by 1
                element.bindingNode.referencedContexts = new UIElement[referenceArraySize];

                int idx = 0;
                for (int i = contextStack.size - referenceArraySize; i < contextStack.size; i++) {
                    element.bindingNode.referencedContexts[idx++] = contextStack.array[i].contextRoot;
                }

                contextStack.Push(new ContextEntry() {
                    contextRoot = element,
                    overrides = StructList<SlotOverride>.Get()
                });

            }

        }

        public void HydrateEntryPoint() {

            contextStack.array[contextStack.size++] = new ContextEntry() {
                contextRoot = element,
                templateData = currentTemplateData
            };

            elementStack.Push(element);
            currentTemplateData.hydrate(this);
            elementStack.Pop();

            Assert.IsTrue(contextStack.size == 1);

            contextStack.array[--contextStack.size] = default;

        }

        public void HydrateElement(Type type) {

            GetTemplateData(type).hydrate(this);

            ContextEntry entry = contextStack.PopUnchecked();
            if (entry.overrides != null) {
                StructList<SlotOverride>.Release(ref entry.overrides);
            }
        }

        public void OverrideSlot(string slotName, int slotIndex, int slotTemplateId) {
            contextStack.array[contextStack.size - 1].overrides.array[slotIndex] = new SlotOverride(slotName, currentTemplateData.slots[slotTemplateId]);
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

        public TemplateData GetTemplateData(Type type) {
            throw new NotImplementedException();
        }


        public void AddChild(UIElement child, int templateIndex) {
            // might want to do do this after the child runs so any child query for parent data returns 0 children consistently
            parent.children[parent.children.size++] = child;
            elementStack.Push(child);
            UIElement lastElement = element;
            UIElement lastParent = parent;
            parent = element;
            element = child;
            currentTemplateData.elements[templateIndex](this); // might not need the child argument, not using the return value
            parent = lastParent;
            element = lastElement;
            elementStack.PopUnchecked();
        }

        public void AddHydratedChild(UIElement child, int templateIndex) {
            // might want to do do this after the child runs so any child query for parent data returns 0 children consistently
            parent.children[parent.children.size++] = child;
            elementStack.Push(child); // maybe don't need element stack? maybe only for debug
            UIElement lastElement = element;
            UIElement lastParent = parent;
            parent = element;
            element = child;
            currentTemplateData.elements[templateIndex](this);
            parent = lastParent;
            element = lastElement;
            elementStack.PopUnchecked();
        }

        public void AddSlotChild(UIElement element, string slotName, int slotIndex) {
            ContextEntry entry = contextStack.array[contextStack.size - 1];
            for (int i = 0; i < entry.overrides.size; i++) {
                ref SlotOverride slotOverride = ref entry.overrides.array[i];
                if (slotOverride.slotName == slotName) {
                    // todo -- might be wrong scope here
                    element.children.array[element.children.size++] = slotOverride.template(this, element);
                    return;
                }
            }

            element.children.array[element.children.size++] = currentTemplateData.slots[slotIndex](this, element);
        }

        public void InitializeEntryPoint(UIElement entry, int attrCount, int childCount) {
            element = entry;
            element.attributes = new StructList<ElementAttribute>(attrCount);
            element.children = new LightList<UIElement>(childCount);
            element.bindingNode = new LinqBindingNode();
            element.bindingNode.root = element;
            element.bindingNode.parent = null;
        }

        // todo -- add style init
        // todo -- finish this
        public void InitializeElement(int attrCount, int childCount) {
            element.parent = parent;
            element.bindingNode = new LinqBindingNode();
            element.bindingNode.root = root;
            element.id = idGenerator++;
            element.attributes = new StructList<ElementAttribute>(attrCount);
            element.children = new LightList<UIElement>(childCount);
        }

        public void InitializeSlot(int attrCount, int childCount) {
            element.parent = parent;
            element.bindingNode = new LinqBindingNode();
            // element.bindingNode.root = scope.rootRef;
            element.id = idGenerator++;
            element.attributes = new StructList<ElementAttribute>(attrCount);
            element.children = new LightList<UIElement>(childCount);
        }

        public void InitializeStaticAttribute(string key, string value) {
            element.attributes[element.attributes.size++] = new ElementAttribute(key, value);
        }

        // might do work for selector indexing later
        public void InitializeDynamicAttribute(string key) {
            element.attributes[element.attributes.size++] = new ElementAttribute(key, string.Empty);
        }

    }

}