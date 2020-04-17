using System;
using UIForia.Elements;
using UIForia.Rendering;
using UIForia.Style;
using UIForia.Util;

namespace UIForia {

    public class StyleSet {

        internal int changeSetId;
        internal UIElement element;
        internal StyleState state;
        public SizedArray<int> sharedStyles;
        public SizedArray<StyleProperty2> activeStyles;

        internal StyleSet() {
            this.changeSetId = -1;
        }

        internal void GetImplicitStyles(StyleState state, StructList<StyleProperty2> properties) {
            // if has implicit styles in target state, populate the properties list with them if not already taken
        }

        internal StyleProperty2 GetDefaultOrInherited(PropertyId propertyId) {
            if ((propertyId.flags & PropertyFlags.Inherited) != 0) {
                return element.parent.styleSet2.GetValue(propertyId);
            }

            // return DefaultStyleValues_Generated
            return default;
        }

        private StyleProperty2 GetValue(PropertyId propertyId) {
            throw new NotImplementedException();
        }

    }

}