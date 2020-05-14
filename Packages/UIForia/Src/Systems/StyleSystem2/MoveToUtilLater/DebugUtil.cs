namespace UIForia {

    public static unsafe class DebugUtil {

        public static T[] PointerListToArray<T>(T* items, int count) where T : unmanaged {
            T[] retn = new T[count];
            for (int i = 0; i < count; i++) {
                retn[i] = items[i];
            }

            return retn;
        }

    }

}