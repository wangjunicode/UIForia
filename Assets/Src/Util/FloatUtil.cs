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
        public struct Reinterpret {

            [FieldOffset(0)] public readonly int intValue;
            [FieldOffset(0)] public readonly float floatValue;

            public Reinterpret(float floatValue) {
                intValue = 0;
                this.floatValue = floatValue;
            }

            public Reinterpret(int intValue) {
                floatValue = 0;
                this.intValue = intValue;
            }

        }
        
        public static int EncodeToInt(float value) {
            return new Reinterpret(value).intValue;
        }

        public static float DecodeToFloat(int value) {
            return new Reinterpret(value).floatValue;
        }

    }

}