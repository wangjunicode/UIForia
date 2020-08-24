using System;
using System.Collections.Generic;
using UIForia.Elements;
using UIForia.Graphics.ShapeKit;
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

    public enum VertexSetup {

        Simple,
        Lit,
        Full

    }

    public struct UIForiaGeometryShapeData {

        public VertexSetup type;

    }

    public abstract class UIForiaMesh {

        [ThreadStatic] internal static UIVertexHelper vertexHelper;
        [ThreadStatic] internal static ThisOtherThing.UI.ShapeUtils.ShapeKit shapeKit;

        public void FillRect(float x, float y, float width, float height) {
            shapeKit.AddRect(ref vertexHelper, x, y, width, height, Color.red);
        }

        public void StrokeRect() { }

        public void FillUnityMesh(Mesh mesh) { }

    }

    public struct UIForiaVertexSimple {

        public float3 position;
        public float2 texCoord;
        public Color color;

    }

    public struct UIForiaVertexLit {

        public float3 position;
        public float3 normal;
        public float2 texCoord;
        public Color color;

    }

    public struct UIForiaVertexFull {

        public float3 position;
        public float3 normal;
        public float2 texCoord0;
        public float2 texCoord1;
        public Color color;

    }

    public class UIForiaBatchMesh_Default {

        internal StructList<float3> position;
        internal StructList<float2> texCoord;

        public void AddVertex(UIForiaVertexSimple vertex) { }

    }

    internal unsafe struct ElementBatch {

        public ElementDrawInfo* elements;
        public int count;

    }

    public unsafe class RenderContext3 : IDisposable {

        internal ushort localDrawId;
        internal ushort renderIndex;

        internal DataList<DrawInfo2> drawList;
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
        private int maskTextureId;

        private bool hasPendingMaterialOverrides;
        private DataList<MaterialPropertyOverride> materialValueOverrides;
        private MaterialPropertyOverride* currentOverrideProperties;
        private DrawInfoFlags currentFlagSet;
        private MaterialId activeMaterialId;
        internal AxisAlignedBounds2DUShort uvBorderBounds;
        internal LightList<RenderTexture> renderTextures;

        internal Dictionary<int, Material> materialMap;
        public Dictionary<int, Mesh> meshMap;

        private Gradient gradient;

        public RenderContext3(ResourceManager resourceManager) {
            this.resourceManager = resourceManager;
            this.materialMap = new Dictionary<int, Material>();
            this.meshMap = new Dictionary<int, Mesh>();
            this.textureMap = new Dictionary<int, Texture>(32);
            this.callbacks = new StructList<CommandBufferCallback>(4);
            this.drawList = new DataList<DrawInfo2>(64, Allocator.Persistent);
            this.stackAllocator = new PagedByteAllocator(TypedUnsafe.Kilobytes(16), Allocator.Persistent, Allocator.Persistent);
            this.materialValueOverrides = new DataList<MaterialPropertyOverride>(16, Allocator.Persistent);
            this.renderTextures = new LightList<RenderTexture>();
        }

        public void Setup(ElementId elementId, MaterialId materialId, int renderIndex, float4x4* transform) {
            this.elementId = elementId;
            this.localDrawId = 0;
            this.renderIndex = (ushort) renderIndex;
            this.defaultMatrix = transform;
            this.defaultBGTexture = 0;
            this.defaultOutlineTexture = 0;
            this.maskTextureId = 0;
            this.gradient = null;
            hasPendingMaterialOverrides = false;
            currentFlagSet = 0;
            activeMaterialId = materialId;
            uvBorderBounds = default;
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

        public void DrawMesh2(Mesh mesh) {
            if (mesh.vertexCount < 500) { }
            else { }
        }

        public void DrawUIForiaMesh(UIForiaMesh mesh) {
            // switch (mesh.type) {
            //     case UIForiaMeshType.Simple:
            //         positionBuffer.AddRange(mesh.positions);
            //         drawList.Add(new DrawInfo2() {
            //             matrix = defaultMatrix,
            //             drawType = DrawType2.UIForiaMesh_Simple,
            //             localBounds = mesh.GetBounds2D(),
            //             elementId = elementId,
            //             materialId = activeMaterialId,
            //             materialData = default,
            //             shapeData = default,
            //             drawSortId = new DrawSortId() {
            //                 baseRenderIdx = renderIndex,
            //                 localRenderIdx = localDrawId++
            //             }
            //             
            //         });
            //         break;
            // }
        }

        internal void AddTexture(Texture texture) {
            // GetHashCode() returns the texture's instanceId but without an IsMainThread() check
            textureMap[texture.GetHashCode()] = texture;
        }

        public void SetMaskTexture(Texture texture) {
            if (!ReferenceEquals(texture, null)) {
                maskTextureId = texture.GetHashCode();
                AddTexture(texture);
            }
        }

        public void SetMaskTexture(TextureReference textureReference) {
            if (!ReferenceEquals(textureReference?.texture, null)) {
                maskTextureId = textureReference.textureId;
                // uvBorderBounds = textureReference.uvBorderRect;
                AddTexture(textureReference.texture);
            }
        }

        public void SetBackgroundTexture(Texture texture) {
            uvBorderBounds = default;
            if (!ReferenceEquals(texture, null)) {
                defaultBGTexture = texture.GetHashCode();
                AddTexture(texture);
            }
        }

        public void SetBackgroundTexture(TextureReference textureReference) {
            if (!ReferenceEquals(textureReference?.texture, null)) {
                defaultBGTexture = textureReference.textureId;
                uvBorderBounds = textureReference.uvBorderRect;
                AddTexture(textureReference.texture);
            }
            else {
                uvBorderBounds = default;
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

        public void DrawMesh(Mesh mesh) {
            int meshId = mesh.GetHashCode();
            meshMap[meshId] = mesh;

            if (hasPendingMaterialOverrides) {
                CommitMaterialModifications();
            }

            Bounds bounds = mesh.bounds;
            DrawType2 drawType = DrawType2.Mesh2D;
            if (bounds.size.z != 0) {
                return; // todo -- mesh3d and handle culling differently
            }

            drawList.Add(new DrawInfo2() {
                matrix = defaultMatrix,
                elementId = elementId,
                drawType = drawType,
                materialId = activeMaterialId,
                localBounds = new AxisAlignedBounds2D(bounds.min.x, bounds.min.y, bounds.max.x, bounds.max.y),
                materialData = currentOverrideProperties,
                shapeData = (void*) meshId,
                drawSortId = new DrawSortId() {
                    localRenderIdx = localDrawId++,
                    baseRenderIdx = renderIndex
                }
            });
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

        public void DrawElementShadow(float x, float y, in ElementDrawDesc drawDesc) {
            if (drawDesc.width <= 0 || drawDesc.height <= 0) {
                return;
            }

            ElementDrawInfo* elementDrawInfo = stackAllocator.Allocate(new ElementDrawInfo() {
                x = x,
                y = y,
                drawDesc = drawDesc,
            });

            drawList.Add(new DrawInfo2() {
                matrix = defaultMatrix,
                elementId = elementId,
                drawType = DrawType2.UIForiaShadow,
                materialId = MaterialId.UIForiaShadow,
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
        
        public void DrawElement(float x, float y, in ElementDrawDesc drawDesc) {
            if (drawDesc.width <= 0 || drawDesc.height <= 0) {
                return;
            }

            ElementDrawInfo* elementDrawInfo = stackAllocator.Allocate(new ElementDrawInfo() {
                x = x,
                y = y,
                drawDesc = drawDesc,
            });

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
                    },
                    maskTexture = new TextureUsage() {
                        textureId = maskTextureId
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

        public void DrawSlicedElement(float x, float y, in ElementDrawDesc drawDesc) {
            if (drawDesc.width <= 0 || drawDesc.height <= 0) {
                return;
            }

            if (defaultBGTexture == 0 || !drawDesc.HasBorder) {
                DrawElement(x, y, drawDesc);
                return;
            }

            ElementDrawInfo* elementDrawInfo = stackAllocator.Allocate(new ElementDrawInfo() {
                x = x,
                y = y,
                isSliced = true,
                drawDesc = drawDesc,
                uvBorderTop = uvBorderBounds.yMin,
                uvBorderRight = uvBorderBounds.xMax,
                uvBorderBottom = uvBorderBounds.yMax,
                uvBorderLeft = uvBorderBounds.xMin
            });

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

        public struct TextMeshDesc {

            public int count;
            public TextSymbol* textRenderInfo;
            public TextMaterialInfo* materialPtr;
            public int fontAssetId;
            public int start;

        }

        public void DrawTextHighlight(in TextRenderRange render, Color32 color) {
            float x = render.localBounds.xMin;
            float y = render.localBounds.yMin;
            float width = render.localBounds.Width;
            float height = render.localBounds.Height;

            if (width <= 0 || height <= 0) {
                return;
            }

            ElementDrawInfo* elementDrawInfo = stackAllocator.Allocate(new ElementDrawInfo() {
                x = x,
                y = y,
                drawDesc = new ElementDrawDesc(width, height) {
                    backgroundColor = color
                },
            });

            drawList.Add(new DrawInfo2() {
                matrix = defaultMatrix,
                elementId = elementId,
                drawType = DrawType2.UIForiaElement,
                materialId = MaterialId.UIForiaShape,
                localBounds = render.localBounds,
                // could consider making these different allocators to keep similar types together
                materialData = stackAllocator.Allocate(default(ElementMaterialSetup)),

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

        internal void DrawTextCharacters(in TextRenderRange textRenderRange) {
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
            materialMap.Clear();
            drawList.size = 0;
            callbacks.Clear();
            stackAllocator.Clear();
            for (int i = 0; i < renderTextures.size; i++) {
                RenderTexture.ReleaseTemporary(renderTextures[i]);
            }

            renderTextures.Clear();
        }

        public void Dispose() {
            stackAllocator.Dispose();
            drawList.Dispose();
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

        public RenderTexture PushRenderTargetRegion(AxisAlignedBounds2DUShort screenBounds) {
            drawList.Add(new DrawInfo2() {
                drawType = DrawType2.PushRenderTargetRegion,
                matrix = defaultMatrix,
                elementId = elementId,
                shapeData = (void*) renderTextures.size,
                drawSortId = new DrawSortId() {
                    baseRenderIdx = renderIndex,
                    localRenderIdx = localDrawId++
                }
            });
            renderTextures.Add(RenderTexture.GetTemporary(screenBounds.Width, screenBounds.Height, 24, RenderTextureFormat.Default));
            return renderTextures[renderTextures.size - 1];
        }

        public void PopRenderTargetRegion() {
            drawList.Add(new DrawInfo2() {
                drawType = DrawType2.PopRenderTargetRegion,
                matrix = defaultMatrix,
                elementId = elementId,
                drawSortId = new DrawSortId() {
                    baseRenderIdx = renderIndex,
                    localRenderIdx = localDrawId++
                }
            });
        }

        public void SetGradient(Gradient gradient) {
            this.gradient = gradient;
        }

        public void SetMatrix(in float4x4 matrix) {
            defaultMatrix = stackAllocator.Allocate(matrix);
        }

        public void PaintElementBackground(UIElement element) {
            element.renderBox?.PaintBackground3(this);
        }

        public void PaintElementForeground(UIElement element) {
            element.renderBox?.PaintForeground3(this);
        }

        public void SetMaterial(Material material) {
            if (ReferenceEquals(material, null)) return;
            activeMaterialId = new MaterialId(material.GetHashCode());
            materialMap[activeMaterialId.index] = material;
            hasPendingMaterialOverrides = false;
            materialValueOverrides.size = 0;
        }

    }

    public struct ElementDrawInfo {

        public float x;
        public float y;
        public ElementDrawDesc drawDesc;
        public bool isSliced;
        public ushort uvBorderTop;
        public ushort uvBorderRight;
        public ushort uvBorderBottom;
        public ushort uvBorderLeft;

    }

}