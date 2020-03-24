namespace UIForia.Util {

    public struct SizedArray<T> {

        public int size;
        public T[] array;

        public SizedArray(int capacity) {
            this.size = 0;
            this.array = new T[capacity];
        }

        public SizedArray(T[] data) {
            this.size = data.Length;
            this.array = data;
        }

        public T this[int idx] {
            get => array[idx];
            set => array[idx] = value;
        }

        public T Add(in T item) {
            if (size + 1 >= array.Length) {
                System.Array.Resize(ref array, size + array.Length * 2);
            }

            array[size] = item;
            size++;
            return item;
        }

    }

}