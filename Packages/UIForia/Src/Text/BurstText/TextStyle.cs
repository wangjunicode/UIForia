using UIForia.Graphics;
using Unity.Mathematics;
using UnityEngine;

namespace UIForia.Text {

    public struct TextStyle {

        // public FontAssetInfo fontAsset; // make make this a handle or this struct gets huge
        public int fontAssetId;

        public FontStyle fontStyle;
        public TextScript scriptStyle;
        public TextAlignment alignment;
        public TextTransform textTransform;
        public WhitespaceMode whitespaceMode;

        public UIFixedLength fontSize;
        
        public Color32 faceColor;
        public Color32 outlineColor;
        public Color32 glowColor;
        public Color32 underlayColor;

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
        public float lineHeight;

        public float2 faceUVScrollSpeed;
        public float2 outlineUVScrollSpeed;

        public float2 faceUVOffset;
        public float2 outlineUVOffset;

        public TextUVMapping faceUVModeHorizontal;
        public TextUVMapping faceUVModeVertical;

        public TextUVMapping outlineUVModeHorizontal;
        public TextUVMapping outlineUVModeVertical;

        public float characterRotation;

        public float characterOffsetX;
        public float characterOffsetY;
        
        public float characterScale;
        public float faceSoftness;
        public int faceTextureId;
        public int outlineTextureId;

    }

}