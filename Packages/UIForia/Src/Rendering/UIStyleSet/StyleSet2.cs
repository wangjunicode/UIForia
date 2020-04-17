using System.Collections.Generic;
using UIForia.Elements;
using UIForia.Rendering;
using UIForia.Selectors;
using UIForia.Util;
using Unity.Collections;

namespace UIForia.Style {

    public class StyleSet2 {

        internal UIElement element;

        internal int traversalIndex;
        internal ushort splitIndex;
        internal ushort usageCount;
        internal StyleUsage[] styleUsages;
        internal StyleSystem2 styleSystem;
        internal StructList<StyleGroup> dynamicGroups;
        internal StyleSystem2.ChangeSet changeSet;
        internal StructList<int> affectedSelectorIds;
        internal StyleState activeStates;
        internal LightList<Selector> selectors;
        internal StructList<GucciSystem.SelectorEffect> selectorEffects;

        internal int baseStyleId;

        internal NativeArray<StyleUsage> usages2;

        public StyleSet2(StyleSystem2 styleSystem, UIElement element) {
            this.styleSystem = styleSystem;
            this.element = element;
        }

        internal void Destroy() {
            // style property needs some object data like strings. buuut i gotta not leak that
        }

        // todo -- dont release list in template compiler
        public void SetDynamicStyles(StructList<StyleGroup> styleGroupList) {
            if (dynamicGroups == null) {
                styleSystem.SetDynamicStyles(this, styleGroupList);
                return;
            }

            if (dynamicGroups.size != styleGroupList.size) {
                styleSystem.SetDynamicStyles(this, styleGroupList);
                return;
            }

            for (int i = 0; i < dynamicGroups.size; i++) {
                if (dynamicGroups.array[i].id != styleGroupList.array[i].id) {
                    styleSystem.SetDynamicStyles(this, styleGroupList);
                    return;
                }
            }

            styleGroupList.Release();
        }

        public void Initialize(StyleGroup styleGroup) {
            baseStyleId = styleGroup.id;

            int cnt = styleGroup.normal.properties.Length
                      + styleGroup.focus.properties.Length
                      + styleGroup.hover.properties.Length
                      + styleGroup.active.properties.Length;

            usageCount = (ushort) cnt;
            styleUsages = new StyleUsage[cnt + 2];

            ushort idx = 0;

            StylePriority priority = new StylePriority(SourceType.Shared, StyleState.Normal);
            for (int i = 0; i < styleGroup.normal.properties.Length; i++) {
                ref StyleUsage usage = ref styleUsages[idx++];
                usage.property = styleGroup.normal.properties[i];
                usage.sourceId.id = (ushort) styleGroup.id;
                usage.priority = priority;
            }

            splitIndex = idx;

            priority = new StylePriority(SourceType.Shared, StyleState.Hover);
            for (int i = 0; i < styleGroup.hover.properties.Length; i++) {
                ref StyleUsage usage = ref styleUsages[idx++];
                usage.property = styleGroup.hover.properties[i];
                usage.sourceId.id = (ushort) styleGroup.id;
                usage.priority = priority;
            }

            priority = new StylePriority(SourceType.Shared, StyleState.Active);
            for (int i = 0; i < styleGroup.active.properties.Length; i++) {
                ref StyleUsage usage = ref styleUsages[idx++];
                usage.property = styleGroup.active.properties[i];
                usage.sourceId.id = (ushort) styleGroup.id;
                usage.priority = priority;
            }

            priority = new StylePriority(SourceType.Shared, StyleState.Focused);
            for (int i = 0; i < styleGroup.focus.properties.Length; i++) {
                ref StyleUsage usage = ref styleUsages[idx++];
                usage.property = styleGroup.focus.properties[i];
                usage.sourceId.id = (ushort) styleGroup.id;
                usage.priority = priority;
            }
        }

        public static StylePriorityComparer s_StylePriorityComparer = new StylePriorityComparer();

        public class StylePriorityComparer : IComparer<StyleUsage> {

            public int Compare(StyleUsage x, StyleUsage y) {
                int diff = (int) x.property.propertyId - (int) y.property.propertyId;

                if (diff != 0) return diff;

                return x.priority.value - y.priority.value;
            }

        }

        public void SetStyleProperty(in StyleProperty property, StyleState state = StyleState.Normal) {
            styleSystem.SetInstanceProperty(this, property, state);
        }

        public StyleProperty GetProperty(StylePropertyId propertyId) {
            int num1 = 0;
            int num2 = splitIndex;

            int target = (int) propertyId;

            while (num1 <= num2) {
                int index1 = num1 + (num2 - num1 >> 1);

                int compareResult = (int) styleUsages[index1].property.propertyId - target;

                if (compareResult == 0) {
                    for (int i = index1 + 1; i < splitIndex; i++) {
                        if ((int) styleUsages[i].property.propertyId != target) {
                            return styleUsages[i - 1].property;
                        }
                    }

                    return styleUsages[index1].property;
                }

                if (compareResult < 0) {
                    num1 = index1 + 1;
                }
                else {
                    num2 = index1 - 1;
                }
            }

            return default;
        }

        public void RemoveSelector(int changeSelectorId) {
            if (selectors == null) return;
            for (int i = 0; i < selectors.size; i++) {
                if (selectors.array[i].id == changeSelectorId) {
                    selectors.SwapRemoveAt(i);
                    return;
                }
            }
        }

    }

}