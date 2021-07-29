using System.Diagnostics;
using System.Runtime.InteropServices;
using UIForia.Layout;

namespace UIForia.Text {

    [StructLayout(LayoutKind.Sequential)]
    internal struct TextCharacterRun {

        public int tagId; // 3 bytes is fine here 
        public float fontSize; // could be ems -> needs resolving. encode as negative if ems or 0 to restore default font size from style
        public ushort fontId;
        public CharacterRunFlags flags;
        public HarfbuzzDirectionByte directionByte;
        public HarfbuzzScriptIdByte scriptIdByte;
        public HarfbuzzLanguageIdByte languageIdByte;
        
        public float x;
        public float y;
        public float width;
        public float height;
        
        public ushort lineIndex;
        public ushort utilValue; // combine vAlign here? 
        public VerticalAlignment verticalAlignment;
        public int shapeResultIndex; //3  bytes is fine 

        public ushort length;
        
        public ushort GlyphCount {
            get => utilValue;
            set => utilValue = value;
        }

        public ushort StretchParts {
            get => utilValue;
            set => utilValue = value;
        }
        
        public ShapingCharacterRun GetShapingRunInfo(int shapeIndex, int start, int length) {
            
            return new ShapingCharacterRun() {
                fontId = fontId,
                flags = flags,
                start = start,
                length = length,
                directionByte = directionByte,
                fontSize = fontSize,
                languageIdByte = languageIdByte,
                scriptIdByte = scriptIdByte,
                shapeIndex = shapeIndex
            };
            
        }
        
        public bool IsWhiteSpace => (flags & CharacterRunFlags.Whitespace) != 0;

        public bool IsNewLine => (flags & CharacterRunFlags.NewLine) != 0;

        public bool IsCharacterRun => (flags & CharacterRunFlags.Characters) != 0;
        
        public bool IsRendered => GetRunType() == CharacterRunType.Characters;
        
        public bool IsLastInGroup => (flags & CharacterRunFlags.LastInGroup) != 0;

        public int NewLineCount => IsNewLine ? utilValue : 0;

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