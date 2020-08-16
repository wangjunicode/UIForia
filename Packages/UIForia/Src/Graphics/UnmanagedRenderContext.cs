using System;
using UIForia.Util.Unsafe;
using Unity.Collections;
using Unity.Mathematics;

namespace UIForia.Graphics {

    internal unsafe struct UnmanagedRenderContext : IDisposable {

        public ElementId elementId;
        public float4x4* defaultMatrix;
        public int defaultBGTexture;
        public int defaultOutlineTexture;
        public ushort localDrawId;
        public ushort renderIndex;
        public PagedByteAllocator stackAllocator;
        internal DataList<DrawInfo2> drawList;

        public void Setup(ElementId elementId, MaterialId materialId, int renderIndex, float4x4* transform) {
            this.elementId = elementId;
            this.localDrawId = 0;
            this.renderIndex = (ushort) renderIndex;
            this.defaultMatrix = transform;
            this.defaultBGTexture = 0;
            this.defaultOutlineTexture = 0;
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
                localBounds = new AxisAlignedBounds2D(x, y, x + drawDesc.width, y + drawDesc.height), // compute based on matrix? probably

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

        public void Initialize() {
            this.drawList = new DataList<DrawInfo2>(64, Allocator.Persistent);
            this.stackAllocator = new PagedByteAllocator(TypedUnsafe.Kilobytes(16), Allocator.Persistent, Allocator.Persistent);
        }

        public void Dispose() {
            stackAllocator.Dispose();
            drawList.Dispose();
        }

    }

}