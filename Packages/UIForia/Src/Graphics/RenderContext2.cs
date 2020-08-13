using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ThisOtherThing.UI;
using ThisOtherThing.UI.ShapeUtils;
using UIForia.Layout;
using UIForia.ListTypes;
using UIForia.Rendering;
using UIForia.Systems;
using UIForia.Text;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace UIForia.Graphics {

    public enum ShapeType {

        None = 0,
        Rect = 1 << 1,
        RoundedRect = 1 << 2,
        Arc = 1 << 3,
        Polygon = 1 << 4,
        Line = 1 << 5,
        Ellipse = 1 << 6,
        SDFText = 1 << 8,

    }

    [Flags]
    public enum DrawType {

        Mesh = 1 << 0,
        Shape = 1 << 1,
        Callback = 1 << 2,

        PushClipRect = 1 << 9,

        PopClipper,
        PushClipScope,

        // Shape = Rect | RoundedRect | Arc | Polygon | Line | Ellipse,

        BeginStencilClip,

        PushStencilClip

    }

    [StructLayout(LayoutKind.Explicit)]
    public struct MaterialPropertyValue {

        [FieldOffset(0)] public int textureId;
        [FieldOffset(0)] public float floatValue;
        [FieldOffset(0)] public Color colorValue;
        [FieldOffset(0)] public Vector4 vectorValue;

    }

    public struct ShapeId {

        internal int id;
        private RenderContext2 ctx;

        internal ShapeId(RenderContext2 ctx, int id) {
            this.id = id;
            this.ctx = ctx;
        }

    }

    public struct MaterialPropertyOverride : IComparable<MaterialPropertyOverride> {

        public int shaderPropertyId;
        public MaterialPropertyType propertyType;
        public MaterialPropertyValue value;

        public int CompareTo(MaterialPropertyOverride other) {
            return shaderPropertyId - other.shaderPropertyId;
        }

    }

    // for now this is a class, when styles become unmanaged this will become a struct

    public struct TextureUsage {

        public int textureId;
        public AxisAlignedBounds2DUShort uvRect; // could be ushort because pixels aligned on ints and only need max ~4k per axis
        public UVTransform uvTransform;

    }

    public struct GradientUsage {

        public int gradientId;
        public float rotation;
        public int type;

    }

    public struct ElementMeshStyle {

        public float2 size;
        public float4 borderSize;
        public MeshType meshType;
        public MeshFillDirection fillDirection;
        public MeshFillOrigin fillOrigin;
        public float fillAmount;
        public float outlineWidth; // shared with material for sdf case

    }

    public struct UVTransformPadded {

        public half uvOffsetX;
        public half uvOffsetY;
        public half uvScaleX;
        public half uvScaleY;

        public half uvTileX;
        public half uvTileY;
        public float uvRotation; // could be ushort 
        public float padding; // align on sizeof(float4)

    }

    public struct UVTransform {

        // could easily be halfs since I dont really need precision
        // could maybe pack these in ushorts using 8.8 fixed point, dont need to be really high precision
        public half uvOffsetX;
        public half uvOffsetY;
        public half uvScaleX;
        public half uvScaleY;

        public half uvTileX;
        public half uvTileY;
        public half uvRotation;

    }

    public struct ElementShadowStyle {

        public Color32 shadowColor;
        public float shadowOffsetX;
        public float shadowOffsetY;
        public float shadowSizeX;
        public float shadowSizeY;
        public float shadowSpread;
        public float shadowBlur;

    }

    public struct ElementStyle {

        public float opacity;

        public ElementMeshStyle meshStyle;
        public UVTransform uvTransform;
        public ElementShadowStyle shadowStyle;

        public Color32 backgroundColor;
        public Color32 backgroundTint;
        public Color32 outlineColor;

        public float4 maskUvs;
        public int maskTexture;

        public float outlineWidth; // (softness would be cool!) ask andres about this and inner/outer shadow

        public Rect uvBounds;

        public Texture backgroundImage;
        public BackgroundFit bgFit;

        public Color32 borerColorTop;
        public Color32 borerColorRight;
        public Color32 borerColorBottom;
        public Color32 borerColorLeft;

        public UIFixedLength bevelTopLeft;
        public UIFixedLength bevelTopRight;
        public UIFixedLength bevelBottomRight;
        public UIFixedLength bevelBottomLeft;

        public UIFixedLength radiusTopLeft;
        public UIFixedLength radiusTopRight;
        public UIFixedLength radiusBottomRight;
        public UIFixedLength radiusBottomLeft;

        public float shineX;
        public float shineY;
        public float shinePower;

        public int toneEffect;
        public float toneFactor;

        public int gradientId;
        public int backgroundImageId;

    }

    public struct ShapeCacheId {

        internal int frameId;
        internal int sourceContextId;
        internal int cacheId;

    }

    public unsafe class RenderContext2 {

        internal int frameId;
        internal int localDrawIdx;
        internal int renderCallId;
        internal MaterialId activeMaterialId;
        internal MaterialDatabase2 materialDatabase;
        internal ResourceManager resourceManager;
        internal PagedByteAllocator stackBuffer;

        private float4x4* defaultMatrix;
        private Color32 color;
        private bool hasPendingMaterialOverrides;
        private DataList<MaterialPropertyOverride> materialValueOverrides;
        private MaterialPropertyOverride* currentOverrideProperties;
        private DrawInfoFlags currentFlagSet;
        internal Dictionary<int, Texture> textureMap;

        internal LightList<Mesh> meshList;
        internal List_DrawInfo drawList;
        internal StructList<DeferredTextInfo> deferredTextList;

        private RenderSystem renderSystem;
        private int stencilClipStart;

        internal RenderContext2(RenderSystem renderSystem, ResourceManager resourceManager) {
            this.renderSystem = renderSystem;
            this.resourceManager = resourceManager;
            this.materialDatabase = resourceManager.materialDatabase;

            this.meshList = new LightList<Mesh>();
            this.drawList = new List_DrawInfo(32, Allocator.Persistent);
            this.stackBuffer = new PagedByteAllocator(TypedUnsafe.Kilobytes(16), Allocator.Persistent, Allocator.TempJob);
            this.materialValueOverrides = new DataList<MaterialPropertyOverride>(16, Allocator.Persistent);
            this.textureMap = new Dictionary<int, Texture>();
        }

        internal void Dispose() {
            stackBuffer.Dispose();
            materialValueOverrides.Dispose();
            drawList.Dispose();
        }

        internal void Clear() {
            stencilClipStart = 0;
            drawList.size = 0;
            meshList.Clear();
            stackBuffer.Clear();
            textureMap.Clear();
        }

        public void SetMaterial(MaterialId materialId) {
            activeMaterialId = materialId;
            materialValueOverrides.size = 0;
            currentOverrideProperties = null;
            hasPendingMaterialOverrides = false;
            currentFlagSet = 0;
        }

        public bool SetMaterialFloat(int shaderKey, float value) {

            if (!materialDatabase.HasFloatProperty(activeMaterialId, shaderKey)) {
                return false;
            }

            hasPendingMaterialOverrides = true;

            currentFlagSet |= (DrawInfoFlags.HasMaterialOverrides | DrawInfoFlags.HasNonTextureOverrides);

            MaterialPropertyOverride overrideProperty = new MaterialPropertyOverride() {
                propertyType = MaterialPropertyType.Float,
                shaderPropertyId = shaderKey,
                value = new MaterialPropertyValue() {
                    floatValue = value,
                }
            };

            for (int i = 0; i < materialValueOverrides.size; i++) {
                if (materialValueOverrides[i].shaderPropertyId == shaderKey) {
                    materialValueOverrides[i] = overrideProperty;
                    return true;
                }
            }

            materialValueOverrides.Add(overrideProperty);
            return true;
        }

        public bool SetMaterialVector(int shaderKey, float4 value) {

            if (!materialDatabase.HasFloatProperty(activeMaterialId, shaderKey)) {
                return false;
            }

            hasPendingMaterialOverrides = true;

            currentFlagSet |= (DrawInfoFlags.HasMaterialOverrides | DrawInfoFlags.HasNonTextureOverrides);

            MaterialPropertyOverride overrideProperty = new MaterialPropertyOverride() {
                propertyType = MaterialPropertyType.Vector,
                shaderPropertyId = shaderKey,
                value = new MaterialPropertyValue() {
                    vectorValue = value
                }
            };

            for (int i = 0; i < materialValueOverrides.size; i++) {
                if (materialValueOverrides[i].shaderPropertyId == shaderKey) {
                    materialValueOverrides[i] = overrideProperty;
                    return true;
                }
            }

            materialValueOverrides.Add(overrideProperty);
            return true;
        }

        public bool SetMaterialColor(int shaderKey, Color32 value) {

            if (!materialDatabase.HasFloatProperty(activeMaterialId, shaderKey)) {
                return false;
            }

            hasPendingMaterialOverrides = true;

            currentFlagSet |= (DrawInfoFlags.HasMaterialOverrides | DrawInfoFlags.HasNonTextureOverrides);

            MaterialPropertyOverride overrideProperty = new MaterialPropertyOverride() {
                propertyType = MaterialPropertyType.Color,
                shaderPropertyId = shaderKey,
                value = new MaterialPropertyValue() {
                    colorValue = color
                }
            };

            for (int i = 0; i < materialValueOverrides.size; i++) {
                if (materialValueOverrides[i].shaderPropertyId == shaderKey) {
                    materialValueOverrides[i] = overrideProperty;
                    return true;
                }
            }

            materialValueOverrides.Add(overrideProperty);
            return true;
        }

        private void SetFontTexture(int shaderKey, int textureId) {
            hasPendingMaterialOverrides = true;
            currentFlagSet |= DrawInfoFlags.HasMaterialOverrides;

            MaterialPropertyOverride overrideProperty = new MaterialPropertyOverride() {
                propertyType = MaterialPropertyType.Texture,
                shaderPropertyId = shaderKey,
                value = new MaterialPropertyValue() {
                    textureId = textureId
                }
            };

            MaterialPropertyOverride* matOverrides = materialValueOverrides.GetArrayPointer();
            int size = materialValueOverrides.size;
            for (int i = 0; i < size; i++) {
                if (matOverrides[i].shaderPropertyId == shaderKey) {
                    matOverrides[i] = overrideProperty;
                    return;
                }
            }

            materialValueOverrides.Add(overrideProperty);
        }

        public bool SetMaterialTexture(int shaderKey, Texture texture) {

            // todo -- may only work if exposed via property which is not what I want
            if (!materialDatabase.HasTextureProperty(activeMaterialId, shaderKey)) {
                return false;
            }

            int textureId = texture.GetHashCode(); // returns instance Id without a main thread check

            textureMap[textureId] = texture;

            hasPendingMaterialOverrides = true;
            currentFlagSet |= DrawInfoFlags.HasMaterialOverrides;

            MaterialPropertyOverride overrideProperty = new MaterialPropertyOverride() {
                propertyType = MaterialPropertyType.Texture,
                shaderPropertyId = shaderKey,
                value = new MaterialPropertyValue() {
                    textureId = textureId
                }
            };

            MaterialPropertyOverride* matOverrides = materialValueOverrides.GetArrayPointer();
            int size = materialValueOverrides.size;
            for (int i = 0; i < size; i++) {
                if (matOverrides[i].shaderPropertyId == shaderKey) {
                    matOverrides[i] = overrideProperty;
                    return true;
                }
            }

            materialValueOverrides.Add(overrideProperty);
            return true;

        }

        public bool SetMaterialTexture(string key, Texture texture) {
            return SetMaterialTexture(Shader.PropertyToID(key), texture);
        }

        public void SetColor(Color32 color) {
            this.color = color;
        }

        // change material = reset
        // draw shape = commit changes
        // draw shape + alter state = write to active set
        private void CommitMaterialModifications() {

            int size = materialValueOverrides.size;
            MaterialPropertyOverride* writePtr = stackBuffer.Allocate<MaterialPropertyOverride>(size);

            if (size > 1) {
                // todo -- bubble sort is probably better here, low item count, fully local, less overhead
                NativeSortExtension.Sort(materialValueOverrides.GetArrayPointer(), size);
            }

            TypedUnsafe.MemCpy(writePtr, materialValueOverrides.GetArrayPointer(), size);
            currentOverrideProperties = writePtr;

            hasPendingMaterialOverrides = false;

        }

        public void PushClipRect(Rect rect) {

            AxisAlignedBounds2D bounds = default;

            // todo -- get rid fo math.transform, its slow
            float3 topLeft = math.transform(*defaultMatrix, new float3(rect.xMin, rect.yMin, 0));
            float3 bottomRight = math.transform(*defaultMatrix, new float3(rect.xMax, rect.yMax, 0));

            bounds.xMin = topLeft.x;
            bounds.yMin = topLeft.y;
            bounds.xMax = bottomRight.x;
            bounds.yMax = bottomRight.y;

            drawList.Add(new DrawInfo() {
                type = DrawType.PushClipRect,
                matrix = defaultMatrix,
                shapeData = stackBuffer.Allocate(bounds)
            });

        }

        public void FillRoundRect(float x, float y, float width, float height, in Corner corner) {

            FillRoundRect(x, y, width, height, new CornerProperties() {
                topLeft = corner,
                topRight = corner,
                bottomLeft = corner,
                bottomRight = corner,
            });

        }

        public void FillRoundRect(float x, float y, float width, float height, in CornerProperties cornerProperties) {
            if (hasPendingMaterialOverrides) {
                CommitMaterialModifications();
            }

            drawList.Add(new DrawInfo() {
                type = DrawType.Shape,
                shapeType = ShapeType.RoundedRect,
                flags = currentFlagSet,
                vertexLayout = VertexLayout.UIForiaDefault, // todo -- compute via geometry instead
                matrix = defaultMatrix,
                materialId = activeMaterialId,
                materialOverrideValues = currentOverrideProperties,
                materialOverrideCount = materialValueOverrides.size,
                renderCallId = renderCallId,
                localDrawIdx = localDrawIdx++,
                shapeData = stackBuffer.Allocate(new RoundedRectData() {
                    type = ShapeMode.Fill | ShapeMode.AA,
                    x = x,
                    y = y,
                    width = width,
                    height = height,
                    color = color,
                    edgeGradient = new EdgeGradientData() { },
                    cornerProperties = cornerProperties
                })
            });
        }

        public struct StencilScope : IDisposable {

            private RenderContext2 ctx;

            internal StencilScope(RenderContext2 ctx) {
                this.ctx = ctx;
            }

            public void Dispose() {
                ctx.PopClip();
            }

        }

        public StencilScope PushStencilScope() {
            PushStencilClip();
            return new StencilScope(this);
        }

        public void FillRect(float x, float y, float width, float height) {
            if (hasPendingMaterialOverrides) {
                CommitMaterialModifications();
            }

            drawList.Add(new DrawInfo() {
                type = DrawType.Shape,
                shapeType = ShapeType.Rect,
                flags = currentFlagSet,
                vertexLayout = VertexLayout.UIForiaDefault,
                matrix = defaultMatrix,
                materialId = activeMaterialId,
                materialOverrideValues = currentOverrideProperties,
                materialOverrideCount = materialValueOverrides.size,
                renderCallId = renderCallId,
                localDrawIdx = localDrawIdx++,
                shapeData = (byte*) stackBuffer.Allocate(new RectData() {
                    type = ShapeMode.Fill | ShapeMode.AA,
                    x = x,
                    y = y,
                    width = width,
                    height = height,
                    color = color,
                    edgeGradient = new EdgeGradientData() { }
                })
            });
        }

        public void FillRect(float2 position, float2 size) {
            FillRect(position.x, position.y, size.x, size.y);
        }

        public void DrawSDFText(in TextInfo textInfo) {

            // todo -- only do this if we processed text and the 'requires render processing' flag is set
            // todo -- if text has many characters, do this with .Run()
            if (textInfo.requiresRenderProcessing) {
                for (int lineIdx = 0; lineIdx < textInfo.lineInfoList.size; lineIdx++) {
                    TextLineInfo line = textInfo.lineInfoList[lineIdx];
                    int wordStart = line.wordStart;
                    int wordEnd = wordStart + line.wordCount;

                    int charStart = textInfo.layoutSymbolList[wordStart].wordInfo.charStart;
                    int charEnd = textInfo.layoutSymbolList[wordEnd].wordInfo.charEnd;

                    for (int charIdx = charStart; charIdx < charEnd; charIdx++) {
                        ref TextSymbol symbol = ref textInfo.symbolList[charIdx];

                        if (symbol.type == TextSymbolType.FontPush) {
                            // layoutSystem.application.resourceManager.GetFontAssetInfo(symbol.fontId);
                        }

                        if (symbol.type == TextSymbolType.FontPop) {
                            // fontstack.Pop();
                        }

                        if (symbol.type == TextSymbolType.Sprite) { }

                    }

                }
            }
            else {

                if (activeMaterialId.index == MaterialId.UIForiaSDFText.index) {
                 //  int fontTextureId = resourceManager.GetFontTextureId(textInfo.textStyle.fontAssetId);
                   // SetFontTexture(Shader.PropertyToID("_MainTex"), fontTextureId);
                }

                if (hasPendingMaterialOverrides) {
                    CommitMaterialModifications();
                }

                // ctx.DrawSDFTextAndGetSpans() -> now
                // ctx.DrawSDFText() -> defer, this is default

                TextInfoRenderData* textRenderData = stackBuffer.Allocate(new TextInfoRenderData() {
                    symbolList = textInfo.symbolList,
                    alignedTextBounds = default,
                    layoutSymbolList = textInfo.layoutSymbolList,
                    lineInfoList = textInfo.lineInfoList,
                    resolvedFontSize = textInfo.resolvedFontSize
                });

                // TextInfoRenderSpan textSpan = new TextInfoRenderSpan() {
                //     outlineColor = textInfo.textStyle.outlineColor,
                //     glowColor = textInfo.textStyle.glowColor,
                //     underlayColor = textInfo.textStyle.underlayColor,
                //     faceColor = textInfo.textStyle.faceColor,
                //
                //     fontStyle = textInfo.textStyle.fontStyle,
                //
                //     textInfo = textRenderData,
                //
                //     fontSize = textInfo.resolvedFontSize,
                //     fontAssetId = textInfo.textStyle.fontAssetId,
                //
                //     faceUVScroll = textInfo.textStyle.faceUVScrollSpeed,
                //     faceUVOffset = textInfo.textStyle.faceUVOffset,
                //
                //     faceUVModeH = textInfo.textStyle.faceUVModeHorizontal,
                //     faceUVModeV = textInfo.textStyle.faceUVModeVertical,
                //
                //     rotation = textInfo.textStyle.characterRotation,
                //     vertexOffsetX = textInfo.textStyle.characterOffsetX,
                //     vertexOffsetY = textInfo.textStyle.characterOffsetY,
                //
                //     charScale = textInfo.textStyle.characterScale,
                //
                //     lineIndex = 0,
                //
                //     outlineWidth = textInfo.textStyle.outlineWidth,
                //     outlineSoftness = textInfo.textStyle.outlineSoftness,
                //
                //     glowOuter = textInfo.textStyle.glowOuter,
                //     glowOffset = textInfo.textStyle.glowOffset,
                //     glowInner = textInfo.textStyle.glowInner,
                //     glowPower = textInfo.textStyle.glowPower,
                //
                //     underlayX = textInfo.textStyle.underlayX,
                //     underlayY = textInfo.textStyle.underlayY,
                //     underlayDilate = textInfo.textStyle.underlayDilate,
                //     underlaySoftness = textInfo.textStyle.underlaySoftness,
                //
                //     faceDilate = textInfo.textStyle.faceDilate,
                //     faceSoftness = textInfo.textStyle.faceSoftness,
                //
                //     outlineUVScroll = textInfo.textStyle.outlineUVScrollSpeed,
                //     outlineUVOffset = textInfo.textStyle.outlineUVOffset
                //
                // };

                AxisAlignedBounds2D bounds = default; // todo -- figure out how to handle text overflow so i dont generate tons of lines that will just be clipped. ComputeAABBFromBounds(*overflowBounds);

                for (int i = 0; i < textInfo.lineInfoList.size; i++) {
                    ref TextLineInfo lineInfo = ref textInfo.lineInfoList[i];
                //    textSpan.lineIndex = i;
                    int lineWordStart = lineInfo.wordStart;
                    int lineWordEnd = lineWordStart + lineInfo.wordCount;

                    // todo when an effect spans multiple text spans I won't know which one renders first
                    // i either need to have 1 of them control all the text effects
                    // or somehow recompute the joins in the controlling one since I dont know which will render first
                 //   textSpan.nextSpanOnLine = null;
                 //   textSpan.prevSpanOnLine = null;

                    // if offscreen dont render

                    float3 p0 = new float3(lineInfo.x, lineInfo.y, 0);
                    float3 p1 = new float3(lineInfo.x + lineInfo.width, lineInfo.y, 0);
                    float3 p2 = new float3(lineInfo.x + lineInfo.width, lineInfo.y + lineInfo.height, 0);
                    float3 p3 = new float3(lineInfo.x, lineInfo.y + lineInfo.height, 0);

                    // todo -- bring this back in a way that isn't stupid slow
                    // todo -- only do this if entire text box isnt contained by clipper
                    // p0 = math.transform(*defaultMatrix, p0);
                    // p1 = math.transform(*defaultMatrix, p1);
                    // p2 = math.transform(*defaultMatrix, p2);
                    // p3 = math.transform(*defaultMatrix, p3);

                    float xMin = float.MaxValue;
                    float xMax = float.MinValue;
                    float yMin = float.MaxValue;
                    float yMax = float.MinValue;

                    if (p0.x < xMin) xMin = p0.x;
                    if (p1.x < xMin) xMin = p1.x;
                    if (p2.x < xMin) xMin = p2.x;
                    if (p3.x < xMin) xMin = p3.x;

                    if (p0.x > xMax) xMax = p0.x;
                    if (p1.x > xMax) xMax = p1.x;
                    if (p2.x > xMax) xMax = p2.x;
                    if (p3.x > xMax) xMax = p3.x;

                    if (p0.y < yMin) yMin = p0.y;
                    if (p1.y < yMin) yMin = p1.y;
                    if (p2.y < yMin) yMin = p2.y;
                    if (p3.y < yMin) yMin = p3.y;

                    if (p0.y > yMax) yMax = p0.y;
                    if (p1.y > yMax) yMax = p1.y;
                    if (p2.y > yMax) yMax = p2.y;
                    if (p3.y > yMax) yMax = p3.y;

                    // bool overlappingOrContains = xMax >= bounds.xMin && xMin <= bounds.xMax && yMax >= bounds.yMin && yMin <= bounds.yMax;
                    //
                    // if (!overlappingOrContains) {
                    //     continue;
                    // }

                    // textSpan.symbolStart = textInfo.layoutSymbolList[lineWordStart].wordInfo.charStart;
                    // textSpan.symbolEnd = textInfo.layoutSymbolList[lineWordEnd - 1].wordInfo.charEnd;
                    // textSpan.materialFeatureSet = GetTextFeatureSet(textSpan);
                    //
                    // drawList.Add(new DrawInfo() {
                    //     type = DrawType.Shape,
                    //     shapeType = ShapeType.SDFText,
                    //     flags = currentFlagSet,
                    //     materialId = activeMaterialId,
                    //     vertexLayout = VertexLayout.UIForiaSDFText,
                    //     matrix = defaultMatrix,
                    //     materialOverrideValues = currentOverrideProperties,
                    //     materialOverrideCount = materialValueOverrides.size,
                    //     renderCallId = renderCallId,
                    //     localDrawIdx = localDrawIdx++,
                    //     shapeData = (byte*) stackBuffer.Allocate(textSpan)
                    // });

                }

            }

        }

        private static TextMaterialFeatures GetTextFeatureSet(in TextInfoRenderSpan span) {

            TextMaterialFeatures features = default;

            if (span.faceTextureId != 0 || span.outlineTextureId != 0) {
                features |= TextMaterialFeatures.FaceTextures;
            }

            if (span.glowOffset != 0 || span.glowOuter != 0 || span.glowInner != 0 || span.glowPower != 0) {
                features |= TextMaterialFeatures.Glow;
            }

            if (span.underlayX != 0 || span.underlayY != 0 || span.underlayDilate != 0 || span.underlaySoftness != 0) {
                features |= TextMaterialFeatures.Underlay;
            }

            return features;

        }

        private static AxisAlignedBounds2D ComputeAABBFromBounds(in OverflowBounds orientedBounds) {
            float xMin = float.MaxValue;
            float xMax = float.MinValue;
            float yMin = float.MaxValue;
            float yMax = float.MinValue;
            if (orientedBounds.p0.x < xMin) xMin = orientedBounds.p0.x;
            if (orientedBounds.p1.x < xMin) xMin = orientedBounds.p1.x;
            if (orientedBounds.p2.x < xMin) xMin = orientedBounds.p2.x;
            if (orientedBounds.p3.x < xMin) xMin = orientedBounds.p3.x;

            if (orientedBounds.p0.x > xMax) xMax = orientedBounds.p0.x;
            if (orientedBounds.p1.x > xMax) xMax = orientedBounds.p1.x;
            if (orientedBounds.p2.x > xMax) xMax = orientedBounds.p2.x;
            if (orientedBounds.p3.x > xMax) xMax = orientedBounds.p3.x;

            if (orientedBounds.p0.y < yMin) yMin = orientedBounds.p0.y;
            if (orientedBounds.p1.y < yMin) yMin = orientedBounds.p1.y;
            if (orientedBounds.p2.y < yMin) yMin = orientedBounds.p2.y;
            if (orientedBounds.p3.y < yMin) yMin = orientedBounds.p3.y;

            if (orientedBounds.p0.y > yMax) yMax = orientedBounds.p0.y;
            if (orientedBounds.p1.y > yMax) yMax = orientedBounds.p1.y;
            if (orientedBounds.p2.y > yMax) yMax = orientedBounds.p2.y;
            if (orientedBounds.p3.y > yMax) yMax = orientedBounds.p3.y;

            return new AxisAlignedBounds2D(xMin, yMin, xMax, yMax);
        }

        public static MaterialId GetSDFMaterialId(in TextInfoRenderSpan span) {

            // todo -- if these all sit in the same struct we can probably just memcmp 
            if (span.faceTextureId != 0 || span.outlineTextureId != 0) {
                return MaterialId.UIForiaSDFTextEffect;
            }

            if (span.glowOffset != 0 || span.glowOuter != 0 || span.glowInner != 0 || span.glowPower != 0) {
                return MaterialId.UIForiaSDFTextEffect;
            }

            if (span.underlayX != 0 || span.underlayY != 0 || span.underlayDilate != 0 || span.underlaySoftness != 0) {
                return MaterialId.UIForiaSDFTextEffect;
            }

            return MaterialId.UIForiaSDFText;
        }

        private static int s_TextGlowColor = Shader.PropertyToID("_GlowColor");
        private static int s_TextUnderlayColor = Shader.PropertyToID("_UnderlayColor");
        private static int s_TextOutlineColor = Shader.PropertyToID("_OutlineColor");

        private void SetupTextMPB(ref TextInfoRenderSpan span) {

            // fixed (TextStyleBuffer* a = &TextStyleBuffer.defaultValue) 
            // fixed (TextStyleBuffer* b = &span.textStyleBuffer) {
            //     
            //     // Material = "UIForia::TextEffect"
            //     
            //     // material:UIForia_TextEffect.glowPower = 1;
            //     
            //     if (TypedUnsafe.MemCmp(a, b, 1)) {
            //         return;
            //     }
            //
            //     // if (span.faceTextureId != 0) {
            //     //     SetMaterialTexture(s_FaceTexture, span.textStyleBuffer.faceTextureId);
            //     // }
            //     //
            //     // if (span.faceTextureId != 0) {
            //     //     SetMaterialTexture(s_OutlineTexture, span.textStyleBuffer.faceTextureId);
            //     // }
            //     
            //     // SetMaterialColor(s_TextGlowColor, span.glowColor);
            //     // SetMaterialColor(s_TextGlowColor, span.outlineColor);
            //     // SetMaterialColor(s_TextGlowColor, span.underlayColor);
            //     // SetMaterialColor(s_TextGlowColor, span.faceColor);
            //     // SetMaterialVector(s_TextColorVector, default);
            //     // SetMaterialVector(s_TextGlowProperties, span.glowInner);
            //     // SetMaterialVector(s_TextOutlineProperties, span.glowInner);
            //     // SetMaterialVector(s_TextUnderlayProperties, span.outlineWidth);
            //     // SetMaterialVector(s_TextScaleValues, span.scaleValues);
            //     // SetMaterialVector(s_TextUVScrollSpeed, span.scaleValues);
            //     // SetMaterialVector(s_TextUVScrollOffset, span.scaleValues);
            //
            //     
            // }

        }
        // todo -- figure out what can be packed into byte / ushort

        internal void Setup(MaterialId materialId, int renderCallId, float4x4* matrix) {
            this.localDrawIdx = 0;
            this.renderCallId = renderCallId;
            this.activeMaterialId = materialId;
            this.SetMaterial(activeMaterialId);
            this.defaultMatrix = matrix;
            this.color.r = 255;
            this.color.g = 255;
            this.color.b = 255;
            this.color.a = 255;
        }

        internal GeometryWriter GetGeometryInfo(int i) {
            return new GeometryWriter();
        }

        internal MaterialWriter GetMaterialWriter(int i) {
            return new MaterialWriter();
        }

        public ShapeId GetLastShape() {

            if (drawList.size == 0) {
                throw new Exception("No draw calls");
            }

            return new ShapeId(this, drawList.size - 1);

        }

        public void BeginStencilClip() {
            stencilClipStart = drawList.size;

            drawList.Add(new DrawInfo() {
                type = DrawType.BeginStencilClip
            });

        }

        public void BeginSoftMask() {

            // drawList.Add(new DrawInfo() {
            //     type = DrawType.BeginSoftMask
            // });

        }

        private void AddDrawInfo(in DrawInfo drawInfo) {

            drawList.Add(drawInfo);
        }

        public struct StencilClipInfo {

            public bool isVisible;
            public int drawInfoCount;

        }

        public void PopClip() {
            drawList.Add(new DrawInfo() {
                type = DrawType.PopClipper
            });
        }

        public void PushStencilClip(bool renderClipShape = false) {
            if (stencilClipStart < 0) {
                return;
            }

            if (drawList.size - stencilClipStart == 0) {
                stencilClipStart = -1;
                return;
            }

            drawList.Add(new DrawInfo() {
                type = DrawType.PushStencilClip,
                localDrawIdx = localDrawIdx++,
                renderCallId = renderCallId,
                shapeType = ShapeType.None,
                shapeData = stackBuffer.Allocate(new StencilClipInfo() {
                    isVisible = renderClipShape,
                    drawInfoCount = drawList.size - stencilClipStart
                })
            });

        }

        // make sure we align on sizeof(float4)
        public struct SDFShapeMaterialInfo {

            public byte radiusTopLeft;
            public byte radiusTopRight;
            public byte radiusBottomRight;
            public byte radiusBottomLeft;

            public byte bevelTopLeft;
            public byte bevelTopRight;
            public byte bevelBottomRight;
            public byte bevelBottomLeft;

            public float opacity;
            public float outlineWidth;

            // uv transforms in alternate buffer, these are just indices. 99% of the time will be 0 (the no-op transform)
            public ushort bodyUVTransformIdx;
            public ushort outlineUVTransformIdx;

            public Color32 outlineColor;
            public Color32 backgroundColor;
            public Color32 backgroundTint;
            public float2 size;

            public float4x4 transform; // move, compress or drop this

        }

        // todo make this not suck, only percent and px really makes sense. not really sure about px with dpi scale for this either
        public static float ResolveFixedSize(float baseSize, UIFixedLength length) {
            switch (length.unit) {
                default:
                case UIFixedUnit.Unset:
                case UIFixedUnit.Pixel:
                case UIFixedUnit.Em:
                case UIFixedUnit.ViewportWidth:
                case UIFixedUnit.ViewportHeight:
                    return length.value;

                case UIFixedUnit.Percent:
                    return baseSize * length.value;
            }
        }

        // shape baking options
        // do it now
        // allocate & figure it out later
        // just need to know how many shapes we have up front, their local bounds (transform later in burst), and their shape descriptor info, and material settings

        public struct RectShapeData { }

        public struct TextShapeDesc { }

        public void DrawElement(in Matrix4x4 matrix, Size size, ElementStyle elementStyle) {

            // 3 things we might draw
            //  - border box
            //  - body / outline
            //  - shadow
            // check and draw border
            // cache before calling for internals

            bool drawBody = (elementStyle.backgroundColor.a / 255f) * elementStyle.opacity >= 0.01f || !ReferenceEquals(elementStyle.backgroundImage, null);
            bool drawOutline = (elementStyle.outlineWidth > 0 && elementStyle.outlineColor.a * elementStyle.opacity >= 0.01f);

            // todo -- branch for non sdf shape type

            float min = size.width < size.height ? size.width : size.height;

            if (min <= 0) min = 0.0001f;

            float halfMin = min * 0.5f;

            float resolvedBorderRadiusTopLeft = ResolveFixedSize(min, elementStyle.radiusTopLeft);
            float resolvedBorderRadiusTopRight = ResolveFixedSize(min, elementStyle.radiusTopRight);
            float resolvedBorderRadiusBottomLeft = ResolveFixedSize(min, elementStyle.radiusBottomLeft);
            float resolvedBorderRadiusBottomRight = ResolveFixedSize(min, elementStyle.radiusBottomRight);

            // move to shader?
            float minScale = 1f / min;
            resolvedBorderRadiusTopLeft = Mathf.Clamp(resolvedBorderRadiusTopLeft, 0, halfMin) * minScale;
            resolvedBorderRadiusTopRight = Mathf.Clamp(resolvedBorderRadiusTopRight, 0, halfMin) * minScale;
            resolvedBorderRadiusBottomLeft = Mathf.Clamp(resolvedBorderRadiusBottomLeft, 0, halfMin) * minScale;
            resolvedBorderRadiusBottomRight = Mathf.Clamp(resolvedBorderRadiusBottomRight, 0, halfMin) * minScale;

            byte b0 = (byte) (((resolvedBorderRadiusTopLeft * 1000)) * 0.5f);
            byte b1 = (byte) (((resolvedBorderRadiusTopRight * 1000)) * 0.5f);
            byte b2 = (byte) (((resolvedBorderRadiusBottomLeft * 1000)) * 0.5f);
            byte b3 = (byte) (((resolvedBorderRadiusBottomRight * 1000)) * 0.5f);
            if (drawBody || drawOutline) { }

            if (drawBody && drawOutline) { }
            else if (drawBody) {
                DrawInfo drawInfo = new DrawInfo();
                drawInfo.matrix = default;
                drawInfo.type = DrawType.Shape;
                drawInfo.shapeType = ShapeType.Rect;
                drawInfo.geometryInfo = stackBuffer.Allocate<GeometryInfo>();
                drawInfo.materialId = MaterialId.UIForiaShape;
                drawInfo.materialOverrideCount = 0;
                drawInfo.materialOverrideValues = default; // textures go here
                drawInfo.renderCallId = renderCallId;
                drawInfo.localDrawIdx = localDrawIdx++;
                // shapeList.Add(drawInfoIdx);
                // shapeList.Add(drawInfoIdx);
            }
            else if (drawOutline) { }

            if ((elementStyle.shadowStyle.shadowColor.a * elementStyle.opacity >= 0.01f)) {
                // draw shadow
            }

        }

    }

    [Flags]
    public enum ColorMode : byte {

        None = 0,
        Color = 1 << 0,
        Texture = 1 << 1,
        TextureTint = 1 << 2,
        LetterBoxTexture = 1 << 3,
        Cover = 1 << 4,
        //LinearGradient = 1 << 4,
        //RadialGradient = 1 << 5,
        //CornerGradient = 1 << 6,


    }

    public enum MaskVisibility {

        Visible,
        Hidden

    }

    public enum MaskType {

        Bounds,
        SoftGeometry, // including AA
        HardGeometry, // not including AA
        Material, // user material 
        Texture

    }

    public unsafe struct MaterialWriter { }

    // maybe this should be a shared instance
    public unsafe struct GeometryWriter {

        public int vertexCount;
        public int triangleCount;

        internal PagedByteAllocator* allocator;

        public bool changed;

        public void SetPositions(IList<float3> positions) { }

        public void SetTexCoord(int index, float4[] coords, int start = 0, int count = -1) {
            if (start < 0) {
                start = 0;
            }

            if (count < 0) {
                count = coords.Length - start;
            }

        }

        public void SetTexCoord(int index, float2[] coords, int start, int count) { }

    }

    [Flags]
    public enum ShapeMode {

        Fill = 1 << 0,
        Shadow = 1 << 1,
        Stroke = 1 << 2,
        AA = 1 << 3

    }

}