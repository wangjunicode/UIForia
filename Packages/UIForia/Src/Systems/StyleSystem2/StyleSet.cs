using System;
using System.Collections.Generic;
using UIForia.Elements;
using UIForia.Style;
using UIForia.Util;

namespace UIForia {

    public unsafe struct StackLongBuffer16 {

        public int size;
        public fixed long array[16];

    }
    
    public unsafe struct StackLongBuffer64 {

        public int size;
        public fixed long array[64];

    }

    public class StyleSet {

        internal ushort stateAndSharedStyleChangeSetId;
        internal int changeSetId;
        internal UIElement element;
        internal StyleState2 state;
        public SizedArray<StyleId> sharedStyles; // maybe should be UnsafeList so it can be pointed to in a burst job
        public SizedArray<StyleProperty2> activeStyles;

        internal readonly VertigoStyleSystem styleSystem;

        internal StyleSet(UIElement element, VertigoStyleSystem styleSystem) {
            this.element = element;
            this.styleSystem = styleSystem;
            this.stateAndSharedStyleChangeSetId = ushort.MaxValue;
        }

        // if has implicit styles in target state, populate the properties list with them if not already taken
        internal void GetImplicitStyles(StyleState2 state, StructList<StyleProperty2> properties) { }

        internal StyleProperty2 GetDefaultOrInherited(PropertyId propertyId) {
            if ((propertyId.flags & PropertyFlags.Inherited) != 0) {
                return element.parent.styleSet2.GetValue(propertyId);
            }

            return default; // return DefaultStyleValues_Generated
        }

        private StyleProperty2 GetValue(PropertyId propertyId) {
            throw new NotImplementedException();
        }

        public void SetSharedStyles(IList<StyleId> styles) {
            if (styles.Count >= 16) {
                // todo -- diagnostic
            }

            unsafe {
                StackLongBuffer16 buffer = default;
                for (int i = 0; i < styles.Count; i++) {
                    buffer.array[buffer.size++] = styles[i];
                }

                styleSystem.SetSharedStyles(this, ref buffer);
            } 
        }

        internal unsafe void SetSharedStyles(ref StackLongBuffer16 styles) {
            // I'm not sure what the right way to handle duplicates is
            // honestly its probably best to just apply them twice and assume user error
            // might play animations twice or something but should be ok in most use cases
            // inspector would show the duplicate and maybe highlight it as a possible problem

            styleSystem.SetSharedStyles(this, ref styles);
        }

    }

}