using System;
using System.Diagnostics;
using UnityEngine;

namespace UIForia {

    [DebuggerDisplay("{unit}({value})")]
    public struct UITimeMeasurement {

        public readonly float value;
        public readonly UITimeMeasurementUnit unit;

        public UITimeMeasurement(float value, UITimeMeasurementUnit unit = UITimeMeasurementUnit.Milliseconds) {
            this.value = value;
            this.unit = unit;
        }

        public float AsSeconds {
            get {
                switch (unit) {
                    case UITimeMeasurementUnit.Milliseconds: return value * 0.001f;
                    case UITimeMeasurementUnit.Percentage: throw new Exception("Cannot convert % to seconds.");

                    case UITimeMeasurementUnit.Unset:
                    case UITimeMeasurementUnit.Seconds:
                    default:
                        return value;
                }
            }
        }

        public float AsMilliseconds {
            get {
                switch (unit) {
                    case UITimeMeasurementUnit.Seconds: return value * 1000;
                    case UITimeMeasurementUnit.Percentage: throw new Exception("Cannot convert % to milliseconds.");

                    case UITimeMeasurementUnit.Unset:
                    case UITimeMeasurementUnit.Milliseconds:
                    default:
                        return value;
                }
            }
        }

        public int AsMillisecondsOrValue {
            get {
                switch (unit) {
                    case UITimeMeasurementUnit.Seconds: return (int)value * 1000;
                    case UITimeMeasurementUnit.Percentage: 
                    case UITimeMeasurementUnit.Unset:
                    case UITimeMeasurementUnit.Milliseconds:
                    default:
                        return (int)value;
                }
            }
        }

        public static bool operator ==(UITimeMeasurement a, UITimeMeasurement b) {
            return Mathf.Approximately(a.value, b.value) && a.unit == b.unit;
        }

        public static bool operator !=(UITimeMeasurement a, UITimeMeasurement b) {
            return !(a == b);
        }

        public override string ToString() {
            switch (unit) {
                case UITimeMeasurementUnit.Percentage:
                    return value * 100 + "%";

                case UITimeMeasurementUnit.Seconds:
                    return value + "s";

                default:
                case UITimeMeasurementUnit.Milliseconds:
                case UITimeMeasurementUnit.Unset:
                    return value + "ms";
            }
        }

        public bool Equals(UITimeMeasurement other) {
            return value.Equals(other.value) && unit == other.unit;
        }

        public override bool Equals(object obj) {
            return obj is UITimeMeasurement other && Equals(other);
        }

        public override int GetHashCode() {
            unchecked {
                return (value.GetHashCode() * 397) ^ (int) unit;
            }
        }

        public int ToMilliseconds(int durationMS) {
            switch (unit) {

                case UITimeMeasurementUnit.Percentage:
                    return (int)(value * (float)durationMS);

                case UITimeMeasurementUnit.Seconds:
                    return (int)(value / 1000f);

                default:
                case UITimeMeasurementUnit.Unset:
                case UITimeMeasurementUnit.Milliseconds:
                    return (int)value;
            }
        }

        public float ToPercentage(int durationMS) {
            switch (unit) {

                case UITimeMeasurementUnit.Percentage:
                    return value;

                case UITimeMeasurementUnit.Seconds:
                    return ((float)value * 1000f)/ (float)durationMS;

                default:
                case UITimeMeasurementUnit.Unset:
                case UITimeMeasurementUnit.Milliseconds:
                    return (float)value / (float)durationMS;
            }
        }

    }

}