using System.Runtime.InteropServices;
using UIForia.Elements;

namespace UIForia {

    [StructLayout(LayoutKind.Sequential)]
    public struct ElementMetaInfo {

        public byte generation;
        public byte __padding__;
        public UIElementFlags flags;

    }

}