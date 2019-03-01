using System.Diagnostics;
using UIForia.Extensions;
using UnityEngine;

namespace UIForia.Util {

    public static class ColorUtil {

        public static readonly Color UnsetValue = new Color(-1, -1, -1, -1);

        [DebuggerStepThrough]
        public static bool IsDefined(Color color) {
            return color.IsDefined();
        }
        
    }

}