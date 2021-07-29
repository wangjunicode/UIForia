using System;
using System.Diagnostics;
using UnityEngine;

namespace UIForia {

    public enum UIAngleUnit : byte {

        Degrees = Unit.Degrees,
        Percent = Unit.Percent,
        Radians = Unit.Radians

    }

    public struct UIAngle : IEquatable<UIAngle> {

        public readonly float value;
        public readonly UIAngleUnit unit;

        [DebuggerStepThrough]
        public UIAngle(float value, UIAngleUnit unit = UIAngleUnit.Degrees) {
            this.value = value;
            this.unit = unit;
        }

        public UIAngle ToDegrees() {
            switch (unit) {

                default:
                case UIAngleUnit.Degrees:
                    return this;

                case UIAngleUnit.Percent:
                    return new UIAngle((value * 360f));

                case UIAngleUnit.Radians:
                    return new UIAngle((value * Mathf.Rad2Deg));

            }
        }

        public UIAngle ToRadians() {
            switch (unit) {

                default:
                case UIAngleUnit.Degrees:
                    return new UIAngle(value * Mathf.Deg2Rad, UIAngleUnit.Radians);

                case UIAngleUnit.Percent:
                    return new UIAngle(value * (2 * Mathf.PI), UIAngleUnit.Radians);

                case UIAngleUnit.Radians:
                    return this;
            }
        }

        public UIAngle ToPercent() {
            switch (unit) {

                default:
                case UIAngleUnit.Degrees:
                    return new UIAngle(value / 360f, UIAngleUnit.Percent);

                case UIAngleUnit.Percent:
                    return this;

                case UIAngleUnit.Radians:
                    return new UIAngle(value / (2 * Mathf.PI), UIAngleUnit.Percent);
            }
        }

        public static bool operator ==(UIAngle self, UIAngle other) {
            return self.value == other.value && self.unit == other.unit;
        }

        public static bool operator !=(UIAngle self, UIAngle other) {
            return !(self == other);
        }
        
        public bool Equals(UIAngle other) {
            return ((float.IsNaN(value) && float.IsNaN(other.value)) || value == other.value && unit == other.unit);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            return obj is UIAngle length && Equals(length);
        }

        public override int GetHashCode() {
            unchecked {
                return (value.GetHashCode() * 397) ^ (int) unit;
            }
        }

        public static implicit operator UIAngle(int value) {
            return new UIAngle(value, UIAngleUnit.Degrees);
        }

        public static implicit operator UIAngle(float value) {
            return new UIAngle(value, UIAngleUnit.Degrees);
        }

        public static implicit operator UIAngle(double value) {
            return new UIAngle((float) value, UIAngleUnit.Degrees);
        }

        public static UIAngle Percent(float value) {
            return new UIAngle(value, UIAngleUnit.Percent);
        }

        public static UIAngle Degrees(float value) {
            return new UIAngle(value, UIAngleUnit.Degrees);
        }

        public static UIAngle Radians(float value) {
            return new UIAngle(value, UIAngleUnit.Radians);
        }

        public override string ToString() {
            return $"{value} {unit}";
        }

    }

}