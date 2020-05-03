using System;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace UIForia {

    public unsafe struct ByteBuffer : IDisposable {

        public byte* data;
        private int sizeInBytes;
        private int capacityInBytes;
        private Allocator allocator;

        public ByteBuffer(int initialSizeInBytes, Allocator allocator) {
            initialSizeInBytes = math.max(64, BitUtil.EnsurePowerOfTwo(initialSizeInBytes));
            this.capacityInBytes = initialSizeInBytes;
            this.sizeInBytes = 0;
            this.allocator = allocator;
            this.data = TypedUnsafe.Malloc<byte>(initialSizeInBytes, allocator);
        }

        public void WriteUnchecked<T>(T item) where T : unmanaged {
            T* typedData = (T*) (data + sizeInBytes);
            *typedData = item;
            sizeInBytes += sizeof(T);
        }

        public void Write<T>(T item) where T : unmanaged {
            EnsureAdditionalCapacity<T>(1);
            T* typedData = (T*) (data + sizeInBytes);
            *typedData = item;
            sizeInBytes += sizeof(T);
        }

        public void Write<T>(T* items, int itemCount) where T : unmanaged { }

        public int WriteRange<T>(T* items, int itemCount) where T : unmanaged {
            EnsureAdditionalCapacity<T>(itemCount);
            int retn = sizeInBytes;
            T* typedData = (T*) (data + sizeInBytes);
            TypedUnsafe.MemCpy(typedData, items, itemCount);
            sizeInBytes += sizeof(T) * itemCount;

            return retn;
        }

        public int WriteRange<T>(T[] items, int itemCount) where T : unmanaged {
            fixed (T* itemsptr = items) {
                return WriteRange<T>(itemsptr, itemCount);
            }
        }

        public void EnsureAdditionalCapacity<T>(int itemCount) where T : unmanaged {
            int addedByteCount = sizeof(T) * itemCount;
            if (sizeInBytes + addedByteCount < capacityInBytes) {
                return;
            }

            TypedUnsafe.Resize(ref data, sizeInBytes, (sizeInBytes + addedByteCount) * 2, allocator);
        }

        public void EnsureAdditionalCapacity<T, U>(int itemCount) where T : unmanaged where U : unmanaged {
            int addedByteCount = (sizeof(T) * itemCount) + (sizeof(U) * itemCount);
            if (sizeInBytes + addedByteCount < capacityInBytes) {
                return;
            }

            TypedUnsafe.Resize(ref data, sizeInBytes, (sizeInBytes + addedByteCount) * 2, allocator);
        }

        public void Dispose() {
            if (data != null) {
                UnsafeUtility.Free(data, allocator);
            }

            this = default;
        }

        public int GetWritePosition() {
            return sizeInBytes;
        }

    }

}