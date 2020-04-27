using System;
using System.Runtime.InteropServices;
using UIForia.Elements;
using UIForia.Style;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine.Assertions;

namespace UIForia {

    public unsafe struct RebuildInfo {

        public SharedStyleRebuildInfo sharedStyles;

    }

    public class VertigoStyleSystem {

        internal static LightList<VertigoStyleSheet> s_DebugSheets;

        internal readonly LightList<VertigoStyleSheet> styleSheets;

        internal LightList<string> styleNameTable; // todo -- make this dev only since allocation scheme sucks
        internal PagedSplitBufferList<PropertyId, long> sharedPropertyTable;
        internal UnmanagedPagedList<VertigoStyle> styleTable;
        internal UnmanagedPagedList<VertigoSelector> selectorTable;
        internal UnmanagedList<RebuildInfo> rebuildTable;

        internal UnmanagedList<SharedStyleChangeSet> sharedStyleChangeSets; // stack based buffer allocator, clear on frame end
        internal UnmanagedList<StyleSetData> styleDataMap;
        internal UnmanagedList<StyleSetInstanceData> instanceDataMap;

        public VertigoStyleSystem() {

            styleNameTable = new LightList<string>(128);
            sharedPropertyTable = new PagedSplitBufferList<PropertyId, long>(1024, Allocator.Persistent);
            styleTable = new UnmanagedPagedList<VertigoStyle>(128, Allocator.Persistent);
            selectorTable = new UnmanagedPagedList<VertigoSelector>(64, Allocator.Persistent);
            styleDataMap = new UnmanagedList<StyleSetData>(32, Allocator.Persistent); // todo this should be an UnsafeChunkedList I think
            instanceDataMap = new UnmanagedList<StyleSetInstanceData>(16, Allocator.Persistent); // todo this should be an UnsafeChunkedList I think
            sharedStyleChangeSets = new UnmanagedList<SharedStyleChangeSet>(32, Allocator.Persistent);
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
            sharedPropertyTable.Dispose();
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

        internal UIElement rootElement;

        public unsafe struct ConstructPreAnimationStyles : IJob {

            // input = instance, shared, selector, animator, transition

            public void Execute() { }

        }

        // the more i can do all these operations efficiently in isolation, the better the whole system gets
        // just combine the results at the end and use that as our output. memory shouldn't be a huge concern
        // focus on cache locality where it makes a difference but lean more on parallel than cache coherence 
        // for our main performance boost.

        public unsafe struct AssembleRebuildList : IJob {

            public UnmanagedList<StyleSetData> table;
            public UnmanagedList<StyleSetData> rebuildList;

            public void Execute() {

                for (int i = 0; i < table.size; i++) {

                    if (table.array[i].needsRebuild) {
                        rebuildList.Add(table.array[i]);
                    }

                    table.array[i].needsRebuild = false;
                    table.array[i].changeSetId = ushort.MaxValue;
                }

            }

        }

        public UnmanagedPagedList<VertigoStylePropertyInfo> styleInfoTable;

        public unsafe void OnUpdate() {

            int totalEnabledCount = 100; // don't know this without a traversal

            // rebuild map = new UnmanagedList<bool>(highestElementId, Allocator.TempJob); // this or a per thread rebuild me list

            // per-thread allocators for data
            // combine changes into persistent buffer at the end of frame when we know all the data
            // this means allocating intermediate buffers for all properties we want to handle
            // which is good for locality anyway
            // with a decent block allocator in style root this can be pretty fast
            // gc old pointers, assign new. new pointers found in rebuild data
            // memcpy in bulk should be fine
            // must be done single threaded but can happen in parallel with layout or rendering

            UnmanagedList<ElementTraversalInfo> traversalInfo = new UnmanagedList<ElementTraversalInfo>(totalEnabledCount, Allocator.TempJob);

            UnmanagedPagedList<StyleStateGroup>.PerThread addedStyleStateGroups = new UnmanagedPagedList<StyleStateGroup>.PerThread(64, Allocator.TempJob);
            UnmanagedPagedList<StyleStateGroup>.PerThread removedStyleStateGroups = new UnmanagedPagedList<StyleStateGroup>.PerThread(64, Allocator.TempJob);

            UnmanagedList<StyleStateGroup> mergedAddedStyleStateGroups = new UnmanagedList<StyleStateGroup>(Allocator.TempJob);
            UnmanagedList<StyleStateGroup> mergedRemovedStyleStateGroups = new UnmanagedList<StyleStateGroup>(Allocator.TempJob);

            PagedSplitBufferList<PropertyId, long>.PerThread perThreadSharedStyleOutput = new PagedSplitBufferList<PropertyId, long>.PerThread(256, Allocator.TempJob);
            // selectors and animation and shared styles all depend on computed style state diff

            JobHandle traversalIndex = new TraversalIndexJob_Managed() {
                rootElementHandle = GCHandle.Alloc(rootElement),
                traversalInfo = traversalInfo
            }.Schedule();

            JobHandle styleStateDiff = VertigoScheduler.ParallelForRange(sharedStyleChangeSets.size, 10, new ComputeStyleStateDiff() {
                    changeSets = sharedStyleChangeSets,
                    addedStyleStateGroups = addedStyleStateGroups,
                    removedStyleStateGroups = removedStyleStateGroups
                })
                .Then(
                    new MergePerThreadPageLists<StyleStateGroup>() {
                        perThreadLists = addedStyleStateGroups,
                        outputList = mergedAddedStyleStateGroups
                    },
                    new MergePerThreadPageLists<StyleStateGroup>() {
                        perThreadLists = removedStyleStateGroups,
                        outputList = mergedRemovedStyleStateGroups
                    }
                );

            JobHandle buildSharedStylesHandle = VertigoScheduler.Await(styleStateDiff)
                .ThenParallelForRange(sharedStyleChangeSets.size, 10, new BuildSharedStyles() {
                    rebuildTable = rebuildTable,
                    changedSharedStyles = default, // todo -- this list needs to be created somewhere 
                    styleTable = styleInfoTable,
                    perThreadStyleOutput = perThreadSharedStyleOutput, // todo -- job to copy these styles to persistent location
                    stylePropertyTable = sharedPropertyTable
                });

            JobHandle selectors = RunSelectors(styleStateDiff, traversalIndex);

            JobHandle sharedStyleAnimations = RunSharedStyleAnimations(styleStateDiff);

            VertigoScheduler.Await(buildSharedStylesHandle, selectors).Then(new AssembleRebuildList() {
                table = styleDataMap,

            });

            ConstructPreAnimationStyles preAnimationStyles = new ConstructPreAnimationStyles() { };

            JobHandle selectorAnimations = RunSelectorAnimations(selectors);

            JobHandle finalStyles = VertigoScheduler.Await(sharedStyleAnimations, selectorAnimations);

            finalStyles.Complete();

            addedStyleStateGroups.Dispose();
            mergedAddedStyleStateGroups.Dispose();
            removedStyleStateGroups.Dispose();
            mergedRemovedStyleStateGroups.Dispose();
            perThreadSharedStyleOutput.Dispose();
            traversalInfo.Dispose();
            // style.computed.PropertyX;
            // style.animated.PropertyY;

            // output?
            // list of rebuilt elements
            // list of rebuild elements with dirty?
            // rebuild element + up to date style list per element?
            // at least for testing I think it makes sense to compute up to date style info for elements

            // probably have a base value + animated snapshot value for each property
            // this lets me ask "are you animating? and if yes give me a formula I can plug my data into" to resolve final values in layout system

            EndFrame();
        }

        private JobHandle RunSharedStyleAnimations(JobHandle styleStateDiff) {
            return new JobHandle();
        }

        private JobHandle RunSelectorAnimations(JobHandle selectorDep) {
            return new JobHandle();
        }

        private JobHandle RunSelectors(JobHandle styleStateDiff, JobHandle traversalIndexJob) {
            return new JobHandle();
        }

        internal unsafe void EndFrame() {
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

            changeSetData.newState = styleSetData.state | StyleState2Byte.Normal;
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

        }

        // would be great to do this in bulk for elements
        // not sure how to create n elements from compiled templates though
        // repeat is probably the issue
        // technically a given entry point knows how many elements it will create though

        public unsafe int CreatesStyleData() {

            // todo -- this needs lovin. don't always add, keep free list index for removal
            // probably add via chunked list instead of regular list
            styleDataMap.EnsureAdditionalCapacity(1);

            ref StyleSetData data = ref styleDataMap.array[styleDataMap.size++];
            data.state = StyleState2Byte.Normal;
            data.changeSetId = ushort.MaxValue;
            data.sharedStyleCount = 0;
            data.instanceData = null;

            return styleDataMap.size - 1;
        }

        public StyleState2 GetState(int styleDataId) {
            if (styleDataId == -1) {
                return default;
            }

            return (StyleState2) styleDataMap[styleDataId].state;
        }

        // default to having space for 8 properties
        // todo -- would be awesome to use a better allocator here and not the Unity one so we can ensure some degree of locality
        private unsafe InstanceStyleData* CreateInstanceStyleData(int capacity = 8) {
            void* rawData = UnsafeUtility.Malloc(sizeof(InstanceStyleData) + (capacity * sizeof(PropertyId)) + (capacity * sizeof(long)), 4, Allocator.Persistent);
            PropertyId* keyStart = (PropertyId*) ((long*) rawData + sizeof(InstanceStyleData));
            long* dataStart = (long*) (keyStart + (capacity * sizeof(PropertyId)));

            InstanceStyleData* data = (InstanceStyleData*) rawData;
            data->keys = keyStart;
            data->data = dataStart;
            data->capacity = capacity;

            data->totalStyleCount = 0;
            data->usedStyleCount = 0;
            return data;
        }

        // todo -- would be awesome to use a better allocator here and not the Unity one so we can ensure some degree of locality
        private unsafe InstanceStyleData* ResizeInstanceStyleData(InstanceStyleData* current, int size) {
            InstanceStyleData* newptr = CreateInstanceStyleData(size + 8);
            newptr->totalStyleCount = current->totalStyleCount;
            newptr->usedStyleCount = current->usedStyleCount;
            UnsafeUtility.MemCpy(newptr->keys, current->keys, sizeof(PropertyId) * current->usedStyleCount);
            UnsafeUtility.MemCpy(newptr->data, current->data, sizeof(long) * current->usedStyleCount);
            UnsafeUtility.Free(current, Allocator.Persistent);
            return newptr;
        }

        // note: if data is a pointer type we assume the handle has already been allocated at this point
        private unsafe void AddOrUpdateInstanceStyle(StyleSetData data, PropertyId propertyId, long styleData, StyleState2Byte state) {

            data.instanceData = data.instanceData != null
                ? data.instanceData
                : CreateInstanceStyleData();

            int idx = -1;
            InstanceStyleData* instanceStyleData = data.instanceData;

            for (int i = 0; i < instanceStyleData->totalStyleCount; i++) {
                if (instanceStyleData->keys[i] == propertyId) {
                    idx = i;

                    if (instanceStyleData->data[i] == styleData) {
                        return;
                    }

                    instanceStyleData->data[i] = styleData;
                    break;
                }
            }

            if (idx == -1) {

                if (instanceStyleData->totalStyleCount + 1 >= instanceStyleData->capacity) {
                    data.instanceData = ResizeInstanceStyleData(data.instanceData, instanceStyleData->totalStyleCount + 1);
                    instanceStyleData = data.instanceData;
                }

                instanceStyleData->keys[instanceStyleData->totalStyleCount] = propertyId;
                instanceStyleData->data[instanceStyleData->totalStyleCount] = styleData;
                instanceStyleData->totalStyleCount++;
            }

            if ((state & data.state) != 0) {
                // rebuild if not already rebuilding
                // 
            }

        }

        private unsafe void RemoveInstanceStyle(StyleSetData data, PropertyId propertyId, StyleState2Byte state) {

            if (data.instanceData == null) {
                return;
            }

            InstanceStyleData* instanceStyleData = data.instanceData;

            int idx = -1;
            for (int i = 0; i < instanceStyleData->totalStyleCount; i++) {
                if (instanceStyleData->keys[i] == propertyId) {
                    idx = i;
                    break;
                }
            }

            if (idx == -1) return;

            if ((propertyId.typeFlags & PropertyTypeFlags.RequireDestruction) != 0) {
                long currentValue = instanceStyleData->data[idx];
                if (currentValue != 0) {
                    IntPtr ptr = (IntPtr) currentValue;
                    if (ptr != IntPtr.Zero) {
                        GCHandle.FromIntPtr(ptr).Free();
                    }
                }
            }

            if ((state & data.state) != 0) {
                // rebuild if not already rebuilding
            }
        }

        internal void SetInstanceStyle(StyleSet styleSet, PropertyId propertyId, long? styleData, StyleState2Byte state) {
            StyleSetData data = styleDataMap[styleSet.styleDataId];

            propertyId.state = state;

            if (styleData.HasValue) {
                AddOrUpdateInstanceStyle(data, propertyId, styleData.Value, state);
            }
            else {
                RemoveInstanceStyle(data, propertyId, state);
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

        // todo -- create in bulk where possible
        internal int CreatesStyle(string styleName) {
            styleNameTable.Add(styleName);
            int nameIdx = styleNameTable.size - 1;
            int styleIdx = styleTable.Add(new VertigoStyle());
            Assert.AreEqual(nameIdx, styleIdx);
            return styleIdx;
        }

        internal unsafe void EnterState(StyleSet styleSet, StyleState2 state) {
            StyleSetData data = styleDataMap[styleSet.styleDataId];
            if (data.changeSetId != ushort.MaxValue) {
                bool hasStateStyles = false;
                for (int i = 0; i < data.sharedStyleCount; i++) {
                    StyleId styleId = data.sharedStyles[i];
                    if (styleId.DefinesState(state)) {
                        hasStateStyles = true;
                    }
                }

                if (hasStateStyles) {
                    CreateStyleChangeSet(styleSet.styleDataId, ref data);
                    ref SharedStyleChangeSet changeSetData = ref sharedStyleChangeSets.array[data.changeSetId];
                    changeSetData.newState |= (StyleState2Byte) (state);
                }
            }
        }

        internal unsafe void ExitState(StyleSet styleSet, StyleState2 state) {
            StyleSetData data = styleDataMap[styleSet.styleDataId];
            if (data.changeSetId != ushort.MaxValue) {
                bool hasStateStyles = false;
                for (int i = 0; i < data.sharedStyleCount; i++) {
                    StyleId styleId = data.sharedStyles[i];
                    if (styleId.DefinesState(state)) {
                        hasStateStyles = true;
                    }
                }

                if (hasStateStyles) {
                    CreateStyleChangeSet(styleSet.styleDataId, ref data);
                    ref SharedStyleChangeSet changeSetData = ref sharedStyleChangeSets.array[data.changeSetId];
                    changeSetData.newState &= (StyleState2Byte) (~state);
                }
            }
        }

    }

    public unsafe struct InstanceStyleData {

        public int capacity;
        public int totalStyleCount;
        public int usedStyleCount;
        public PropertyId* keys;
        public long* data;

    }

}