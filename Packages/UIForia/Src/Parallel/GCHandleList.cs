using System;
using UIForia.Util;

namespace UIForia {

    public struct GCHandleList<T> : IDisposable {

        public GCHandle<LightList<T>> handle;

        public GCHandleList(LightList<T> list) {
            handle = new GCHandle<LightList<T>>(list);
        }

        public LightList<T> Get() {
            return handle.Get();
        }

        public void Dispose() {
            handle.Dispose();
        }

    }

}