using System;
using System.Diagnostics;
using ThisOtherThing.UI.ShapeUtils;
using UIForia.Graphics;
using UIForia.Graphics.ShapeKit;
using UIForia.Layout;
using UIForia.Systems;
using UIForia.UIInput;
using UIForia.Util;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace UIForia.Rendering {

    [DebuggerDisplay("{element.ToString()}")]
    public class StandardRenderBox2 : RenderBox { // todo -- make a new sealed class with isBuiltIn flag set to allow user to inherit this 

        protected bool requireRendering;

        public ElementDrawDesc drawDesc;

        protected Texture maskTexture;
        protected Texture outlineTexture;
        protected Texture backgroundTexture;

        protected int backgroundTextureId;
        protected int outlineTextureId;
        protected OverflowHandling overflowHandling;
        protected Gradient gradient;
        protected Size size;
        
        public override void OnInitialize() {
            isBuiltIn = true;
            isElementBox = true;
            drawDesc.opacity = MathUtil.FloatPercentageToUshort(element.style.Opacity);
            drawDesc.backgroundColor = element.style.BackgroundColor;
            drawDesc.backgroundTint = element.style.BackgroundTint;
            drawDesc.outlineColor = element.style.OutlineColor;
            drawDesc.outlineWidth = 0; // set later since its size relative

            drawDesc.radiusTL = 0;
            drawDesc.radiusTR = 0;
            drawDesc.radiusBR = 0;
            drawDesc.radiusBL = 0;

            drawDesc.bevelTL = 0;
            drawDesc.bevelTR = 0;
            drawDesc.bevelBR = 0;
            drawDesc.bevelBL = 0;

            backgroundTexture = element.style.BackgroundImage;
            backgroundTextureId = ReferenceEquals(backgroundTexture, null) ? 0 : backgroundTexture.GetHashCode();

            drawDesc.borderColorTop = element.style.BorderColorTop;
            drawDesc.borderColorRight = element.style.BorderColorRight;
            drawDesc.borderColorBottom = element.style.BorderColorBottom;
            drawDesc.borderColorLeft = element.style.BorderColorLeft;

            drawDesc.bgColorMode = GetColorMode(drawDesc.backgroundColor, drawDesc.backgroundTint, backgroundTextureId, element.style.BackgroundFit == BackgroundFit.Cover);
            if (backgroundTexture != null) {
                drawDesc.uvTop = (ushort) ResolveRelativeLength(backgroundTexture.height, element.style.BackgroundRectMinY);
                drawDesc.uvRight = (ushort) ResolveRelativeLength(backgroundTexture.width, element.style.BackgroundRectMaxX);
                drawDesc.uvBottom = (ushort) ResolveRelativeLength(backgroundTexture.height, element.style.BackgroundRectMaxY);
                drawDesc.uvLeft = (ushort) ResolveRelativeLength(backgroundTexture.width, element.style.BackgroundRectMinX);
                drawDesc.uvScaleX = element.style.BackgroundImageScaleX;
                drawDesc.uvScaleY = element.style.BackgroundImageScaleY;
                drawDesc.uvOffsetX = 0; // cannot set until we know size
                drawDesc.uvOffsetY = 0; // cannot set until we know size
            }
            else {
                drawDesc.uvScaleX = 1;
                drawDesc.uvScaleY = 1;
                drawDesc.uvOffsetX = 1;
                drawDesc.uvOffsetY = 1;
            }

            drawDesc.uvRotation = MathUtil.FloatPercentageToUshort(element.style.BackgroundImageRotation.ToPercent().value);

            requireRendering = false; // set after we get a size change
            overflowHandling = 0;
            if (element.style.OverflowX != Overflow.Visible) {
                overflowHandling |= OverflowHandling.Horizontal;
            }

            if (element.style.OverflowY != Overflow.Visible) {
                overflowHandling |= OverflowHandling.Vertical;
            }
        }

        public static ColorMode GetColorMode(Color32 mainColor, Color32 tintColor, int textureId, bool useCoverMode) {
            ColorMode colorMode = 0;

            if (textureId != 0) {
                colorMode |= ColorMode.Texture;
            }

            if (tintColor.a > 0) {
                colorMode |= ColorMode.TextureTint;
            }

            if (mainColor.a > 0) {
                colorMode |= ColorMode.Color;
            }

            if (useCoverMode) {
                colorMode |= ColorMode.Cover;
            }

            return colorMode;
        }

        // send with style update, do this after layout finishes
        // sync point but i had that anyway

        private float ResolveRelativeLength(float baseValue, UIFixedLength length) {
            switch (length.unit) {
                default:
                    return length.value;

                case UIFixedUnit.Percent:
                    return length.value * baseValue;
            }
        }

        public override void OnSizeChanged(Size size) {
            // re-compute radius & bevel
            this.size = size;
            float min = size.width < size.height ? size.width : size.height;

            if (min <= 0) min = 0.0001f;

            float halfMin = min * 0.5f;
            float minScale = 1f / min;

            float resolvedBorderRadiusTopLeft = Mathf.Clamp(ResolveRelativeLength(min, element.style.BorderRadiusTopLeft), 0, halfMin) * minScale;
            float resolvedBorderRadiusTopRight = Mathf.Clamp(ResolveRelativeLength(min, element.style.BorderRadiusTopRight), 0, halfMin) * minScale;
            float resolvedBorderRadiusBottomLeft = Mathf.Clamp(ResolveRelativeLength(min, element.style.BorderRadiusBottomRight), 0, halfMin) * minScale;
            float resolvedBorderRadiusBottomRight = Mathf.Clamp(ResolveRelativeLength(min, element.style.BorderRadiusBottomLeft), 0, halfMin) * minScale;

            byte r0 = (byte) (((resolvedBorderRadiusTopLeft * 1000)) * 0.5f);
            byte r1 = (byte) (((resolvedBorderRadiusTopRight * 1000)) * 0.5f);
            byte r2 = (byte) (((resolvedBorderRadiusBottomLeft * 1000)) * 0.5f);
            byte r3 = (byte) (((resolvedBorderRadiusBottomRight * 1000)) * 0.5f);

            float resolvedCornerBevelTopLeft = ResolveRelativeLength(halfMin, element.style.CornerBevelTopLeft);
            float resolvedCornerBevelTopRight = ResolveRelativeLength(halfMin, element.style.CornerBevelTopRight);
            float resolvedCornerBevelBottomRight = ResolveRelativeLength(halfMin, element.style.CornerBevelBottomRight);
            float resolvedCornerBevelBottomLeft = ResolveRelativeLength(halfMin, element.style.CornerBevelBottomLeft);

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

            drawDesc.outlineWidth = ResolveRelativeLength(halfMin, element.style.OutlineWidth);
            drawDesc.outlineColorMode = GetColorMode(drawDesc.outlineColor, default, outlineTextureId, false);
            OffsetRect border = element.layoutResult.border;
            drawDesc.borderTop = border.top;
            drawDesc.borderRight = border.right;
            drawDesc.borderBottom = border.bottom;
            drawDesc.borderLeft = border.left;
            requireRendering = drawDesc.opacity > 0 && (drawDesc.bgColorMode != 0 || (drawDesc.outlineWidth > 0 && drawDesc.outlineColorMode != 0) || drawDesc.HasBorder && drawDesc.HasPaintedBorder);
            ComputeUVTransform();
            ComputeMeshFill();
        }

        protected void ComputeUVTransform() {
            if ((drawDesc.bgColorMode & ColorMode.Texture) != 0) {
                drawDesc.uvScaleX = element.style.BackgroundImageScaleX;
                drawDesc.uvScaleY = element.style.BackgroundImageScaleY;
                drawDesc.uvOffsetX = ResolveRelativeLength(size.width, element.style.BackgroundImageOffsetX); // cannot set until we know size
                drawDesc.uvOffsetY = ResolveRelativeLength(size.height, element.style.BackgroundImageOffsetY); // cannot set until we know size
            }
            else {
                drawDesc.uvScaleX = 1;
                drawDesc.uvScaleY = 1;
                drawDesc.uvOffsetX = 0;
                drawDesc.uvOffsetY = 0;
            }
        }

        protected void ComputeMeshFill() {
            switch (element.style.MeshType) {
                case MeshType.None:
                    drawDesc.meshFillOpenAmount = ushort.MaxValue;
                    drawDesc.meshFillOffsetX = 0;
                    drawDesc.meshFillOffsetY = 0;
                    drawDesc.meshFillDirection = 0;
                    drawDesc.meshFillRadius = float.MaxValue;
                    drawDesc.meshFillInvert = 0;
                    drawDesc.meshFillRotation = 0;
                    break;

                case MeshType.Manual: {
                    drawDesc.meshFillOpenAmount = MathUtil.FloatPercentageToUshort(element.style.MeshFillAmount); // maybe use 2 bits at the end for encoding direction & inversion
                    drawDesc.meshFillOffsetX = ResolveRelativeLength(size.width, element.style.MeshFillOffsetX);
                    drawDesc.meshFillOffsetY = ResolveRelativeLength(size.height, element.style.MeshFillOffsetY);

                    drawDesc.meshFillDirection = element.style.MeshFillDirection == MeshFillDirection.Clockwise ? (byte) 0 : byte.MaxValue;
                    drawDesc.meshFillRadius = ResolveRelativeLength(math.min(size.width, size.height), element.style.MeshFillRadius);
                    drawDesc.meshFillInvert = 0;
                    drawDesc.meshFillRotation = MathUtil.FloatPercentageToUshort(element.style.MeshFillRotation.ToPercent().value);
                    break;
                }

                case MeshType.Radial90_TopLeft: {
                    float amount = MathUtil.RemapRange(1 - element.style.MeshFillAmount, 0f, 1f, 0f, 0.25f);
                    drawDesc.meshFillOpenAmount = MathUtil.FloatPercentageToUshort(amount); // maybe use 2 bits at the end for encoding direction & inversion
                    drawDesc.meshFillOffsetX = -1;
                    drawDesc.meshFillOffsetY = -1;
                    drawDesc.meshFillDirection = element.style.MeshFillDirection == MeshFillDirection.Clockwise ? (byte) 0 : byte.MaxValue;
                    drawDesc.meshFillRadius = float.MaxValue;
                    drawDesc.meshFillInvert = 0;
                    drawDesc.meshFillRotation = 0;
                    break;
                }

                case MeshType.Radial90_BottomLeft: {
                    float amount = MathUtil.RemapRange(1 - element.style.MeshFillAmount, 0f, 1f, 0.25f, 0.5f); //.Clamp(element.style.MeshFillAmount / 0.25f, 0, 0.25f);
                    drawDesc.meshFillOpenAmount = MathUtil.FloatPercentageToUshort(amount); // maybe use 2 bits at the end for encoding direction & inversion
                    drawDesc.meshFillOffsetX = -1;
                    drawDesc.meshFillOffsetY = size.height + 1;
                    drawDesc.meshFillDirection = element.style.MeshFillDirection == MeshFillDirection.Clockwise ? (byte) 0 : byte.MaxValue;
                    drawDesc.meshFillRadius = float.MaxValue;
                    drawDesc.meshFillInvert = 1;
                    drawDesc.meshFillRotation = 0;
                    break;
                }

                case MeshType.Radial90_TopRight: {
                    float amount = MathUtil.RemapRange(1 - element.style.MeshFillAmount, 0f, 1f, 0.75f, 1f); //.Clamp(element.style.MeshFillAmount / 0.25f, 0, 0.25f);
                    drawDesc.meshFillOpenAmount = MathUtil.FloatPercentageToUshort(amount); // maybe use 2 bits at the end for encoding direction & inversion
                    drawDesc.meshFillOffsetX = size.width + 1;
                    drawDesc.meshFillOffsetY = -1;
                    drawDesc.meshFillDirection = element.style.MeshFillDirection == MeshFillDirection.Clockwise ? (byte) 0 : byte.MaxValue;
                    drawDesc.meshFillRadius = float.MaxValue;
                    drawDesc.meshFillInvert = 0;
                    drawDesc.meshFillRotation = 0;
                    break;
                }

                case MeshType.Radial90_BottomRight: {
                    float amount = MathUtil.RemapRange(1 - element.style.MeshFillAmount, 0f, 1f, 0.5f, 0.75f); //.Clamp(element.style.MeshFillAmount / 0.25f, 0, 0.25f);
                    drawDesc.meshFillOpenAmount = MathUtil.FloatPercentageToUshort(amount); // maybe use 2 bits at the end for encoding direction & inversion
                    drawDesc.meshFillOffsetX = size.width + 1;
                    drawDesc.meshFillOffsetY = size.height + 1;
                    drawDesc.meshFillDirection = element.style.MeshFillDirection == MeshFillDirection.Clockwise ? (byte) 0 : byte.MaxValue;
                    drawDesc.meshFillInvert = 1;
                    drawDesc.meshFillRadius = float.MaxValue;
                    drawDesc.meshFillRotation = 0;
                    break;
                }

                case MeshType.Horizontal_Left: {
                    float amount = element.style.MeshFillAmount;
                    drawDesc.meshFillOpenAmount = MathUtil.FloatPercentageToUshort(0.5f); // maybe use 2 bits at the end for encoding direction & inversion
                    drawDesc.meshFillOffsetX = size.width * amount;
                    drawDesc.meshFillOffsetY = 0;
                    drawDesc.meshFillDirection = 0;
                    drawDesc.meshFillInvert = 1;
                    drawDesc.meshFillRadius = float.MaxValue;
                    drawDesc.meshFillRotation = ushort.MaxValue / 2;
                    break;
                }

                case MeshType.Horizontal_Right: {
                    float amount = element.style.MeshFillAmount;
                    drawDesc.meshFillOpenAmount = MathUtil.FloatPercentageToUshort(0.5f); // maybe use 2 bits at the end for encoding direction & inversion
                    drawDesc.meshFillOffsetX = size.width * (1 - amount);
                    drawDesc.meshFillOffsetY = 0;
                    drawDesc.meshFillDirection = 0;
                    drawDesc.meshFillInvert = 0;
                    drawDesc.meshFillRadius = float.MaxValue;
                    drawDesc.meshFillRotation = ushort.MaxValue / 2;
                    break;
                }

                case MeshType.Vertical_Top: {
                    float amount = element.style.MeshFillAmount;
                    drawDesc.meshFillOpenAmount = MathUtil.FloatPercentageToUshort(0.5f); // maybe use 2 bits at the end for encoding direction & inversion
                    drawDesc.meshFillOffsetX = 0;
                    drawDesc.meshFillOffsetY = size.height * amount;
                    drawDesc.meshFillDirection = 0;
                    drawDesc.meshFillInvert = 0;
                    drawDesc.meshFillRadius = float.MaxValue;
                    drawDesc.meshFillRotation = ushort.MaxValue / 2;
                    break;
                }

                case MeshType.Radial360_Top: {
                    float amount = element.style.MeshFillAmount;
                    drawDesc.meshFillOpenAmount = MathUtil.FloatPercentageToUshort(amount);
                    drawDesc.meshFillOffsetX = size.width * 0.5f;
                    drawDesc.meshFillOffsetY = size.height * 0.5f;
                    drawDesc.meshFillDirection = (byte) (element.style.MeshFillDirection == MeshFillDirection.CounterClockwise ? 0 : 1);
                    drawDesc.meshFillInvert = 0; //(byte) (element.style.MeshFillInvert == Invert ? 1 : 0);
                    drawDesc.meshFillRadius = float.MaxValue;
                    drawDesc.meshFillRotation = ushort.MaxValue / 2;
                    break;
                }

                case MeshType.Radial360_Bottom: {
                    float amount = element.style.MeshFillAmount;
                    drawDesc.meshFillOpenAmount = MathUtil.FloatPercentageToUshort(amount);
                    drawDesc.meshFillOffsetX = size.width * 0.5f;
                    drawDesc.meshFillOffsetY = size.height * 0.5f;
                    drawDesc.meshFillDirection = (byte) (element.style.MeshFillDirection == MeshFillDirection.CounterClockwise ? 0 : 1);
                    drawDesc.meshFillInvert = 0; //(byte) (element.style.MeshFillInvert == Invert ? 1 : 0);
                    drawDesc.meshFillRadius = float.MaxValue;
                    drawDesc.meshFillRotation = 0;
                    break;
                }

                case MeshType.Radial360_Right: {
                    float amount = element.style.MeshFillAmount;
                    drawDesc.meshFillOpenAmount = MathUtil.FloatPercentageToUshort(amount);
                    drawDesc.meshFillOffsetX = size.width * 0.5f;
                    drawDesc.meshFillOffsetY = size.height * 0.5f;
                    drawDesc.meshFillDirection = (byte) (element.style.MeshFillDirection == MeshFillDirection.CounterClockwise ? 0 : 1);
                    drawDesc.meshFillInvert = 0; //(byte) (element.style.MeshFillInvert == Invert ? 1 : 0);
                    drawDesc.meshFillRadius = float.MaxValue;
                    drawDesc.meshFillRotation = ushort.MaxValue / 4;
                    break;
                }

                case MeshType.Radial360_Left: {
                    float amount = element.style.MeshFillAmount;
                    drawDesc.meshFillOpenAmount = MathUtil.FloatPercentageToUshort(amount);
                    drawDesc.meshFillOffsetX = size.width * 0.5f;
                    drawDesc.meshFillOffsetY = size.height * 0.5f;
                    drawDesc.meshFillDirection = (byte) (element.style.MeshFillDirection == MeshFillDirection.CounterClockwise ? 0 : 1);
                    drawDesc.meshFillInvert = 0; //(byte) (element.style.MeshFillInvert == Invert ? 1 : 0);
                    drawDesc.meshFillRadius = float.MaxValue;
                    drawDesc.meshFillRotation = 3 * (ushort.MaxValue / 4);
                    break;
                }
            }
        }

        public override void OnStylePropertyChanged(StyleProperty[] propertyList, int propertyCount) {
            bool recomputeDrawing = false;
            bool recomputeMeshFill = false;
            bool recomputeUVTransform = false;
            bool recomputeBGParams = false;

            for (int i = 0; i < propertyCount; i++) {
                ref StyleProperty property = ref propertyList[i];
                switch (property.propertyId) {
                    case StylePropertyId.OutlineWidth:
                        recomputeDrawing = true;
                        float halfMin = math.min(size.width, size.height) * 0.5f;
                        drawDesc.outlineWidth = ResolveRelativeLength(halfMin, property.AsUIFixedLength);
                        break;

                    case StylePropertyId.BorderColorTop:
                        drawDesc.borderColorTop = property.AsColor32;
                        break;

                    case StylePropertyId.BorderColorRight:
                        drawDesc.borderColorRight = property.AsColor32;
                        break;

                    case StylePropertyId.BorderColorBottom:
                        drawDesc.borderColorBottom = property.AsColor32;
                        break;

                    case StylePropertyId.BorderColorLeft:
                        drawDesc.borderColorLeft = property.AsColor32;
                        break;

                    case StylePropertyId.OutlineColor:
                        recomputeDrawing = true;
                        drawDesc.outlineColor = property.AsColor32;
                        break;

                    case StylePropertyId.BackgroundColor:
                        recomputeDrawing = true;
                        drawDesc.backgroundColor = property.AsColor32;
                        break;

                    case StylePropertyId.BackgroundTint:
                        recomputeDrawing = true;
                        drawDesc.backgroundTint = property.AsColor32;
                        break;

                    case StylePropertyId.BackgroundImage:
                        recomputeDrawing = true;
                        recomputeBGParams = true;
                        break;

                    case StylePropertyId.Gradient:
                        gradient = property.AsGradient;
                        break;

                    case StylePropertyId.GradientOffsetX:
                    case StylePropertyId.GradientOffsetY:
                        break;

                    case StylePropertyId.GradientMode:
                        break;

                    case StylePropertyId.BackgroundImageRotation:
                        drawDesc.uvRotation = MathUtil.FloatPercentageToUshort(element.style.BackgroundImageRotation.ToPercent().value);
                        break;

                    case StylePropertyId.BackgroundImageOffsetX:
                    case StylePropertyId.BackgroundImageOffsetY:
                    case StylePropertyId.BackgroundImageScaleX:
                    case StylePropertyId.BackgroundImageScaleY:
                        recomputeUVTransform = true;
                        break;

                    case StylePropertyId.OverflowX:
                        if (property.AsOverflow == Overflow.Visible || property.AsOverflow == Overflow.Unset) {
                            overflowHandling &= ~OverflowHandling.Horizontal;
                        }
                        else {
                            overflowHandling |= OverflowHandling.Horizontal;
                        }

                        break;

                    case StylePropertyId.OverflowY:
                        if (property.AsOverflow == Overflow.Visible || property.AsOverflow == Overflow.Unset) {
                            overflowHandling &= ~OverflowHandling.Vertical;
                        }
                        else {
                            overflowHandling |= OverflowHandling.Vertical;
                        }

                        break;

                    case StylePropertyId.Opacity:
                        recomputeDrawing = true;
                        drawDesc.opacity = MathUtil.FloatPercentageToUshort(property.AsFloat);
                        break;

                    case StylePropertyId.MeshFillAmount:
                    case StylePropertyId.MeshFillOrigin:
                    case StylePropertyId.MeshFillDirection:
                    case StylePropertyId.MeshType:
                    case StylePropertyId.MeshFillOffsetX:
                    case StylePropertyId.MeshFillOffsetY:
                    case StylePropertyId.MeshFillRadius:
                    case StylePropertyId.MeshFillRotation:
                        recomputeMeshFill = true;
                        break;

                    // todo -- remove these?
                    case StylePropertyId.BackgroundImageTileX:
                    case StylePropertyId.BackgroundImageTileY:
                        break;
                }
            }

            if (recomputeBGParams) {
                backgroundTexture = element.style.BackgroundImage;
                backgroundTextureId = 0;
                if (ReferenceEquals(backgroundTexture, null)) {
                    backgroundTextureId = backgroundTexture.GetHashCode();
                    drawDesc.uvTop = (ushort) ResolveRelativeLength(backgroundTexture.height, element.style.BackgroundRectMinY);
                    drawDesc.uvRight = (ushort) ResolveRelativeLength(backgroundTexture.width, element.style.BackgroundRectMaxX);
                    drawDesc.uvBottom = (ushort) ResolveRelativeLength(backgroundTexture.height, element.style.BackgroundRectMaxY);
                    drawDesc.uvLeft = (ushort) ResolveRelativeLength(backgroundTexture.width, element.style.BackgroundRectMinX);
                }
            }

            if (recomputeDrawing) {
                drawDesc.bgColorMode = GetColorMode(drawDesc.backgroundColor, drawDesc.backgroundTint, backgroundTextureId, element.style.BackgroundFit == BackgroundFit.Cover);
                drawDesc.outlineColorMode = GetColorMode(drawDesc.outlineColor, default, outlineTextureId, false);
                OffsetRect border = element.layoutResult.border;
                drawDesc.borderTop = border.top;
                drawDesc.borderRight = border.right;
                drawDesc.borderBottom = border.bottom;
                drawDesc.borderLeft = border.left;
                requireRendering = drawDesc.opacity > 0 && (drawDesc.bgColorMode != 0 || (drawDesc.outlineWidth > 0 && drawDesc.outlineColorMode != 0) || drawDesc.HasBorder && drawDesc.HasPaintedBorder);
                recomputeUVTransform = false;
                ComputeUVTransform();
            }

            if (recomputeUVTransform && requireRendering) {
                ComputeUVTransform();
            }

            if (recomputeMeshFill && requireRendering) {
                ComputeMeshFill();
            }
        }

        private RenderTexture targetTexture;

        public override void PaintBackground3(RenderContext3 ctx) {
            // if (element.style.ShadowOpacity == 0) { // todo cheating -- change this to a real property
            //     targetTexture = ctx.PushRenderTargetRegion(new AxisAlignedBounds2DUShort(0, 0, (ushort) (element.layoutResult.actualSize.width - element.layoutResult.HorizontalPaddingBorder), (ushort) (element.layoutResult.actualSize.height - element.layoutResult.VerticalPaddingBorder)));
            //     return;
            // }

            if (requireRendering) {
                OffsetRect border = element.layoutResult.border;
                drawDesc.borderTop = border.top;
                drawDesc.borderRight = border.right;
                drawDesc.borderBottom = border.bottom;
                drawDesc.borderLeft = border.left;
                ctx.SetBackgroundTexture(backgroundTexture);
                ctx.SetMaskTexture(maskTexture);
                // todo -- future me, restore 9 slice borders
                // if (backgroundTexture != null && AxisAlignedBounds2DUShort.HasValue(backgroundTexture.uvBorderRect)) {
                    // ctx.DrawSlicedElement(0, 0, drawDesc);
                // }
                // else {
                    ctx.DrawElement(0, 0, size.width, size.height, drawDesc);
                // }
            }

            if (overflowHandling != 0) {
                // todo -- add an overflow offset style
                float clipX = 0;
                float clipY = 0;
                float clipWidth = float.MaxValue;
                float clipHeight = float.MaxValue;

                if ((overflowHandling & OverflowHandling.Horizontal) != 0) {
                    clipWidth = size.width;
                }

                if ((overflowHandling & OverflowHandling.Vertical) != 0) {
                    clipHeight = size.height;
                }

                switch (element.style.ClipBounds) { // todo -- cache this
                    case ClipBounds.ContentBox: {
                        OffsetRect border = element.layoutResult.border;
                        OffsetRect padding = element.layoutResult.padding;
                        clipX = border.left + padding.left;
                        clipY = (border.top + padding.top);
                        clipWidth -= (clipX + border.right + padding.right);
                        clipHeight -= (border.top + padding.top + border.bottom + padding.bottom);
                        break;
                    }

                    case ClipBounds.BorderBox: {
                        OffsetRect border = element.layoutResult.border;
                        clipX = border.left;
                        clipY = border.top;
                        clipWidth -= (border.left + border.right);
                        clipHeight -= (border.bottom + border.top);
                        break;
                    }
                }

                // return new RenderBounds(0, 0, element.layoutResult.actualSize.width, element.layoutResult.actualSize.height);
                ctx.PushClipRect(clipX, clipY, clipWidth, clipHeight);
                // ctx.BeginStencilClip();
                //
                // ctx.DrawElement(0, 0, drawDesc);
                //
                // ctx.PushStencilClip();
            }
        }

        // tmp
        private Mesh mesh;

        public static void SetMaterialProperties(Material material, LightList<MaterialStyleProperty> properties) {
            for (int i = 0; i < properties.size; i++) {
                ref MaterialStyleProperty property = ref properties.array[i];
                switch (property.propertyType) {
                    case MaterialPropertyType.Color:
                        material.SetColor(property.shaderKey, property.property.AsColor32);
                        break;
                    case MaterialPropertyType.Float:
                    case MaterialPropertyType.Range:
                        material.SetFloat(property.shaderKey, property.property.AsFloat);
                        break;
                    case MaterialPropertyType.Vector:
                        float2 val = property.property.AsFloat2;
                        material.SetVector(property.shaderKey, new Vector4(val.x, val.y, 0, 0)); // todo -- now sure how to set this
                        break;
                    case MaterialPropertyType.Texture:
                        material.SetTexture(property.shaderKey, property.property.AsTexture);
                        break;
                }
            }
        }

        public override void PaintForeground3(RenderContext3 ctx) {
            if (overflowHandling != 0) {
                ctx.PopClipRect();
                // ctx.PopStencilClip();
            }

            //    if (element.style.ShadowOpacity == 0) { // cheating -- change this to a real property
            //        ctx.PopRenderTargetRegion();
            //
            //        Material material = Resources.Load<Material>("UIForiaDissolve");
            //        ctx.SetMaterial(material);
            //
            //        LightList<MaterialStyleProperty> materialStyles = LightList<MaterialStyleProperty>.Get();
            //
            //        MaterialId matId = element.application.ResourceManager.GetMaterialId("dissolve");
            //
            //        element.style.GetMaterialProperties(matId, materialStyles);
            //
            //     //   SetMaterialProperties(material, materialStyles);
            //        materialStyles.Release();
            //        
            //        material.SetTexture("_MainTex", targetTexture);
            //        material.SetTexture("_NoiseTex", Resources.Load<Texture2D>("DissolveNoise"));
            // //       material.SetFloat("_EffectFactor", element.style.OutlineWidth.value);
            //        material.SetFloat("_Softness", 1f);
            //        material.SetFloat("_Width", 0.5f);
            //        // material.SetColor("_DissolveColor", drawDesc.backgroundColor);
            //
            //        // // element.style.SetCustomProperty("MaterialName::PropertyName", value);
            //        //
            //        // // element.style.GetActiveMaterialParameters();
            //        UIVertexHelper helper = UIVertexHelper.Create(Allocator.Temp);
            //
            //        using (ShapeKit shapeKit = new ShapeKit()) {
            //            shapeKit.SetDrawMode(DrawMode.Normal);
            //
            //            shapeKit.AddRect(ref helper, 0, 0, 300, 600, Color.blue);
            //            mesh = mesh ?? new Mesh();
            //            mesh.MarkDynamic();
            //            mesh.Clear(true);
            //            helper.FillMesh(mesh);
            //        }
            //
            //        helper.Dispose();
            //        ctx.DrawMesh(mesh);
            //
            //        // ctx.SetBackgroundTexture(targetTexture);
            //        // ctx.SetMaskTexture(Resources.Load<Texture2D>("Images/Cloud_Mask"));
            //        // drawDesc.uvTop = 0;
            //        // drawDesc.uvLeft = 0;
            //        // drawDesc.uvBottom = (ushort) targetTexture.height;
            //        // drawDesc.uvRight = (ushort) targetTexture.width;
            //        // drawDesc.bgColorMode |= ColorMode.Texture;
            //        // drawDesc.maskFlags |= MaskFlags.UseMaskTexture;
            //        // drawDesc.maskSoftness = 0.5f;
            //        // ctx.DrawElement(0, 0, drawDesc);
            //    }
        }

    }

}