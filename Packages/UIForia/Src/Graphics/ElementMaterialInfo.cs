using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;

namespace UIForia.Graphics {

    [AssertSize(48)]
    [StructLayout(LayoutKind.Explicit)]
    public struct UIForiaMaterialInfo {

        [FieldOffset(0)] public TextMaterialInfo textMaterial;
        [FieldOffset(0)] public ElementMaterialInfo elementElementMaterial;

    }
    
    ///<summary>
    /// must be aligned on 16 byte boundaries for shader performance
    /// </summary>
    [AssertSize(48)]
    [StructLayout(LayoutKind.Sequential)]
    public struct TextMaterialInfo {

        public Color32 faceColor;
        public Color32 outlineColor;
        public Color32 glowColor;
        public Color32 underlayColor;

        public byte outlineWidth;
        public byte outlineSoftness;
        public byte underlayDilate;
        public byte underlaySoftness;

        public float underlayX;
        public float underlayY;

        public byte glowOffset;
        public byte glowPower;
        public byte glowInner;
        public byte glowOuter;

        public float alphaClip;

        public float zPosition;
        public float opacity;

        public ushort scale;
        public ushort weight;

    }

    ///<summary>
    /// must be aligned on 16 byte boundaries for shader performance
    /// </summary>
    [AssertSize(48)]
    [StructLayout(LayoutKind.Sequential)]
    public struct ElementMaterialInfo {

        public Color32 backgroundColor;
        
        public float2 size;
        public float zPosition;

        public Color32 backgroundTint;
        public Color32 outlineColor;
        public Color32 outlineTint;

        // float4 hdrIntensities?
        public byte radius0;
        public byte radius1;
        public byte radius2;
        public byte radius3;

        public byte bevel0;
        public byte bevel1;
        public byte bevel2;
        public byte bevel3;

        public float opacity;
        public float outlineWidth;
        public ColorMode bodyColorMode;
        public ColorMode outlineColorMode;

        // 2 bytes free here 
        public ushort padding;

    }

}