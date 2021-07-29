using UIForia.Elements;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace UIForia.Style {

    [BurstCompile(DisableSafetyChecks = UIApplication.DisableJobSafety)]
    internal unsafe struct DisableElementMaps : IJob {

        public DataList<ElementId> activeElements;
        public CheckedArray<RuntimeTraversalInfo> runtimeInfo;

        [NativeDisableUnsafePtrRestriction] public TempList<ElementId>* invokeDisableList;

        public void Execute() {

            int longsPerMap = LongBoolMap.GetMapSize(runtimeInfo.size);

            ulong* buffer = TypedUnsafe.MallocCleared<ulong>(longsPerMap * 3, Allocator.Temp);

            LongBoolMap elementMap = new LongBoolMap(buffer + (longsPerMap * 0), longsPerMap);
            LongBoolMap disabledThisFrame = new LongBoolMap(buffer + (longsPerMap * 1), longsPerMap);
            ElementMap toInvokeDisable = new ElementMap(buffer + (longsPerMap * 2), longsPerMap);

            for (int i = 0; i < activeElements.size; i++) {
                elementMap.Set(activeElements[i].index);
            }

            for (int i = 0; i < runtimeInfo.size; i++) {

                if ((runtimeInfo.array[i].flags & UIElementFlags.Enabled) == 0 || elementMap.Get(i)) {
                    continue;
                }

                runtimeInfo.array[i].flags &= ~UIElementFlags.Enabled;

                disabledThisFrame.Set(i);

                if ((runtimeInfo.array[i].flags & UIElementFlags.RequireDisableInvoke) != 0) { 
                    toInvokeDisable.Set(i);
                }

            }

            *invokeDisableList = toInvokeDisable.ToTempList(Allocator.TempJob);

          
            TypedUnsafe.Dispose(buffer, Allocator.Temp);
        }

    }

    [BurstCompile(DisableSafetyChecks = UIApplication.DisableJobSafety)]
    internal unsafe struct SetupElementMaps : IJob {

        public CheckedArray<RuntimeTraversalInfo> metaTable;
        public CheckedArray<ElementId> activeElements;

        public ElementMap deadOrDisabledMap;
        public ElementMap rebuildStyleMap;
        public ElementMap initMap;
        public ElementMap activeMap;

        private const UIElementFlags k_RequireRebuild = UIElementFlags.InitThisFrame | UIElementFlags.StyleListChanged; //  | UIElementFlags.SelectorListChanged;

        public void Execute() {

            for (int i = 0; i < activeElements.size; i++) {

                ElementId elementId = activeElements[i];

                activeMap.Set(elementId);

                // could be in both maps if was destroyed and then re-using the element id this frame 
                if ((metaTable[elementId.index].flags & k_RequireRebuild) != 0) {
                    rebuildStyleMap.Set(elementId);
                }

                if ((metaTable[elementId.index].flags & UIElementFlags.InitThisFrame) != 0) {
                    initMap.Set(elementId);
                }
            }

            for (int i = 0; i < activeMap.longCount; i++) {
                deadOrDisabledMap.map[i] = ~activeMap.map[i];
            }

        }

    }

}