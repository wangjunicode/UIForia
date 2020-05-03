using System;

namespace UIForia.Util.Unsafe {

    public unsafe struct RawSplitBuffer {

        public void* keys;
        public void* data;
        public int size;
        public int capacity;

    }

    public unsafe struct SplitBuffer<TKey, TData> where TKey : unmanaged where TData : unmanaged {

        public int size;

        public SplitBuffer(RawSplitBuffer computed) {
            this = default;
        }

        // public static SplitBuffer<TKey, TData> Create(int preambleSize, int capacity) {
        //     void* rawData = UnsafeUtility.Malloc(preambleSize + (capacity * sizeof(TKey)) + (capacity * sizeof(TData)), 4, Allocator.Persistent);
        //     TKey* keyStart = (TKey*) ((long*) rawData + preambleSize);
        //     long* dataStart = (long*) (keyStart + (capacity * sizeof(TKey)));
        //
        //     RawSplitBuffer* rawSplitBuffer = (RawSplitBuffer*) (rawData + preambleSize);
        //     InstanceStyleData* data = (InstanceStyleData*) rawData;
        //     data->keys = keyStart;
        //     data->data = dataStart;
        //     data->capacity = capacity;
        //
        //     data->totalStyleCount = 0;
        //     data->usedStyleCount = 0;
        //     return new SplitBuffer<TKey, TData>();    
        // }

        public void Add(TKey key, TData value) { }

        public RawSplitBuffer GetRawBuffer() {
            return default;
        }

        public void GetPointers(TKey* keys, TData* values) {
            throw new NotImplementedException();
        }

    }

}