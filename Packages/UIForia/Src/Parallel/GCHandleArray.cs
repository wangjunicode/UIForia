using System;

namespace UIForia {

    public struct GCHandleArray<T> : IDisposable {

        public GCHandle<T[]> handle;

        public GCHandleArray(T[] list) {
            handle = new GCHandle<T[]>(list);
        }

        public T[] Get() {
            return handle.Get();
        }

        public void Dispose() {
            handle.Dispose();
        }

    }

}