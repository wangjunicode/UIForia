using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Rendering {

    public static class FloatUtil {

        public const float UnsetValue = float.NaN;

        [DebuggerStepThrough]
        public static bool IsDefined(float value) {
            return !float.IsNaN(value);
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct Conversion {

            [FieldOffset(0)] public readonly int intValue;
            [FieldOffset(0)] public readonly float floatValue;

            public Conversion(float floatValue) {
                intValue = 0;
                this.floatValue = floatValue;
            }

            public Conversion(int intValue) {
                floatValue = 0;
                this.intValue = intValue;
            }

        }
        
        public static int EncodeToInt(float value) {
            return new Conversion(value).intValue;
        }

        public static float DecodeToFloat(int value) {
           return new Conversion(value).floatValue;
        }

    }

}