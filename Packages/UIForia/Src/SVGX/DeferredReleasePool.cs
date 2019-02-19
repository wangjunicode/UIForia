using System;
using System.Collections.Generic;
using UIForia.Util;

namespace SVGX {

    internal class DeferredReleasePool<T> where T : class, new() {

        private readonly Queue<T> pool;
        private readonly LightList<T> releaseQueue;

        private readonly Action<T> releaseHandler;
        
        public DeferredReleasePool(Action<T> releaseHandler = null) {
            this.releaseHandler = releaseHandler;
            pool = new Queue<T>();
            releaseQueue = new LightList<T>();
        }

        public void FlushReleaseQueue() {
            for (int i = 0; i < releaseQueue.Count; i++) {
                releaseHandler?.Invoke(releaseQueue[i]);
                pool.Enqueue(releaseQueue[i]);
            }

            releaseQueue.Clear();
        }
        
        public T GetAndQueueForRelease() {
            T retn = null;
            if (pool.Count > 0) {
                retn = pool.Dequeue();
            }

            retn = new T();
            releaseQueue.Add(retn);
            return retn;
        }

        public T Get() {
            if (pool.Count > 0) {
                return pool.Dequeue();
            }

            return new T();
        }

        public void Release(T item) {
            releaseHandler?.Invoke(item);
            pool.Enqueue(item);
        }

    }

}