using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UIForia.Extensions;
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
        [FieldOffset(4)] public int effectId;

        private static readonly Dictionary<string, TextSymbolType> s_SymbolMap = new Dictionary<string, TextSymbolType>();

        public static TextSymbolType GetOrCreateSymbolType(string symbolName) {
            if (s_SymbolMap.TryGetValue(symbolName, out TextSymbolType symbolType)) {
                return symbolType;
            }

            s_SymbolMap.Add(symbolName, (TextSymbolType) (256 + s_SymbolMap.Count));
            return (TextSymbolType) (256 + s_SymbolMap.Count);
        }

        public unsafe void SetData<T>(T data) where T : unmanaged {
            if (sizeof(T) > sizeof(BurstCharInfo)) {
                throw new Exception($"When setting symbol data the data struct can be no larger than {sizeof(BurstCharInfo)}. {typeof(T).GetTypeName()} is {sizeof(T)} bytes and cannot be used.");
            }

            charInfo = *(BurstCharInfo*) (void*) &data;
        }

        public unsafe void SetSymbolData<T>(uint symbolType, T data) where T : unmanaged {
            if (sizeof(T) > sizeof(BurstCharInfo)) {
                throw new Exception($"When setting symbol data the data struct can be no larger than {sizeof(BurstCharInfo)}. {typeof(T).GetTypeName()} is {sizeof(T)} bytes and cannot be used.");
            }

            if (symbolType < 256) {
                throw new Exception($"When defining your own symbol type, the symbolTypeId must be greater than 256. {symbolType} is not");
            }

            charInfo = *(BurstCharInfo*) (void*) &data;
        }

        public unsafe T GetData<T>() where T : unmanaged {
            if (sizeof(T) > sizeof(BurstCharInfo)) {
                return default;
            }

            // not sure how to avoid this copy since cant take address of struct member
            BurstCharInfo c = charInfo;
            return *(T*) (void*) &c;
        }

        internal bool ConvertToLayoutSymbol(out TextLayoutSymbol symbol) {
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
                    if ((uint) type > 256) {
                        foreach (KeyValuePair<string, TextSymbolType> kvp in s_SymbolMap) {
                            if (kvp.Value == type) {
                                return kvp.Key;
                            }
                        }

                        return "Unknown Symbol";
                    }
                    
                    return type.ToString();
            }
        }

    }

}