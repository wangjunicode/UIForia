using System.Diagnostics;
using Src;

namespace Rendering {

    [DebuggerDisplay("{x}, {y}, {width}, {height}")]
    public struct LayoutRect {

        public UIMeasurement x;
        public UIMeasurement y;
        public UIMeasurement width;
        public UIMeasurement height;
        
        public LayoutRect(UIMeasurement x, UIMeasurement y, UIMeasurement width, UIMeasurement height) {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }
        
    }

}