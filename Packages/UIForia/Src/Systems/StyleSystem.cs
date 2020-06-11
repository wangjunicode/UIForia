using System;
using System.Collections.Generic;
using Src.Systems;
using UIForia.Elements;
using UIForia.Rendering;
using UIForia.Util;
using UnityEngine.Profiling;

namespace UIForia.Systems {

    public interface IStyleChangeHandler {

        void OnStylePropertyChanged(in StyleProperty property);

    }

    public class StyleSystem  {

        private readonly Stack<UIElement> elementStack;
        private readonly StructList<ChangeSet> changeSets;
        private ElementSystem elementSystem;
        
        public StyleSystem(ElementSystem elementSystem) {
            this.elementSystem = elementSystem;
            this.elementStack = new Stack<UIElement>(16);
            this.changeSets = new StructList<ChangeSet>();
        }

        public void FlushChangeSets(ElementSystem elementSystem, LayoutSystem layoutSystem, RenderSystem renderSystem) {
            Profiler.BeginSample("StyleSystem::FlushChangeSets");

            // if disabled or destroyed, move on
            // if enabled this frame, move on
            
            // todo -- dont even add things that were enabled / created this frame to the change set list

            for (int idx = 1; idx < changeSets.size; idx++) {

                ref ChangeSet changeSet = ref changeSets.array[idx];
                UIElementFlags flags = elementSystem.metaTable[changeSet.element.id].flags;

                if ((flags & UIElementFlags.EnabledFlagSet) != (UIElementFlags.EnabledFlagSet) || (flags & UIElementFlags.EnableStateChanged) != 0) {
                    changeSet.element.style.changeSetId = 0;
                    changeSet.changes.size = 0;
                    changeSet.element = null;
                    // need to swap or we kill our reference to the array 
                    ChangeSet tmp = changeSet;
                    int end = changeSets.size - 1;
                    changeSets[idx--] = changeSets[--changeSets.size];
                    changeSets.array[end] = tmp;
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

            changeSets.size = 1; // start at size 1. DO NOT CLEAR, keeping array references
            
            Profiler.EndSample();
        }

        // todo -- style attributes probably broken right now
        public void OnAttributeSet(UIElement element, string attributeName, string currentValue, string attributeValue) {
            element.style.UpdateApplicableAttributeRules();
        }

        // i need to know if an element had its enable state changed this frame
        private void AddToChangeSet(UIElement element, StyleProperty property) {

            int changeId = element.style.changeSetId;
            if (changeId == 0) {
                changeId = changeSets.size;
                element.style.changeSetId = changeId;
                if (changeSets.array.Length > changeId && changeSets.array[changeId].changes.array != null) {
                    changeSets.array[changeId].changes.Add(property);
                    changeSets.array[changeId].element = element;
                    changeSets.size++;
                }
                else {
                    SizedArray<StyleProperty> changeList = new SizedArray<StyleProperty>(8);
                    changeList.Add(property);
                    changeSets.Add(new ChangeSet() {
                        changes = changeList,
                        element = element
                    });
                }
            }
            else {
                changeSets.array[changeId].changes.Add(property);
            }
        }

        // sets property to either a default or inherited value
        public void UnsetStyleProperty(UIElement element, StyleProperty property) {
            
            UIElementFlags flags = elementSystem.metaTable[element.id].flags;

            if ((flags & UIElementFlags.EnabledFlagSet) == (UIElementFlags.EnabledFlagSet) &&  (flags & UIElementFlags.EnableStateChanged) == 0) {
                AddToChangeSet(element, property);
            }

            if (property.propertyId == StylePropertyId.TextFontSize) {
                elementSystem.emTable[element.id] = default;
            }
            
        }

        public void SetStyleProperty(UIElement element, StyleProperty property) {

            UIElementFlags flags = elementSystem.metaTable[element.id].flags;

            if ((flags & UIElementFlags.EnabledFlagSet) == (UIElementFlags.EnabledFlagSet) &&  (flags & UIElementFlags.EnableStateChanged) == 0) {
                AddToChangeSet(element, property);
            }
            
            if (property.propertyId == StylePropertyId.TextFontSize) {
                elementSystem.emTable[element.id].styleValue = property.AsUIFixedLength;
            }
            
            // should probably just defer inheritance
            if (!StyleUtil.IsInherited(property.propertyId) || element.ChildCount == 0) {
                return;
            }

            if (!property.hasValue) {

                UIElement ptr = element.parent;

                StyleProperty parentProperty = new StyleProperty(property.propertyId);

                while (ptr != null) {
                    parentProperty = ptr.style.GetPropertyValue(property.propertyId, out bool isDefault);
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

            UIElement p = element.GetFirstChild();
            
            while (p != null) {
                elementStack.Push(p);
                p = p.GetNextSibling();
            }
            
            while (elementStack.Count > 0) {
                UIElement descendant = elementStack.Pop();

                if (!descendant.style.SetInheritedStyle(property)) {
                    continue;
                }

                AddToChangeSet(descendant, property);

                p = descendant.GetFirstChild();
                
                while (p != null) {
                    elementStack.Push(p);
                    p = p.GetNextSibling();
                }

            }
        }

        private struct ChangeSet {

            public UIElement element;
            public SizedArray<StyleProperty> changes;

        }

    }

}