using System;
using System.Diagnostics;
using Src;

namespace Rendering {

    [Flags]
    public enum DimensionFields {

        Width = 1 << 0,
        Height = 1 << 1

    }

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