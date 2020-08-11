using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UIForia.ListTypes;
using UIForia.Text;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using Gradient = UIForia.Rendering.Gradient;

namespace UIForia.Graphics {

    internal unsafe struct ElementBatch {

        public ElementDrawInfo* elements;
        public int count;

    }

    public unsafe class RenderContext3 : IDisposable {

        internal ushort localDrawId;
        internal ushort renderIndex;

        internal DataList<DrawInfo2> drawList;
        private HeapAllocated<float4x4> dummyMatrix;
        private PagedByteAllocator stackAllocator;
        private float4x4* defaultMatrix;

        internal StructList<CommandBufferCallback> callbacks;
        internal Dictionary<int, Texture> textureMap;
        internal ResourceManager resourceManager;
        private TextMaterialInfo* textMaterialPtr;
        private ElementId elementId;
        private int stencilClipStart;
        private int defaultOutlineTexture;
        private int defaultBGTexture;
        private bool hasPendingMaterialOverrides;
        private DataList<MaterialPropertyOverride> materialValueOverrides;
        private MaterialPropertyOverride* currentOverrideProperties;
        private DrawInfoFlags currentFlagSet;
        private MaterialId activeMaterialId;

        public RenderContext3(ResourceManager resourceManager) {
            this.resourceManager = resourceManager;
            this.textureMap = new Dictionary<int, Texture>(32);
            this.callbacks = new StructList<CommandBufferCallback>(4);
            this.dummyMatrix = new HeapAllocated<float4x4>(float4x4.identity);
            this.drawList = new DataList<DrawInfo2>(64, Allocator.Persistent);
            this.stackAllocator = new PagedByteAllocator(TypedUnsafe.Kilobytes(16), Allocator.Persistent, Allocator.Persistent);
            this.materialValueOverrides = new DataList<MaterialPropertyOverride>(16, Allocator.Persistent);
        }

        public void Setup(ElementId elementId, MaterialId materialId, int renderIndex, float4x4* transform) {
            this.elementId = elementId;
            this.localDrawId = 0;
            this.renderIndex = (ushort) renderIndex;
            this.defaultMatrix = transform;
            this.defaultBGTexture = 0;
            this.defaultOutlineTexture = 0;
            this.gradient = null;
            hasPendingMaterialOverrides = false;
            currentFlagSet = 0;
            activeMaterialId = materialId;
        }

        public void Callback(object context, Action<object, CommandBuffer> callback) {

            int callbackIdx = callbacks.size;

            callbacks.Add(new CommandBufferCallback() {
                context = context,
                callback = callback
            });

            drawList.Add(new DrawInfo2() {
                matrix = defaultMatrix,
                drawType = DrawType2.Callback,
                localBounds = default,
                materialData = null,
                shapeData = stackAllocator.Allocate(callbackIdx),
                drawSortId = new DrawSortId() {
                    localRenderIdx = localDrawId++,
                    baseRenderIdx = renderIndex
                }
            });

        }

        internal void AddTexture(Texture texture) {
            // GetHashCode() returns the texture's instanceId but without an IsMainThread() check
            textureMap[texture.GetHashCode()] = texture;
        }

        public void SetBackgroundTexture(Texture texture) {
            if (!ReferenceEquals(texture, null)) {
                defaultBGTexture = texture.GetHashCode();
                AddTexture(texture);
            }

        }

        public void SetOutlineTexture(Texture texture) {
            if (!ReferenceEquals(texture, null)) {
                defaultOutlineTexture = texture.GetHashCode();
                AddTexture(texture);
            }

        }

        private void CommitMaterialModifications() {

            int size = materialValueOverrides.size;
            MaterialPropertyOverride* writePtr = stackAllocator.Allocate<MaterialPropertyOverride>(size);

            if (size > 1) {
                // todo -- bubble sort is probably better here, low item count, fully local, less overhead
                NativeSortExtension.Sort(materialValueOverrides.GetArrayPointer(), size);
            }

            TypedUnsafe.MemCpy(writePtr, materialValueOverrides.GetArrayPointer(), size);
            currentOverrideProperties = writePtr;

            hasPendingMaterialOverrides = false;

        }

        public enum GeometryType {

            Quad

        }

        public void SetMaterial(MaterialId materialId) {
            activeMaterialId = materialId;
            materialValueOverrides.size = 0;
            currentOverrideProperties = null;
            hasPendingMaterialOverrides = false;
            currentFlagSet = 0;
        }

        public void DrawQuad(float x, float y, float width, float height) {

            if (width <= 0 || height <= 0) {
                return;
            }

            if (hasPendingMaterialOverrides) {
                CommitMaterialModifications();
            }

            drawList.Add(new DrawInfo2() {
                matrix = defaultMatrix,
                elementId = elementId,
                drawType = DrawType2.UIForiaGeometry,
                materialId = activeMaterialId,
                localBounds = new AxisAlignedBounds2D(x, y, x + width, y + height),
                materialData = currentOverrideProperties,
                shapeData = (void*) (int) GeometryType.Quad,
                drawSortId = new DrawSortId() {
                    localRenderIdx = localDrawId++,
                    baseRenderIdx = renderIndex
                }
            });

        }

        public void DrawElement(float x, float y, in ElementDrawDesc drawDesc) {

            if (drawDesc.width <= 0 || drawDesc.height <= 0) {
                return;
            }

            ElementDrawInfo* elementDrawInfo = stackAllocator.Allocate(new ElementDrawInfo() {
                x = x,
                y = y,
                opacity = 1f,
                materialId = 0,
                uvTransformId = 0,
                drawDesc = drawDesc,
            });

            if (gradient != null) {
            }
            
            drawList.Add(new DrawInfo2() {
                matrix = defaultMatrix,
                elementId = elementId,
                drawType = DrawType2.UIForiaElement,
                materialId = MaterialId.UIForiaShape,
                localBounds = new AxisAlignedBounds2D(x, y, x + drawDesc.width, y + drawDesc.height),
                // could consider making these different allocators to keep similar types together
                materialData = stackAllocator.Allocate(new ElementMaterialSetup() {
                    bodyTexture = new TextureUsage() {
                        textureId = defaultBGTexture
                    }
                }),

                shapeData = stackAllocator.Allocate(new ElementBatch() {
                    count = 1,
                    elements = elementDrawInfo
                }),

                drawSortId = new DrawSortId() {
                    localRenderIdx = localDrawId++,
                    baseRenderIdx = renderIndex
                }
            });
        }

        internal void DrawElementBodyInternal(in SDFMeshDesc meshDesc, in AxisAlignedBounds2D bounds, in ElementMaterialSetup materialSetup) {
            drawList.Add(new DrawInfo2() {
                matrix = defaultMatrix,
                elementId = elementId,
                drawType = DrawType2.UIForiaElement, // todo -- generate geometry instead if material id is not what we expect
                materialId = MaterialId.UIForiaShape,
                localBounds = bounds,
                // could consider making these different allocators to keep similar types together
                materialData = stackAllocator.Allocate(materialSetup),
                shapeData = stackAllocator.Allocate(meshDesc),
                drawSortId = new DrawSortId() {
                    localRenderIdx = localDrawId++,
                    baseRenderIdx = renderIndex
                }
            });
        }

        public struct TextMeshDesc {

            public int count;
            public TextSymbol* textRenderInfo;
            public TextMaterialInfo* materialPtr;
            public int fontAssetId;
            public int start;

        }

        internal void DrawTextCharacters(TextRenderRange textRenderRange) {

            TextMaterialSetup textMaterialSetup = new TextMaterialSetup();

            textMaterialSetup.faceTexture = textRenderRange.texture0;
            textMaterialSetup.outlineTexture = textRenderRange.texture1;
            // todo this feels a bit brittle
            textMaterialSetup.fontTextureId = resourceManager.fontAssetMap[textRenderRange.fontAssetId].atlasTextureId;

            drawList.Add(new DrawInfo2() {
                matrix = defaultMatrix,
                elementId = elementId,
                drawType = DrawType2.UIForiaText,
                materialId = MaterialId.UIForiaSDFText,
                localBounds = textRenderRange.localBounds,
                // could consider making these different allocators to keep similar types together
                materialData = stackAllocator.Allocate(textMaterialSetup),
                shapeData = stackAllocator.Allocate(new TextMeshDesc() {
                    materialPtr = textMaterialPtr,
                    fontAssetId = textRenderRange.fontAssetId,
                    textRenderInfo = textRenderRange.symbols,
                    start = textRenderRange.characterRange.start,
                    count = textRenderRange.characterRange.length
                }),
                drawSortId = new DrawSortId() {
                    localRenderIdx = localDrawId++,
                    baseRenderIdx = renderIndex
                }
            });

        }

        public void Clear() {
            materialValueOverrides.size = 0;
            textureMap.Clear();
            drawList.size = 0;
            callbacks.Clear();
            stackAllocator.Clear();
        }

        public void Dispose() {
            stackAllocator.Dispose();
            drawList.Dispose();
            dummyMatrix.Dispose();
            materialValueOverrides.Dispose();
        }

        public int GetFontTextureId(int textStyleFontAssetId) {
            return resourceManager.GetFontTextureId(textStyleFontAssetId);
        }

        // todo -- if I am ABSOLUTELY sure nothing nefarious can happen to the material array while I'm prepping for render then I can avoid this copy/alloc
        internal void SetTextMaterials(in List_TextMaterialInfo textInfoMaterialBuffer) {
            if (textInfoMaterialBuffer.array != null) {
                textMaterialPtr = stackAllocator.Allocate(textInfoMaterialBuffer.array, textInfoMaterialBuffer.size);
            }
            else {
                textMaterialPtr = null;
            }
        }

        public void PushClipRect(float x, float y, float clipWidth, float clipHeight) {
            drawList.Add(new DrawInfo2() {
                elementId = elementId,
                matrix = defaultMatrix,
                drawType = DrawType2.PushClipRect,
                localBounds = new AxisAlignedBounds2D(x, y, clipWidth, clipHeight),
                drawSortId = new DrawSortId() {
                    localRenderIdx = localDrawId++,
                    baseRenderIdx = renderIndex
                }
            });

        }

        public void PopClipRect() {
            drawList.Add(new DrawInfo2() {
                matrix = defaultMatrix,
                elementId = elementId,
                drawType = DrawType2.PopClipRect,
                drawSortId = new DrawSortId() {
                    localRenderIdx = localDrawId++,
                    baseRenderIdx = renderIndex
                }
            });
        }

        public void BeginStencilClip() {
            // todo -- i think i need to defend against nesting these, can nest but not while beginning, must push before starting a new one. also need to auto push if not done at end of draw method
            stencilClipStart = drawList.size;

            drawList.Add(new DrawInfo2() {
                drawType = DrawType2.BeginStencilClip,
                matrix = defaultMatrix,
                elementId = elementId,
                drawSortId = new DrawSortId() {
                    baseRenderIdx = renderIndex,
                    localRenderIdx = localDrawId++
                }
            });

        }

        public void PushStencilClip() {

            if (stencilClipStart <= 0) {
                throw new Exception("You need to call BeginStencilClip() before pushing a stencil clip");
            }

            drawList.Add(new DrawInfo2() {
                drawType = DrawType2.PushStencilClip,
                matrix = defaultMatrix,
                elementId = elementId,
                drawSortId = new DrawSortId() {
                    baseRenderIdx = renderIndex,
                    localRenderIdx = localDrawId++
                }
                // shapeData = (void*)stencilClipStart
            });
            //Assert.IsTrue(drawList[drawList.size - stencilClipStart].drawType == DrawType2.BeginStencilClip);
            //drawList[drawList.size - stencilClipStart].shapeData = (void*) (drawList.size - stencilClipStart);

            stencilClipStart = 0;
        }

        public void PopStencilClip() {
            drawList.Add(new DrawInfo2() {
                drawType = DrawType2.PopStencilClip,
                matrix = defaultMatrix,
                elementId = elementId,
                drawSortId = new DrawSortId() {
                    baseRenderIdx = renderIndex,
                    localRenderIdx = localDrawId++
                }
            });
        }

        private Gradient gradient;
        
        public void SetGradient(Gradient gradient) {
            this.gradient = gradient;
        }

    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ElementDrawDesc {

        // all material data goes here
        // any unpacking/re-arranging will happen later when building 

        public byte radiusTL;
        public byte radiusTR;
        public byte radiusBR;
        public byte radiusBL;

        public ushort bevelTL;
        public ushort bevelTR;
        public ushort bevelBR;
        public ushort bevelBL;

        public Color32 backgroundColor;
        public Color32 backgroundTint;

        public ushort opacity;
        public ColorMode bgColorMode;
        public ColorMode outlineColorMode;
        
        public float outlineWidth;
        public Color32 outlineColor;
        public float width;
        public float height;

        public ushort uvTop;
        public ushort uvLeft;
        public ushort uvRight;
        public ushort uvBottom;

        public ushort meshFillOpenAmount;
        public ushort meshFillRotation;

        public float meshFillOffsetX;
        public float meshFillOffsetY;

        public float meshFillRadius;

        public byte meshFillDirection;
        public byte meshFillInvert;
        public ushort uvRotation;
        
        public float uvScaleX;
        public float uvScaleY;
        public float uvOffsetX;
        public float uvOffsetY;
        
        public GradientMode gradientMode;
        public float gradientRotation;
        public float gradientOffsetX;
        public float gradientOffsetY;

    }

    public struct ElementDrawInfo {

        public float x;
        public float y;
        public int materialId;
        public int uvTransformId;

        public float opacity;

        public ElementDrawDesc drawDesc;
        // other per-element data 

    }

}