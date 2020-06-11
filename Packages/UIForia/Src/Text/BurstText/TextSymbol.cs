using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine;

namespace UIForia.Text {

     [StructLayout(LayoutKind.Explicit)]
     [DebuggerDisplay("{DebuggerView()}")]
    public struct TextSymbol {

        [FieldOffset(0)] public TextSymbolType type;
        [FieldOffset(4)] public BurstCharInfo charInfo;
        [FieldOffset(4)] public UIFixedLength length;
        [FieldOffset(4)] public Color32 color;
        [FieldOffset(4)] public TextTransform textTransform;
        [FieldOffset(4)] public int fontId;

        public bool ConvertToLayoutSymbol(out TextLayoutSymbol symbol) {
            switch (type) {

                default:
                case TextSymbolType.Character:
                case TextSymbolType.NoBreakPush:
                case TextSymbolType.NoBreakPop:
                case TextSymbolType.ColorPush:
                case TextSymbolType.ColorPop:
                case TextSymbolType.TextTransformPush:
                case TextSymbolType.TextTransformPop:
                    symbol = default;
                    return false;

                case TextSymbolType.HorizontalSpace:
                    symbol = new TextLayoutSymbol() {
                        type = TextLayoutSymbolType.HorizontalSpace,
                        space = length
                    };
                    return true;

            }
        }

        public string DebuggerView() {
            switch (type) {

                case TextSymbolType.Character:
                    return "Character = " + (char) charInfo.character;

                case TextSymbolType.NoBreakPush:
                    return "No Break - Push";

                case TextSymbolType.NoBreakPop:
                    return "No Break - Pop";

                case TextSymbolType.HorizontalSpace:
                    return "Horizontal Space " + length;

                case TextSymbolType.ColorPush:
                    return "Color - Push (" + color + ")";

                case TextSymbolType.ColorPop:
                    return "Color - Pop";

                case TextSymbolType.TextTransformPush:
                    return "Text Transform - Push (" + textTransform + ")";

                case TextSymbolType.TextTransformPop:
                    return "Text Transform - Pop";

                default:
                    return type.ToString();
            }
        }

    }

}