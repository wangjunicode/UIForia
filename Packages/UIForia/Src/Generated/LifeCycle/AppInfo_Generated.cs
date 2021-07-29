using Unity.Collections;
using UIForia.Util.Unsafe;
using UIForia.Util;

namespace UIForia {

    internal unsafe partial struct AppInfo {

        partial void GeneratedInitialize() {
                    
            styleUsageIdGenerator = new UIForia.HeapAllocated<int>(default);
            executionTokenGenerator = new UIForia.HeapAllocated<int>(default);
            instancePropertyTable = new UIForia.Util.Unsafe.DataList<UIForia.Style.PropertyContainer>(elementCount / 2, Allocator.Persistent);
            styleIdTable = new UIForia.Util.Unsafe.DataList<UIForia.Style.StyleId>(elementCount * 4, Allocator.Persistent);
            newQuerySubscriptions = new UIForia.Util.Unsafe.DataList<UIForia.Style.QuerySubscription>.Shared(elementCount / 2, Allocator.Persistent);
            styleUsages = new UIForia.Util.Unsafe.DataList<UIForia.Style.StyleUsage>.Shared(maxElementId, Allocator.Persistent);
            styleUsageResults = new UIForia.Util.Unsafe.DataList<UIForia.Style.StyleUsageQueryResult>.Shared(maxElementId, Allocator.Persistent);
            styleUsageToIndex = new UIForia.Util.Unsafe.DataList<int>.Shared(maxElementId, Allocator.Persistent);
            styleUsageIdFreeList = new UIForia.Util.Unsafe.DataList<int>.Shared(64, Allocator.Persistent);
            queryResultList = new UIForia.Util.Unsafe.DataList<UIForia.LongBoolMap>.Shared(queryTableCount, Allocator.Persistent);
            queryTableSubscriptionList = new UIForia.Util.Unsafe.DataList<UIForia.ListTypes.List_QuerySubscription>.Shared(queryTableCount, Allocator.Persistent);
            invalidatedElementBuffer = new UIForia.Util.Unsafe.DataList<ulong>(LongBoolMap.GetMapSize(maxElementId), Allocator.Persistent);
            styleRebuildBuffer = new UIForia.Util.Unsafe.DataList<ulong>(LongBoolMap.GetMapSize(maxElementId), Allocator.Persistent);
            blockRebuildBuffer = new UIForia.Util.Unsafe.DataList<ulong>(LongBoolMap.GetMapSize(maxElementId), Allocator.Persistent);
            initMapBuffer = new UIForia.Util.Unsafe.DataList<ulong>(LongBoolMap.GetMapSize(maxElementId), Allocator.Persistent);
            activeMapBuffer = new UIForia.Util.Unsafe.DataList<ulong>(LongBoolMap.GetMapSize(maxElementId), Allocator.Persistent);
            newQuerySubscriberMapBuffer = new UIForia.Util.Unsafe.DataList<ulong>(LongBoolMap.GetMapSize(queryTableCount), Allocator.Persistent);
            perFrameAnimationCommands = new UIForia.Util.Unsafe.DataList<UIForia.AnimationCommand>(8, Allocator.Persistent);
            colorVariableList = new UIForia.Util.Unsafe.DataList<UIForia.ColorVariable>(8, Allocator.Persistent);
            valueVariableList = new UIForia.Util.Unsafe.DataList<UIForia.ValueVariable>(8, Allocator.Persistent);
            textureVariableList = new UIForia.Util.Unsafe.DataList<UIForia.TextureVariable>(8, Allocator.Persistent);
            blockChanges = new UIForia.Util.Unsafe.DataList<UIForia.Style.StyleBlockChanges>(32, Allocator.Persistent);
            activeAnimationList = new UIForia.Util.Unsafe.DataList<UIForia.AnimationInstance>(32, Allocator.Persistent);
            viewRootIds = new UIForia.Util.Unsafe.DataList<UIForia.ElementId>(8, Allocator.Persistent);

        }

        partial void GeneratedEnsureCapacity() {
                    
            newQuerySubscriptions.EnsureCapacity(elementCount / 2);
            styleUsages.EnsureCapacity(maxElementId);
            styleUsageResults.EnsureCapacity(maxElementId);
            styleUsageToIndex.EnsureCapacity(maxElementId);
            queryResultList.EnsureCapacity(queryTableCount);
            queryTableSubscriptionList.EnsureCapacity(queryTableCount);
            invalidatedElementBuffer.EnsureCapacity(LongBoolMap.GetMapSize(maxElementId));
            styleRebuildBuffer.EnsureCapacity(LongBoolMap.GetMapSize(maxElementId));
            blockRebuildBuffer.EnsureCapacity(LongBoolMap.GetMapSize(maxElementId));
            initMapBuffer.EnsureCapacity(LongBoolMap.GetMapSize(maxElementId));
            activeMapBuffer.EnsureCapacity(LongBoolMap.GetMapSize(maxElementId));
            newQuerySubscriberMapBuffer.EnsureCapacity(LongBoolMap.GetMapSize(queryTableCount));
        
        }

        partial void GeneratedClear() {
                    
            TypedUnsafe.MemClear(invalidatedElementBuffer.GetArrayPointer(), invalidatedElementBuffer.capacity);
            TypedUnsafe.MemClear(styleRebuildBuffer.GetArrayPointer(), styleRebuildBuffer.capacity);
            TypedUnsafe.MemClear(blockRebuildBuffer.GetArrayPointer(), blockRebuildBuffer.capacity);
            TypedUnsafe.MemClear(initMapBuffer.GetArrayPointer(), initMapBuffer.capacity);
            TypedUnsafe.MemClear(activeMapBuffer.GetArrayPointer(), activeMapBuffer.capacity);
            TypedUnsafe.MemClear(newQuerySubscriberMapBuffer.GetArrayPointer(), newQuerySubscriberMapBuffer.capacity);
            TypedUnsafe.MemClear(perFrameAnimationCommands.GetArrayPointer(), perFrameAnimationCommands.capacity);
            TypedUnsafe.MemClear(colorVariableList.GetArrayPointer(), colorVariableList.capacity);
            TypedUnsafe.MemClear(valueVariableList.GetArrayPointer(), valueVariableList.capacity);
            TypedUnsafe.MemClear(textureVariableList.GetArrayPointer(), textureVariableList.capacity);

        }

        partial void GeneratedDispose() {
                    
            styleUsageIdGenerator.Dispose();
            executionTokenGenerator.Dispose();
            instancePropertyTable.Dispose();
            styleIdTable.Dispose();
            newQuerySubscriptions.Dispose();
            styleUsages.Dispose();
            styleUsageResults.Dispose();
            styleUsageToIndex.Dispose();
            styleUsageIdFreeList.Dispose();
            queryResultList.Dispose();
            for(int i = 0; i < queryTableSubscriptionList.size; i++) {
                queryTableSubscriptionList[i].Dispose();
            }
            queryTableSubscriptionList.Dispose();
            invalidatedElementBuffer.Dispose();
            styleRebuildBuffer.Dispose();
            blockRebuildBuffer.Dispose();
            initMapBuffer.Dispose();
            activeMapBuffer.Dispose();
            newQuerySubscriberMapBuffer.Dispose();
            perFrameAnimationCommands.Dispose();
            colorVariableList.Dispose();
            valueVariableList.Dispose();
            textureVariableList.Dispose();
            blockChanges.Dispose();
            activeAnimationList.Dispose();
            viewRootIds.Dispose();
            UIForia.Util.Unsafe.TypedUnsafe.Dispose(queryResultBuffer, Allocator.Persistent);
            queryResultBuffer = default;
            shapeCache.Dispose();
            stringTagger.Dispose();

        }
    }
}
