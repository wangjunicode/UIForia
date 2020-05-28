using System.Runtime.InteropServices;

namespace UIForia.Layout {

    public interface IListInterface {

        ref ListInterface GetListInterface();

        int ItemSize { get; }

    }

    public unsafe struct ListDebugView<T> where T : unmanaged {

        public int size;
        public int capacity;
        public T[] array;

        public ListDebugView(ListInterface list) {
            this.size = list.size;
            this.capacity = list.capacity;
            this.array = PointerListToArray((T*) list.array, list.size);
        }

        public ListDebugView(FlexLayoutBoxBurst.List_FlexItem target) : this(target.GetListInterface()) { }

        public static T[] PointerListToArray(T* items, int count) {
            T[] retn = new T[count];
            for (int i = 0; i < count; i++) {
                retn[i] = items[i];
            }

            return retn;
        }

    }

    [AssertSize(16)]
    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct ListInterface {

        [FieldOffset(0)] public void* array;
        [FieldOffset(8)] public int size;
        [FieldOffset(12)] public int capacity;

    }

}