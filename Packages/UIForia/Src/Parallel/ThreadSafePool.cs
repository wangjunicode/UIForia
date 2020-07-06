using System;
using System.Collections.Concurrent;

namespace UIForia {

    public class ThreadSafePool<T> where T : class {

        public T[] perThreadData;
        public ConcurrentQueue<T> queue;
        public Func<T> create;
        public Action<T> clear;

        public ThreadSafePool(int threadCount, Func<T> create, Action<T> clear) {
            perThreadData = new T[threadCount];
            this.queue = new ConcurrentQueue<T>();
            this.create = create;
            this.clear = clear;
        }

        public void Clear() {
            for (int i = 0; i < perThreadData.Length; i++) {
                if (perThreadData[i] != null) {
                    clear?.Invoke(perThreadData[i]);
                    queue.Enqueue(perThreadData[i]);
                    perThreadData[i] = null;
                }
            }
        }

        public T GetForThread(int threadIdx) {

            if (perThreadData[threadIdx] != null) {
                return perThreadData[threadIdx];
            }

            if (queue.TryDequeue(out T result)) {
                perThreadData[threadIdx] = result;
                return result;
            }

            T value = create();
            perThreadData[threadIdx] = value;
            return value;

        }

        public int GetActiveCount() {
            int count = 0;
            
            for (int i = 0; i < perThreadData.Length; i++) {
                if (perThreadData[i] != null) {
                    count++;
                }
            }

            return count;
        }


    }

}