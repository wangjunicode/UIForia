﻿using System;
using System.Diagnostics;
using UIForia.Graphics;
using UIForia.Layout;
using UIForia.Util;
using Unity.Mathematics;
using UnityEngine;

namespace UIForia.Rendering {

    [DebuggerDisplay("{element.ToString()}")]
    public class StandardRenderBox2 : RenderBox {

        protected bool requireRendering;

        public ElementDrawDesc drawDesc;

        protected TextureReference outlineTexture;
        protected TextureReference backgroundTexture;
        protected int backgroundTextureId;
        protected int outlineTextureId;
        protected OverflowHandling overflowHandling;

        public override void OnInitialize() {

            drawDesc.opacity = MathUtil.Float01ToByte(element.style.Opacity);
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

            drawDesc.bgColorMode = GetColorMode(drawDesc.backgroundColor, drawDesc.backgroundTint, backgroundTextureId);
            requireRendering = false; // set after we get a size change
            overflowHandling = 0;
            if (element.style.OverflowX != Overflow.Visible) {
                overflowHandling |= OverflowHandling.Horizontal;
            }

            if (element.style.OverflowY != Overflow.Visible) {
                overflowHandling |= OverflowHandling.Vertical;
            }
        }

        public static ColorMode GetColorMode(Color32 mainColor, Color32 tintColor, int textureId) {
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

            drawDesc.width = size.width;
            drawDesc.height = size.height;

            drawDesc.bevelTL = b0;
            drawDesc.bevelTR = b1;
            drawDesc.bevelBR = b2;
            drawDesc.bevelBL = b3;

            drawDesc.radiusTL = r0;
            drawDesc.radiusTR = r1;
            drawDesc.radiusBR = r2;
            drawDesc.radiusBL = r3;

            drawDesc.outlineWidth = ResolveRelativeLength(halfMin, element.style.OutlineWidth);
            drawDesc.outlineColorMode = GetColorMode(drawDesc.outlineColor, default, outlineTextureId);
            requireRendering = drawDesc.opacity > 0 && (drawDesc.bgColorMode != 0 || (drawDesc.outlineWidth > 0 && drawDesc.outlineColorMode != 0));

            ComputeMeshFill();

        }

        protected void ComputeMeshFill() {
            
            
            // float pieDirection = 1; // can be a sign bit or flag elsewhere
            // float pieOpenAmount = 0.125; 
            // float pieRotation = 0; //frac(_Time.y) * PI * 2;
            // float pieRadius = 2 * max(size.x, size.y);
            // float2 pieOffset = (size * float2(0.0, 0.0)) + halfUV;
            // float invertPie = 1;
            
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
                
                case MeshType.Radial90_TopLeft: {
                    float amount = MathUtil.RemapRange(1 - element.style.MeshFillAmount, 0f, 1f, 0f, 0.25f);
                    drawDesc.meshFillOpenAmount = MathUtil.FloatPercentageToUshort(amount); // maybe use 2 bits at the end for encoding direction & inversion
                    drawDesc.meshFillOffsetX = -1;
                    drawDesc.meshFillOffsetY = -1;
                    drawDesc.meshFillDirection = element.style.MeshFillDirection == MeshFillDirection.Clockwise ? (byte)0 : byte.MaxValue;
                    drawDesc.meshFillRadius = float.MaxValue;
                    drawDesc.meshFillInvert = 0;
                    drawDesc.meshFillRotation = 0;
                    break;
                }

                case MeshType.Radial90_BottomLeft: {
                    float amount = MathUtil.RemapRange(1 - element.style.MeshFillAmount, 0f, 1f, 0.25f, 0.5f); //.Clamp(element.style.MeshFillAmount / 0.25f, 0, 0.25f);
                    drawDesc.meshFillOpenAmount = MathUtil.FloatPercentageToUshort(amount); // maybe use 2 bits at the end for encoding direction & inversion
                    drawDesc.meshFillOffsetX = -1;
                    drawDesc.meshFillOffsetY = drawDesc.height + 1;
                    drawDesc.meshFillDirection = element.style.MeshFillDirection == MeshFillDirection.Clockwise ? (byte)0 : byte.MaxValue;
                    drawDesc.meshFillRadius = float.MaxValue;
                    drawDesc.meshFillInvert = 1;
                    drawDesc.meshFillRotation = 0;
                    break;
                }
                
                case MeshType.Radial90_TopRight: {
                    float amount = MathUtil.RemapRange(1 - element.style.MeshFillAmount, 0f, 1f, 0.75f, 1f); //.Clamp(element.style.MeshFillAmount / 0.25f, 0, 0.25f);
                    drawDesc.meshFillOpenAmount = MathUtil.FloatPercentageToUshort(amount); // maybe use 2 bits at the end for encoding direction & inversion
                    drawDesc.meshFillOffsetX = drawDesc.width + 1;
                    drawDesc.meshFillOffsetY = -1;
                    drawDesc.meshFillDirection = element.style.MeshFillDirection == MeshFillDirection.Clockwise ? (byte)0 : byte.MaxValue;
                    drawDesc.meshFillRadius = float.MaxValue;
                    drawDesc.meshFillInvert = 0;
                    drawDesc.meshFillRotation = 0;
                    break;
                }
                
                case MeshType.Radial90_BottomRight: {
                    float amount = MathUtil.RemapRange(1 - element.style.MeshFillAmount, 0f, 1f, 0.5f, 0.75f); //.Clamp(element.style.MeshFillAmount / 0.25f, 0, 0.25f);
                    drawDesc.meshFillOpenAmount = MathUtil.FloatPercentageToUshort(amount); // maybe use 2 bits at the end for encoding direction & inversion
                    drawDesc.meshFillOffsetX = drawDesc.width + 1;
                    drawDesc.meshFillOffsetY = drawDesc.height + 1;
                    drawDesc.meshFillDirection = element.style.MeshFillDirection == MeshFillDirection.Clockwise ? (byte)0 : byte.MaxValue;
                    drawDesc.meshFillInvert = 1;
                    drawDesc.meshFillRadius = float.MaxValue;
                    drawDesc.meshFillRotation = 0; 
                    break;
                }

                case MeshType.Horizontal_Left: {
                    float amount = element.style.MeshFillAmount;
                    drawDesc.meshFillOpenAmount = MathUtil.FloatPercentageToUshort(0.5f); // maybe use 2 bits at the end for encoding direction & inversion
                    drawDesc.meshFillOffsetX = drawDesc.width * amount;
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
                    drawDesc.meshFillOffsetX = drawDesc.width * (1 - amount);
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
                    drawDesc.meshFillOffsetY = drawDesc.height * amount;
                    drawDesc.meshFillDirection = 0;
                    drawDesc.meshFillInvert = 0;
                    drawDesc.meshFillRadius = float.MaxValue;
                    drawDesc.meshFillRotation = ushort.MaxValue / 2;
                    break;
                }
                
            }
        }
        
        public override void Enable() { }

        public override void OnStylePropertyChanged(StyleProperty[] propertyList, int propertyCount) {
            bool recomputeDrawing = false;
            bool recomputeMeshFill = false;
            
            for (int i = 0; i < propertyCount; i++) {
                ref StyleProperty property = ref propertyList[i];
                switch (property.propertyId) {

                    case StylePropertyId.OutlineWidth:
                        recomputeDrawing = true;
                        float halfMin = math.min(drawDesc.width, drawDesc.height) * 0.5f;
                        drawDesc.outlineWidth = ResolveRelativeLength(halfMin, property.AsUIFixedLength);
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
                        backgroundTexture = property.AsTexture;
                        backgroundTextureId = ReferenceEquals(backgroundTexture, null) ? 0 : backgroundTexture.GetHashCode();
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
                        drawDesc.opacity = MathUtil.Float01ToByte(property.AsFloat);
                        break;

                    case StylePropertyId.MeshFillAmount:
                    case StylePropertyId.MeshFillOrigin:
                    case StylePropertyId.MeshFillDirection:
                    case StylePropertyId.MeshType:
                        recomputeMeshFill = true;
                        break;

                    case StylePropertyId.BackgroundImageScaleX:
                        break;

                    case StylePropertyId.BackgroundImageScaleY:
                        break;

                    case StylePropertyId.BackgroundImageRotation:
                        break;

                    case StylePropertyId.BackgroundImageTileX:
                        break;

                    case StylePropertyId.BackgroundImageTileY:
                        break;
                }
            }

            if (recomputeDrawing) {
                drawDesc.bgColorMode = GetColorMode(drawDesc.backgroundColor, drawDesc.backgroundTint, backgroundTextureId);
                drawDesc.outlineColorMode = GetColorMode(drawDesc.outlineColor, default, outlineTextureId);
                requireRendering = drawDesc.opacity > 0 && (drawDesc.bgColorMode != 0 || (drawDesc.outlineWidth > 0 && drawDesc.outlineColorMode != 0));
            }

            if (recomputeMeshFill) {
                ComputeMeshFill();
            }

        }

        public override void PaintBackground3(RenderContext3 ctx) {

            if (requireRendering) {
                ctx.SetBackgroundTexture(backgroundTexture?.texture);
                ctx.DrawElement(0, 0, drawDesc);
            }

            if (overflowHandling != 0) {

                // todo -- add an overflow offset style
                float clipX = 0;
                float clipY = 0;
                float clipWidth = float.MaxValue;
                float clipHeight = float.MaxValue;

                if ((overflowHandling & OverflowHandling.Horizontal) != 0) {
                    clipWidth = drawDesc.width;
                }

                if ((overflowHandling & OverflowHandling.Vertical) != 0) {
                    clipHeight = drawDesc.height;
                }

                ctx.PushClipRect(clipX, clipY, clipWidth, clipHeight);
                // ctx.BeginStencilClip();
                //
                // ctx.DrawElement(0, 0, drawDesc);
                //
                // ctx.PushStencilClip();

            }

            // var copy = drawDesc;
            // copy.width = 10;
            // copy.outlineWidth = 0;
            // copy.bevelTL = 0;
            // copy.bevelTR = 0;
            // copy.bevelBR = 0;
            // copy.bevelBL = 0;
            // copy.radiusTL = 0;
            // copy.radiusTR = 0;
            // copy.radiusBR = 0;
            // copy.radiusBL = 0;
            // copy.backgroundColor = Color.white;
            // ctx.DrawElement(300, 0, copy);

        }

        public override void PaintForeground3(RenderContext3 ctx) {
            if (overflowHandling != 0) {
                ctx.PopClipRect();
                // ctx.PopStencilClip();
            }
        }

    }

}