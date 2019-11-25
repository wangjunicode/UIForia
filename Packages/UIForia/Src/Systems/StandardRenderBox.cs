using System.Diagnostics;
using SVGX;
using UIForia.Layout;
using UIForia.Systems;
using UIForia.Util;
using Unity.Collections.LowLevel.Unsafe;
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
        protected UIForiaGeometry shadowGeometry;

        protected Color32 borderColorTop;
        protected Color32 borderColorRight;
        protected Color32 borderColorBottom;
        protected Color32 borderColorLeft;
        protected Color32 backgroundColor;
        protected Color32 backgroundTint;
        protected Texture backgroundImage;

        protected UIFixedLength borderRadiusTopLeft;
        protected UIFixedLength borderRadiusTopRight;
        protected UIFixedLength borderRadiusBottomLeft;
        protected UIFixedLength borderRadiusBottomRight;

        protected UIFixedLength cornerBevelTopLeft;
        protected UIFixedLength cornerBevelTopRight;
        protected UIFixedLength cornerBevelBottomLeft;
        protected UIFixedLength cornerBevelBottomRight;

        public StandardRenderBox() {
            this.uniqueId = "UIForia::StandardRenderBox";
            this.geometry = new UIForiaGeometry();
        }

        public override void OnInitialize() {
            base.OnInitialize();
            geometry.Clear();
            lastSize = new Size(-1, -1);
            geometryNeedsUpdate = true;
            dataNeedsUpdate = true;
        }

        public override void Enable() {
            borderColorTop = element.style.BorderColorTop;
            borderColorRight = element.style.BorderColorRight;
            borderColorBottom = element.style.BorderColorBottom;
            borderColorLeft = element.style.BorderColorLeft;
            backgroundColor = element.style.BackgroundColor;
            backgroundTint = element.style.BackgroundTint;
            backgroundImage = element.style.BackgroundImage;
            borderRadiusTopLeft = element.style.BorderRadiusTopLeft;
            borderRadiusTopRight = element.style.BorderRadiusTopRight;
            borderRadiusBottomLeft = element.style.BorderRadiusBottomLeft;
            borderRadiusBottomRight = element.style.BorderRadiusBottomRight;
            cornerBevelTopLeft = element.style.CornerBevelTopLeft;
            cornerBevelTopRight = element.style.CornerBevelTopRight;
            cornerBevelBottomRight = element.style.CornerBevelBottomRight;
            cornerBevelBottomLeft = element.style.CornerBevelBottomLeft;
            opacity = element.style.Opacity;
        }

        public override void OnStylePropertyChanged(StructList<StyleProperty> propertyList) {
            StyleProperty[] properties = propertyList.array;
            int count = propertyList.size;

            base.OnStylePropertyChanged(propertyList);
            
            for (int i = 0; i < count; i++) {
                ref StyleProperty property = ref properties[i];

                switch (property.propertyId) {
                    case StylePropertyId.BackgroundTint:
                        backgroundTint = property.AsColor;
                        dataNeedsUpdate = true;
                        break;
                    case StylePropertyId.BackgroundColor:
                        backgroundColor = property.AsColor;
                        dataNeedsUpdate = true;
                        dataNeedsUpdate = true;
                        break;
                    case StylePropertyId.BorderColorTop:
                        borderColorTop = property.AsColor;
                        dataNeedsUpdate = true;
                        break;
                    case StylePropertyId.BorderColorRight:
                        borderColorRight = property.AsColor;
                        dataNeedsUpdate = true;
                        break;
                    case StylePropertyId.BorderColorBottom:
                        borderColorBottom = property.AsColor;
                        dataNeedsUpdate = true;
                        break;
                    case StylePropertyId.BorderColorLeft:
                        borderColorLeft = property.AsColor;
                        dataNeedsUpdate = true;
                        break;
                    case StylePropertyId.BackgroundImage:
                        backgroundImage = property.AsTexture;
                        dataNeedsUpdate = true;
                        break;
                    case StylePropertyId.BorderRadiusBottomLeft:
                        borderRadiusBottomLeft = property.AsUIFixedLength;
                        dataNeedsUpdate = true;
                        break;
                    case StylePropertyId.BorderRadiusBottomRight:
                        borderRadiusBottomRight = property.AsUIFixedLength;
                        dataNeedsUpdate = true;
                        break;
                    case StylePropertyId.BorderRadiusTopLeft:
                        borderRadiusTopLeft = property.AsUIFixedLength;
                        dataNeedsUpdate = true;
                        break;
                    case StylePropertyId.BorderRadiusTopRight:
                        borderRadiusTopRight = property.AsUIFixedLength;
                        dataNeedsUpdate = true;
                        break;
                    case StylePropertyId.Opacity:
                        opacity = property.AsFloat;
                        dataNeedsUpdate = true;
                        break;
                    case StylePropertyId.CornerBevelTopLeft:
                        cornerBevelTopLeft = property.AsUIFixedLength;
                        dataNeedsUpdate = true;
                        break;
                    case StylePropertyId.CornerBevelTopRight:
                        cornerBevelTopRight = property.AsUIFixedLength;
                        dataNeedsUpdate = true;
                        break;
                    case StylePropertyId.CornerBevelBottomRight:
                        cornerBevelBottomRight = property.AsUIFixedLength;
                        dataNeedsUpdate = true;
                        break;
                    case StylePropertyId.CornerBevelBottomLeft:
                        cornerBevelBottomLeft = property.AsUIFixedLength;
                        dataNeedsUpdate = true;
                        break;
                    
                    case StylePropertyId.BackgroundFit:
                    case StylePropertyId.BackgroundImageScaleX:
                    case StylePropertyId.BackgroundImageScaleY:
                    case StylePropertyId.BackgroundImageRotation:
                    case StylePropertyId.BackgroundImageTileX:
                    case StylePropertyId.BackgroundImageTileY:
                    case StylePropertyId.BackgroundImageOffsetX:
                    case StylePropertyId.BackgroundImageOffsetY:
                    case StylePropertyId.ShadowColor:
                    case StylePropertyId.ShadowTint:
                    case StylePropertyId.ShadowOffsetX:
                    case StylePropertyId.ShadowOffsetY:
                    case StylePropertyId.ShadowSizeX:
                    case StylePropertyId.ShadowSizeY:
                    case StylePropertyId.ShadowIntensity:
                        dataNeedsUpdate = true;
                        break;
//                        shadowNeedsUpdate = true;
//                        break;
                }
            }
        }

        private void UpdateGeometry(in Size size) {
            geometryNeedsUpdate = false;
            dataNeedsUpdate = true;
            geometry.Clear();

            float width = size.width;
            float height = size.height;
            float min = Mathf.Min(width, height);

            float bevelTopLeft = ResolveFixedSize(element, min, cornerBevelTopLeft);
            float bevelTopRight = ResolveFixedSize(element, min, cornerBevelTopRight);
            float bevelBottomRight = ResolveFixedSize(element, min, cornerBevelBottomRight);
            float bevelBottomLeft = ResolveFixedSize(element, min, cornerBevelBottomLeft);

            float radiusTopLeft = ResolveFixedSize(element, min, borderRadiusTopLeft);
            float radiusTopRight = ResolveFixedSize(element, min, borderRadiusTopRight);
            float radiusBottomRight = ResolveFixedSize(element, min, borderRadiusBottomRight);
            float radiusBottomLeft = ResolveFixedSize(element, min, borderRadiusBottomLeft);

            Vector2 pivotOffset = element.layoutResult.pivotOffset;
            //new Vector2(-element.layoutBox.pivotX * size.width, -element.layoutBox.pivotY * size.height);

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
                geometry.FillRect(size.width, size.height, pivotOffset);
            }
            else {
                geometry.FillRect(size.width, size.height, pivotOffset);
            }

            if (backgroundImage != null) {
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
                            float x = (bgPositionX + positions[i].x - pivotOffset.x) / (bgScaleX * width);
                            float y = (bgPositionY + positions[i].y + pivotOffset.y) / (bgScaleY * -height);
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
                            float x = (posX + bgPositionX + positions[i].x - pivotOffset.x) / (bgScaleX * newWidth);
                            float y = (posY + bgPositionY + positions[i].y + pivotOffset.y) / (bgScaleY * -newHeight);
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
            if (backgroundColor.a <= 0 && backgroundImage == null) {
                if (borderColorTop.a + borderColorBottom.a + borderColorLeft.a + borderColorRight.a == 0) {
                    didRender = false;
                }
            }

            didRender = true;

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

            float resolvedBorderRadiusTopLeft = ResolveFixedSize(element, min, borderRadiusTopLeft);
            float resolvedBorderRadiusTopRight = ResolveFixedSize(element, min, borderRadiusTopRight);
            float resolvedBorderRadiusBottomLeft = ResolveFixedSize(element, min, borderRadiusBottomLeft);
            float resolvedBorderRadiusBottomRight = ResolveFixedSize(element, min, borderRadiusBottomRight);

            resolvedBorderRadiusTopLeft = Clamp(resolvedBorderRadiusTopLeft, 0, halfMin) / min;
            resolvedBorderRadiusTopRight = Clamp(resolvedBorderRadiusTopRight, 0, halfMin) / min;
            resolvedBorderRadiusBottomLeft = Clamp(resolvedBorderRadiusBottomLeft, 0, halfMin) / min;
            resolvedBorderRadiusBottomRight = Clamp(resolvedBorderRadiusBottomRight, 0, halfMin) / min;

            byte b0 = (byte) (((resolvedBorderRadiusTopLeft * 1000)) * 0.5f);
            byte b1 = (byte) (((resolvedBorderRadiusTopRight * 1000)) * 0.5f);
            byte b2 = (byte) (((resolvedBorderRadiusBottomLeft * 1000)) * 0.5f);
            byte b3 = (byte) (((resolvedBorderRadiusBottomRight * 1000)) * 0.5f);

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

            Vector4 c = default;
            unsafe {
                Vector4* cp = &c;
                Color32* b = stackalloc Color32[2];
                b[0] = backgroundColor;
                b[1] = backgroundTint;
                UnsafeUtility.MemCpy(cp, b, sizeof(Color32) * 2);

                c.z = borderLeftAndTop;
                c.w = borderRightAndBottom;
            }

            geometry.packedColors = c;

            int val = BitUtil.SetHighLowBits((int) ShapeType.RoundedRect, (int) colorMode);

            float viewWidth = element.View.Viewport.width;
            float viewHeight = element.View.Viewport.height;
            float emSize = 0; //element.style.GetResolvedFontSize(); expensive, cache this
            float resolvedCornerBevelTopLeft = UIFixedLength.Resolve(cornerBevelTopLeft, halfMin, emSize, viewWidth, viewHeight);
            float resolvedCornerBevelTopRight = UIFixedLength.Resolve(cornerBevelTopRight, halfMin, emSize, viewWidth, viewHeight);
            float resolvedCornerBevelBottomRight = UIFixedLength.Resolve(cornerBevelBottomRight, halfMin, emSize, viewWidth, viewHeight);
            float resolvedCornerBevelBottomLeft = UIFixedLength.Resolve(cornerBevelBottomLeft, halfMin, emSize, viewWidth, viewHeight);

            geometry.cornerData = new Vector4(resolvedCornerBevelTopLeft, resolvedCornerBevelTopRight, resolvedCornerBevelBottomLeft, resolvedCornerBevelBottomRight);
            geometry.objectData = new Vector4(val, VertigoUtil.PackSizeVector(element.layoutResult.actualSize), packedBorderRadii, opacity);
            geometry.mainTexture = backgroundImage;
        }

        private static float Clamp(float test, float min, float max) {
            if (test < min) return min;
            if (test > max) return max;
            return test;
        }

        public override void PaintBackground(RenderContext ctx) {
            Size newSize = element.layoutResult.actualSize;

            if (geometryNeedsUpdate || (newSize != lastSize)) {
                UpdateGeometry(newSize);
                lastSize = newSize;
            }

            // todo -- fix caching issue here
            if (dataNeedsUpdate) {
                UpdateMaterialData();
                dataNeedsUpdate = false;
            }

            if (element.style.ShadowColor.a > 0) {
                float min = math.min(element.layoutResult.actualSize.width, element.layoutResult.actualSize.height);

                if (min <= 0) min = 0.0001f;

                float halfMin = min * 0.5f;

                float resolvedBorderRadiusTopLeft = ResolveFixedSize(element, min, borderRadiusTopLeft);
                float resolvedBorderRadiusTopRight = ResolveFixedSize(element, min, borderRadiusTopRight);
                float resolvedBorderRadiusBottomLeft = ResolveFixedSize(element, min, borderRadiusBottomLeft);
                float resolvedBorderRadiusBottomRight = ResolveFixedSize(element, min, borderRadiusBottomRight);

                resolvedBorderRadiusTopLeft = Clamp(resolvedBorderRadiusTopLeft, 0, halfMin) / min;
                resolvedBorderRadiusTopRight = Clamp(resolvedBorderRadiusTopRight, 0, halfMin) / min;
                resolvedBorderRadiusBottomLeft = Clamp(resolvedBorderRadiusBottomLeft, 0, halfMin) / min;
                resolvedBorderRadiusBottomRight = Clamp(resolvedBorderRadiusBottomRight, 0, halfMin) / min;

                byte b0 = (byte) (((resolvedBorderRadiusTopLeft * 1000)) * 0.5f);
                byte b1 = (byte) (((resolvedBorderRadiusTopRight * 1000)) * 0.5f);
                byte b2 = (byte) (((resolvedBorderRadiusBottomLeft * 1000)) * 0.5f);
                byte b3 = (byte) (((resolvedBorderRadiusBottomRight * 1000)) * 0.5f);

                float packedBorderRadii = VertigoUtil.BytesToFloat(b0, b1, b2, b3);

                float viewWidth = element.View.Viewport.width;
                float viewHeight = element.View.Viewport.height;

                UIStyleSet style = element.style;
                shadowGeometry = shadowGeometry ?? new UIForiaGeometry();
                shadowGeometry.Clear();
                int paintMode = (int) ((style.ShadowTint.a > 0) ? PaintMode.ShadowTint : PaintMode.Shadow);
                Vector2 position = Vector2.zero;
                Vector2 size = element.layoutResult.actualSize + new Vector2(style.ShadowSizeX, style.ShadowSizeY) + new Vector2(style.ShadowIntensity, style.ShadowIntensity);
                position -= new Vector2(style.ShadowSizeX, style.ShadowSizeY) * 0.5f;
                position -= new Vector2(style.ShadowIntensity, style.ShadowIntensity) * 0.5f;
                float x = 0; // todo -- pivot n stuff MeasurementUtil.ResolveOffsetMeasurement(element.layoutBox, viewWidth, viewHeight, style.ShadowOffsetX, element.layoutResult.actualSize.width);
                float y = 0; // todo -- pivot n stuff MeasurementUtil.ResolveOffsetMeasurement(element.layoutBox, viewWidth, viewHeight, style.ShadowOffsetY, element.layoutResult.actualSize.height);
                position.x += x;
                position.y += y;
                shadowGeometry.mainTexture = null;
                int val = BitUtil.SetHighLowBits((int) ShapeType.RoundedRect, paintMode);
                shadowGeometry.objectData = geometry.objectData;
                shadowGeometry.objectData.x = val;
                shadowGeometry.objectData.y = VertigoUtil.PackSizeVector(size);
                shadowGeometry.objectData.z = packedBorderRadii;

                Vector4 v = default;

                unsafe {
                    Vector4* vp = &v;
                    Color32* b = stackalloc Color32[2];
                    b[0] = style.ShadowColor;
                    b[1] = style.ShadowTint;
                    UnsafeUtility.MemCpy(vp, b, sizeof(Color32) * 2);
                    v.z = style.ShadowIntensity;
                    v.w = style.ShadowOpacity;
                }

                float emSize = 0; //element.style.GetResolvedFontSize(); expensive, cache this
                float resolvedCornerBevelTopLeft = UIFixedLength.Resolve(cornerBevelTopLeft, halfMin, emSize, viewWidth, viewHeight);
                float resolvedCornerBevelTopRight = UIFixedLength.Resolve(cornerBevelTopRight, halfMin, emSize, viewWidth, viewHeight);
                float resolvedCornerBevelBottomRight = UIFixedLength.Resolve(cornerBevelBottomRight, halfMin, emSize, viewWidth, viewHeight);
                float resolvedCornerBevelBottomLeft = UIFixedLength.Resolve(cornerBevelBottomLeft, halfMin, emSize, viewWidth, viewHeight);

                shadowGeometry.cornerData = new Vector4(resolvedCornerBevelTopLeft, resolvedCornerBevelTopRight, resolvedCornerBevelBottomRight, resolvedCornerBevelBottomLeft);
                shadowGeometry.packedColors = v;
                Size s = element.layoutResult.actualSize;
                Vector2 pivotOffset = default; // todo -- this! new Vector2(-element.layoutBox.pivotX * s.width, -element.layoutBox.pivotY * s.height);
                shadowGeometry.FillRect(size.x, size.y, pivotOffset + position);
                ctx.DrawBatchedGeometry(shadowGeometry, new GeometryRange(shadowGeometry.positionList.size, shadowGeometry.triangleList.size), element.layoutResult.matrix.ToMatrix4x4(), clipper);
            }

            if (!didRender) {
                return;
            }

            Matrix4x4 matrix = default;
            element.layoutResult.matrix.GetMatrix4x4(ref matrix);
            ctx.DrawBatchedGeometry(geometry, range, matrix, clipper);
        }

    }

}