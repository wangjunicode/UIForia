using System.Runtime.InteropServices;
using UIForia.Style;
using UIForia.Util;

namespace UIForia.Style2 {

    [StructLayout(LayoutKind.Explicit)]
    internal struct StyleBodyPart {

        [FieldOffset(0)] public readonly BodyPartType type;
        [FieldOffset(4)] public readonly StyleProperty2 property;
        [FieldOffset(4)] public readonly ReflessCharSpan stringData;
        [FieldOffset(4)] public readonly int start;
        [FieldOffset(8)] public readonly int end;
        [FieldOffset(12)] public readonly PropertyId propertyId;

        public StyleBodyPart(BodyPartType type, in StyleProperty2 property) : this() {
            this.type = type;
            this.property = property;
        }

        public StyleBodyPart(BodyPartType type, in ReflessCharSpan stringData) : this() {
            this.type = type;
            this.stringData = stringData;
        }

        public StyleBodyPart(BodyPartType type, int start, int end) : this() {
            this.type = type;
            this.start = start;
            this.end = end;
        }

        public StyleBodyPart(BodyPartType type, PropertyId propertyId, in ReflessCharSpan stringData) : this() {
            this.type = type;
            this.stringData = stringData;
            this.propertyId = propertyId;
        }

    }

}