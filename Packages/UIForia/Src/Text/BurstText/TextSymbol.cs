using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UIForia.Extensions;
using UIForia.Graphics;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Text {

    public struct UnderlayInfo {

        private Color32 color;
        private float offsetX;
        private float offsetY;
        private ushort flags;
        private byte dilate;
        private byte softness;

        public void SetSoftness(byte softness) {
            this.softness = MathUtil.Float01ToByte(softness);
            flags |= 5;
        }

        public byte GetSoftness(byte softness) {
            return (flags & 5) == 0 ? softness : this.softness;
        }
        
        public void SetDilate(float dilate) {
            this.dilate = MathUtil.Float01ToByte(dilate);
            flags |= 4;
        }

        public byte GetDilate(byte dilate) {
            return (flags & 4) == 0 ? dilate : this.dilate;
        }

        public void SetOffsetX(float x) {
            offsetX = x;
            flags |= 2;
        }
        
        public void SetOffsetY(float y) {
            offsetY = y;
            flags |= 3;
        }

        public float GetOffsetX(float x) {
            return (flags & 2) == 0 ? x : offsetX;
        }
        
        public float GetOffsetY(float y) {
            return (flags & 3) == 0 ? y : offsetY;
        }
        
        public void SetColor(in Color32 color) {
            this.color = color;
            this.flags |= 1;
        }
        
        public Color32 GetColor(in Color32 defaultValue) {
            return (flags & 1) == 0 ? defaultValue : color;
        }
        
    }

    public struct TextEffectSymbolInfo {

        public int spawnerId;
        public int instanceId;
        public bool isActive;

    }

    [AssertSize(52)]
    [StructLayout(LayoutKind.Explicit)]
    [DebuggerDisplay("{DebuggerView()}")]
    public struct TextSymbol {

        [FieldOffset(0)] public TextSymbolType type;
        [FieldOffset(4)] public BurstCharInfo charInfo;
        [FieldOffset(4)] public UIFixedLength length;
        [FieldOffset(4)] public Color32 color;
        [FieldOffset(4)] public TextTransform textTransform;
        [FieldOffset(4)] public int fontId;
        [FieldOffset(4)] public TextEffectId effectId;
        [FieldOffset(4)] public float floatValue;
        [FieldOffset(4)] public UnderlayInfo underlay;
        [FieldOffset(4)] public TextureUsage textureSetup;
        [FieldOffset(4)] public TextEffectSymbolInfo effectInfo;

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