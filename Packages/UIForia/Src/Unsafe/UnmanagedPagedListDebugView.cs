namespace UIForia.Util.Unsafe {

   
    public unsafe struct UnmanagedPagedListDebugView<T> where T : unmanaged {

        public int pageCount;
        public int pageSize;
        public int pageSizeIndex;

        public PageDebugView[] pages;

        public UnmanagedPagedListDebugView(PagedList<T> target) {
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