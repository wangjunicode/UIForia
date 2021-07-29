using AOT;
using UIForia.Elements;
using UIForia.Util.Unsafe;
using Unity.Burst;

namespace UIForia.Style {

    [BurstCompile]
    internal static unsafe class RestorePerFrameFlags {

        public delegate void CallbackFn(RuntimeTraversalInfo* elementMetaInfo, RuntimeTraversalInfo* runtimeInfo, int tableSize);

        private static readonly CallbackFn s_Callback = BurstCompiler.CompileFunctionPointer<CallbackFn>(Invoke).Invoke;

        public static void RestoreFlags(RuntimeTraversalInfo* elementMetaInfo, RuntimeTraversalInfo[] metaTable, int tableSize) {
            fixed (RuntimeTraversalInfo* table = metaTable) {
                s_Callback(elementMetaInfo, table, tableSize);
            }
        }

        [MonoPInvokeCallback(typeof(CallbackFn))]
        [BurstCompile(DisableSafetyChecks = UIApplication.DisableJobSafety)]
        private static void Invoke(RuntimeTraversalInfo* threadData, RuntimeTraversalInfo* appData, int tableSize) {

            const UIElementFlags k_Flags = (UIElementFlags.InitThisFrame | UIElementFlags.StyleListChanged); //  | UIElementFlags.SelectorListChanged);

            TypedUnsafe.MemCpy(threadData, appData, tableSize);
            
            for (int i = 0; i < tableSize; i++) {
                appData[i].flags &= ~k_Flags; 
            }

        }

    }
    
}