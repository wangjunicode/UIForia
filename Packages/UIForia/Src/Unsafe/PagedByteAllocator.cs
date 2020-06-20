using System;
using UIForia.ListTypes;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UIForia.Util.Unsafe {

    public unsafe struct BytePage {

        public int size;
        public int capacity;
        public byte* array;

    }

    public unsafe struct PagedByteAllocator : IDisposable {

        public readonly Allocator allocator;
        public readonly Allocator pageAllocator;
        public int basePageByteSize;
        private List_BytePage pages;

        public PagedByteAllocator(int basePageByteSize, Allocator allocator, Allocator pageAllocator) {
            this.allocator = allocator;
            this.pageAllocator = pageAllocator;
            this.basePageByteSize = basePageByteSize < 1024 ? 1024 : basePageByteSize;
            this.pages = new List_BytePage(4, allocator);
        }

        private void CreatePage(int capacity) {

            pages.Add(new BytePage() {
                size = 0,
                array = (byte*) UnsafeUtility.Malloc(capacity, 4, pageAllocator),
                capacity = capacity
            });
        }

        public T* Allocate<T>(in T item) where T : unmanaged {
            int byteSize = sizeof(T);

            if (pages.size == 0) {
                CreatePage(basePageByteSize > byteSize ? basePageByteSize : byteSize);
            }

            ref BytePage bytePage = ref pages.array[pages.size - 1];

            if (bytePage.size + byteSize >= bytePage.capacity) {

                int capacity = basePageByteSize > byteSize ? basePageByteSize : byteSize;
                pages.Add(new BytePage() {
                    capacity = capacity,
                    size = 0,
                    array = (byte*) UnsafeUtility.Malloc(capacity, 4, pageAllocator)
                });
                bytePage = ref pages[pages.size - 1];

            }

            T* ptr = (T*) (bytePage.array + bytePage.size);
            *ptr = item;
            bytePage.size += byteSize;
            return ptr;
        }

        public T* Allocate<T>(T* items, int count) where T : unmanaged {

            int byteSize = count * sizeof(T);

            if (pages.size == 0) {
                CreatePage(basePageByteSize > byteSize ? basePageByteSize : byteSize);
            }

            ref BytePage bytePage = ref pages.array[pages.size - 1];

            if (bytePage.size + byteSize >= bytePage.capacity) {

                int capacity = basePageByteSize > byteSize ? basePageByteSize : byteSize;
                pages.Add(new BytePage() {
                    capacity = capacity,
                    size = 0,
                    array = (byte*) UnsafeUtility.Malloc(capacity, 4, pageAllocator)
                });
                bytePage = ref pages[pages.size - 1];

            }

            T* ptr = (T*) (bytePage.array + bytePage.size);
            TypedUnsafe.MemCpy(ptr, items, count);
            bytePage.size += byteSize;
            return ptr;
        }

        public T* Allocate<T>(int count = 1) where T : unmanaged {

            int byteSize = count * sizeof(T);

            if (pages.size == 0) {
                CreatePage(basePageByteSize > byteSize ? basePageByteSize : byteSize);
            }

            ref BytePage bytePage = ref pages.array[pages.size - 1];

            if (bytePage.size + byteSize >= bytePage.capacity) {

                int capacity = basePageByteSize > byteSize ? basePageByteSize : byteSize;
                pages.Add(new BytePage() {
                    capacity = capacity,
                    size = 0,
                    array = (byte*) UnsafeUtility.Malloc(capacity, 4, pageAllocator)
                });
                bytePage = ref pages[pages.size - 1];

            }

            T* ptr = (T*) (bytePage.array + bytePage.size);
            bytePage.size += byteSize;
            return ptr;

        }

        public void Clear() {
            for (int i = 0; i < pages.size; i++) {
                UnsafeUtility.Free(pages[i].array, pageAllocator);
            }

            pages.size = 0;

        }

        public void Dispose() {
            for (int i = 0; i < pages.size; i++) {
                UnsafeUtility.Free(pages[i].array, pageAllocator);
            }

            pages.Dispose();
            this = default;
        }

        public struct Shared {

            private PagedByteAllocator* state;

            public Shared(int basePageByteSize, Allocator allocator, Allocator pageAllocator) {
                state = TypedUnsafe.Malloc<PagedByteAllocator>(allocator);
                *state = new PagedByteAllocator(basePageByteSize, allocator, pageAllocator);
            }

            public T* Allocate<T>(int count = 1) where T : unmanaged {
                return state->Allocate<T>(count);
            }

            public T* Allocate<T>(in T item) where T : unmanaged {
                return state->Allocate<T>(item);
            }

            public T* Allocate<T>(T* items, int itemCount) where T : unmanaged {
                return state->Allocate<T>(items, itemCount);
            }

            public void Clear() {
                state->Clear();
            }

            public void Dispose() {
                if (state == null) {
                    return;
                }

                Allocator allocator = state->allocator;
                state->Dispose();
                TypedUnsafe.Dispose(state, allocator);
                this = default;
            }

        }

    }

}