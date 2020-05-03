using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Collections;

namespace UIForia {

    public unsafe struct PackedBuffer {

        private Index* indices;
        private void* objects;
        private int objectSize;
        private int indexCount;
        private int freeListEnqueue;
        private int freeListDequeue;
        private ushort objectCount;

        private const int INDEX_MASK = 0xffff;
        private const int NEW_OBJECT_ID_ADD = 0x10000;

        [AssertSize(8)]
        private struct Index {

            public int id;
            internal ushort index;
            internal ushort next;

        }

        public static PackedBuffer Create<T>(int initialSize, Allocator allocator) where T : unmanaged, IPackedBufferObject {
            initialSize = (int) BitUtil.EnsurePowerOfTwo((uint) initialSize);
            PackedBuffer buffer = new PackedBuffer();
            buffer.objectCount = 0;
            
            buffer.indices = TypedUnsafe.Malloc<Index>(initialSize, allocator);
            buffer.objects = TypedUnsafe.Malloc<T>(initialSize, allocator);
            
            for (int i = 0; i < initialSize; i++) {
                buffer.indices[i].id = i;
                buffer.indices[i].next = (ushort) (i + 1);
            }

            buffer.freeListDequeue = 0;
            buffer.freeListEnqueue = initialSize - 1;

            return buffer;
        }

        public bool HasId(int id) {
            ref Index index = ref indices[id & INDEX_MASK];
            return index.id == id && index.index != ushort.MaxValue;
        }

        public T Lookup<T>(int id) where T : unmanaged, IPackedBufferObject {
            T* typedObjects = (T*) objects;
            return typedObjects[indices[id & INDEX_MASK].index];
        }
        
        public void Lookup<T>(int * ids, int count, T* output) where T : unmanaged, IPackedBufferObject {
            T* typedObjects = (T*) objects;
            
            for(int i = 0; i < count; i++) {
                int id = ids[i];
                
                ref Index index = ref indices[id & INDEX_MASK];
                
                if (index.id == id && index.index != ushort.MaxValue) {
                    output[i] = typedObjects[indices[id & INDEX_MASK].index];
                }
                else {
                    output[i] = default;
                }
                
            }
        }

        public int Add<T>(T item) where T : unmanaged, IPackedBufferObject {
            ref Index index = ref indices[freeListDequeue];
            freeListDequeue = index.next;
            index.id += NEW_OBJECT_ID_ADD;
            index.index = objectCount++;
            T* typedObjects = (T*) objects;
            item.id = index.id;
            typedObjects[index.index] = item;
            return index.id;
        }

        public void Remove<T>(int id) where T : unmanaged, IPackedBufferObject {
            ref Index index = ref indices[id & INDEX_MASK];
            T* typedObjects = (T*) objects;
            T o = typedObjects[--objectCount];
            indices[o.id & INDEX_MASK].index = index.index;
            index.index = ushort.MaxValue;
            indices[freeListEnqueue].next = (ushort) (id & INDEX_MASK);
            freeListEnqueue = id & INDEX_MASK;
        }

        public interface IPackedBufferObject {

            int id { get; set; }

        }

    }

}