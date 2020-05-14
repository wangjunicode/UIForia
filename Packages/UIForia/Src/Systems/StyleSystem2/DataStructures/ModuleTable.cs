namespace UIForia {

    /// <summary>
    /// The intention here is to index by a styleId and not to provide an Add interface
    /// </summary>
    public unsafe struct ModuleTable<T> where T : unmanaged {

        public readonly T* array;

        public ModuleTable(T* array) {
            this.array = array;
        }

        public ref T this[ModuleId id] {
            get => ref array[id.index];
        }

        public int ItemSize {
            get => sizeof(T);
        }

    }

}