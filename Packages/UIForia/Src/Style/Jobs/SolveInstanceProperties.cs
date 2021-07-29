using System;
using UIForia.Elements;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace UIForia.Style {

    [BurstCompile(DisableSafetyChecks = UIApplication.DisableJobSafety)]
    internal unsafe struct SolveInstanceProperties : IJob {

        public DataList<PropertyContainer> instancePropertyTable;
        public CheckedArray<InstancePropertyInfo> properties;
        public CheckedArray<ElementId> activeElements;

        public int totalPropertyTypeCount;

        [NativeDisableUnsafePtrRestriction] public InstancePropertyUpdateSet* instancePropertyUpdates;

        [NativeDisableUnsafePtrRestriction] public LockedBumpAllocator* perFrameBumpAllocator;

        public void Execute() {

            int requiredPropertyCount = 0;
            int elementWithInstancePropertyCount = 0;

            // count how many elements have instance properties
            for (int i = 0; i < activeElements.size; i++) {
                int propertyCount = properties[i].listSlice.length;
                if (propertyCount > 0) {
                    requiredPropertyCount += propertyCount;
                    elementWithInstancePropertyCount++;
                }
            }

            // get a list with one entry per enabled element w/ instance properties
            TempList<InstancePropertyInfo> tempList = TypedUnsafe.MallocUnsizedTempList<InstancePropertyInfo>(elementWithInstancePropertyCount, Allocator.Temp);

            // copy the property data into our new list 
            for (int i = 0; i < activeElements.size; i++) {
                if (properties[i].listSlice.length > 0) {
                    tempList.array[tempList.size++] = properties[i];
                }
            }

            // get a buffer to hold all of our instance property data
            TempList<PropertyContainer> propertyBuffer = TypedUnsafe.MallocUnsizedTempList<PropertyContainer>(requiredPropertyCount, Allocator.Temp);

            // get a buffer so we can count how many of each property type we have
            TempList<int> propertyCountByType = TypedUnsafe.MallocClearedTempList<int>(totalPropertyTypeCount, Allocator.Temp);
            propertyCountByType.size = totalPropertyTypeCount;

            for (int i = 0; i < tempList.size; i++) {
                SmallListSlice slice = tempList.array[i].listSlice;
                int end = slice.start + slice.length;
                for (int j = slice.start; j < end; j++) {
                    propertyCountByType[instancePropertyTable[j].propertyIndex]++;
                }
            }

            for (int i = 0; i < tempList.size; i++) {
                SmallListSlice slice = tempList.array[i].listSlice;
                int end = slice.start + slice.length;
                for (int j = slice.start; j < end; j++) {
                    propertyBuffer.array[propertyBuffer.size++] = instancePropertyTable[j];
                }
            }

            instancePropertyUpdates->updateLists = perFrameBumpAllocator->Allocate<InstancePropertyUpdateList>(totalPropertyTypeCount);
            instancePropertyUpdates->buffer = perFrameBumpAllocator->Allocate<PropertyContainer>(requiredPropertyCount);

            PropertyContainer* allocPtr = instancePropertyUpdates->buffer;

            // now we can allocate exact sized arrays for all of our shared style updates
            for (int i = 0; i < totalPropertyTypeCount; i++) {
                int count = propertyCountByType[i];
                instancePropertyUpdates->updateLists[i].size = 0; // size will be set when adding the values
                instancePropertyUpdates->updateLists[i].array = allocPtr;
                allocPtr += count;
            }

            for (int i = 0; i < propertyBuffer.size; i++) {

                ref PropertyContainer propertyContainer = ref propertyBuffer.array[i];

                ref InstancePropertyUpdateList updateList = ref instancePropertyUpdates->updateLists[propertyContainer.propertyIndex];

                updateList.array[updateList.size++] = propertyContainer;
            }

            propertyCountByType.Dispose();
            propertyBuffer.Dispose();
            tempList.Dispose();

        }

    }

}