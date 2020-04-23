using System;
using UIForia.Elements;
using UIForia.Style;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Assertions;

namespace UIForia {
    
    public class VertigoStyleSystem {

        internal static LightList<VertigoStyleSheet> s_DebugSheets;

        internal const ushort k_ImplicitModuleId = 0;

        public UIElement rootElement;

        internal readonly LightList<VertigoStyleSheet> styleSheets;

        internal LightList<string> styleNameTable; // todo -- make this dev only since allocation scheme sucks
        internal UnsafePagedList<StyleProperty2> propertyTable;
        internal UnsafePagedList<VertigoStyle> styleTable;
        internal UnsafePagedList<VertigoSelector> selectorTable;

        internal UnsafeList<SharedStyleChangeSet> sharedStyleChangeSets; // stack based buffer allocator, clear on frame end
        internal UnsafeList<StyleSetData> styleDataMap;
        internal UnsafeList<StyleSetInstanceData> instanceDataMap;
        internal UnsafeList<InstanceStyleChangeSet> instanceChanges;

        public VertigoStyleSystem() {

#if UNITY_EDITOR
            AssertSize.AssertSizes();
#endif
            styleNameTable = new LightList<string>(128);
            propertyTable = new UnsafePagedList<StyleProperty2>(256, Allocator.Persistent);
            styleTable = new UnsafePagedList<VertigoStyle>(128, Allocator.Persistent);
            selectorTable = new UnsafePagedList<VertigoSelector>(64, Allocator.Persistent);
            styleDataMap = new UnsafeList<StyleSetData>(32, Allocator.Persistent); // todo this should be an UnsafeChunkedList I think
            instanceDataMap = new UnsafeList<StyleSetInstanceData>(16, Allocator.Persistent); // todo this should be an UnsafeChunkedList I think
            instanceChanges = new UnsafeList<InstanceStyleChangeSet>(32, Allocator.Persistent);
            sharedStyleChangeSets = new UnsafeList<SharedStyleChangeSet>(32, Allocator.Persistent);
            styleSheets = new LightList<VertigoStyleSheet>();
            s_DebugSheets = styleSheets;

        }

        internal bool TryResolveStyle(string sheetName, string styleName, out StyleId styleId) {
            styleId = default;
            VertigoStyleSheet sheet = GetStyleSheet(sheetName);
            return sheet != null && sheet.TryGetStyle(styleName, out styleId);
        }

        public unsafe void Destroy() {

            for (int i = 0; i < styleDataMap.size; i++) {
                StyleSetData data = styleDataMap.array[i];
                // if (data.sharedStyles != default) {
                //     UnsafeUtility.Free(data.sharedStyles, Allocator.Persistent);
                //     // data.sharedStyles = default;
                // }
            }

            styleTable.Dispose();
            propertyTable.Dispose();
            styleDataMap.Dispose();
            selectorTable.Dispose();
            sharedStyleChangeSets.Dispose();
            for (int i = 0; i < styleSheets.size; i++) {
                styleSheets.array[i].Destroy();
            }
        }

        public void AddStyleSheet(string name, Action<StyleSheetBuilder> sheetAction) {
            // todo - ensure name is unique

            StyleSheetBuilder builder = new StyleSheetBuilder(this);
            // VertigoStyleSheet sheet = new VertigoStyleSheet(name, new StyleSheetId(k_ImplicitModuleId, (ushort) styleSheets.size));
            sheetAction?.Invoke(builder);
            // sheet.styleRange = new RangeInt(styleProperties.size, 0);
            // styleSheets.Add(sheet);
        }

        public void AddStyleSheet(Action<StyleSheetBuilder> sheetAction) {
            AddStyleSheet(string.Empty, sheetAction);
        }

        internal void AddStyleSheet(VertigoStyleSheet[] styleSheets) {
            // this is the compile case where we want to batch add these
        }

        public struct SharedStyleJobData {

            public NativeList<int> rebuildList;
            public NativeList<StyleStateGroup> addedList;
            public NativeList<StyleStateGroup> removedList;

        }

        public unsafe void OnUpdate() {

            int changeCount = sharedStyleChangeSets.size;
            int batchSize = 1;
            int changeProcessBatchCount = 1;

            LightList<SharedStyleJobData> data = new LightList<SharedStyleJobData>(changeProcessBatchCount);
            NativeArray<JobHandle> sharedStyleUpdateHandles = new NativeArray<JobHandle>(changeProcessBatchCount, Allocator.TempJob);

            for (int i = 0; i < changeProcessBatchCount; i++) {

                SharedStyleJobData jobData = new SharedStyleJobData {
                    addedList = new NativeList<StyleStateGroup>(32, Allocator.TempJob),
                    removedList = new NativeList<StyleStateGroup>(32, Allocator.TempJob),
                    rebuildList = new NativeList<int>(changeCount, Allocator.TempJob) // todo -- proper size
                };

                ProcessSharedStyleUpdatesJob job = new ProcessSharedStyleUpdatesJob() {
                    addedList = jobData.addedList,
                    removedList = jobData.removedList,
                    rebuildList = jobData.rebuildList,
                    changeSets = new UnsafeSpan<SharedStyleChangeSet>(0, changeCount, sharedStyleChangeSets.array)
                };

                data.Add(jobData);

                sharedStyleUpdateHandles[i] = job.Schedule();
            }

            JobHandle.CompleteAll(sharedStyleUpdateHandles);

            for (int i = 0; i < data.size; i++) {
                data[i].addedList.Dispose();
                data[i].removedList.Dispose();
                data[i].rebuildList.Dispose();
            }

            sharedStyleUpdateHandles.Dispose();

            EndFrame();
        }

        internal unsafe void EndFrame() {
            for (int i = 0; i < sharedStyleChangeSets.size; i++) {
                // can be parallel but probably doesn't need to be
                // sharedStyleChangeSets.array[i].ch = ushort.MaxValue;
            }

            sharedStyleChangeSets.size = 0;
        }

        public VertigoStyleSheet GetStyleSheet(string sheetName) {
            // cannot sort the list, if this is a common use case then keep a dictionary 
            for (int i = 0; i < styleSheets.size; i++) {
                if (styleSheets.array[i].name == sheetName) {
                    return styleSheets.array[i];
                }
            }

            return null;
        }

        private unsafe void CreateStyleChangeSet(int styleDataId, ref StyleSetData styleSetData) {

            sharedStyleChangeSets.EnsureAdditionalCapacity(1);

            styleSetData.changeSetId = (ushort) sharedStyleChangeSets.size;

            sharedStyleChangeSets.size++;

            ref SharedStyleChangeSet changeSetData = ref sharedStyleChangeSets.array[styleSetData.changeSetId];

            changeSetData.newState = StyleState2Byte.Normal;
            changeSetData.originalState = styleSetData.state | StyleState2Byte.Normal;
            changeSetData.styleDataId = (ushort) styleDataId;
            changeSetData.newStyleCount = 0;
            changeSetData.oldStyleCount = styleSetData.sharedStyleCount;

            // StyleId* oldStyles = changeSetData.oldStyles;
            // for (int i = 0; i < styleSetData.sharedStyleCount; i++) {
            //     oldStyles[i] = styleSetData.sharedStyles[i];
            // }

        }

        // assumes at least 1 of the groups changed or order was altered in some way
        public unsafe void SetSharedStyles(StyleSet styleSet, ref StackLongBuffer8 newStyleBuffer) {

            ref StyleSetData styleData = ref styleDataMap.array[styleSet.styleDataId];

            if (styleData.changeSetId == ushort.MaxValue) {
                CreateStyleChangeSet(styleSet.styleDataId, ref styleData);
            }

            ref SharedStyleChangeSet changeSetData = ref sharedStyleChangeSets.GetChecked(styleData.changeSetId);

            StyleId* newStyles = changeSetData.newStyles;
            changeSetData.newStyleCount = (byte) newStyleBuffer.size;

            for (int i = 0; i < newStyleBuffer.size; i++) {
                // newStyles[i] = newStyleBuffer.array[i];
            }

        }

        public unsafe int CreatesStyleData(int elementId) {

            // todo -- this needs lovin. don't always add, keep free list index for removal
            // probably add via chunked list instead of regular list
            styleDataMap.EnsureAdditionalCapacity(1);

            ref StyleSetData data = ref styleDataMap.array[styleDataMap.size++];
            data.state = StyleState2Byte.Normal;
            data.changeSetId = ushort.MaxValue;
            data.sharedStyleCount = 0;

            return styleDataMap.size - 1;
        }

        internal void OnElementDestroyed() {
            // styleDataFreeList.Add(elementId);

        }

        public StyleState2 GetState(int styleDataId) {
            if (styleDataId == -1) {
                return default;
            }

            return (StyleState2) styleDataMap[styleDataId].state;
        }

        internal unsafe void SetInstanceStyle(StyleSet styleSet, in StyleProperty2 property, StyleState2Byte state) {

            // search instance properties for property

            StyleSetData data = styleDataMap[styleSet.styleDataId];

            StyleSetInstanceData instance = instanceDataMap[data.instanceDataId];

            StyleProperty2* ptr = instance.properties;

            for (int i = 0; i < instance.propertyCount; i++) {

                ref StyleProperty2 p = ref ptr[i];

                if (p.state == state && p.propertyId == property.propertyId) {
                    if (p == property) {
                        return;
                    }

                    break;
                }

            }

            // get a copy and set the state flag appropriately
            StyleProperty2 update = property;
            update.state = state;

            if (data.changeSetId == -1) {
                CreateStyleChangeSet(styleSet.styleDataId, ref data);
            }

            SharedStyleChangeSet changeSet = sharedStyleChangeSets.array[data.changeSetId];

            // if (changeSet.instanceChangeId == -1) {
            //     // create it
            //     data.instanceDataId = (ushort) instanceDataMap.size;
            //     InstanceStyleChangeSet instanceStyleChangeSet = new InstanceStyleChangeSet();
            //     instanceStyleChangeSet.propertyCount = 1;
            //     instanceStyleChangeSet.properties = AllocateTempPropertyList();
            //     instanceChanges.array[changeSet.instanceChangeId] = instanceStyleChangeSet;
            // }
            // else {
            //     ref InstanceStyleChangeSet instanceChangeSet = ref instanceChanges.array[changeSet.instanceChangeId];
            //
            //     ptr = instanceChangeSet.properties;
            //
            //     // if this property id + state combo is already in the list, replace it with new value
            //     // if not, allocate a new slot and resize list if needed
            //     for (int i = 0; i < instanceChangeSet.propertyCount; i++) {
            //
            //         ref StyleProperty2 p = ref ptr[i];
            //
            //         // re-organize layout of property id to make this 1 check maybe
            //         if (p.state != state || p.propertyId != property.propertyId) {
            //             continue;
            //         }
            //
            //         if (p == property) {
            //             return;
            //         }
            //
            //         if (BitUtil.NextMultipleOf16(instance.propertyCount) != BitUtil.NextMultipleOf16(instance.propertyCount + 1)) {
            //             ResizeTempPropertyList(ref instanceChangeSet.properties, instanceChangeSet.propertyCount, instanceChangeSet.propertyCount + 1);
            //         }
            //
            //         instanceChangeSet.properties[instanceChangeSet.propertyCount++] = update;
            //
            //         return;
            //
            //     }
            //
            // }

        }

        private unsafe int ResizeTempPropertyList(ref StyleProperty2* properties, int oldSize, int size) {
            StyleProperty2* oldptr = properties;
            int newSize = BitUtil.NextMultipleOf16(size);
            properties = (StyleProperty2*) UnsafeUtility.Malloc(sizeof(StyleProperty2) * newSize, 4, Allocator.TempJob);
            UnsafeUtility.MemCpy(properties, oldptr, oldSize * sizeof(StyleProperty2));
            UnsafeUtility.Free(oldptr, Allocator.TempJob);
            return newSize;
        }

        private unsafe StyleProperty2* AllocateTempPropertyList(int count = 8) {
            return (StyleProperty2*) UnsafeUtility.Malloc(sizeof(StyleProperty2) * BitUtil.NextMultipleOf16(count), 4, Allocator.TempJob);
        }

        private unsafe void ReleaseTempPropertyList(StyleProperty2* list) {
            UnsafeUtility.Free(list, Allocator.TempJob);
        }

        private unsafe bool TryGetInstanceProperty(StyleProperty2* instanceDataProperties, ushort propertyCount, StyleProperty2 property, out StyleProperty2 styleProperty2) {
            styleProperty2 = default;
            return default;
            if (propertyCount != 0) {
                // StyleProperty2* properties = instanceData.properties;
                // for (int i = 0; i < instanceData.normal; i++) {
                //     if (properties[i].propertyId.index == property.propertyId.index) {
                //         properties[i] = property;
                //     }
                // }
            }
        }

        public static VertigoStyleSheet GetSheetForStyle(int id) {
            for (int i = 0; i < s_DebugSheets.size; i++) {
                VertigoStyleSheet styleSheet = s_DebugSheets[i];
                if (styleSheet.styleRange.start < id) {
                    return styleSheet;
                }
            }

            return null;
        }

        internal int CreatesStyle(string styleName) {
            styleNameTable.Add(styleName);
            int nameIdx = styleNameTable.size - 1; 
            int styleIdx = styleTable.Add(new VertigoStyle());
            Assert.AreEqual(nameIdx, styleIdx);
            return styleIdx;
        }

    }

}