using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UIForia.Elements;
using UIForia.Style;
using UIForia.Util;

namespace UIForia {

    public unsafe struct StackLongBuffer8 {

        public int size;
        public fixed long array[8];

    }

    [AssertSize(16)]
    public unsafe struct StyleSetInstanceData {

        public int changeSetId;
        public int propertyCount;
        public StyleProperty2* properties;

    }

    // current size = 64 bytes
    [AssertSize(64)]
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct StyleSetData {

        public ushort changeSetId;
        public ushort instanceDataId;
        public int selectorDataId;
        public StyleState2Byte state;           
        public byte sharedStyleCount;       
        public fixed int sharedStyles[StyleSet.k_MaxSharedStyles];

    }

    public unsafe class StyleSet {

        // could denormalize `state` to be always up to do date here and just handle style application separately
        
        internal UIElement element;
        public const int k_MaxSharedStyles = 7;
        
        internal int styleDataId; // if i convert stylesystem to use paged list then this can be a pointer because pages won't ever reallocate. for now it must be an index because the backing list can resize, invalidating a pointer
        internal readonly VertigoStyleSystem styleSystem;

        internal StyleSet(UIElement element, VertigoStyleSystem styleSystem) {
            this.element = element;
            this.styleSystem = styleSystem;
            this.styleDataId = styleSystem.CreatesStyleData(element.id);
        }

        public StyleState2 state {
            get => styleSystem.GetState(styleDataId);
        }
        
        internal StyleProperty2 GetDefaultOrInherited(PropertyId propertyId) {
            if ((propertyId.typeFlags & PropertyTypeFlags.Inherited) != 0) {
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
                StackLongBuffer8 buffer = default;
                for (int i = 0; i < styles.Count; i++) {
                    buffer.array[buffer.size++] = styles[i];
                }

                styleSystem.SetSharedStyles(this, ref buffer);
            } 
        }

        internal unsafe void SetSharedStyles(ref StackLongBuffer8 styles) {
            // I'm not sure what the right way to handle duplicates is
            // honestly its probably best to just apply them twice and assume user error
            // might play animations twice or something but should be ok in most use cases
            // inspector would show the duplicate and maybe highlight it as a possible problem

            styleSystem.SetSharedStyles(this, ref styles);
        }

        public void SetStyleProperty(StyleProperty2 property, StyleState2 state = StyleState2.Normal) {
            // if setting to same value we still do work
            // if setting the same property twice we take the last one
            styleSystem.SetInstanceStyle(this, property, (StyleState2Byte)state);
        }

        public void RemoveStyleProperty(PropertyId propertyId, StyleState2 state = StyleState2.Normal) {
            
            StyleProperty2 property = new StyleProperty2(propertyId, default) {
                remove = true
            };
            
            styleSystem.SetInstanceStyle(this, property, (StyleState2Byte)state);
        }

    }

}