using System.Diagnostics;
using Src.Extensions;
using UnityEngine;

namespace Src.Rendering {

    public static class ColorUtil {

        public static readonly Color UnsetValue = new Color(-1, -1, -1, -1);

        [DebuggerStepThrough]
        public static bool IsDefined(Color color) {
            return color.IsDefined();
        }
        
    }

}