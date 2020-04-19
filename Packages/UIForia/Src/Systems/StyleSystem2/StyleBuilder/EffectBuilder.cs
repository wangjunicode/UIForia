using System;
using UIForia.Elements;
using UIForia.Style;
using UIForia.Util;

namespace UIForia {

    public class EffectBuilder {

        internal StyleEventFlags eventFlags;

        internal readonly StructList<StyleProperty2> properties;
        internal readonly StructList<EventHookDefinition> eventHooks;

        internal EffectBuilder() {
            properties = new StructList<StyleProperty2>();
            eventHooks = new StructList<EventHookDefinition>();
        }

        public void Clear() {
            eventFlags = 0;
            properties.size = 0;
            eventHooks.size = 0;
        }

        public void Set(StyleProperty2 property) {
            for (int i = 0; i < properties.size; i++) {
                if (properties.array[i].propertyId.id == property.propertyId.id) {
                    properties.array[i] = property;
                    return;
                }
            }

            properties.Add(property);
        }

        // todo -- be careful with this and user code, need to enqueue the user code ones to the end of the frame
        // maybe only accept strings that get parsed
        public void AddEventHook(StyleEvent type, Action<UIElement> action) {
            switch (type) {

                case StyleEvent.Enter:
                    eventFlags |= StyleEventFlags.HasEnterEvents;
                    break;

                case StyleEvent.Exit:
                    eventFlags |= StyleEventFlags.HasExitEvents;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            eventHooks.Add(new EventHookDefinition(type, action));
        }

    }

}