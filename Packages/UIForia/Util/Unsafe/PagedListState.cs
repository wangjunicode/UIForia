using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UIForia.Util.Unsafe {

    [AssertSize(32)]
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct PagedListState : IDisposable {

        internal UntypedPagedListPage* pages;
        internal int pageCapacity;
        internal int pageCount;
        internal int pageSize;
        internal int pageSizeIndex;
        internal Allocator allocator;

        public static PagedListState* Create(uint pageSize, Allocator allocator) {
            pageSize = BitUtil.EnsurePowerOfTwo(pageSize < 8 ? 8 : pageSize);
            PagedListState* state = (PagedListState*) UnsafeUtility.Malloc(UnsafeUtility.SizeOf<PagedListState>(), UnsafeUtility.AlignOf<PagedListState>(), allocator);
            state->pageCapacity = 4;
            state->pages = (UntypedPagedListPage*) UnsafeUtility.Malloc(UnsafeUtility.SizeOf<UntypedPagedListPage>() * state->pageCapacity, UnsafeUtility.AlignOf<PagedListState>(), allocator);
            state->pageSize = (int) pageSize;
            state->pageCount = 0;
            state->pageSizeIndex = BitUtil.GetPowerOfTwoBitIndex(pageSize);
            state->allocator = allocator;
            return state;
        }

        public int CountElements() {
            int count = 0;

            for (int i = 0; i < pageCount; i++) {
                count += pages[i].size;
            }

            return count;

        }

        public void CreateNewPage() {
            pageCapacity *= 2;
            UntypedPagedListPage* ptr = (UntypedPagedListPage*) UnsafeUtility.Malloc(pageCapacity * sizeof(UntypedPagedListPage), UnsafeUtility.AlignOf<PagedListState>(), allocator);

            if (pages != null) {
                UnsafeUtility.MemCpy(ptr, pages, sizeof(UntypedPagedListPage) * pageCount);
                UnsafeUtility.Free(pages, allocator);
            }

            pages = ptr;
        }

        public void Dispose() {
            for (int i = 0; i < pageCount; i++) {
                UnsafeUtility.Free(pages[i].data, allocator);
            }

            if (pageCount > 0) {
                UnsafeUtility.Free(pages, allocator);
            }

            pages = default;
            pageCapacity = 0;
            pageCount = 0;
        }

    }

}