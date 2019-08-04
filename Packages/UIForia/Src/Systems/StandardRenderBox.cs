using SVGX;
using UIForia.Elements;
using UIForia.Extensions;
using UIForia.Layout;
using UIForia.Util;
using Unity.Mathematics;
using UnityEngine;
using Vertigo;
using Earcut = Vertigo.Earcut;

namespace UIForia.Rendering {

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

        private void UpdateGeometry(in Size size) {
            geometryNeedsUpdate = false;

            bool hasBorder = element.layoutResult.border.IsZero;

            Color c;
            geometry.Clear();
            // geometry.FillRectUniformBorder_Miter(size.width, size.height);

            float width = size.width;
            float height = size.height;

            geometry.ClipCornerRect(new Size(width, height), new UIForiaGeometry.CornerDef() {
                topLeftX = (25 * 0.5f),
                topLeftY = (25 * 0.5f),
                topRightX = (25 * 0.5f),
                topRightY = (25 * 0.5f),
                bottomRightX = (25 * 0.5f),
                bottomRightY = (25 * 0.5f),
                bottomLeftX = (25 * 0.5f),
                bottomLeftY = (25 * 0.5f),
            });

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

                // Now calculate the X,Y position of the upper-left corner 
                // (one of these will always be zero)
                int posX = (int) ((width - (originalWidth * ratio)) / 2);
                int posY = (int) ((height - (originalHeight * ratio)) / 2);

                // graphic.Clear(Color.White); // white padding
                // graphic.DrawImage(image, posX, posY, newWidth, newHeight);

                for (int i = 0; i < geometry.texCoordList0.size; i++) {
                    float x = (posX + positions[i].x) / (bgScaleX * newWidth);
                    float y = ((posY + positions[i].y) / (bgScaleY * -newHeight));
                    float newX = (cosX * x) - (sinX * y);
                    float newY = (sinX * x) + (cosX * y);
                    texCoord0[i].x = x;
                    texCoord0[i].y = 1 - y;
                    texCoord1[i].y = 1;
                    // texCoord1[i] = new Vector4(x, 1 - y, width, height);
                }
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

            Color32 backgroundColor = element.style.BackgroundColor;
            Color32 backgroundTint = element.style.BackgroundTint;
            Texture backgroundImage = element.style.BackgroundImage;

            if (backgroundColor.a <= 0 && backgroundImage == null) {
                return;
            }

            float packedBackgroundColor = VertigoUtil.ColorToFloat(element.style.BackgroundColor);
            if (!element.style.BackgroundColor.IsDefined()) {
                packedBackgroundColor = VertigoUtil.ColorToFloat(new Color32(0, 0, 0, 0));
            }
            
            float packedBackgroundTint = VertigoUtil.ColorToFloat(element.style.BackgroundTint);
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
            
            float borderRadiusTopLeft = element.style.BorderRadiusTopLeft.value;
            float borderRadiusTopRight = element.style.BorderRadiusTopRight.value;
            float borderRadiusBottomLeft = element.style.BorderRadiusBottomLeft.value;
            float borderRadiusBottomRight = element.style.BorderRadiusBottomRight.value;
            
            float min = math.min(element.layoutResult.actualSize.width, element.layoutResult.actualSize.height);
            
            if (min <= 0) min = 0.0001f;
            
            float halfMin = min * 0.5f;

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