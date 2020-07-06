using ThisOtherThing.UI.ShapeUtils;
using UIForia.Graphics.ShapeKit;
using UIForia.Util.Unsafe;
using Unity.Collections;

namespace UIForia.Systems {

    public unsafe struct ShapeDataBuffer : IPerThreadCompatible {

        private Allocator allocator;

        public ShapeKit* shapeKit;
        public UIVertexHelper* vertexHelper;
        public PagedByteAllocator byteAllocator;

        public void Dispose() {
            if (shapeKit != null) {
                shapeKit->Dispose();
            }

            if (vertexHelper != null) {
                vertexHelper->Dispose();
            }

            byteAllocator.Dispose();
            TypedUnsafe.Dispose(shapeKit, allocator);
            TypedUnsafe.Dispose(vertexHelper, allocator);
            this = default;
        }

        public void InitializeForThread(Allocator inputAllocator) {
            this = default;

            // input allocator is persistent, don't want that!

            this.allocator = Allocator.TempJob;

            this.shapeKit = TypedUnsafe.Malloc<ShapeKit>(allocator);
            this.vertexHelper = TypedUnsafe.Malloc<UIVertexHelper>(allocator);
            this.byteAllocator = new PagedByteAllocator(TypedUnsafe.Kilobytes(128), allocator, allocator);
            *shapeKit = new ShapeKit(allocator);
            *vertexHelper = UIVertexHelper.Create(allocator);
        }

        public bool IsInitialized {
            get => shapeKit != null && vertexHelper != null;
        }

    }

}