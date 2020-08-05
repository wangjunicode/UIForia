using System;
using System.Diagnostics;
using UIForia.Graphics;
using UIForia.Layout;
using UIForia.Rendering.Vertigo;
using UIForia.Systems;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Vertigo;
using Debug = UnityEngine.Debug;

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

        public void PaintBackground(RenderContext ctx) {
            base.PaintBackground(ctx);
            //  imageGeometry.mainTexture = ((UIImageElement) element).texture;
            // ctx.DrawBatchedGeometry(imageGeometry, new GeometryRange(0, 4, 0, 6), element.layoutResult.matrix.ToMatrix4x4());
        }

        [BurstCompile]
        private unsafe struct Job : IJob {

            public DataList<TextureRef> textures;

            public void Execute() {

                for (int i = 0; i < textures.size; i++) {
                    textures[i].channel = -1;
                }

                NativeSortExtension.Sort(textures.GetArrayPointer(), textures.size);

                int unplaced = new AtlasPacker() {width = 2048, height = 2048}.Pack(textures, 0);

                if (unplaced > 0) {
                    unplaced = new AtlasPacker() {width = 2048, height = 2048}.Pack(textures, 1);
                }
                else {
                    return;
                }

                if (unplaced > 0) {
                    unplaced = new AtlasPacker() {width = 2048, height = 2048}.Pack(textures, 2);
                }
                else {
                    return;
                }

                if (unplaced > 0) {
                    unplaced = new AtlasPacker() {width = 2048, height = 2048}.Pack(textures, 3);
                }

            }

        }

    }

    public struct Gradient {

        public int type;
        public float angle;

        // data fits in 3 float4s
        public Color32 c0;
        public Color32 c1;
        public Color32 c2;
        public Color32 c3;
        public Color32 c4;
        public Color32 c5;
        public Color32 c6;
        public Color32 c7;

        public ushort a0;
        public ushort a1;

        public ushort a2;
        public ushort a3;

        public ushort a4;
        public ushort a5;

        public ushort a6;
        public ushort a7;

        public PackedGradientColors Pack() {
            PackedGradientColors retn = default;

            return default;
        }

    }

    public struct PackedGradientColors {

        public float4 colorRange0;
        public float4 colorRange1;
        public float4 alphaRange;

    }

    [Flags]
    public enum OverflowHandling {

        Horizontal = 1 << 0,
        Vertical = 1 << 1

    }

    [DebuggerDisplay("{element.ToString()}")]
    public class StandardRenderBox2 : RenderBox {

        protected bool requireRendering;

        public ElementDrawDesc drawDesc;

        protected Texture outlineTexture;
        protected Texture backgroundTexture;
        protected int backgroundTextureId;
        protected int outlineTextureId;
        protected OverflowHandling overflowHandling;

        public override void OnInitialize() {

            drawDesc.opacity = 255;
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

            if (textureId > 0) {
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
        }

        public override void Enable() { }

        public override void OnStylePropertyChanged(StyleProperty[] propertyList, int propertyCount) {
            bool recomputeDrawing = false;

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

                    case StylePropertyId.MeshFillAmount:
                        break;

                    case StylePropertyId.MeshFillOrigin:
                        break;

                    case StylePropertyId.MeshFillDirection:
                        break;

                    case StylePropertyId.MeshType:
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

        }

        public override void PaintBackground3(RenderContext3 ctx) {

            if (requireRendering) {

                ctx.SetBackgroundTexture(backgroundTexture);
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
        protected Color32 shadowColor;
        protected Texture backgroundImage;

        protected UIFixedLength borderRadiusTopLeft;
        protected UIFixedLength borderRadiusTopRight;
        protected UIFixedLength borderRadiusBottomLeft;
        protected UIFixedLength borderRadiusBottomRight;

        protected UIFixedLength cornerBevelTopLeft;
        protected UIFixedLength cornerBevelTopRight;
        protected UIFixedLength cornerBevelBottomLeft;
        protected UIFixedLength cornerBevelBottomRight;

        protected MeshType meshType;
        protected float meshFillAmount;
        protected MeshFillOrigin meshFillOrigin;
        protected MeshFillDirection meshFillDirection;

        public MaterialId materialId;
        // public LayoutResult[] layoutTable;

        private PooledMesh mesh;
        private MaterialPropertyBlock propertyBlock;
        private static readonly int s_Main = Shader.PropertyToID("_MainTex");

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
            // renderSystem.GetTextureId();
            // renderSystem.GetTextureId();
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
            shadowColor = element.style.ShadowColor;
            materialId = element.style.Material;
            opacity = element.style.Opacity;

            meshType = element.style.MeshType;
            meshFillAmount = element.style.MeshFillAmount;
            meshFillDirection = element.style.MeshFillDirection;
            meshFillOrigin = element.style.MeshFillOrigin;

        }

        public override void OnStylePropertyChanged(StyleProperty[] propertyList, int propertyCount) {
            base.OnStylePropertyChanged(propertyList, propertyCount);

            for (int i = 0; i < propertyCount; i++) {
                ref StyleProperty property = ref propertyList[i];

                switch (property.propertyId) {
                    case StylePropertyId.MeshFillAmount:
                        meshFillAmount = property.AsFloat;
                        geometryNeedsUpdate = true;
                        break;

                    case StylePropertyId.MeshType:
                        meshType = property.AsMeshType;
                        geometryNeedsUpdate = true;
                        break;

                    case StylePropertyId.MeshFillDirection:
                        meshFillDirection = property.AsMeshFillDirection;
                        geometryNeedsUpdate = true;
                        break;

                    case StylePropertyId.MeshFillOrigin:
                        meshFillOrigin = property.AsMeshFillOrigin;
                        geometryNeedsUpdate = true;
                        break;

                    case StylePropertyId.Material:
                        materialId = property.AsMaterialId;
                        break;

                    case StylePropertyId.BackgroundTint:
                        backgroundTint = property.AsColor;
                        dataNeedsUpdate = true;
                        break;

                    case StylePropertyId.BackgroundColor:
                        backgroundColor = property.AsColor;
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

                    case StylePropertyId.ShadowColor:
                        shadowColor = property.AsColor;
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

            Vector2 pivotOffset = element.layoutResult.pivotOffset;

            // geometry.FillRect(size.width, size.height);
            geometry.FillMeshType(0, 0, width, height, meshType, meshFillOrigin, meshFillAmount, meshFillDirection);

            if (!ReferenceEquals(backgroundImage, null)) {
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
            if (backgroundColor.a <= 0 && ReferenceEquals(backgroundImage, null)) {
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

            int val = BitUtil.SetHighLowBits((int) 0, (int) colorMode);

            float viewWidth = element.View.Viewport.width;
            float viewHeight = element.View.Viewport.height;
            float emSize = element.style.GetResolvedFontSize();
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

        public void PaintBackground(RenderContext ctx) {

            // if (materialId.id != 0) {
            //     RenderFromMaterial(ctx);
            //     return;
            // }

            Size newSize = element.layoutResult.actualSize;

            // todo re-implement cache
            if (true || geometryNeedsUpdate || (newSize != lastSize)) {
                UpdateGeometry(newSize);
                lastSize = newSize;
            }

            // todo -- fix caching issue here
            if (dataNeedsUpdate) {
                UpdateMaterialData();
                dataNeedsUpdate = false;
            }

            if (shadowColor.a > 0 && opacity > 0) {

                ViewParameters viewParameters = new ViewParameters() {
                    viewWidth = element.View.Viewport.width,
                    viewHeight = element.View.Viewport.height,
                    applicationWidth = element.application.Width,
                    applicationHeight = element.application.Height
                };

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
                float x = 0; //MeasurementUtil.ResolveOffsetMeasurement(layoutTable, element, viewParameters, style.ShadowOffsetX, element.layoutResult.actualSize.width);
                float y = 0; //MeasurementUtil.ResolveOffsetMeasurement(layoutTable, element, viewParameters, style.ShadowOffsetY, element.layoutResult.actualSize.height);
                position.x += x;
                position.y += y;
                shadowGeometry.mainTexture = null;
                int val = BitUtil.SetHighLowBits((int) 0, paintMode);
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
                Vector2 pivotOffset = default; // todo -- this! new Vector2(-element.layoutBox.pivotX * s.width, -element.layoutBox.pivotY * s.height);
                shadowGeometry.FillRect(size.x, size.y, pivotOffset + position);
                ctx.DrawBatchedGeometry(shadowGeometry, new GeometryRange(shadowGeometry.positionList.size, shadowGeometry.triangleList.size), element.layoutResult.matrix.ToMatrix4x4(), clipper);
            }

            if (!didRender) {
                return;
            }

            ctx.DrawBatchedGeometry(geometry, range, element.layoutResult.GetWorldMatrix(), clipper);
        }

        public void RenderFromMaterial(RenderContext ctx) {

            // if (!element.application.materialDatabase.TryGetMaterial(materialId, out MaterialInfo info)) { 
            //     return;
            // }
            //
            // if (ReferenceEquals(mesh, null)) {
            //     mesh = new PooledMesh(null);
            // }
            //
            // Size size = element.layoutResult.actualSize;
            //
            // propertyBlock = propertyBlock ?? new MaterialPropertyBlock();
            //
            // if (!ReferenceEquals(backgroundImage, null)) {
            //     propertyBlock.SetTexture(s_Main, backgroundImage);
            // }
            //
            // element.application.materialDatabase.GetInstanceProperties(element.id, materialId, propertyBlock);
            //
            // // todo -- will want to set the properties that are expected by UI shaders
            // // clip rect is in world space it seems, I'm not sure how to replicate it. seems to be in 2 point form, x + height & width + y? seems odd
            // // propertyBlock.SetVector("_ClipRect", new Vector4(100, 100, 100, 100));
            //
            // geometry.Clear();
            // geometry.FillMeshType(0, 0, size.width, size.height, meshType, meshFillOrigin, meshFillAmount, meshFillDirection);
            // geometry.ToMesh(mesh);
            //
            // Matrix4x4 matrix = default;
            // element.layoutResult.matrix.GetMatrix4x4(ref matrix);
            // ctx.DrawMesh(mesh.mesh, info.material, propertyBlock, matrix);

        }

    }

}