using System.Runtime.InteropServices;

namespace UIForia.Text {

    [StructLayout(LayoutKind.Sequential)]
    internal struct ShapingCharacterRun {

        public int shapeIndex;
        public ushort fontId;
        public ushort __unused__;
        public float fontSize; // could be ems -> needs resolving. encode as negative if ems or 0 to restore default font size from style
        
        public int start;
        public int length;
        
        public CharacterRunFlags flags;
        public HarfbuzzDirectionByte directionByte;
        public HarfbuzzScriptIdByte scriptIdByte;
        public HarfbuzzLanguageIdByte languageIdByte;

    }

}