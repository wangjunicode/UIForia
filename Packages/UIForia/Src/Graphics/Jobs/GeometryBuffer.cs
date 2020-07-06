using UIForia.Util.Unsafe;
using Unity.Collections;

namespace UIForia.Graphics {

    public struct GeometryBuffer : IPerThreadCompatible {

        public PagedByteAllocator data;

        public void Dispose() {
            data.Dispose();
        }

        public void InitializeForThread(Allocator allocator) {
            data = new PagedByteAllocator(TypedUnsafe.Megabytes(1), allocator, Allocator.TempJob);
        }

        public bool IsInitialized {
            get => data.basePageByteSize != 0;
        }

    }

}