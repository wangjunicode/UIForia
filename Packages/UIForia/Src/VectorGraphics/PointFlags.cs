using System;

namespace Packages.UIForia.Src.VectorGraphics {

    [Flags]
    public enum PointFlags {

        Corner = 0x01,
        Left = 0x02,
        Bevel = 0x04,
        InnerBevel = 0x08,

    };

}