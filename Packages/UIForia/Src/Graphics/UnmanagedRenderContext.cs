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