using UIForia.Style;
using UIForia.Util;

namespace UIForia {

    public class EffectBuilder {

        internal StructList<StyleProperty2> properties;

        public void Set(StyleProperty2 property) {
            properties = properties ?? new StructList<StyleProperty2>();
            for (int i = 0; i < properties.size; i++) {
                if (properties.array[i].propertyId.index == property.propertyId.index) {
                    properties.array[i] = property;
                    return;
                }
            }

            properties.Add(property);
        }

    }

}