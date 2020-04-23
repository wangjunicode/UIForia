using System;
using System.Runtime.InteropServices;
using UIForia.Elements;
using UIForia.Style;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Assertions;

namespace UIForia {

    public unsafe struct BatchRangeJobData {

        public readonly int index;
        public readonly int maxJobCount;
        public readonly int minBatchSize;

        public BatchRangeJobData(int index, int minBatchSize, int maxJobCount = -1) {
            this.index = index;
            this.minBatchSize = minBatchSize < 1 ? 1 : minBatchSize;
            this.maxJobCount = (maxJobCount <= 0) ? 8 : maxJobCount;
        }

        public bool TryGetJobBatchRange(int workItemCount, out RangeInt range) {
            range = default;
            if (workItemCount <= 0) {
                return false;
            }

            int jobCount = math.clamp((workItemCount / minBatchSize), 1, maxJobCount);

            if (index >= jobCount) {
                return false;
            }

            int rangeSize = (workItemCount / jobCount);
            int remaining = workItemCount - (rangeSize * jobCount);

            int* sizes = stackalloc int[jobCount];

            for (int i = 0; i < jobCount; i++) {
                sizes[i] = rangeSize;
            }

            int idx = 0;
            while (remaining > 0) {
                sizes[idx]++;
                remaining--;
                idx++;
                if (idx == jobCount) idx = 0;
            }

            int start = 0;
            for (int i = 0; i < index; i++) {
                start += sizes[i];
            }

            range = new RangeInt(start, sizes[index]);
            return true;
        }

    }

    public class VertigoStyleSystem {

        internal static LightList<VertigoStyleSheet> s_DebugSheets;

        internal const ushort k_ImplicitModuleId = 0;

        public UIElement rootElement;

        internal readonly LightList<VertigoStyleSheet> styleSheets;

        internal LightList<string> styleNameTable; // todo -- make this dev only since allocation scheme sucks
        internal UnmangedPagedList<StyleProperty2> propertyTable;
        internal UnmangedPagedList<VertigoStyle> styleTable;
        internal UnmangedPagedList<VertigoSelector> selectorTable;

        internal Util.Unsafe.UnmanagedList<SharedStyleChangeSet> sharedStyleChangeSets; // stack based buffer allocator, clear on frame end
        internal Util.Unsafe.UnmanagedList<StyleSetData> styleDataMap;
        internal Util.Unsafe.UnmanagedList<StyleSetInstanceData> instanceDataMap;
        internal Util.Unsafe.UnmanagedList<InstanceStyleChangeSet> instanceChanges;

        public VertigoStyleSystem() {

#if UNITY_EDITOR
            AssertSize.AssertSizes();
#endif
            styleNameTable = new LightList<string>(128);
            propertyTable = new UnmangedPagedList<StyleProperty2>(256, Allocator.Persistent);
            styleTable = new UnmangedPagedList<VertigoStyle>(128, Allocator.Persistent);
            selectorTable = new UnmangedPagedList<VertigoSelector>(64, Allocator.Persistent);
            styleDataMap = new Util.Unsafe.UnmanagedList<StyleSetData>(32, Allocator.Persistent); // todo this should be an UnsafeChunkedList I think
            instanceDataMap = new Util.Unsafe.UnmanagedList<StyleSetInstanceData>(16, Allocator.Persistent); // todo this should be an UnsafeChunkedList I think
            instanceChanges = new Util.Unsafe.UnmanagedList<InstanceStyleChangeSet>(32, Allocator.Persistent);
            sharedStyleChangeSets = new Util.Unsafe.UnmanagedList<SharedStyleChangeSet>(32, Allocator.Persistent);
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

        private static readonly int s_ProcessorCount = SystemInfo.processorCount;

        private static unsafe int GetJobParameters(int workItemCount, int minBatchSize, int* output) {
            if (workItemCount <= 0) return 0;
            if (minBatchSize <= 0) minBatchSize = 4;

            int jobCount = Mathf.Clamp((workItemCount / minBatchSize), 1, s_ProcessorCount);

            int rangeSize = (workItemCount / jobCount);
            int remaining = workItemCount - (rangeSize * jobCount);

            for (int i = 0; i < jobCount; i++) {
                output[i] = rangeSize;
            }

            int idx = 0;
            while (remaining > 0) {
                output[idx]++;
                remaining--;
                idx++;
                if (idx == jobCount) idx = 0;
            }

            return jobCount;
        }

        [AssertSize(16)]
        public unsafe struct PropertyChangeRange {

            public int styleSetId;
            public int count;
            public StyleProperty2* properties;

        }

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct RebuildStyleJob : IJob {

            public BatchRangeJobData rangeJobData;

            [ReadOnly] public NativeList<int> rebuildList;

            public UnmangedPagedList<StyleProperty2>.PerThread perThreadPagedStyleList;
            public UnmangedPagedList<PropertyChangeRange>.PerThread perThreadPagedChangeRangeList;
            
            public UnmangedPagedList<VertigoStyle> styleTable;
            public UnmangedPagedList<StyleProperty2> stylePropertyTable;

            [NativeSetThreadIndex] private int threadIndex;

            public void Execute() {

                if (!rangeJobData.TryGetJobBatchRange(rebuildList.Length, out RangeInt range)) {
                    return;
                }

                UnmangedPagedList<StyleProperty2> outputPropertyList = perThreadPagedStyleList.GetListForThread(threadIndex);
                UnmangedPagedList<PropertyChangeRange> changeRanges = perThreadPagedChangeRangeList.GetListForThread(threadIndex);
                UnmanagedList<StyleProperty2> buffer = new UnmanagedList<StyleProperty2>(135, Allocator.TempJob);

                for (int index = range.start; index < range.end; index++) {
                    buffer.size = 0;
                    // need a list to output properties to
                    // probably best to fill a scratch list and copy into destination
                    // paged lists aren't a bad idea here I think

                    // stylePropertyList.AddRangeToSamePage(items, )

                    BitBuffer256 buffer256 = default;
                    IntBoolMap activeMap = new IntBoolMap(buffer256.data, 8);

                    StyleSetData styleData = default;

                    StyleState2 state = (StyleState2) styleData.state;

                    // store currently active style set
                    // would then store all inherited properties as well

                    // should i store a style list per element?
                    // easy to check values
                    // much more memory usage
                    // cache method to opt in makes a lot more sense to me
                    // when setting a property i still need to see if it would apply 
                    // if properties set but not for an active state -> no-op

                    // build list of instance properties first
                    if (styleData.instanceDataId != -1) {
                        StyleSetInstanceData instanceData = default;
                        // sort and only add states we want
                        for (int i = 0; i < instanceData.propertyCount; i++) {
                            ref StyleProperty2 property = ref instanceData.properties[i];
                            if (activeMap.TrySetIndex(property.propertyId.index)) {
                                buffer.Add(property);
                            }
                        }
                    }

                    // now add selectors
                    if (styleData.selectorDataId != -1) {
                        SelectorEffectData selectorEffectData = default;
                        for (int i = 0; i < selectorEffectData.count; i++) {
                            ref StyleProperty2 property = ref selectorEffectData.properties[i];
                            if (activeMap.TrySetIndex(property.propertyId.index)) {
                                buffer.Add(property);
                            }
                        }
                    }

                    // todo -- for style properties that are 'compiled' statically we can re-arrange the buffer to read 
                    // property Id's up front and skip t
                    for (int sharedStyleIndex = 0; sharedStyleIndex < styleData.sharedStyleCount; sharedStyleIndex++) {
                        StyleId styleId = styleData.sharedStyles[sharedStyleIndex];
                        VertigoStyle style = styleTable[styleId.index];

                        if ((style.propertyOffset == ushort.MaxValue)) {
                            continue;
                        }

                        StyleProperty2* properties = stylePropertyTable.GetPointer(style.propertyOffset);

                        if ((state & StyleState2.Active) != 0) {
                            for (int i = 0; i < style.activeCount; i++) {
                                ref StyleProperty2 property = ref properties[i];
                                if (activeMap.TrySetIndex(property.propertyId.index)) {
                                    buffer.Add(property);
                                }
                            }
                        }

                        properties += style.activeCount;

                        if ((state & StyleState2.Focused) != 0) {
                            for (int i = 0; i < style.focusCount; i++) {
                                ref StyleProperty2 property = ref properties[i];
                                if (activeMap.TrySetIndex(property.propertyId.index)) {
                                    buffer.Add(property);
                                }
                            }
                        }

                        properties += style.focusCount;

                        if ((state & StyleState2.Hover) != 0) {
                            for (int i = 0; i < style.focusCount; i++) {
                                ref StyleProperty2 property = ref properties[i];
                                if (activeMap.TrySetIndex(property.propertyId.index)) {
                                    buffer.Add(property);
                                }
                            }
                        }

                        properties += style.hoverCount;

                        for (int i = 0; i < style.focusCount; i++) {
                            ref StyleProperty2 property = ref properties[i];
                            if (activeMap.TrySetIndex(property.propertyId.index)) {
                                buffer.Add(property);
                            }
                        }

                        RangeInt propertyRange = outputPropertyList.AddRangeToSamePage(buffer.array, buffer.size);
                        changeRanges.Add(new PropertyChangeRange());

                    }

                }

            }

        }

        public unsafe struct StyleChangeSet {

            public StyleProperty2* currentStyles;
            public BitBuffer256 trackedProperties;

            public void TrackProperty(PropertyId propertyId) {
                new IntBoolMap((uint*) UnsafeUtility.AddressOf(ref trackedProperties), 8).SetIndex(propertyId.index);
            }

            public void UntrackProperty(PropertyId propertyId) {
                new IntBoolMap((uint*) UnsafeUtility.AddressOf(ref trackedProperties), 8).UnsetIndex(propertyId.index);
            }

            public void UpdatePropertyValue(StyleProperty2 property) {
                
            }

        }

        public unsafe struct UpdateChangeListJobs : IJobParallelForDeferBatched {

            public UnmanagedList<PropertyChangeRange> propertyChangeRanges;
            public UnmangedPagedList<StyleProperty2> propertyTables;
            public UnmangedPagedList<StyleSetData> styleSetDataTable;
            
            public void Execute(int start, int end) {
            
                for (int i = start; i < end; i++) {
                    PropertyChangeRange changeRange = propertyChangeRanges[i];
                    StyleSetData data = styleSetDataTable[changeRange.styleSetId];
                    
                    if (data.hasChangeDetector) {
                        StyleChangeSet changeSet = default;
                        IntBoolMap map = new IntBoolMap((uint*)UnsafeUtility.AddressOf(ref changeSet.trackedProperties), 8);
                        for (int j = 0; j < changeRange.count; j++) {
                            ref StyleProperty2 property = ref changeRange.properties[j];
                            if (map[property.propertyId.index]) {
                                // buffer change and append/re-allocate in 1 step if we need to
                                
                            }
                        }
                    }
                    
                }
                
                
                
            }

        }

        public unsafe void OnUpdate() {

            int changeCount = sharedStyleChangeSets.size;

            int* sizes = stackalloc int[s_ProcessorCount];

            int jobCount = GetJobParameters(changeCount, 16, sizes);

            LightList<SharedStyleJobData> data = new LightList<SharedStyleJobData>(jobCount);
            NativeArray<JobHandle> sharedStyleUpdateHandles = new NativeArray<JobHandle>(jobCount, Allocator.TempJob);

            int lastRange = 0;

            for (int i = 0; i < jobCount; i++) {

                SharedStyleJobData jobData = new SharedStyleJobData {
                    addedList = new NativeList<StyleStateGroup>(sizes[i] * 4, Allocator.TempJob),
                    removedList = new NativeList<StyleStateGroup>(sizes[i] * 4, Allocator.TempJob),
                    rebuildList = new NativeList<int>(sizes[i], Allocator.TempJob)
                };

                ProcessSharedStyleUpdatesJob job = new ProcessSharedStyleUpdatesJob() {
                    addedList = jobData.addedList,
                    removedList = jobData.removedList,
                    rebuildList = jobData.rebuildList,
                    changeSets = new UnsafeSpan<SharedStyleChangeSet>(lastRange, sizes[i], sharedStyleChangeSets.array)
                };

                // defer batch rebuild job
                // write output into each change set i guess since we don't have a way to know ranges ahead of time.
                // could create fixed buffers of style property count for each rebuilt element
                // kinda of a lot of memory though

                // defer but give me 1 - 4 jobs and let me know their indices?

                // schedule n jobs 
                // early out if no work waiting for them
                // use some metric to know they should pick up work or not
                // if each one computes a range
                // and knows it index
                // it should know if it has data or not to work with

                lastRange += sizes[i];

                data.Add(jobData);

                sharedStyleUpdateHandles[i] = job.Schedule();
            }

            JobHandle handle = JobHandle.CombineDependencies(sharedStyleUpdateHandles);

            NativeList<int> rebuildList = new NativeList<int>(32, Allocator.TempJob);

            // RebuildParallelJob().Schedule(rebuild);

            // rebuild in parallel somehow
            // need update lists
            // basically need parallel writes

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

        // would be great to do this in bulk for elements
        // not sure how to create n elements from compiled templates though
        // repeat is probably the issue
        // technically a given entry point knows how many elements it will create though

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