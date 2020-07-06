using System.Runtime.InteropServices;
using UIForia.Systems;
using UIForia.Text;
using Unity.Mathematics;
using UnityEngine;
using FontStyle = UIForia.Text.FontStyle;

namespace UIForia.Graphics {

    public enum TextUVMapping : byte {

        Character,
        Line,
        Word,
        Bounds

    }

    internal unsafe struct DeferredTextInfo {

        public int renderCallId;
        public int localDrawIdx;
        public float4x4* matrix;
        public int materialOverrideCount;
        public MaterialPropertyOverride* materialPropertyOverrides;
        public OverflowBounds overflowBounds;
        public DrawInfoFlags flags;
        public TextInfo* textInfo;

    }

    public struct TextStyleBuffer {

        public int faceTextureId;
        public int outlineTextureId;
        
        public Color32 outlineColor;
        public Color32 glowColor;
        public Color32 underlayColor;
        public Color32 faceColor;
        
        public float2 faceUVScroll;
        public float2 faceUVOffset;
        public float2 outlineUVScroll;
        public float2 outlineUVOffset;
        public float outlineWidth;
        public float outlineSoftness;
        public float glowOuter;
        public float glowOffset;
        public float glowInner;
        public float glowPower;

        public float underlayX;
        public float underlayY;
        public float underlayDilate;
        public float underlaySoftness;
        
        public float faceDilate;
        public static TextStyleBuffer defaultValue;

    }
    
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct TextInfoRenderSpan {

        public Color32 outlineColor;
        public Color32 glowColor;
        public Color32 underlayColor;
        public Color32 faceColor;

        public TextInfoRenderData* textInfo;
        public int symbolStart;
        public int symbolEnd;
        
       // public TextStyleBuffer textStyleBuffer;
       public int faceTextureId;
       public int outlineTextureId;
       
        public float2 faceUVScroll;
        public float2 faceUVOffset;

        public float2 outlineUVScroll;
        public float2 outlineUVOffset;

        public TextUVMapping faceUVModeH;
        public TextUVMapping faceUVModeV;

        public float rotation;
        public float vertexOffsetX;
        public float vertexOffsetY;

        public float charScale;
        public int lineIndex;
        
        public float outlineWidth;
        public float outlineSoftness;
        public float glowOuter;
        public float glowOffset;
        public float glowInner;
        public float glowPower;

        public float underlayX;
        public float underlayY;
        public float underlayDilate;
        public float underlaySoftness;
        
        public float faceDilate;
        public float faceSoftness;
        
        public float fontSize;
        public int fontAssetId;
        public FontStyle fontStyle;
        
        public TextInfoRenderSpan * nextSpanOnLine;
        public TextInfoRenderSpan* prevSpanOnLine;
        public TextMaterialFeatures materialFeatureSet;

        public bool UseUnderlay {
            get => underlayColor.a > 0 || underlayDilate > 0 || underlaySoftness > 0;
        }

    }

}