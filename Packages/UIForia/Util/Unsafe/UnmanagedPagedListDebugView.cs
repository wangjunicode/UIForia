namespace UIForia.Util.Unsafe {

    public unsafe struct UnmanagedListDebugView<T> where T : unmanaged {

        public int size;
        public int capacity;
        public T[] items;
        
        public UnmanagedListDebugView(UnmanagedList<T> target) {
            UnmanagedListState* state = target.GetStatePointer();
            size = state->size;
            capacity = state->capacity;
            items = new T[size];
            for (int i = 0; i < size; i++) {
                items[i] = state->Get<T>(i);
            }
        }

    }

    public unsafe struct UnmanagedPagedListDebugView<T> where T : unmanaged {

        public int pageCount;
        public int pageSize;
        public int pageSizeIndex;

        public PageDebugView[] pages;

        public UnmanagedPagedListDebugView(UnmanagedPagedList<T> target) {
            pageCount = target.state->pageCount;
            pageSize = target.state->pageSizeLimit;
            pageSizeIndex = target.state->pageSizeIndex;
            pages = new PageDebugView[pageCount];
            for (int i = 0; i < pageCount; i++) {
                pages[i] = new PageDebugView(target.state->pages[i]);
            }
        }

        public struct PageDebugView {

            public int size;
            public T[] data;

            public PageDebugView(UntypedPagedListPage pageData) {
                size = pageData.size;
                data = new T[size];
                T* array = (T*) pageData.data;
                for (int i = 0; i < size; i++) {
                    data[i] = array[i];
                }
            }

        }

    }

}