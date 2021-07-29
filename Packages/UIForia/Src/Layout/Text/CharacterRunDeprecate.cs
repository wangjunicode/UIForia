using System.Diagnostics;
using System.Runtime.InteropServices;
using UIForia.Layout;

namespace UIForia.Text {

    [AssertSize(24)]
    [StructLayout(LayoutKind.Sequential)]
    internal struct CharacterRunDeprecate {

        public int tagId; // 3 bytes is fine here 
        public ushort fontId;
        public ushort length;
        public float fontSize; // could be ems -> needs resolving. encode as negative if ems or 0 to restore default font size from style
        public int start;
        public CharacterRunFlags flags;
        public HarfbuzzDirectionByte directionByte;
        public HarfbuzzScriptIdByte scriptIdByte;
        public HarfbuzzLanguageIdByte languageIdByte;

        public bool IsWhiteSpace => (flags & CharacterRunFlags.Whitespace) != 0;

        public bool IsNewLine => (flags & CharacterRunFlags.NewLine) != 0;

        public bool IsLastInGroup {
            get => (flags & CharacterRunFlags.LastInGroup) != 0;
        }
        
        public bool IsRendered => GetRunType() == CharacterRunType.Characters;
        
        public bool IsCharacterRun => (flags & CharacterRunFlags.Characters) != 0;

        public int NewLineCount => IsNewLine ? length : 0;

        [DebuggerStepThrough]
        public ShapeCacheKey GetCacheKey() {
            return new ShapeCacheKey() {
                directionByte = directionByte,
                fontId = fontId,
                fontSize = fontSize,
                scriptByte = scriptIdByte,
                stringTag = tagId,
                languageIdByte = languageIdByte
            };
        }

        public CharacterRunType GetRunType() {

            // todo -- handle other cases 
            if ((flags & CharacterRunFlags.NewLine) != 0) {
                return CharacterRunType.NewLine;
            }

            if ((flags & CharacterRunFlags.Whitespace) != 0) {
                return CharacterRunType.Whitespace;
            }
            
            if ((flags & CharacterRunFlags.HorizontalSpace) != 0) {
                return CharacterRunType.HorizontalSpace;
            }
            
            if ((flags & CharacterRunFlags.InlineSpace) != 0) {
                return CharacterRunType.ReserveSpace;
            }
            
            return CharacterRunType.Characters;
        }

    }

}