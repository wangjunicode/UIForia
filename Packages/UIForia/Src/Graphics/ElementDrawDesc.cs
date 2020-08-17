using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;

namespace UIForia.Graphics {

    [StructLayout(LayoutKind.Sequential)]
    public struct ElementDrawDesc {

        // all material data goes here
        // any unpacking/re-arranging will happen later when building 

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

        public float borderTop;
        public float borderRight;
        public float borderBottom;
        public float borderLeft;
        
        public Color32 borderColorTop;
        public Color32 borderColorRight;
        public Color32 borderColorBottom;
        public Color32 borderColorLeft;

        public float outlineWidth;
        public Color32 outlineColor;
        public float width;
        public float height;

        public ushort uvTop;
        public ushort uvLeft;
        public ushort uvRight;
        public ushort uvBottom;

        public ushort meshFillOpenAmount;
        public ushort meshFillRotation;

        public float meshFillOffsetX;
        public float meshFillOffsetY;

        public float meshFillRadius;

        public byte meshFillDirection;
        public byte meshFillInvert;
        public ushort uvRotation;

        public float uvScaleX;
        public float uvScaleY;
        public float uvOffsetX;
        public float uvOffsetY;

        public GradientMode gradientMode;
        public float gradientRotation;
        public float gradientOffsetX;
        public float gradientOffsetY;

        public ElementDrawDesc(float width, float height) : this() {
            this.width = width;
            this.height = height;
            this.uvScaleX = 1;
            this.uvScaleY = 1;
            this.meshFillOpenAmount = ushort.MaxValue;
            this.opacity = ushort.MaxValue;
            this.meshFillRadius = ushort.MaxValue;
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
                long * topRight = (long*)&descptr->borderColorTop;
                long * bottomLeft = (long*)&descptr->borderColorBottom;
                return *topRight == *bottomLeft;
            }
        }

    }

}