using System.Diagnostics;

namespace UIForia {

    [AssertSize(16)]
    public unsafe struct ListHandle {

        public int size;
        public readonly int capacity;
        public readonly void* pData;

        public ListHandle(int capacity, void* pData, int size) {
            this.capacity = capacity;
            this.pData = pData;
            this.size = size;
        }

    }

    public unsafe struct ListHandleDebugView<T> where T : unmanaged {

        public int size;
        public int capacity;
        public T[] items;
        
        public ListHandleDebugView(ListHandle<T> target) {
            size = target.size;
            capacity = target.capacity;
            items = new T[size];
            for (int i = 0; i < size; i++) {
                items[i] = target.pData[i];
            }
        }

    }

    [AssertSize(16)]
    [DebuggerTypeProxy(typeof(ListHandleDebugView<>))]
    public unsafe struct ListHandle<T> where T : unmanaged {

        public int size;
        public readonly int capacity;
        public readonly T* pData;

        public ListHandle(ListHandle x) {
            this.capacity = x.capacity;
            this.pData = (T*) x.pData;
            this.size = x.size;
        }

        public ListHandle(T* pData, int size, int capacity) {
            this.pData = pData;
            this.size = size;
            this.capacity = capacity;
        }

        public bool Add(in T item) {
            if (size + 1 >= capacity) return false;
            pData[size++] = item;
            return true;
        }

        public ListHandle ToUntypedHandle() {
            return new ListHandle(capacity, pData, size);
        }

        public static implicit operator ListHandle(in ListHandle<T> handle) {
            return new ListHandle(handle.capacity, handle.pData, handle.size);
        }

    }

}