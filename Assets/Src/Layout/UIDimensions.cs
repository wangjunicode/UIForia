using System.Diagnostics;
using Src;

namespace Rendering {

    [DebuggerDisplay("{width}, {height}")]
    public struct Dimensions {


        public UIMeasurement width;
        public UIMeasurement height;

        public Dimensions(UIMeasurement width, UIMeasurement height) {
            this.width = width;
            this.height = height;
        }

        public static Dimensions Unset => new Dimensions(UIMeasurement.Unset, UIMeasurement.Unset);

        public bool IsDefined() {
            return width.IsDefined() && height.IsDefined();
        }

    }

}