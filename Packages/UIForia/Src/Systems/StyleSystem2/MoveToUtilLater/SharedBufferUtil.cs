using UIForia.Util.Unsafe;

namespace UIForia {

    public static class SharedBufferUtil {

        public static unsafe void Move<T>(ref ElementTable<T> table, ref byte* buffer, int oldCapacity, int requiredCapacity) where T : unmanaged {
            T* typedBuffer = (T*) buffer;

            if (table.array != null && oldCapacity > 0) {
                TypedUnsafe.MemCpy(typedBuffer, table.array, oldCapacity);
            }
            
            T* newSection = typedBuffer + oldCapacity;
            TypedUnsafe.MemClear(newSection, (requiredCapacity - oldCapacity));
            table = new ElementTable<T>(typedBuffer);
            buffer = (byte*) (typedBuffer + requiredCapacity);
        }

    }

}