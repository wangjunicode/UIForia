using System;
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

    public class StandardRenderBox : RenderBox {

        protected bool geometryNeedsUpdate;
        protected bool dataNeedsUpdate;

        protected float borderTop;
        protected float borderRight;
        protected float borderBottom;
        protected float borderLeft;
        protected Size lastSize;
        protected GeometryRange range;
        protected UIForiaGeometry geometry;

        public StandardRenderBox() {
            this.uniqueId = "UIForia::StandardRenderBox";
            this.geometry = new UIForiaGeometry();
        }

        public override Rect RenderBounds => new Rect(0, 0, element.layoutResult.actualSize.width, element.layoutResult.actualSize.height);

        public override void OnInitialize() {
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

        private float ResolveFixedSize(float baseSize, UIFixedLength length) {
            switch (length.unit) {
                case UIFixedUnit.Unset:
                case UIFixedUnit.Pixel:
                    return length.value;
                case UIFixedUnit.Percent:
                    return baseSize * length.value;
                case UIFixedUnit.Em:
                    return element.style.GetResolvedFontSize() * length.value;
                case UIFixedUnit.ViewportWidth:
                    return element.View.Viewport.width * length.value;
                case UIFixedUnit.ViewportHeight:
                    return element.View.Viewport.height * length.value;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void UpdateGeometry(in Size size) {
            geometryNeedsUpdate = false;

            Color c;
            geometry.Clear();

            float width = size.width;
            float height = size.height;
            float min = Mathf.Min(width, height);

            float bevelTopLeft = ResolveFixedSize(min, element.style.CornerBevelTopLeft);
            float bevelTopRight = ResolveFixedSize(min, element.style.CornerBevelTopRight);
            float bevelBottomRight = ResolveFixedSize(min, element.style.CornerBevelBottomRight);
            float bevelBottomLeft =ResolveFixedSize(min, element.style.CornerBevelBottomLeft);
            
            float radiusTopLeft = ResolveFixedSize(min, element.style.BorderRadiusTopLeft);
            float radiusTopRight = ResolveFixedSize(min, element.style.BorderRadiusTopRight);
            float radiusBottomRight = ResolveFixedSize(min, element.style.BorderRadiusBottomRight);
            float radiusBottomLeft = ResolveFixedSize(min, element.style.BorderRadiusBottomLeft);

            float packedBorderColorTop = VertigoUtil.ColorToFloat(element.style.BorderColorTop);
            float packedBorderColorRight = VertigoUtil.ColorToFloat(element.style.BorderColorRight);
            float packedBorderColorBottom = VertigoUtil.ColorToFloat(element.style.BorderColorBottom);
            float packedBorderColorLeft = VertigoUtil.ColorToFloat(element.style.BorderColorLeft);
            
            const float EdgeDistance0 = 0;
            const float EdgeDistance1 = 1;
            const float ReserveForObjectIndex = 0;
            
            int startVert = geometry.positionList.size;
            
            if (radiusBottomLeft > 0 ||
                radiusBottomRight > 0 ||
                radiusTopLeft > 0 ||
                radiusTopRight > 0 ||
                bevelTopRight > 0 ||
                bevelTopLeft > 0 ||
                bevelBottomLeft > 0 ||
                bevelBottomRight > 0) {
                

                geometry.ClipCornerRect(new Size(width, height), new UIForiaGeometry.CornerDef() {
                    topLeftX = bevelTopLeft,
                    topLeftY = bevelTopLeft,
                    topRightX = bevelTopRight,
                    topRightY = bevelTopRight,
                    bottomRightX = bevelBottomRight,
                    bottomRightY = bevelBottomRight,
                    bottomLeftX = bevelBottomLeft,
                    bottomLeftY = bevelBottomLeft,
                });
                
                Vector4[] texCoord1 = geometry.texCoordList1.array;
                OffsetRect border = element.layoutResult.border;
                float borderLeftAndTop = VertigoUtil.PackSizeVector(border.left, border.top);
                float borderLeftAndBottom = VertigoUtil.PackSizeVector(border.left, border.bottom);
                float borderRightAndTop = VertigoUtil.PackSizeVector(border.right, border.top);
                float borderRightAndBottom = VertigoUtil.PackSizeVector(border.right, border.bottom);
                texCoord1[startVert + 0] = new Vector4(packedBorderColorTop, packedBorderColorLeft, borderLeftAndTop, ReserveForObjectIndex);
                texCoord1[startVert + 1] = new Vector4(packedBorderColorTop, packedBorderColorLeft, borderLeftAndTop, ReserveForObjectIndex);
                texCoord1[startVert + 2] = new Vector4(packedBorderColorTop, packedBorderColorRight, borderRightAndTop, ReserveForObjectIndex);
                texCoord1[startVert + 3] = new Vector4(packedBorderColorTop, packedBorderColorRight, borderRightAndTop, ReserveForObjectIndex);
                texCoord1[startVert + 4] = new Vector4(packedBorderColorBottom, packedBorderColorRight, borderRightAndBottom, ReserveForObjectIndex);
                texCoord1[startVert + 5] = new Vector4(packedBorderColorBottom, packedBorderColorRight, borderRightAndBottom, ReserveForObjectIndex);
                texCoord1[startVert + 6] = new Vector4(packedBorderColorBottom, packedBorderColorLeft, borderLeftAndBottom, ReserveForObjectIndex);
                texCoord1[startVert + 7] = new Vector4(packedBorderColorBottom, packedBorderColorLeft, borderLeftAndBottom, ReserveForObjectIndex);
                texCoord1[startVert + 8] = new Vector4(0, 0, EdgeDistance1, ReserveForObjectIndex);
            }
            else {
                Vector4[] texCoord1 = geometry.texCoordList1.array;
                geometry.FillRectUniformBorder_Miter(width, size.height);
                texCoord1[startVert + 0] = new Vector4(packedBorderColorTop, packedBorderColorLeft, EdgeDistance0, ReserveForObjectIndex);
                texCoord1[startVert + 1] = new Vector4(packedBorderColorTop, packedBorderColorRight, EdgeDistance0, ReserveForObjectIndex);
                texCoord1[startVert + 2] = new Vector4(packedBorderColorBottom, packedBorderColorRight, EdgeDistance0, ReserveForObjectIndex);
                texCoord1[startVert + 3] = new Vector4(packedBorderColorBottom, packedBorderColorRight, EdgeDistance0, ReserveForObjectIndex);
            }

            if (element.style.BackgroundImage != null) {
                Vector3[] positions = geometry.positionList.array;
                Vector4[] texCoord0 = geometry.texCoordList0.array;
                Vector4[] texCoord1 = geometry.texCoordList1.array;

                float bgPositionX = element.style.BackgroundImageOffsetX.value;
                float bgPositionY = element.style.BackgroundImageOffsetY.value;

                float bgScaleX = element.style.BackgroundImageScaleX.value;
                float bgScaleY = element.style.BackgroundImageScaleY.value;
                float bgRotation = element.style.BackgroundImageRotation.value;


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

//                
//                
            }

            range = new GeometryRange(0, geometry.positionList.size, 0, geometry.triangleList.size);

            // corner join: Bevel | Miter | Round
            // border only
            // border uniform
            // border rounded
            // border fill only
            // background cover
            // background box (border, padding, content)
            // uv transform
        }


        public override void PaintBackground(RenderContext ctx) {
            Size newSize = element.layoutResult.actualSize;

            if (geometryNeedsUpdate || (newSize != lastSize)) {
                UpdateGeometry(newSize);
                lastSize = newSize;
            }

            Color backgroundColor = element.style.BackgroundColor;
            Color backgroundTint = element.style.BackgroundTint;
            Texture backgroundImage = element.style.BackgroundImage;

            if (backgroundColor.a <= 0 && backgroundImage == null) {
                return;
            }

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
            colorMode |= PaintMode.LetterBoxTexture;

            // could include a letterbox color in packed colors

            // todo resolve as size
           

            float min = math.min(element.layoutResult.actualSize.width, element.layoutResult.actualSize.height);

            if (min <= 0) min = 0.0001f;

            float halfMin = min * 0.5f;

            float borderRadiusTopLeft = ResolveFixedSize(min, element.style.BorderRadiusTopLeft);
            float borderRadiusTopRight = ResolveFixedSize(min, element.style.BorderRadiusTopRight);
            float borderRadiusBottomLeft = ResolveFixedSize(min, element.style.BorderRadiusBottomLeft);
            float borderRadiusBottomRight = ResolveFixedSize(min, element.style.BorderRadiusBottomRight);
            
            borderRadiusTopLeft = math.clamp(borderRadiusTopLeft, 0, halfMin) / min;
            borderRadiusTopRight = math.clamp(borderRadiusTopRight, 0, halfMin) / min;
            borderRadiusBottomLeft = math.clamp(borderRadiusBottomLeft, 0, halfMin) / min;
            borderRadiusBottomRight = math.clamp(borderRadiusBottomRight, 0, halfMin) / min;

            byte b0 = (byte) (((borderRadiusTopLeft * 1000)) * 0.5f);
            byte b1 = (byte) (((borderRadiusTopRight * 1000)) * 0.5f);
            byte b2 = (byte) (((borderRadiusBottomLeft * 1000)) * 0.5f);
            byte b3 = (byte) (((borderRadiusBottomRight * 1000)) * 0.5f);

            float packedBorderRadii = VertigoUtil.BytesToFloat(b0, b1, b2, b3);

            geometry.packedColors = new Color(packedBackgroundColor, packedBackgroundTint, (int) colorMode, 0);
            geometry.objectData = new Vector4((int) ShapeType.RoundedRect, VertigoUtil.PackSizeVector(element.layoutResult.actualSize), packedBorderRadii, 0);
            geometry.mainTexture = backgroundImage;
            ctx.DrawBatchedGeometry(geometry, range, element.layoutResult.matrix.ToMatrix4x4());
        }

    }

}