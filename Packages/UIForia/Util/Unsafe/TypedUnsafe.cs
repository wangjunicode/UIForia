using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UIForia.Util.Unsafe {

    public static unsafe class TypedUnsafe {

        public static T* Malloc<T>(int count, Allocator allocator) where T : unmanaged {
            return (T*) UnsafeUtility.Malloc(count * sizeof(T), UnsafeUtility.AlignOf<T>(), allocator);
        }

        public static T* MallocDefault<T>(Allocator allocator) where T : unmanaged {
            T* retn = (T*) UnsafeUtility.Malloc(sizeof(T), UnsafeUtility.AlignOf<T>(), allocator);
            UnsafeUtility.MemClear(retn, sizeof(T));
            return retn;
        }

        public static T* MallocDefault<T>(uint count, Allocator allocator) where T : unmanaged {
            T* retn = (T*) UnsafeUtility.Malloc(count * sizeof(T), UnsafeUtility.AlignOf<T>(), allocator);
            UnsafeUtility.MemClear(retn, count * sizeof(T));
            return retn;
        }
        
        public static T* MallocDefault<T>(int count, Allocator allocator) where T : unmanaged {
            T* retn = (T*) UnsafeUtility.Malloc(count * sizeof(T), UnsafeUtility.AlignOf<T>(), allocator);
            UnsafeUtility.MemClear(retn, count * sizeof(T));
            return retn;
        }

        public static void Dispose(void* data, Allocator allocator) {
            if (data == null) return;
            UnsafeUtility.Free(data, allocator);
        }

        public static void MemCpy<T>(T* dest, T* source, int itemCount) where T : unmanaged {
            UnsafeUtility.MemCpy(dest, source, sizeof(T) * itemCount);
        }

        public static int MemCpyAdvance<T>(T* dest, T* source, int itemCount) where T : unmanaged {
            UnsafeUtility.MemCpy(dest, source, sizeof(T) * itemCount);
            return itemCount;
        }

        public static void Resize<T>(ref T* ptr, int oldSize, int newSize, Allocator allocator) where T : unmanaged {
            if (newSize < oldSize) return;
            if (newSize <= 0) return;

            T* newptr = Malloc<T>(newSize, allocator);
            if (ptr != null) {
                UnsafeUtility.MemCpy(newptr, ptr, sizeof(T) * oldSize);
                UnsafeUtility.Free(ptr, allocator);
            }

            ptr = newptr;
        }

        public static void ResizeCleared<T>(ref T* ptr, int oldSize, int newSize, Allocator allocator) where T : unmanaged {
            if (newSize < oldSize) return;
            if (newSize <= 0) return;

            T* newptr = Malloc<T>(newSize, allocator);
            if (ptr != null) {
                UnsafeUtility.MemCpy(newptr, ptr, sizeof(T) * oldSize);
                UnsafeUtility.Free(ptr, allocator);
            }

            UnsafeUtility.MemClear(newptr, newSize - oldSize);
            ptr = newptr;
        }

        public static void MemClear<T>(T* ptr, int itemCount) where T : unmanaged {
            if (ptr == null || itemCount <= 0) return;
            UnsafeUtility.MemClear(ptr, itemCount * sizeof(T));
        }

        public static int SizeOf<T, U, V>(int itemCount = 1) where T : unmanaged where U : unmanaged where V : unmanaged {
            return (sizeof(T) * itemCount) + (sizeof(U) * itemCount) + (sizeof(V) * itemCount);
        }

        public static int SizeOf<T, U>(int itemCount = 1) where T : unmanaged where U : unmanaged {
            return (sizeof(T) * itemCount) + (sizeof(U) * itemCount);
        }

        public static int SizeOf<T>(int itemCount) where T : unmanaged {
            return sizeof(T) * itemCount;
        }

    }

}