using System;
using System.Diagnostics;
using SVGX;
using UIForia.Elements;
using UIForia.Extensions;
using UIForia.Layout;
using UIForia.Util;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using Vertigo;
using ColorMode = SVGX.ColorMode;

namespace UIForia.Rendering {

    public enum BackgroundFit {

        Unset = 0,
        ScaleDown = 1 << 0,
        Cover = 1 << 1,
        Contain = 1 << 2,
        Fill = 1 << 3,
        None = 1 << 4

    }

    public class ImageRenderBox : StandardRenderBox {

        private UIForiaGeometry imageGeometry;

        public override void OnInitialize() {
            base.OnInitialize();
            imageGeometry = new UIForiaGeometry();
        }

        public override void PaintBackground(RenderContext ctx) {
            base.PaintBackground(ctx);
            //  imageGeometry.mainTexture = ((UIImageElement) element).texture;
            // ctx.DrawBatchedGeometry(imageGeometry, new GeometryRange(0, 4, 0, 6), element.layoutResult.matrix.ToMatrix4x4());
        }

    }

    [DebuggerDisplay("{element.ToString()}")]
    public class StandardRenderBox : RenderBox {

        protected bool geometryNeedsUpdate;
        protected bool dataNeedsUpdate;
        protected bool shadowNeedsUpdate;
        protected bool hasShadow;
        protected Size lastSize;
        protected GeometryRange range;
        protected UIForiaGeometry geometry;
        protected UIForiaGeometry shadowGeometry;

        public StandardRenderBox() {
            this.uniqueId = "UIForia::StandardRenderBox";
            this.geometry = new UIForiaGeometry();
        }

//        public override Rect RenderBounds => new Rect(0, 0, element.layoutResult.actualSize.width, element.layoutResult.actualSize.height);

        public override void OnInitialize() {
            base.OnInitialize();
            geometry.Clear();
            lastSize = new Size(-1, -1);
            geometryNeedsUpdate = true;
            dataNeedsUpdate = true;
            shadowNeedsUpdate = true;
        }

        private void UpdateShadow() {
            shadowGeometry = shadowGeometry ?? new UIForiaGeometry();
            shadowGeometry.Clear();

            Size size = element.layoutResult.actualSize;

            float width = size.width;
            float height = size.height;
            float min = Mathf.Min(width, height);

            float bevelTopLeft = ResolveFixedSize(element, min, element.style.CornerBevelTopLeft);
            float bevelTopRight = ResolveFixedSize(element, min, element.style.CornerBevelTopRight);
            float bevelBottomRight = ResolveFixedSize(element, min, element.style.CornerBevelBottomRight);
            float bevelBottomLeft = ResolveFixedSize(element, min, element.style.CornerBevelBottomLeft);

            float radiusTopLeft = ResolveFixedSize(element, min, element.style.BorderRadiusTopLeft);
            float radiusTopRight = ResolveFixedSize(element, min, element.style.BorderRadiusTopRight);
            float radiusBottomRight = ResolveFixedSize(element, min, element.style.BorderRadiusBottomRight);
            float radiusBottomLeft = ResolveFixedSize(element, min, element.style.BorderRadiusBottomLeft);

            if (radiusBottomLeft > 0 ||
                radiusBottomRight > 0 ||
                radiusTopLeft > 0 ||
                radiusTopRight > 0 ||
                bevelTopRight > 0 ||
                bevelTopLeft > 0 ||
                bevelBottomLeft > 0 ||
                bevelBottomRight > 0) {
                shadowGeometry.ClipCornerRect(size, new CornerDefinition() {
                    topLeftX = bevelTopLeft,
                    topLeftY = bevelTopLeft,
                    topRightX = bevelTopRight,
                    topRightY = bevelTopRight,
                    bottomRightX = bevelBottomRight,
                    bottomRightY = bevelBottomRight,
                    bottomLeftX = bevelBottomLeft,
                    bottomLeftY = bevelBottomLeft,
                });
            }
            else {
                shadowGeometry.FillRect(size.width, size.height);
            }
        }

        public override void OnStylePropertyChanged(StructList<StyleProperty> propertyList) {
            StyleProperty[] properties = propertyList.array;
            int count = propertyList.size;


            for (int i = 0; i < count; i++) {
                ref StyleProperty property = ref properties[i];

                switch (property.propertyId) {
                    case StylePropertyId.BackgroundColor:
                    case StylePropertyId.BorderColorTop:
                    case StylePropertyId.BorderColorRight:
                    case StylePropertyId.BorderColorBottom:
                    case StylePropertyId.BorderColorLeft:
                    case StylePropertyId.BackgroundImage:
                    case StylePropertyId.BackgroundFit:
                    case StylePropertyId.BorderRadiusBottomLeft:
                    case StylePropertyId.BorderRadiusBottomRight:
                    case StylePropertyId.BorderRadiusTopLeft:
                    case StylePropertyId.BorderRadiusTopRight:
                    case StylePropertyId.CornerBevelTopLeft:
                    case StylePropertyId.CornerBevelTopRight:
                    case StylePropertyId.CornerBevelBottomRight:
                    case StylePropertyId.CornerBevelBottomLeft:
                    case StylePropertyId.BackgroundImageScaleX:
                    case StylePropertyId.BackgroundImageScaleY:
                    case StylePropertyId.BackgroundImageRotation:
                    case StylePropertyId.BackgroundImageTileX:
                    case StylePropertyId.BackgroundImageTileY:
                    case StylePropertyId.BackgroundImageOffsetX:
                    case StylePropertyId.BackgroundImageOffsetY:
                        dataNeedsUpdate = true;
                        break;
                    case StylePropertyId.ShadowColor:
                    case StylePropertyId.ShadowTint:
                    case StylePropertyId.ShadowOffsetX:
                    case StylePropertyId.ShadowOffsetY:
                    case StylePropertyId.ShadowSizeX:
                    case StylePropertyId.ShadowSizeY:
                    case StylePropertyId.ShadowIntensity:
                        shadowNeedsUpdate = true;
                        break;
                }
            }
        }

        private void UpdateGeometry(in Size size) {
            geometryNeedsUpdate = false;

            geometry.Clear();

            float width = size.width;
            float height = size.height;
            float min = Mathf.Min(width, height);

            float bevelTopLeft = ResolveFixedSize(element, min, element.style.CornerBevelTopLeft);
            float bevelTopRight = ResolveFixedSize(element, min, element.style.CornerBevelTopRight);
            float bevelBottomRight = ResolveFixedSize(element, min, element.style.CornerBevelBottomRight);
            float bevelBottomLeft = ResolveFixedSize(element, min, element.style.CornerBevelBottomLeft);

            float radiusTopLeft = ResolveFixedSize(element, min, element.style.BorderRadiusTopLeft);
            float radiusTopRight = ResolveFixedSize(element, min, element.style.BorderRadiusTopRight);
            float radiusBottomRight = ResolveFixedSize(element, min, element.style.BorderRadiusBottomRight);
            float radiusBottomLeft = ResolveFixedSize(element, min, element.style.BorderRadiusBottomLeft);


            if (radiusBottomLeft > 0 ||
                radiusBottomRight > 0 ||
                radiusTopLeft > 0 ||
                radiusTopRight > 0 ||
                bevelTopRight > 0 ||
                bevelTopLeft > 0 ||
                bevelBottomLeft > 0 ||
                bevelBottomRight > 0) {
//                geometry.ClipCornerRect(size, new CornerDefinition() {
//                    topLeftX = bevelTopLeft,
//                    topLeftY = bevelTopLeft,
//                    topRightX = bevelTopRight,
//                    topRightY = bevelTopRight,
//                    bottomRightX = bevelBottomRight,
//                    bottomRightY = bevelBottomRight,
//                    bottomLeftX = bevelBottomLeft,
//                    bottomLeftY = bevelBottomLeft,
//                });
                geometry.FillRect(size.width, size.height);

            }
            else {
                geometry.FillRect(size.width, size.height);
            }

            if (element.style.BackgroundImage != null) {
                Vector3[] positions = geometry.positionList.array;
                Vector4[] texCoord0 = geometry.texCoordList0.array;

                float bgPositionX = element.style.BackgroundImageOffsetX.value;
                float bgPositionY = element.style.BackgroundImageOffsetY.value;

                float bgScaleX = element.style.BackgroundImageScaleX;
                float bgScaleY = element.style.BackgroundImageScaleY;
                float bgRotation = element.style.BackgroundImageRotation;

                float sinX = Mathf.Sin(bgRotation * Mathf.Deg2Rad);
                float cosX = Mathf.Cos(bgRotation * Mathf.Deg2Rad);

                float originalWidth = element.style.BackgroundImage.width;
                float originalHeight = element.style.BackgroundImage.height;

                float ratioX = width / element.style.BackgroundImage.width;
                float ratioY = height / element.style.BackgroundImage.height;

                // use whichever multiplier is smaller
                float ratio = ratioX < ratioY ? ratioX : ratioY;

                // now we can get the new height and width
                int newHeight = (int) (originalHeight * ratio);
                int newWidth = (int) (originalWidth * ratio);

                // Now calculate the X,Y position of the upper-left corner (one of these will always be zero)
                int posX = (int) ((width - (originalWidth * ratio)) / 2);
                int posY = (int) ((height - (originalHeight * ratio)) / 2);

                switch (element.style.BackgroundFit) {
                    case BackgroundFit.Fill:
                        for (int i = 0; i < geometry.texCoordList0.size; i++) {
                            float x = (bgPositionX + positions[i].x) / (bgScaleX * width);
                            float y = (bgPositionY + positions[i].y) / (bgScaleY * -height);
                            float newX = (cosX * x) - (sinX * y);
                            float newY = (sinX * x) + (cosX * y);
                            texCoord0[i].x = newX;
                            texCoord0[i].y = 1 - newY;
                        }

                        break;

                    case BackgroundFit.ScaleDown:
                        break;

                    case BackgroundFit.Cover:
                        break;

                    case BackgroundFit.Contain:
                        for (int i = 0; i < geometry.texCoordList0.size; i++) {
                            float x = (posX + bgPositionX + positions[i].x) / (bgScaleX * newWidth);
                            float y = (posY + bgPositionY + positions[i].y) / (bgScaleY * -newHeight);
                            float newX = (cosX * x) - (sinX * y);
                            float newY = (sinX * x) + (cosX * y);
                            texCoord0[i].x = newX;
                            texCoord0[i].y = 1 - newY;
                        }

                        break;

                    case BackgroundFit.None:

                        break;
                }
            }

            range = new GeometryRange(0, geometry.positionList.size, 0, geometry.triangleList.size);
        }

        private void UpdateMaterialData() {
            Color backgroundColor = element.style.BackgroundColor;
            Color backgroundTint = element.style.BackgroundTint;
            Texture backgroundImage = element.style.BackgroundImage;

            Color32 borderColorTop = element.style.BorderColorTop;
            Color32 borderColorRight = element.style.BorderColorRight;
            Color32 borderColorBottom = element.style.BorderColorBottom;
            Color32 borderColorLeft = element.style.BorderColorLeft;

            if (backgroundColor.a <= 0 && backgroundImage == null) {
                didRender = false;
                if (borderColorTop.a <= 0 && borderColorRight.a <= 0 && borderColorLeft.a <= 0 && borderColorBottom.a <= 0) {
                    return;
                }
            }

            didRender = true;
            float packedBackgroundColor = VertigoUtil.ColorToFloat(backgroundColor);
            float packedBackgroundTint = VertigoUtil.ColorToFloat(backgroundTint);

            PaintMode colorMode = PaintMode.None;

            if (backgroundImage != null) {
                colorMode |= PaintMode.Texture;
            }

            if (backgroundTint.a > 0) {
                colorMode |= PaintMode.TextureTint;
            }

            if (backgroundColor.a > 0) {
                colorMode |= PaintMode.Color;
            }

            // if keeping aspect ratio
            // could include a letterbox color in packed colors
            colorMode |= PaintMode.LetterBoxTexture;

            float min = math.min(element.layoutResult.actualSize.width, element.layoutResult.actualSize.height);

            if (min <= 0) min = 0.0001f;

            float halfMin = min * 0.5f;

            float borderRadiusTopLeft = ResolveFixedSize(element, min, element.style.BorderRadiusTopLeft);
            float borderRadiusTopRight = ResolveFixedSize(element, min, element.style.BorderRadiusTopRight);
            float borderRadiusBottomLeft = ResolveFixedSize(element, min, element.style.BorderRadiusBottomLeft);
            float borderRadiusBottomRight = ResolveFixedSize(element, min, element.style.BorderRadiusBottomRight);

            borderRadiusTopLeft = math.clamp(borderRadiusTopLeft, 0, halfMin) / min;
            borderRadiusTopRight = math.clamp(borderRadiusTopRight, 0, halfMin) / min;
            borderRadiusBottomLeft = math.clamp(borderRadiusBottomLeft, 0, halfMin) / min;
            borderRadiusBottomRight = math.clamp(borderRadiusBottomRight, 0, halfMin) / min;

            byte b0 = (byte) (((borderRadiusTopLeft * 1000)) * 0.5f);
            byte b1 = (byte) (((borderRadiusTopRight * 1000)) * 0.5f);
            byte b2 = (byte) (((borderRadiusBottomLeft * 1000)) * 0.5f);
            byte b3 = (byte) (((borderRadiusBottomRight * 1000)) * 0.5f);

            float packedBorderRadii = VertigoUtil.BytesToFloat(b0, b1, b2, b3);

            // HUGE FUCKING WARNING:
            // the following code is what I humbly refer to as: "Making C# my bitch"
            // Here's whats going on. We want to pass colors into the shader
            // we want to use 32 bits for a color instead of 128 (4 floats per color)
            // we can only send float values to the shaders thanks to unity limitations.
            // if we want to reinterpret the value directly as a float, the language has some
            // safety features that prevent float overflows, if the value we pass in is too large
            // then we get one of bits in our float flipped. We don't want this since it gives
            // the wrong value in the shader. For example if we pass in the color (128, 128, 128, 255)
            // we actually decode (128, 128, 192, 255) in the shader. This is bad.
            // 
            // the below major hack skips the type system entirely but just setting bytes directly in memory 
            // which the runtime never checks since we never assigned to a float value. Awesome!

            Vector4 v = default;

            unsafe {
                Vector4* vp = &v;
                Color32* b = stackalloc Color32[4];
                b[0] = borderColorTop;
                b[1] = borderColorRight;
                b[2] = borderColorBottom;
                b[3] = borderColorLeft;
                UnsafeUtility.MemCpy(vp, b, sizeof(Color32) * 4);
            }

            geometry.miscData = v;

            OffsetRect border = element.layoutResult.border;

            float borderLeftAndTop = VertigoUtil.PackSizeVector(border.left, border.top);
            float borderRightAndBottom = VertigoUtil.PackSizeVector(border.right, border.bottom);

            geometry.packedColors = new Color(packedBackgroundColor, packedBackgroundTint, borderLeftAndTop, borderRightAndBottom);

            int val = BitUtil.SetHighLowBits((int)ShapeType.RoundedRect, (int) colorMode);

            geometry.objectData = new Vector4(val, VertigoUtil.PackSizeVector(element.layoutResult.actualSize), packedBorderRadii, element.style.Opacity);
            geometry.mainTexture = backgroundImage;
        }

        // todo move material update out of paint function
        public override void PaintBackground(RenderContext ctx) {
            Size newSize = element.layoutResult.actualSize;

            if (geometryNeedsUpdate || (newSize != lastSize)) {
                UpdateGeometry(newSize);
                lastSize = newSize;
            }

            if (dataNeedsUpdate) {
                UpdateMaterialData();
                dataNeedsUpdate = false;
            }

            if (element.style.ShadowColor.a > 0) {
                UIStyleSet style = element.style;
                shadowGeometry = shadowGeometry ?? new UIForiaGeometry();
                shadowGeometry.Clear();
                int paintMode = (int) ((style.ShadowTint.a > 0) ? PaintMode.ShadowTint : PaintMode.Shadow);
                Vector2 position = element.layoutResult.localPosition;
                Vector2 size = element.layoutResult.actualSize + new Vector2(style.ShadowSizeX, style.ShadowSizeY) + new Vector2(style.ShadowIntensity, style.ShadowIntensity);
                position -= new Vector2(style.ShadowSizeX, style.ShadowSizeY) * 0.5f;
                position -= new Vector2(style.ShadowIntensity, style.ShadowIntensity) * 0.5f;
           //     position += new Vector2(style.ShadowOffsetX, style.ShadowOffsetY);
                float x = MeasurementUtil.ResolveOffsetMeasurement(element.layoutBox, element.View.Viewport.width, element.View.Viewport.height, style.ShadowOffsetX, element.layoutResult.actualSize.width);
                float y = MeasurementUtil.ResolveOffsetMeasurement(element.layoutBox, element.View.Viewport.width, element.View.Viewport.height, style.ShadowOffsetY, element.layoutResult.actualSize.height);
                position.x += x;
                position.y += y;
                shadowGeometry.mainTexture = null;
                int val = BitUtil.SetHighLowBits((int)ShapeType.RoundedRect, paintMode);
                shadowGeometry.objectData = geometry.objectData;
                shadowGeometry.objectData.x = val;
                shadowGeometry.objectData.y = VertigoUtil.PackSizeVector(size);
                Vector4 v = default;

                unsafe {
                    Vector4* vp = &v;
                    Color32* b = stackalloc Color32[2];
                    b[0] = style.ShadowColor;
                    b[1] = style.ShadowTint;
                    UnsafeUtility.MemCpy(vp, b, sizeof(Color32) * 2);
                    v.z = style.ShadowIntensity;
                }

                shadowGeometry.packedColors = v;
                shadowGeometry.miscData = default;
                shadowGeometry.FillRect(size.x, size.y, position);
                ctx.DrawBatchedGeometry(shadowGeometry, new GeometryRange(shadowGeometry.positionList.size, shadowGeometry.triangleList.size), element.layoutResult.matrix.ToMatrix4x4(), clipper);    
            }

            ctx.DrawBatchedGeometry(geometry, range, element.layoutResult.matrix.ToMatrix4x4(), clipper);
        }

    }

}