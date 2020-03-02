using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace UIForia.Style {

    [Flags]
    public enum PropertyFlags : byte {

        None = 0,
        Inherited = 1 << 0,
        Animated = 1 << 1,
        BuiltIn = 1 << 2,
        RequireDestruction = 1 << 3

    }

    public struct Property {

        public readonly PropertyId id;
        private readonly PropertyData data;

        public Property(PropertyId propertyId, in PropertyData propertyData) {
            this.id = propertyId;
            this.data = propertyData;
        }
        
        public bool HasValue {
            get {
                if ((id.flags & PropertyFlags.RequireDestruction) != 0) {
                    return data.ptr == IntPtr.Zero;
                }
                else {
                    return data.int0 == int.MinValue && data.int1 == int.MinValue;
                }
            }
        }

    }

    [StructLayout(LayoutKind.Explicit)]
    public struct PropertyId {

        [FieldOffset(0)] public ushort id;
        [FieldOffset(3)] public PropertyFlags flags;

    }

    [StructLayout(LayoutKind.Explicit)]
    public struct PropertyData {

        [FieldOffset(0)] public readonly IntPtr ptr;
        [FieldOffset(0)] public readonly int int0;
        [FieldOffset(0)] public readonly float float0;
        [FieldOffset(4)] public readonly int int1;
        [FieldOffset(4)] public readonly float float1;

        public PropertyData(IntPtr ptr) {
            this.float0 = 0;
            this.int0 = 0;
            this.int1 = 0;
            this.float1 = 0;
            this.ptr = ptr;
        }
        
        // need to be careful with GC here, need to only call destroy on properties who allocated the handle, copying the handle should not involve a destroy call
         public PropertyData(object ptr) {
            this.float0 = 0;
            this.int0 = 0;
            this.int1 = 0;
            this.float1 = 0;
            this.ptr = GCHandle.Alloc(ptr, GCHandleType.Pinned).AddrOfPinnedObject();
         }

        public PropertyData(int intVal, float floatVal = 0) {
            this.ptr = default;
            this.float0 = 0;
            this.int0 = intVal;
            this.int1 = 0;
            this.float1 = floatVal;
        }

        public PropertyData(int val0, int val1) {
            this.ptr = default;
            this.float0 = 0;
            this.int0 = val0;
            this.float1 = 0;
            this.int1 = val1;
        }

        public PropertyData(float val0, int val1 = 0) {
            this.ptr = default;
            this.int0 = 0;
            this.float0 = val0;
            this.float1 = 0;
            this.int1 = val1;
        }

        public PropertyData(float val0, float val1) {
            this.ptr = default;
            this.int0 = 0;
            this.float0 = val0;
            this.int1 = 0;
            this.float1 = val1;
        }

        public Texture2D AsTexture => (Texture2D) GCHandle.FromIntPtr(ptr).Target;

    }

}