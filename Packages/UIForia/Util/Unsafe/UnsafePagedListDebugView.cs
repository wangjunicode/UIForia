namespace UIForia.Util.Unsafe {

    public unsafe struct UnsafePagedListDebugView<T> where T : unmanaged {

        public int pageCount;
        public int pageSize;
        public int pageSizeIndex;

        public PageDebugView[] pages;

        public UnsafePagedListDebugView(UnmanagedPagedList<T> target) {
            pageCount = target.state->pageCount;
            pageSize = target.state->pageSize;
            pageSizeIndex = target.state->pageSizeIndex;
            pages = new PageDebugView[pageCount];
            for (int i = 0; i < pageCount; i++) {
                pages[i] = new PageDebugView(target.state->pages[i], pageSize);
            }
        }

        public struct PageDebugView {

            public int size;
            public int capacity;
            public T[] data;

            public PageDebugView(UntypedPagedListPage pageData, int pageCapacity) {
                size = pageData.size;
                capacity = pageCapacity;
                data = new T[size];
                T* array = (T*) pageData.data;
                for (int i = 0; i < size; i++) {
                    data[i] = array[i];
                }
            }

        }

    }

}