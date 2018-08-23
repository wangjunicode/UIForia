using System.Diagnostics;
using Src.Extensions;
using UnityEngine;

namespace Rendering {

    public static class FloatUtil {

        public const float UnsetFloatValue = 3.402823E+38f - 1000;
        public const float UnsetFloatThreshold = 3.402823E+38f - 500f;

        [DebuggerStepThrough]
        public static bool IsDefined(float value) {
            return value < UnsetFloatThreshold;
        }
        
    }

    public static class IntUtil {

        public const int UnsetValue = int.MaxValue;
        
        [DebuggerStepThrough]
        public static bool IsDefined(int value) {
            return value < int.MaxValue;
        }

    }
    public static class ColorUtil {

        public static readonly Color UnsetColorValue = new Color(-1, -1, -1, -1);

        [DebuggerStepThrough]
        public static bool IsDefined(Color color) {
            return color.IsDefined();
        }
        
    }

}