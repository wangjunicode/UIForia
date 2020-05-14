namespace UIForia {

    /// <summary>
    /// The intention here is to index by a selectorId and not to provide an Add interface
    /// </summary>
    public unsafe struct SelectorTable<T> where T : unmanaged {

        public readonly T* array;

        public SelectorTable(T* array) {
            this.array = array;
        }

        public ref T this[StyleId id] {
            get => ref array[id.index];
        }

        public int ItemSize {
            get => sizeof(T);
        }

    }

}