using System;

namespace UIForia {

    public struct PerThreadObject<T> : IDisposable where T : class, new() {
        
        public GCHandle<T[]> handle;
        
        public PerThreadObject(int count) {
            handle = new GCHandle<T[]>(new T[count]);
        }

        public T GetForThread(int threadIndex) {
            T instance = handle.Get()[threadIndex];
            if (instance == null) {
                instance = new T();
                handle.Get()[threadIndex] = instance;
            }
            return instance;
        }

        public void Dispose() {
            handle.Dispose();
        }

    }

}