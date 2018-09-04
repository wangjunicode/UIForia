using System.Diagnostics;

namespace Rendering {

    public static class FloatUtil {

        public const float UnsetFloatValue = 3.402823E+38f - 1000;
        public const float UnsetFloatThreshold = 3.402823E+38f - 500f;

        [DebuggerStepThrough]
        public static bool IsDefined(float value) {
            return value < UnsetFloatThreshold;
        }
        
    }

}