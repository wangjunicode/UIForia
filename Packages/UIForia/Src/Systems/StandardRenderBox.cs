using System;
using System.Diagnostics;
using SVGX;
using UIForia.Elements;
using UIForia.Extensions;
using UIForia.Layout;
using UIForia.Util;
using Unity.Mathematics;
using UnityEngine;
using Vertigo;

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

        public override Rect RenderBounds { get; }

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
        protected Size lastSize;
        protected GeometryRange range;
        protected UIForiaGeometry geometry;

        public StandardRenderBox() {
            this.uniqueId = "UIForia::StandardRenderBox";
            this.geometry = new UIForiaGeometry();
        }

        public override Rect RenderBounds => new Rect(0, 0, element.layoutResult.actualSize.width, element.layoutResult.actualSize.height);

        public override void OnInitialize() {
            base.OnInitialize();
            geometry.Clear();
            lastSize = new Size(-1, -1);
            geometryNeedsUpdate = true;
            dataNeedsUpdate = true;
        }

        public override void OnStylePropertyChanged(StructList<StyleProperty> propertyList) {
            StyleProperty[] properties = propertyList.array;
            int count = propertyList.size;

            bool uvsDirty;
            bool colorsDirty;
            bool geometryDirty;

            for (int i = 0; i < count; i++) {
                ref StyleProperty property = ref properties[i];

                switch (property.propertyId) {
                    case StylePropertyId.BackgroundColor:
                        break;

                    case StylePropertyId.BorderColorTop:

                    case StylePropertyId.BorderColorRight:

                    case StylePropertyId.BorderColorBottom:

                    case StylePropertyId.BorderColorLeft:
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
                geometry.ClipCornerRect(new Size(width, height), new CornerDefinition() {
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

        // todo move material update out of paint function
        public override void PaintBackground(RenderContext ctx) {
            Size newSize = element.layoutResult.actualSize;

            if (geometryNeedsUpdate || (newSize != lastSize)) {
                UpdateGeometry(newSize);
                lastSize = newSize;
            }

            Color backgroundColor = element.style.BackgroundColor;
            Color backgroundTint = element.style.BackgroundTint;
            Texture backgroundImage = element.style.BackgroundImage;

            Color32 borderColorTop = element.style.BorderColorTop;
            Color32 borderColorRight = element.style.BorderColorRight;
            Color32 borderColorBottom = element.style.BorderColorBottom;
            Color32 borderColorLeft = element.style.BorderColorLeft;
            
            // todo -- border also 0
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

            float packedBorderRadii = VertigoUtil.BytesToFloat(b0, b1, b3, b2);

            float packedBorderColorTop = VertigoUtil.ColorToFloat(borderColorTop);
            float packedBorderColorRight = VertigoUtil.ColorToFloat(borderColorRight);
            float packedBorderColorBottom = VertigoUtil.ColorToFloat(borderColorBottom);
            float packedBorderColorLeft = VertigoUtil.ColorToFloat(borderColorLeft);

            geometry.miscData = new Vector4(packedBorderColorTop, packedBorderColorRight, packedBorderColorBottom, packedBorderColorLeft);
            OffsetRect border = element.layoutResult.border;

            float borderLeftAndTop = VertigoUtil.PackSizeVector(border.left, border.top);
            float borderRightAndBottom = VertigoUtil.PackSizeVector(border.right, border.bottom);

            geometry.packedColors = new Color(packedBackgroundColor, packedBackgroundTint, borderLeftAndTop, borderRightAndBottom);
            geometry.objectData = new Vector4((int) ShapeType.RoundedRect, VertigoUtil.PackSizeVector(element.layoutResult.actualSize), packedBorderRadii, (int) colorMode);
            geometry.mainTexture = backgroundImage;

            ctx.DrawBatchedGeometry(geometry, range, element.layoutResult.matrix.ToMatrix4x4(), clipper);
            
        }

    }

}