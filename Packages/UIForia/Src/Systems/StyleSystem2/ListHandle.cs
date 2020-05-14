using System.Diagnostics;

namespace UIForia {

    public unsafe struct TypedListHandle<T> where T : unmanaged {

        public ListHandle* listHandle;

        public TypedListHandle(ListHandle* handle) {
            this.listHandle = handle;
        }

        public int size {
            get => listHandle->size;
            set => listHandle->size = value;
        }

        public ref T this[int idx] {
            get => ref ((T*) listHandle->array)[idx];
            // set => ((T*)listHandle->array)[idx] = value;
        }

        public void Set(int idx, T value) {
            ((T*) listHandle->array)[idx] = value;
        }

        public T* array {
            get => (T*) listHandle->array;
            set => throw new System.NotImplementedException();
        }

        public int capacity {
            get => listHandle->capacity;
            set => listHandle->capacity = value;
        }

        public void SetArray(T* newListData, int capacity) {
            listHandle->array = newListData;
            listHandle->capacity = capacity;
        }

        public void AddUnchecked(in T item) {
            ((T*) listHandle->array)[listHandle->size++] = item;
        }

    }

    [AssertSize(16)]
    public unsafe struct ListHandle {

        public int size;
        public int capacity;
        public void* array;

        public ListHandle(int capacity, void* array, int size) {
            this.capacity = capacity;
            this.array = array;
            this.size = size;
        }

        public T Get<T>(int i) where T : unmanaged {
            return ((T*) (array))[i];
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
                items[i] = target.data[i];
            }
        }

    }

    [AssertSize(16)]
    [DebuggerTypeProxy(typeof(ListHandleDebugView<>))]
    public unsafe struct ListHandle<T> where T : unmanaged {

        public int size;
        public readonly int capacity;
        public readonly T* data;

        public ListHandle(ListHandle x) {
            this.capacity = x.capacity;
            this.data = (T*) x.array;
            this.size = x.size;
        }

        public ListHandle(T* data, int size, int capacity) {
            this.data = data;
            this.size = size;
            this.capacity = capacity;
        }

        public bool Add(in T item) {
            if (size + 1 >= capacity) return false;
            data[size++] = item;
            return true;
        }

        public ListHandle ToUntypedHandle() {
            return new ListHandle(capacity, data, size);
        }

        public static implicit operator ListHandle(in ListHandle<T> handle) {
            return new ListHandle(handle.capacity, handle.data, handle.size);
        }

    }

}