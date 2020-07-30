using System;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;

namespace UIForia.Text {

    [Flags]
    public enum CharacterFlags : ushort {

        UnderlineStart = 1 << 0,
        UnderlineMid = 1 << 1,
        UnderLineEnd = 1 << 2,
        // Renderable = 1 << 3,

        Italic = 1 << 4,
        Bold = 1 << 5,
        Subscript = 1 << 6,
        SuperScript = 1 << 7,

        UseEffectInfo = 1 << 8,

        Visible = 1 << 9, // for non whitespace
        Enabled = 1 << 10, // pretend this symbol doesnt exist

        InvertLeftRightUV = 1 << 11,
        InvertTopBottomRightUV = 1 << 12

    }

    [Flags]
    public enum CharacterDisplayFlags : byte {

        InvertHorizontalUV = 1 << 0,
        InvertVerticalUV = 1 << 1,
        Bold = 1 << 2,
        Italic = 1 << 3,
        UnderlayInner = 1 << 4,
        
        InvertUVs = InvertHorizontalUV | InvertVerticalUV

    }

    [Flags]
    public enum RenderCharacterFlags : ushort {

        Character = 1 << 0,
        Sprite = 1 << 1,

        StrikeStart = 1 << 2,
        StrikeMid = 1 << 3,
        StrikeEnd = 1 << 4,

        UnderlineStart = 1 << 5,
        UnderlineMid = 1 << 6,
        UnderlineEnd = 1 << 7,

    }

    [StructLayout(LayoutKind.Sequential)]
    public struct BurstCharInfo {

        // this is the local position in a word, useful so I dont need to re-measure word offsets after layout changes
        public float2 position;
        public float2 renderPosition; // actual rendering position

        public int character;

        public CharacterFlags flags;
        public ushort wordIndex;

        public ushort glyphIndex;
        public ushort lineIndex; // probably good to keep, but really I just need to know if one char is a different line than the next

        // public float layoutWidthScale; // currently unused, want to use for animation. could be 8.8 fixed point with ushort, dont need precision
        // public float maxAscender;
        public float scale;

        public ushort fontAssetId; // needed to handle glyph lookup and fallback font resolution
        public ushort materialIndex;
        public ushort baseMaterialIndex;

        public byte opacityMultiplier;
        public CharacterDisplayFlags displayFlags;
        public int nextRenderIdx; // could be ushort if I treat this as an offset and not an index
        public int effectIdx;

    }

    // [StructLayout(LayoutKind.Sequential)]
    // public struct RenderedCharacterInfo {
    //
    //     public float2 position;
    //     public RenderCharacterFlags typeFlags;
    //     public byte opacityMultiplier;
    //     public RenderCharacterDisplayFlags displayFlags;
    //     public uint renderedGlyphIndex;
    //     public int effectIndex;
    //     public int symbolIdx;
    //     public int materialIndex;
    //     public float lineAscender;
    //     public float width;
    //     public float height;
    //
    // }

    [AssertSize(64)]
    [StructLayout(LayoutKind.Explicit)]
    public struct TextEffectInfo {

        [FieldOffset(0)] public float4 topLeftData;
        [FieldOffset(0)] public float3 topLeft;
        [FieldOffset(12)] public float scaleTopLeft;

        [FieldOffset(16)] public float4 topRightData;
        [FieldOffset(16)] public float3 topRight;
        [FieldOffset(28)] public float scaleTopRight;

        [FieldOffset(32)] public float4 bottomRightData;
        [FieldOffset(32)] public float3 bottomRight;
        [FieldOffset(44)] public float scaleBottomRight;

        [FieldOffset(48)] public float4 bottomLeftData;
        [FieldOffset(48)] public float3 bottomLeft;
        [FieldOffset(60)] public float scaleBottomLeft;

    }

}