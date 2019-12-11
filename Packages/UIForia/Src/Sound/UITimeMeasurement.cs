using System.Diagnostics;

namespace UIForia.Sound {
    [DebuggerDisplay("{unit}({value})")]
    public struct UITimeMeasurement {
        public readonly float value;
        public readonly UITimeMeasurementUnit unit;

        public UITimeMeasurement(float value, UITimeMeasurementUnit unit = UITimeMeasurementUnit.Milliseconds) {
            this.value = value;
            this.unit = unit;
        }
    }
}
