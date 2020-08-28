using System.Runtime.InteropServices;
using UnityEngine;

namespace UIForia.Graphics {

    [StructLayout(LayoutKind.Sequential)]
    public struct ElementDrawDesc {

        // all material data goes here
        // any unpacking/re-arranging will happen later when building 

        // todo -- merge radius & bevel with flag
        public byte radiusTL;
        public byte radiusTR;
        public byte radiusBR;
        public byte radiusBL;

        public ushort bevelTL;
        public ushort bevelTR;
        public ushort bevelBR;
        public ushort bevelBL;

        public Color32 backgroundColor;
        public Color32 backgroundTint;

        public ushort opacity;
        public ColorMode bgColorMode;
        public ColorMode outlineColorMode;

        public float outlineWidth;
        public Color32 outlineColor;

        public ushort meshFillOpenAmount;
        public ushort meshFillRotation;
        public float meshFillOffsetX;
        public float meshFillOffsetY;
        public float meshFillRadius;

        public byte meshFillDirection;
        public byte meshFillInvert;

        public ushort uvTop;
        public ushort uvLeft;
        public ushort uvRight;
        public ushort uvBottom;
        
        public ushort uvRotation;

        public float uvScaleX;
        public float uvScaleY;
        public float uvOffsetX;
        public float uvOffsetY;

        public MaskFlags maskFlags;
        public float maskSoftness;
        public uint maskTopLeftUV;
        public uint maskBottomRightUV;

        // todo -- remove all this and place in 9slice struct
        public ushort uvBorderTop;
        public ushort uvBorderRight;
        public ushort uvBorderBottom;
        public ushort uvBorderLeft;

        public float borderTop;
        public float borderRight;
        public float borderBottom;
        public float borderLeft;

        public Color32 borderColorTop;
        public Color32 borderColorRight;
        public Color32 borderColorBottom;
        public Color32 borderColorLeft;

        public bool isSliced {
            get => uvBorderTop != 0 || uvBorderLeft != 0 || uvBorderBottom != 0 || uvBorderRight != 0;
        }

        public bool HasPaintedBorder {
            get => borderColorTop.a > 0 || borderColorBottom.a > 0 || borderColorLeft.a > 0 || borderColorRight.a > 0;
        }

        public bool HasUniformBorderColor {
            get => HasUniformBorderColorStatic(ref this);
        }

        public bool HasBorder {
            get => borderTop > 0 || borderLeft > 0 || borderBottom > 0 || borderRight > 0;
        }

        private static unsafe bool HasUniformBorderColorStatic(ref ElementDrawDesc desc) {
            fixed (ElementDrawDesc* descptr = &desc) {
                long* topRight = (long*) &descptr->borderColorTop;
                long* bottomLeft = (long*) &descptr->borderColorBottom;
                return *topRight == *bottomLeft;
            }
        }

        public static ElementDrawDesc Create() {
            return new ElementDrawDesc() {
                opacity = ushort.MaxValue,
                backgroundColor = new Color32(0, 0, 0, 255),
                bgColorMode = ColorMode.Color,
                outlineColorMode = ColorMode.Color,
                uvScaleX = 1,
                uvScaleY = 1
            };
        }

    }

}