using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs.LowLevel.Unsafe;

namespace UIForia.Util.Unsafe {

    public unsafe struct PerThreadPagedListUtil {

        public static void Dispose(ref PagedListState** perThreadData, Allocator allocator) {
            if (perThreadData == null) return;
            for (int i = 0; i < JobsUtility.MaxJobThreadCount; i++) {

                if (perThreadData[i] != null) {
                    perThreadData[i]->Dispose();
                    perThreadData[i] = null;
                }

            }

            UnsafeUtility.Free(perThreadData, allocator);
            perThreadData = null;
            throw new NotImplementedException();
        }

        public static PagedListState* GetListForThread(PagedListState** perThreadData, int threadIndex, Allocator allocator, uint pageSize) {
            if (threadIndex < 0 || threadIndex > JobsUtility.MaxJobThreadCount) {
                return default;
            }

            PagedListState* state = perThreadData[threadIndex];

            if (state == null) {
                state = PagedListState.Create((uint) pageSize, allocator);
                perThreadData[threadIndex] = state;
            }

            return state;
        }

        public static int CountElements(PagedListState** perThreadData) {
            int count = 0;

            for (int i = 0; i < JobsUtility.MaxJobThreadCount; i++) {
                if (perThreadData[i] != null) {
                    count += perThreadData[i]->CountElements();
                }
            }

            return count;
        }

        public static void Create(out PagedListState** perThreadData, Allocator allocator) {
            int size = sizeof(PagedListState*) * JobsUtility.MaxJobThreadCount;
            perThreadData = (PagedListState**) UnsafeUtility.Malloc(size, UnsafeUtility.AlignOf<long>(), allocator);
            UnsafeUtility.MemClear(perThreadData, size);
        }

    }

}