namespace UIForia.Unsafe {

    public unsafe struct AllocatedList<T> where T : unmanaged {

        public int capacity;
        public T* array;

    }

}