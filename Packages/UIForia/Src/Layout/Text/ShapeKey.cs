using System;
using System.Runtime.InteropServices;

namespace UIForia.Text {

    [StructLayout(LayoutKind.Explicit)]
    internal struct ShapeKey : IEquatable<ShapeKey> {

        [FieldOffset(0)] public long v0;
        [FieldOffset(8)] public long v1;

        [FieldOffset(0)] public ShapeKeyData key;

        public bool Equals(ShapeKey other) {
            return v0 == other.v0 && v1 == other.v1;
        }

    }
}