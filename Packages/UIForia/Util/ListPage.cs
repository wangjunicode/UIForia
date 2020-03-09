namespace UIForia.Util {

    public class ListPage<T> {

        public int size;
        public T[] data;
        public ListPage<T> next;

        public ListPage(T[] data) {
            this.data = data;
            this.size = 0;
        }

    }

}