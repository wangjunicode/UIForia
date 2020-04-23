using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Unity.Collections;

namespace UIForia.Style {

    public enum PropertyInfoFlags : ushort {
        Remove,
        State
    }
    
    internal struct StylePropertyInfo {

        public ushort id;
        public PropertyTypeFlags typeFlags;
        public PropertyInfoFlags infoFlags;
        public ushort padding;

    }
    
    [AssertSize(16)]
    [DebuggerDisplay("id = {PropertyParsers.s_PropertyNames[propertyId.index]}")]
    [StructLayout(LayoutKind.Explicit)]
    public partial struct StyleProperty2 : IEquatable<StyleProperty2> {

        // important! do not change these fields outside of constructors
        [FieldOffset(0)] public readonly PropertyId propertyId;
        [FieldOffset(4)] internal StyleState2Byte state;
        [FieldOffset(5)] internal bool remove;
        [FieldOffset(6)] internal byte padding0;
        [FieldOffset(7)] internal byte padding1;
        [FieldOffset(8)] internal readonly int int0;
        [FieldOffset(8)] internal readonly float float0;
        [FieldOffset(8)] internal readonly IntPtr ptr;
        [FieldOffset(12)] internal readonly int int1;
        [FieldOffset(12)] internal readonly float float1;
        
        // still have 4 free bytes that are implicitly padding!

        // public StyleProperty2(PropertyId propertyId, object ptr) {
        //     this.propertyId = propertyId;
        //     this.float0 = 0;
        //     this.int0 = 0;
        //     this.int1 = 0;
        //     this.float1 = 0;
        //     this.ptr = GCHandle.Alloc(ptr, GCHandleType.Pinned).AddrOfPinnedObject(); // todo -- need a way to track that we allocated this -- copying struct will copy flag but we dont want that
        //     // this.propertyId.flags |= PropertyFlags.RequireDestruction; 
        // }

        public StyleProperty2(PropertyId propertyId, int intVal, float floatVal = 0) : this() {
            this.propertyId = propertyId;
            this.int0 = intVal;
            this.float1 = floatVal;
        }

        public StyleProperty2(PropertyId propertyId, IntPtr ptr) : this() {
            this.propertyId = propertyId;
            this.ptr = ptr;
        }
        
        public StyleProperty2(PropertyId propertyId, int val0, int val1) : this() {
            this.propertyId = propertyId;
            this.int0 = val0;
            this.int1 = val1;
        }

        public StyleProperty2(PropertyId propertyId, float val0, int val1 = 0) : this() {
            this.propertyId = propertyId;
            this.float0 = val0;
            this.int1 = val1;
        }

        public StyleProperty2(PropertyId propertyId, float val0, float val1) : this() {
            this.propertyId = propertyId;
            this.float1 = val1;
        }

        public static bool operator ==(in StyleProperty2 a, in StyleProperty2 b) {
            return a.propertyId.index == b.propertyId.index && a.int0 == b.int0 && a.int1 == b.int1;
        }

        public static bool operator !=(in StyleProperty2 a, in StyleProperty2 b) {
            return a.propertyId.index != b.propertyId.index || a.int0 != b.int0 || a.int1 != b.int1;
        }

        public bool Equals(StyleProperty2 b) {
            return propertyId.index == b.propertyId.index && int0 == b.int0 && int1 == b.int1;
        }

        public override bool Equals(object obj) {
            return obj is StyleProperty2 other && Equals(other);
        }

        public override int GetHashCode() {
            unchecked {
                return (propertyId.index * 397) ^ ptr.GetHashCode();
            }
        }

    }
    

}