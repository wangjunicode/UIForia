using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UIForia.Layout;
using UIForia.Style;
using Unity.Mathematics;

namespace UIForia {

    [StructLayout(LayoutKind.Explicit)]
    internal struct InterpolatedStyleValue {

        [FieldOffset(0)] public InterpolatedMeasurement measurement;

        public static InterpolatedStyleValue FromTTEMPLATE(TTEMPLATE next, TTEMPLATE prev, float f) {
            throw new NotImplementedException();
        }

        public static InterpolatedStyleValue FromUIMeasurement(UIMeasurement next, UIMeasurement prev, float t) {
            return new InterpolatedStyleValue() {
                measurement = new InterpolatedMeasurement() {
                    nextUnit_unsolved = next.unit,
                    prevUnit_unsolved = prev.unit,
                    nextValue = next.value,
                    prevValue = prev.value,
                    t = t,
                }
            };

        }

        public static InterpolatedStyleValue FromAspectRatio(AspectRatio next, AspectRatio prev, float f) {
            throw new NotImplementedException();
        }

        public static InterpolatedStyleValue FromByte(byte next, byte prev, float f) {
            throw new NotImplementedException();
        }

        public static InterpolatedStyleValue FromInt32(int next, int prev, float f) {
            throw new NotImplementedException();
        }

        public static InterpolatedStyleValue FromPainterId(PainterId next, PainterId prev, float f) {
            throw new NotImplementedException();
        }

        public static InterpolatedStyleValue FromUIOffset(UIOffset next, UIOffset prev, float f) {
            throw new NotImplementedException();
        }

        public static InterpolatedStyleValue FromUInt16(ushort next, ushort prev, float f) {
            throw new NotImplementedException();
        }

        public static InterpolatedStyleValue Fromhalf(half next, half prev, float f) {
            throw new NotImplementedException();
        }

        public static InterpolatedStyleValue FromUIAngle(UIAngle next, UIAngle prev, float f) {
            throw new NotImplementedException();
        }

        public static InterpolatedStyleValue FromUIColor(UIColor next, UIColor prev, float f) {
            throw new NotImplementedException();
        }

        public static InterpolatedStyleValue FromUIFontSize(UIFontSize next, UIFontSize prev, float f) {
            throw new NotImplementedException();
        }

        public static InterpolatedStyleValue FromGridItemPlacement(GridItemPlacement next, GridItemPlacement prev, float f) {
            throw new NotImplementedException();
        }

        public static InterpolatedStyleValue FromUISpaceSize(UISpaceSize next, UISpaceSize prev, float f) {
            throw new NotImplementedException();
        }

        public static InterpolatedStyleValue FromUISizeConstraint(UISizeConstraint next, UISizeConstraint prev, float f) {
            throw new NotImplementedException();
        }

        public static InterpolatedStyleValue FromTextureInfo(TextureInfo next, TextureInfo prev, float f) {
            throw new NotImplementedException();
        }

        public static InterpolatedStyleValue FromUIFixedLength(UIFixedLength next, UIFixedLength prev, float f) {
            throw new NotImplementedException();
        }

        public static InterpolatedStyleValue FromGridLayoutTemplate(GridLayoutTemplate next, GridLayoutTemplate prev, float f) {
            throw new NotImplementedException();
        }

        public static InterpolatedStyleValue FromFontAssetId(FontAssetId next, FontAssetId prev, float f) {
            throw new NotImplementedException();
        }

    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct InterpolatedMeasurement {

        [FieldOffset(0)] public UIMeasurementUnit prevUnit_unsolved;
        [FieldOffset(0)] public SolvedSizeUnit prevUnit_solved;
        [FieldOffset(2)] public UIMeasurementUnit nextUnit_unsolved;
        [FieldOffset(2)] public SolvedSizeUnit nextUnit_solved;
        [FieldOffset(4)] public float prevValue;
        [FieldOffset(8)] public float nextValue;
        [FieldOffset(12)] public float t;

    }

    [DebuggerDisplay("{unit}({value})")]
    public struct UIMeasurement : IEquatable<UIMeasurement> {

        public readonly float value;
        public readonly UIMeasurementUnit unit;

        [DebuggerStepThrough]
        public UIMeasurement(float value, UIMeasurementUnit unit = UIMeasurementUnit.Pixel) {
            this.unit = unit;
            this.value = value;
        }

        [DebuggerStepThrough]
        public UIMeasurement(int value, UIMeasurementUnit unit = UIMeasurementUnit.Pixel) {
            this.unit = unit;
            this.value = value;
        }

        [DebuggerStepThrough]
        public UIMeasurement(double value, UIMeasurementUnit unit = UIMeasurementUnit.Pixel) {
            this.unit = unit;
            this.value = (float) value;
        }

        public bool Equals(UIMeasurement other) {
            return value.Equals(other.value) && unit == other.unit;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            return obj is UIMeasurement a && Equals(a);
        }

        public override int GetHashCode() {
            unchecked {
                return (value.GetHashCode() * 397) ^ (int) unit;
            }
        }

        public static bool operator ==(UIMeasurement self, UIMeasurement other) {
            if (float.IsNaN(self.value) && float.IsNaN(other.value)) {
                return self.unit == other.unit;
            }

            return self.value == other.value && self.unit == other.unit;
        }

        public static bool operator !=(UIMeasurement self, UIMeasurement other) {
            return !(self == other);
        }

        public static implicit operator UIMeasurement(int value) {
            return new UIMeasurement(value, UIMeasurementUnit.Pixel);
        }

        public static implicit operator UIMeasurement(float value) {
            return new UIMeasurement(value, UIMeasurementUnit.Pixel);
        }

        public static implicit operator UIMeasurement(double value) {
            return new UIMeasurement((float) value, UIMeasurementUnit.Pixel);
        }

        public override string ToString() {
            return value.ToString() + " " + unit.ToString();
        }

        public static string ToStyleString(UIMeasurement measurement) {

            string unit;

            switch (measurement.unit) {

                default:

                case UIMeasurementUnit.Unset:
                case UIMeasurementUnit.Pixel:
                    unit = "px";
                    break;
                
                case UIMeasurementUnit.LineHeight:
                    unit = "lh";
                    break;

                case UIMeasurementUnit.Content:
                    unit = "cnt";
                    break;

                case UIMeasurementUnit.ViewportWidth:
                    unit = "vw";
                    break;

                case UIMeasurementUnit.ViewportHeight:
                    unit = "vh";
                    break;

                case UIMeasurementUnit.Em:
                    unit = "em";
                    break;

                case UIMeasurementUnit.Percent:
                    unit = "%";
                    break;

                case UIMeasurementUnit.Stretch:
                    unit = "s";
                    break;

                case UIMeasurementUnit.StretchContent:
                    unit = "strcnt";
                    break;

                case UIMeasurementUnit.FitContent:
                    unit = "fitcnt";
                    break;

                case UIMeasurementUnit.BackgroundImageWidth:
                    unit = "bw";
                    break;

                case UIMeasurementUnit.BackgroundImageHeight:
                    unit = "bh";
                    break;

                case UIMeasurementUnit.ApplicationWidth:
                    unit = "aw";
                    break;

                case UIMeasurementUnit.ApplicationHeight:
                    unit = "ah";
                    break;

                case UIMeasurementUnit.MaxChild:
                    unit = "mx";
                    break;

                case UIMeasurementUnit.MinChild:
                    unit = "mn";
                    break;

                case UIMeasurementUnit.ParentSize:
                    unit = "psz";
                    break;

                case UIMeasurementUnit.FillRemaining:
                    unit = "fr";
                    break;
            }

            return measurement.value + unit;
        }

    }

}