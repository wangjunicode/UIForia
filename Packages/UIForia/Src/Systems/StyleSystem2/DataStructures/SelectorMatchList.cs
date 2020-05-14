using System;
using UIForia.Util.Unsafe;
using Unity.Collections;

namespace UIForia {

    public struct SelectorMatchList : IPerThreadCompatible {

        public void Dispose() {
            throw new NotImplementedException();
        }

        public bool IsInitialized { get; }

        public void InitializeForThread(Allocator allocator) {
            throw new NotImplementedException();
        }

        public void Add(SelectorMatch selectorMatch) {
            throw new NotImplementedException();
        }

    }

}