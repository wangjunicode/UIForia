using System;
using UIForia.Elements;
using UIForia.Selectors;
using UIForia.Style;
using UIForia.Util;

namespace UIForia {

    public struct EventHookDefinition {

        public StyleEvent type;
        public Action<UIElement> action;

        public EventHookDefinition(StyleEvent type, Action<UIElement> action) {
            this.type = type;
            this.action = action;
        }

    }

    public struct VertigoSelectorDefinition {

        public FromTarget target;
        public Func<UIElement, bool> filter;
        public StructList<StyleProperty2> properties;
        public StructList<EventHookDefinition> events;
        public StyleEventFlags selectorFlags;

    }

    public class StyleGroupBuilder : EffectBuilder {

        internal StyleState2 state;
        internal StructList<VertigoSelectorDefinition> selectors;
        private readonly EffectBuilder builder;
        
        internal StyleGroupBuilder(StyleState2 state) {
            this.state = state;
            this.builder = new EffectBuilder();
        }

        public void AddSelector(FromTarget fromTarget, Func<UIElement, bool> filter, Action<EffectBuilder> action) {
            action?.Invoke(builder);
            selectors = selectors ?? new StructList<VertigoSelectorDefinition>();

            StructList<StyleProperty2> propertyResult = null;
            StructList<EventHookDefinition> styleEventResult = null;

            if (builder.properties.size != 0) {
                propertyResult = StructList<StyleProperty2>.Get();
                propertyResult.AddRange(builder.properties);
            }

            if (builder.eventHooks.size != 0) {
                styleEventResult = StructList<EventHookDefinition>.Get();
                styleEventResult.AddRange(builder.eventHooks);
            }

            builder.Clear();
            
            selectors.Add(new VertigoSelectorDefinition() {
                filter = filter,
                target = fromTarget,
                properties = propertyResult,
                events = styleEventResult
            });
        }

    }

}