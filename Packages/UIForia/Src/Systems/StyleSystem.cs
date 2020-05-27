using System;
using System.Collections.Generic;
using Src.Systems;
using UIForia.Elements;
using UIForia.Rendering;
using UIForia.Util;

namespace UIForia.Systems {

    public interface IStyleChangeHandler {

        void OnStylePropertyChanged(in StyleProperty property);

    }

    public class StyleSystem : IStyleSystem {

        private readonly Stack<UIElement> elementStack;
        private readonly StructList<ChangeSet> changeSets;

        public StyleSystem() {
            this.elementStack = new Stack<UIElement>(16);
            this.changeSets = new StructList<ChangeSet>();
        }

        public void FlushChangeSets(ElementSystem elementSystem, LayoutSystem layoutSystem, RenderSystem renderSystem) {
            
            // if disabled or destroyed, move on
            // if enabled this frame, move on

            for (int idx = 1; idx < changeSets.size; idx++) {
                ref ChangeSet changeSet = ref changeSets.array[idx];
                UIElementFlags flags = elementSystem.metaTable[changeSet.element.id].flags;
                if ((flags & UIElementFlags.EnabledFlagSet) != (UIElementFlags.EnabledFlagSet)) {
                    changeSet.element.style.changeSetId = 0;
                    changeSet.changes.size = 0;
                    changeSet.element = null;
                    changeSets[idx--] = changeSets[--changeSets.size];
                }

            }

            for (int changeId = 1; changeId < changeSets.size; changeId++) {
                
                ChangeSet changeSet = changeSets.array[changeId];

                layoutSystem.HandleStylePropertyUpdates(changeSet.element, changeSet.changes.array, changeSet.changes.size);
                renderSystem.HandleStylePropertyUpdates(changeSet.element, changeSet.changes.array, changeSet.changes.size);
                
                // this is really only for text, find a better way
                if (changeSet.element is IStyleChangeHandler changeHandler) {
                    StyleProperty[] properties = changeSet.changes.array;
                    int count = changeSet.changes.size;
                    for (int i = 0; i < count; i++) {
                        changeHandler.OnStylePropertyChanged(properties[i]);
                    }
                }

            }
            
            for (int changeId = 1; changeId < changeSets.size; changeId++) {
                ref ChangeSet changeSet = ref changeSets.array[changeId];
                changeSet.element.style.changeSetId = 0;
                changeSet.changes.size = 0;
                changeSet.element = null;
            }
            
            changeSets.QuickClear();
            changeSets.size = 1; // start at size 1
        }

        public void OnAttributeSet(UIElement element, string attributeName, string currentValue, string attributeValue) {
            element.style.UpdateApplicableAttributeRules();
        }

        // i need to know if an element had its enable state changed this frame
        private void AddToChangeSet(UIElement element, StyleProperty property) {

            if (element.style.changeSetId == 0) {
                element.style.changeSetId = changeSets.size;

                SizedArray<StyleProperty> changeList = new SizedArray<StyleProperty>(8);
                
                changeList.Add(property);
                
                changeSets.Add(new ChangeSet() {
                    element = element,
                    changes = changeList
                });
            }
            else {
                changeSets.array[element.style.changeSetId].changes.Add(property);
            }
        }

        public struct DeferredStyleInherit {

            public UIElement element;
            public StyleProperty property;

        }

        public void SetStyleProperty(UIElement element, StyleProperty property) {

            AddToChangeSet(element, property);

            // should probably just defer inheritance
            if (!StyleUtil.IsInherited(property.propertyId) || element.children == null || element.children.Count == 0) {
                return;
            }

            if (!property.hasValue) {

                UIElement ptr = element.parent;

                StyleProperty parentProperty = new StyleProperty(property.propertyId);

                while (ptr != null) {
                    parentProperty = ptr.style.GetPropertyValue(property.propertyId);
                    if (parentProperty.hasValue) {
                        break;
                    }

                    ptr = ptr.parent;
                }

                if (!parentProperty.hasValue) {
                    parentProperty = DefaultStyleValues_Generated.GetPropertyValue(property.propertyId);
                }

                property = parentProperty;
            }

            for (int i = 0; i < element.children.Count; i++) {
                elementStack.Push(element.children[i]);
            }

            while (elementStack.Count > 0) {
                UIElement descendant = elementStack.Pop();

                if (!descendant.style.SetInheritedStyle(property)) {
                    continue;
                }

                // todo -- we might want to cache font size lookups for em values, this would be the place 
                // if (property.propertyId == StylePropertyId.TextFontSize) {
                // do caching    
                // }

                AddToChangeSet(descendant, property);

                if (descendant.children == null) {
                    continue;
                }

                for (int i = 0; i < descendant.children.Count; i++) {
                    elementStack.Push(descendant.children[i]);
                }
            }
        }

        private struct ChangeSet {

            public UIElement element;
            public SizedArray<StyleProperty> changes;

        }

    }

}