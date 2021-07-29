using System;
using System.Runtime.InteropServices;
using UIForia.Text;

namespace UIForia.Layout {

    [AssertSize(16)]
    [StructLayout(LayoutKind.Explicit)]
    internal struct ShapeCacheKey : IEquatable<ShapeCacheKey> {

        [FieldOffset(0)] public int stringTag;
        [FieldOffset(4)] private ulong cmpValue;

        [FieldOffset(4)] public float fontSize;
        [FieldOffset(8)] public ushort fontId;
        [FieldOffset(10)] private ushort __unused0__;

        [FieldOffset(12)] private byte __unused1__;
        [FieldOffset(13)] public HarfbuzzScriptIdByte scriptByte;
        [FieldOffset(14)] public HarfbuzzDirectionByte directionByte;
        [FieldOffset(15)] public HarfbuzzLanguageIdByte languageIdByte;

        public bool Equals(ShapeCacheKey other) {
            return stringTag == other.stringTag && cmpValue == other.cmpValue;
        }

        public override bool Equals(object obj) {
            return obj is ShapeCacheKey other && Equals(other);
        }

        public override int GetHashCode() {
            unchecked {
                return (stringTag * 397) ^ cmpValue.GetHashCode();
            }
        }

    }

}