using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UIForia.Util.Unsafe {

    public unsafe struct UnsafeSizedBuffer<T> where T : unmanaged {

        public int size;
        public readonly T* array;

        public UnsafeSizedBuffer(T* array, int size) {
            this.size = size;
            this.array = array;
        }

        public UnsafeSizedBuffer(NativeArray<T> nativeArray, int size = 0) {
            this.size = size;
            this.array = (T*) nativeArray.GetUnsafePtr();
        }
        
        public UnsafeSizedBuffer(NativeSlice<T> slice, int size = 0) {
            this.size = size;
            this.array = (T*) slice.GetUnsafePtr();
        }

    }

}