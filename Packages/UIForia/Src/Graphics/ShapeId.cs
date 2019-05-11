using System.Runtime.InteropServices;

namespace Vertigo {

    [StructLayout(LayoutKind.Explicit)]
    public struct ShapeId {

        [FieldOffset(0)] public int id;
        [FieldOffset(0)] public ushort originId;
        [FieldOffset(2)] public ushort index;

        public ShapeId(ushort contextId, ushort shapeIndex) {
            this.id = 0;
            this.index = shapeIndex;
            this.originId = contextId;
        }

    }

}