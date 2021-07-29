using System;
using System.Diagnostics;
using UIForia.ListTypes;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UIForia.Util.Unsafe {

    public unsafe struct BytePage {

        public int size;
        public int capacity;
        public byte* array;

    }

    internal sealed class BumpListDebuggerView<T> where T : unmanaged {

        private BumpList<T> m_Array;

        public BumpListDebuggerView(BumpList<T> array) => this.m_Array = array;

        public T[] Items => this.m_Array.ToArray();

    }
    
    [DebuggerTypeProxy(typeof(BumpListDebuggerView<>))]
    public unsafe struct BumpList<T> where T : unmanaged {

        public int size;
        public int capacity;
        public T* array;

        public T[] ToArray() {
            return TypedUnsafe.ToArray(array, size);
        }

    }

    public unsafe struct BumpAllocator : IDisposable {

        private DataList<BytePage> pages;
        private readonly Allocator allocator;
        private readonly int basePageByteSize;

        public BumpAllocator(int basePageByteSize, Allocator allocator) {
            this.allocator = allocator;
            this.basePageByteSize = basePageByteSize < 1024 ? 1024 : basePageByteSize;
            this.pages = new DataList<BytePage>(4, allocator);
            CreatePage(basePageByteSize);
        }

        private void CreatePage(int capacity) {
            pages.Add(new BytePage() {
                size = 0,
                array = (byte*) UnsafeUtility.Malloc(capacity, UnsafeUtility.AlignOf<long>(), allocator),
                capacity = capacity
            });
        }

        public T* Allocate<T>(in T item) where T : unmanaged {
            T* ptr = Allocate<T>(1);
            *ptr = item;
            return ptr;
        }

        public T* Allocate<T>(T* items, int count) where T : unmanaged {
            T* ptr = Allocate<T>(count);
            TypedUnsafe.MemCpy(ptr, items, count);
            return ptr;
        }

        public T* AllocateCleared<T>(int count) where T : unmanaged {
            T* ptr = Allocate<T>(count);
            TypedUnsafe.MemClear(ptr, count);
            return ptr;
        }

        public T* Allocate<T>(int count) where T : unmanaged {

            int byteSize = MathUtil.Nearest8(sizeof(T) * count);

            // todo -- use something like this for alignment 
            // ulong alignmentMask = (ulong) (UnsafeUtility.AlignOf<T>() - 1);
            // var end = (ulong)(IntPtr)ptr + (ulong)m_LengthInBytes;
            // end = (end + alignmentMask) & ~alignmentMask;
            // var lengthInBytes = (byte*)(IntPtr)end - (byte*)m_Pointer;
            // lengthInBytes += sizeInBytes;
            
            T* ptr;

            for (int i = 0; i < pages.size; i++) {
                ref BytePage bytePage = ref pages.array[i];
                if (bytePage.size + byteSize < bytePage.capacity) {
                    ptr = (T*) (bytePage.array + bytePage.size);
                    bytePage.size += byteSize;
                    return ptr;
                }
            }

            int required = byteSize > basePageByteSize ? byteSize : basePageByteSize;

            BytePage lastPage = new BytePage() {
                capacity = required,
                size = 0,
                array = (byte*) UnsafeUtility.Malloc(required, 8, allocator)
            };

            ptr = (T*) lastPage.array;
            lastPage.size = byteSize;
            pages.Add(lastPage);

            return ptr;
        }

        public BumpList<T> AllocateList<T>(int count) where T : unmanaged {
            return new BumpList<T>() {
                array = Allocate<T>(count),
                capacity = count,
                size = 0
            };
        }

        public BumpList<T> AllocateListCleared<T>(int count) where T : unmanaged {
            return new BumpList<T>() {
                array = AllocateCleared<T>(count),
                capacity = count,
                size = 0
            };
        }

        public void Clear() {
            // No CAS here since we can just never be in a situation where multiple threads clear or we are fundamentally borked anyway 
            for (int i = 0; i < pages.size; i++) {
                pages.array[i].size = 0;
            }
        }

        public void ClearAndConsolidate() {
            // No CAS here since we can just never be in a situation where multiple threads clear or we are fundamentally borked anyway 
            if (pages.size > 1) {
                long totalSize = 0;

                for (int i = 0; i < pages.size; i++) {
                    totalSize += pages.array[i].capacity;
                }

                for (int i = 0; i < pages.size; i++) {
                    UnsafeUtility.Free(pages.array[i].array, allocator);
                }

                pages.array[0].array = (byte*) UnsafeUtility.Malloc(totalSize, 8, allocator);
                pages.array[0].size = 0;
                pages.array[0].capacity = (int) totalSize;
                pages.size = 1;
            }
            else {
                pages.array[0].size = 0;
            }
        }

        public void Dispose() {
            for (int i = 0; i < pages.size; i++) {
                UnsafeUtility.Free(pages[i].array, allocator);
            }

            pages.Dispose();
            this = default;
        }

    }

}