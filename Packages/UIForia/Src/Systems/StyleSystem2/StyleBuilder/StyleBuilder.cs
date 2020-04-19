using System;
using UIForia.Elements;
using UIForia.Selectors;
using UIForia.Style;
using UIForia.Util;

namespace UIForia {

    public class StyleBuilder {

        internal StyleGroupBuilder normalGroup;
        internal StyleGroupBuilder hoverGroup;
        internal StyleGroupBuilder focusGroup;
        internal StyleGroupBuilder activeGroup;

        public void Set(StyleProperty2 property) {
            normalGroup = normalGroup ?? new StyleGroupBuilder(StyleState2.Normal);
            normalGroup.Set(property);
        }

        public void AddGroup(StyleState2 state, Action<StyleGroupBuilder> builder) {

            if (builder == null) return;

            switch (state) {

                case StyleState2.Normal:
                    normalGroup = normalGroup ?? new StyleGroupBuilder(state);
                    builder.Invoke(normalGroup);
                    break;

                case StyleState2.Hover:
                    hoverGroup = hoverGroup ?? new StyleGroupBuilder(state);
                    builder.Invoke(hoverGroup);
                    break;

                case StyleState2.Focused:
                    focusGroup = focusGroup ?? new StyleGroupBuilder(state);
                    builder.Invoke(focusGroup);
                    break;
                
                case StyleState2.Active:
                    activeGroup = activeGroup ?? new StyleGroupBuilder(state);
                    builder.Invoke(activeGroup);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        private static int HandleGroup(StyleGroupBuilder builder, StyleState2 state, ref StyleState2 definedStates, ref StyleState2 hasSelectorsPerState) {

            if (builder == null) {
                return 0;
            }

            definedStates |= state;

            if (builder.selectors != null) {
                hasSelectorsPerState |= state;
            }

            return builder.properties?.size ?? 0;

        }

        internal void Build(VertigoStyleSheet styleSheet) {

            StyleState2 definedStates = default;
            StyleState2 hasSelectorsPerState = default;

            ushort styleIndex = (ushort) styleSheet.styles.size;
            
            int normalCount = HandleGroup(normalGroup, StyleState2.Normal, ref definedStates, ref hasSelectorsPerState);
            int hoverCount = HandleGroup(hoverGroup, StyleState2.Hover, ref definedStates, ref hasSelectorsPerState);
            int focusCount = HandleGroup(focusGroup, StyleState2.Focused, ref definedStates, ref hasSelectorsPerState);
            int activeCount = HandleGroup(activeGroup, StyleState2.Active, ref definedStates, ref hasSelectorsPerState);
            
            styleSheet.propertyList.EnsureAdditionalCapacity(normalCount + hoverCount + focusCount + activeCount);

            int selectorCount = 0;
            int eventCount = 0;
            
            int selectorOffset = styleSheet.selectors.size;
            int eventOffset = styleSheet.eventList.size;
            
            HandleSelectors(styleSheet, styleIndex, normalGroup, ref selectorCount, ref eventCount);
            HandleSelectors(styleSheet, styleIndex, hoverGroup, ref selectorCount, ref eventCount);
            HandleSelectors(styleSheet, styleIndex, focusGroup, ref selectorCount, ref eventCount);
            HandleSelectors(styleSheet, styleIndex, activeGroup, ref selectorCount, ref eventCount);

            if (selectorCount == 0) selectorOffset = 0;
            if (eventCount == 0) eventCount = 0;
            
            VertigoStyle style = new VertigoStyle(
                new StyleId(styleSheet.id, new LocalStyleId(styleIndex, definedStates, hasSelectorsPerState)),
                (ushort)styleSheet.propertyList.size,
                (ushort)normalCount,
                (ushort)hoverCount,
                (ushort)focusCount,
                (ushort)activeCount,
                (ushort)selectorOffset,
                (ushort)selectorCount,
                (ushort)eventOffset,
                (ushort)eventCount
            );
            
            WriteProperties(styleSheet.propertyList, normalGroup);
            WriteProperties(styleSheet.propertyList, hoverGroup);
            WriteProperties(styleSheet.propertyList, focusGroup);
            WriteProperties(styleSheet.propertyList, activeGroup);

            WriteEventHooks(ref styleSheet.eventList, normalGroup);
            WriteEventHooks(ref styleSheet.eventList, hoverGroup);
            WriteEventHooks(ref styleSheet.eventList, focusGroup);
            WriteEventHooks(ref styleSheet.eventList, activeGroup);

            normalGroup?.Clear();
            hoverGroup?.Clear();
            focusGroup?.Clear();
            activeGroup?.Clear();

            styleSheet.styles.Add(style);
            
        }

        private static void HandleSelectors(VertigoStyleSheet styleSheet, ushort styleIndex, StyleGroupBuilder builder, ref int selectorCount, ref int eventCount) {
            if (builder?.selectors == null || builder.selectors.size == 0) {
                return;
            }

            selectorCount++;
            
            for (int i = 0; i < builder.selectors.size; i++) {

                ref VertigoSelectorDefinition currentSelector = ref builder.selectors.array[i];

                StyleEventFlags styleEventFlags = currentSelector.selectorFlags;

                VertigoSelector selector = new VertigoSelector {
                    id = new SelectorId(styleSheet.id.index, styleIndex, (ushort) styleSheet.selectors.size, builder.state, styleEventFlags), 
                    queryId = styleSheet.selectorQueries.size, 
                    target = currentSelector.target
                };

                if (currentSelector.properties != null && currentSelector.properties.size != 0) {
                    selector.propertyOffset = styleSheet.propertyList.size;
                    selector.propertyCount = currentSelector.properties.size;
                    styleSheet.propertyList.AddRange(currentSelector.properties);
                    currentSelector.properties.Release();
                }

                if (currentSelector.events != null && currentSelector.events.size != 0) {
                    eventCount++;
                    styleSheet.eventList.EnsureAdditionalCapacity(currentSelector.events.size);
                    selector.eventOffset = styleSheet.eventList.size;
                    selector.eventCount = currentSelector.events.size;
                    currentSelector.events.Release();
                }
                
                styleSheet.selectors.Add(selector);
            }
            
        }

        private static void WriteEventHooks(ref SizedArray<EventHook> eventHookList, StyleGroupBuilder builder) {
            if (builder?.eventHooks != null) {
                eventHookList.EnsureCapacity(eventHookList.size + builder.eventHooks.size);
                for (int i = 0; i < builder.eventHooks.size; i++) {
                    // eventHookList.array[eventHookList.size++] = builder.eventHooks; // todo -- conversion here
                }
            }
        }

        private static void WriteProperties(StructList<StyleProperty2> propertyList, StyleGroupBuilder builder) {
            if (builder?.properties != null) {
                propertyList.AddRange(builder.properties);
            }
        }

        public void AddEventHook(StyleEvent enter, Action<UIElement> action) {
            normalGroup = normalGroup ?? new StyleGroupBuilder(StyleState2.Normal);
            normalGroup.AddEventHook(enter, action);
        }

        public void AddSelector(FromTarget fromTarget, Func<UIElement, bool> query, Action<EffectBuilder> effectBuilder) {
            normalGroup = normalGroup ?? new StyleGroupBuilder(StyleState2.Normal);
            normalGroup.AddSelector(fromTarget, query, effectBuilder);
        }

    }

}