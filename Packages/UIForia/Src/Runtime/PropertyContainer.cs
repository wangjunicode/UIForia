using System.Runtime.InteropServices;

namespace UIForia.Style {

    [StructLayout(LayoutKind.Explicit)]
    internal unsafe partial struct PropertyContainer {

        [FieldOffset(0)] public ElementId elementId;
        [FieldOffset(4)] public ushort propertyIndex;
        [FieldOffset(6)] public ushort variableNameId;

        //  [FieldOffset(8)] internal fixed byte bytes[64];

        public PropertyId PropertyId => (PropertyId) propertyIndex;

        public void Set<T>(T value) where T : unmanaged {
            fixed (void* ptr = bytes) {
                T* castBytes = (T*) ptr;
                *castBytes = value;
            }
        }

        public T Get<T>() where T : unmanaged {
            fixed (void* ptr = bytes) {
                return *(T*) ptr;
            }
        }

    }

}