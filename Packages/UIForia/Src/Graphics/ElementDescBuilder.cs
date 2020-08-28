using UIForia.Util;
using UnityEngine;

namespace UIForia.Graphics {

    public class ElementDescBuilder {

        private UIFixedLength bevelTopLeft;
        private UIFixedLength bevelTopRight;
        private UIFixedLength bevelBottomRight;
        private UIFixedLength bevelBottomLeft;
        private UIFixedLength borderRadiusTopLeft;
        private UIFixedLength borderRadiusTopRight;
        private UIFixedLength borderRadiusBottomRight;
        private UIFixedLength borderRadiusBottomLeft;

        private ElementDrawDesc drawDesc;

        public ElementDescBuilder SetBorderRadius(UIFixedLength length) {
            borderRadiusTopLeft = length;
            borderRadiusTopRight = length;
            borderRadiusBottomRight = length;
            borderRadiusBottomLeft = length;
            return this;
        }
        
        public ElementDescBuilder SetCornerBevel(UIFixedLength length) {
            bevelTopLeft = length;
            bevelTopRight = length;
            bevelBottomRight = length;
            bevelBottomLeft = length;
            return this;
        }

        public ElementDescBuilder SetOpacity(float opacity) {
            drawDesc.opacity = MathUtil.FloatPercentageToUshort(opacity);
            return this;
        }

        private float ResolveRelativeLength(float baseValue, UIFixedLength length) {
            switch (length.unit) {
                default:
                    return length.value;

                case UIFixedUnit.Percent:
                    return length.value * baseValue;
            }
        }

        public ElementDrawDesc Build(float width, float height) {

            float min = width < height ? width : height;

            if (min <= 0) min = 0.0001f;

            float halfMin = min * 0.5f;
            float minScale = 1f / min;

            float resolvedBorderRadiusTopLeft = Mathf.Clamp(ResolveRelativeLength(min, borderRadiusTopLeft), 0, halfMin) * minScale;
            float resolvedBorderRadiusTopRight = Mathf.Clamp(ResolveRelativeLength(min, borderRadiusTopRight), 0, halfMin) * minScale;
            float resolvedBorderRadiusBottomLeft = Mathf.Clamp(ResolveRelativeLength(min, borderRadiusBottomRight), 0, halfMin) * minScale;
            float resolvedBorderRadiusBottomRight = Mathf.Clamp(ResolveRelativeLength(min, borderRadiusBottomLeft), 0, halfMin) * minScale;

            byte r0 = (byte) (((resolvedBorderRadiusTopLeft * 1000)) * 0.5f);
            byte r1 = (byte) (((resolvedBorderRadiusTopRight * 1000)) * 0.5f);
            byte r2 = (byte) (((resolvedBorderRadiusBottomLeft * 1000)) * 0.5f);
            byte r3 = (byte) (((resolvedBorderRadiusBottomRight * 1000)) * 0.5f);

            float resolvedCornerBevelTopLeft = ResolveRelativeLength(halfMin, bevelTopLeft);
            float resolvedCornerBevelTopRight = ResolveRelativeLength(halfMin, bevelTopRight);
            float resolvedCornerBevelBottomRight = ResolveRelativeLength(halfMin, bevelBottomRight);
            float resolvedCornerBevelBottomLeft = ResolveRelativeLength(halfMin, bevelBottomLeft);

            ushort b0 = (ushort) resolvedCornerBevelTopLeft;
            ushort b1 = (ushort) resolvedCornerBevelTopRight;
            ushort b2 = (ushort) resolvedCornerBevelBottomRight;
            ushort b3 = (ushort) resolvedCornerBevelBottomLeft;

            drawDesc.bevelTL = b0;
            drawDesc.bevelTR = b1;
            drawDesc.bevelBR = b2;
            drawDesc.bevelBL = b3;

            drawDesc.radiusTL = r0;
            drawDesc.radiusTR = r1;
            drawDesc.radiusBR = r2;
            drawDesc.radiusBL = r3;

            return drawDesc;
        }

    }

}