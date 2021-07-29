using System;
using UIForia.Util;

namespace UIForia {

    public struct PerThreadList<T> : IDisposable {

        public GCHandle<LightList<T>[]> handle;

        public PerThreadList(int count) {
            LightList<T>[] list = new LightList<T>[count];
            handle = new GCHandle<LightList<T>[]>(list);
        }

        public ref LightList<T> GetForThread(int threadIndex) {
            return ref handle.Get()[threadIndex];
        }

        public void Dispose() {
            handle.Dispose();
        }

    }

}