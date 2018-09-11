using System.Diagnostics;

namespace Rendering {

    public static class FloatUtil {

        public const float UnsetFloatValue = float.NaN;

        [DebuggerStepThrough]
        public static bool IsDefined(float value) {
            return !float.IsNaN(value);
        }
        
    }

}