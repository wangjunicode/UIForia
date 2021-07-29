using UIForia.Layout;
using Unity.Mathematics;

namespace UIForia {

    public struct UIValue {

        internal double value;
        internal Unit unit;

        internal static UIValue FromEnum(int enumValue) {
            UIValue retn = new UIValue();
            retn.value = enumValue;
            retn.unit = Unit.Unset;
            return retn;
        }
        
        public UIValue(float value, Unit unit) {
            this.value = value;
            this.unit = unit;
        }

        public static implicit operator byte(UIValue value) {
            return value.unit == Unit.Unset ? (byte) value : (byte) 0;
        }

        public static implicit operator ushort(UIValue value) {
            return value.unit == Unit.Unset ? (ushort) value : (ushort) 0;
        }

        public static implicit operator short(UIValue value) {
            return value.unit == Unit.Unset ? (short) value : (short) 0;
        }

        public static implicit operator int(UIValue value) {
            return value.unit == Unit.Unset ? (int) value : 0;
        }

        public static implicit operator uint(UIValue value) {
            return value.unit == Unit.Unset ? (uint) value : 0;
        }

        public static implicit operator float(UIValue value) {
            return value.unit == Unit.Unset ? (float) value : 0;
        }

        public static implicit operator double(UIValue value) {
            return value.unit == Unit.Unset ? value : 0;
        }

        public static implicit operator half(UIValue value) {
            return (half) (value.unit == Unit.Unset ? (float) value : 0);
        }

        public static implicit operator UIFixedLength(UIValue length) {
            return new UIFixedLength((float) length.value, (UIFixedUnit) length.unit);
        }

        public static implicit operator UIValue(UIFixedLength length) {
            return new UIValue(length.value, (Unit) (int) length.unit);
        }

        public static implicit operator UIMeasurement(UIValue length) {
            return new UIMeasurement((float) length.value, (UIMeasurementUnit) length.unit);
        }

        public static implicit operator UIValue(UIMeasurement measurement) {
            return new UIValue(measurement.value, (Unit) (int) measurement.unit);
        }

        public static implicit operator UITimeMeasurement(UIValue length) {
            return new UITimeMeasurement((float) length.value, (UITimeMeasurementUnit) length.unit);
        }

        public static implicit operator UIValue(UITimeMeasurement measurement) {
            return new UIValue(measurement.value, (Unit) (int) measurement.unit);
        }

        public static implicit operator UIAngle(UIValue length) {
            return new UIAngle((float) length.value, (UIAngleUnit) length.unit);
        }

        public static implicit operator UIValue(UIAngle angle) {
            return new UIValue(angle.value, (Unit) (int) angle.unit);
        }

        public static implicit operator UIOffset(UIValue length) {
            return new UIOffset((float) length.value, (UIOffsetUnit) length.unit);
        }

        public static implicit operator UIValue(UIOffset offset) {
            return new UIValue(offset.value, (Unit) (int) offset.unit);
        }

    }

}