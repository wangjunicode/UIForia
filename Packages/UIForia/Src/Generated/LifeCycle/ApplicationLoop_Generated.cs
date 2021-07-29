using Unity.Collections;
using UIForia.Util.Unsafe;
using UIForia.Util;

namespace UIForia {

    internal unsafe partial struct ApplicationLoop {

        partial void GeneratedInitialize() {
                    
            appInfo = UIForia.Util.Unsafe.TypedUnsafe.Malloc(new UIForia.AppInfo(), Allocator.Persistent);

        }

        partial void GeneratedEnsureCapacity() {
                    
        
        }

        partial void GeneratedClear() {
                    

        }

        partial void GeneratedDispose() {
                    
            appInfo->Dispose();
            UIForia.Util.Unsafe.TypedUnsafe.Dispose(appInfo, Allocator.Persistent);
            appInfo = default;

        }
    }
}
