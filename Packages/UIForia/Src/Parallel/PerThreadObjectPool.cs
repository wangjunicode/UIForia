using System;
using System.Runtime.InteropServices;

namespace UIForia {

    public struct PerThreadObjectPool<T> : IDisposable where T : class {

        private GCHandle poolHandle;

        public PerThreadObjectPool(ThreadSafePool<T> safePool) {
            poolHandle = GCHandle.Alloc(safePool);
        }

        public T GetForThread(int threadIndex) {
            ThreadSafePool<T> safePool = (ThreadSafePool<T>) poolHandle.Target;
            return safePool.GetForThread(threadIndex);
        }

        public void Dispose() {
            if (poolHandle.IsAllocated) {
                poolHandle.Free();
            }
        }

        public ThreadSafePool<T> GetPool() {
            return (ThreadSafePool<T>) poolHandle.Target;
        }

    }

}