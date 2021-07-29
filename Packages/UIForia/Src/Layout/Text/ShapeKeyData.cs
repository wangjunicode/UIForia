using System.Runtime.InteropServices;

namespace UIForia.Text {
    [StructLayout(LayoutKind.Sequential, Size = 16)]
    internal struct ShapeKeyData {

        public ushort fontId;
        public HarfbuzzDirectionByte direction; // default ltr
        public HarfbuzzScriptIdByte scriptId;
        public HarfbuzzLanguageIdByte languageId;
        public float size;

    }
}