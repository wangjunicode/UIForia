using System;
using System.Collections.Generic;
using System.Diagnostics;
using Boo.Lang.Runtime;
using UnityEngine;

namespace UIForia.Sound {
    [DebuggerDisplay("{unit}({value})")]
    public struct UITimeMeasurement {
        public readonly float value;
        public readonly UITimeMeasurementUnit unit;

        public UITimeMeasurement(float value, UITimeMeasurementUnit unit = UITimeMeasurementUnit.Milliseconds) {
            this.value = value;
            this.unit = unit;
        }

        public float AsSeconds() {
            switch (unit) {
                case UITimeMeasurementUnit.Milliseconds: return value * 0.001f;
                case UITimeMeasurementUnit.Percentage: throw new RuntimeException("Cannot convert % to seconds.");
                case UITimeMeasurementUnit.Unset:
                case UITimeMeasurementUnit.Seconds:
                default:
                    return value;
            }
        }

        public float AsMilliseconds() {
            switch (unit) {
                case UITimeMeasurementUnit.Seconds: return value * 1000;
                case UITimeMeasurementUnit.Percentage: throw new RuntimeException("Cannot convert % to milliseconds.");
                case UITimeMeasurementUnit.Unset:
                case UITimeMeasurementUnit.Milliseconds:
                default:
                    return value;
            }
        }

        public static bool operator ==(UITimeMeasurement a, UITimeMeasurement b) {
            return Mathf.Approximately(a.value, b.value) && a.unit == b.unit;
        }
        
        public static bool operator !=(UITimeMeasurement a, UITimeMeasurement b) {
            return !(a == b);
        }
    }
}
