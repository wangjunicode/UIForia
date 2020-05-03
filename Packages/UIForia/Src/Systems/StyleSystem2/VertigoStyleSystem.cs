using System;
using UIForia.Elements;
using UIForia.Style;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Assertions;

// ReSharper disable MemberCanBePrivate.Global

namespace UIForia {

    public class VertigoStyleSystem {

        public const int k_MaxStyleProperties = 256;

        internal UIElement rootElement;

        internal static LightList<VertigoStyleSheet> s_DebugSheets;
        internal static LightList<string> s_DebugStyleNames;

        internal readonly LightList<VertigoStyleSheet> styleSheets;
        internal LightList<string> styleNameTable; // todo -- make this dev only since allocation scheme sucks

        internal StyleResultTable sharedStyleTable;
        internal StyleResultTable selectorStyleTable;
        internal StyleResultTable animatorStyleTable;

        internal PerThread<StyleRebuildResultList> sharedStyleRebuildResult;
        internal PerThread<StyleRebuildResultList> selectorStyleRebuildResult;
        internal PerThread<StyleRebuildResultList> animatorStyleRebuildResult;

        internal SharedStyleChangeSet sharedStyleChangeSet;

        internal PagedSplitBufferList<PropertyId, long> sharedPropertyTable;

        internal UnmanagedPagedList<StyleSetData> styleSetTable;

        public VertigoStyleSystem() {

            styleNameTable = new LightList<string>(128);
            s_DebugStyleNames = styleNameTable;
            sharedPropertyTable = new PagedSplitBufferList<PropertyId, long>(1024, Allocator.Persistent);
            styleSetTable = new UnmanagedPagedList<StyleSetData>(128, Allocator.Persistent);
            styleSheets = new LightList<VertigoStyleSheet>();

            sharedStyleTable = new StyleResultTable();
            selectorStyleTable = new StyleResultTable();
            animatorStyleTable = new StyleResultTable();

            sharedStyleRebuildResult = new PerThread<StyleRebuildResultList>();
            selectorStyleRebuildResult = new PerThread<StyleRebuildResultList>();
            animatorStyleRebuildResult = new PerThread<StyleRebuildResultList>();

            sharedStyleChangeSet = new SharedStyleChangeSet(256, 64, Allocator.Persistent);

            s_DebugSheets = styleSheets;

        }

        internal bool TryResolveStyle(string sheetName, string styleName, out StyleId styleId) {
            styleId = default;
            VertigoStyleSheet sheet = GetStyleSheet(sheetName);
            return sheet != null && sheet.TryGetStyle(styleName, out styleId);
        }

        public unsafe void Destroy() {

            // for (int i = 0; i < styleDataMap.size; i++) {
            //     StyleSetData data = styleDataMap.array[i];
            //     // if (data.sharedStyles != default) {
            //     //     UnsafeUtility.Free(data.sharedStyles, Allocator.Persistent);
            //     //     // data.sharedStyles = default;
            //     // }
            // }

            sharedStyleRebuildResult.Dispose();
            selectorStyleRebuildResult.Dispose();
            animatorStyleRebuildResult.Dispose();

            sharedStyleTable.Dispose();
            selectorStyleTable.Dispose();
            animatorStyleTable.Dispose();

            sharedStyleChangeSet.Dispose();

            sharedPropertyTable.Dispose();
            styleSetTable.Dispose();
            for (int i = 0; i < styleSheets.size; i++) {
                styleSheets.array[i].Destroy();
            }
        }

        // the more i can do all these operations efficiently in isolation, the better the whole system gets
        // just combine the results at the end and use that as our output. memory shouldn't be a huge concern
        // focus on cache locality where it makes a difference but lean more on parallel than cache coherence 
        // for our main performance boost.

        // per-thread allocators for data
        // combine changes into persistent buffer at the end of frame when we know all the data
        // this means allocating intermediate buffers in jobs, which is good for locality anyway

        public unsafe void OnUpdate() {

            // JobHandle traversalIndex = new TraversalIndexJob_Managed() {
            //     rootElementHandle = GCHandle.Alloc(rootElement),
            //     traversalInfo = traversalInfo
            // }.Schedule();

            PerThread<ConvertedStyleList> perThread_ConvertedStyleIds = new PerThread<ConvertedStyleList>(Allocator.TempJob);
            PerThread<StyleRebuildResultList> perThread_RebuildSharedStyles = new PerThread<StyleRebuildResultList>(Allocator.TempJob);
            UnmanagedList<ConvertedStyleId> gathered_ConvertedStyleIds = new UnmanagedList<ConvertedStyleId>(Allocator.TempJob);

            VertigoScheduler.SchedulerStep convertStyleIdsHandle = VertigoScheduler.ParallelForRange(sharedStyleChangeSet.Size, 15, new ConvertStyleIdsToStatePairs() {
                    sharedStyleChangeSet = sharedStyleChangeSet,
                    perThreadOutput = perThread_ConvertedStyleIds
                })
                .Then(new MergePerThreadData<ConvertedStyleList, ConvertedStyleId>() {
                    perThread = perThread_ConvertedStyleIds,
                    gatheredOutput = gathered_ConvertedStyleIds
                });

            VertigoScheduler.Await(convertStyleIdsHandle)
                .Then(new BuildSharedStyles() {
                    convertedStyleList = gathered_ConvertedStyleIds,
                    table_StyleInfo = default, // todo
                    perThread_RebuiltResult = perThread_RebuildSharedStyles
                });

            EndFrame();
            
            perThread_RebuildSharedStyles.Dispose();
            perThread_ConvertedStyleIds.Dispose();
            gathered_ConvertedStyleIds.Dispose(); // must be disposed after perThread_ConvertedStyleIds!
            

            //
            //     JobHandle buildSharedStylesHandle = VertigoScheduler.Await(styleStateDiff)
            //     .ThenParallelForRange(sharedStyleChangeSets.size, 10, new BuildSharedStyles() {
            //         // inputs 
            //         changeSets = sharedStyleChangeSets,
            //         styleTable = stylePropertyInfoTable,
            //         stylePropertyTable = sharedPropertyTable,
            //         // output
            //         perThreadRebuildResults = sharedStyleRebuildResult
            //     })
            //     .Then(new WriteToStyleResultTable() {
            //         perThreadRebuildResults = sharedStyleRebuildResult
            //         // write our re-built styles into the element's shared style storage
            //         // will be queried later as part of rebuild
            //         // sharedStyleTable.Update(changes); for each change mark as free
            //         // traverse & compress 
            //         // or keep block sizes and move as needed
            //         // sharedStyleTable[id].SetBuffer(buffer); -> marks block as free and re-allocates if needed. can use unity persistent allocator for now
            //         // probably other smart things we can do like keep table sorted by update rate or age
            //         // writes need to come from single thread for safety 
            //         // reads need to depend on this job
            //     });
            //
            // JobHandle selectorRebuildHandle = new JobHandle();

            // VertigoScheduler.Await(buildSharedStylesHandle, selectorRebuildHandle)
            //     .Then(new ConstructRebuildList() { })
            //     .ThenParallelForRangeDefer(new StyleRebuildJob() { })
            //     .Then(new ComputeDiffLists());

            // sharedStyleTable.GetUpdatesThisFrame().Concat(instanceSharedTable.GetUpdatesThisFrame())

            // style.computed.PropertyX;
            // style.animated.PropertyY;

            // output?
            // list of rebuilt elements
            // list of rebuild elements with dirty?
            // rebuild element + up to date style list per element?
            // at least for testing I think it makes sense to compute up to date style info for elements

            // probably have a base value + animated snapshot value for each property
            // this lets me ask "are you animating? and if yes give me a formula I can plug my data into" to resolve final values in layout system

            // JobHandle styleStateDiff = VertigoScheduler.ParallelForRange(sharedStyleChangeSet.Size, 10, new ComputeStyleStateDiff() {
            //         sharedStyleChangeSet = sharedStyleChangeSet,
            //         addedStyleStateGroups = addedStyleStateGroups,
            //         removedStyleStateGroups = removedStyleStateGroups
            //     })
            //     .Then(
            //         new MergePerThreadPageLists<StyleStatePair>() {
            //             perThreadLists = addedStyleStateGroups,
            //             outputList = mergedAddedStyleStateGroups
            //         },
            //         new MergePerThreadPageLists<StyleStatePair>() {
            //             perThreadLists = removedStyleStateGroups,
            //             outputList = mergedRemovedStyleStateGroups
            //         }
            //     );
        }

        internal void EndFrame() {

            sharedStyleRebuildResult.Clear();
            selectorStyleRebuildResult.Clear();
            animatorStyleRebuildResult.Clear();
            sharedStyleChangeSet.Clear();
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

        // assumes at least 1 of the groups changed or order was altered in some way
        // if this gets called multiple times in a frame for 1 element we just allocate a new 
        // property range and ignore the hole since that memory is released every frame.
        public unsafe void SetSharedStyles(StyleSet styleSet, ref StackIntBuffer7 newStyleBuffer) {
            fixed (int* buffer = newStyleBuffer.array) {
                ref StyleSetData styleData = ref styleSetTable.GetReference(styleSet.styleDataId);
                sharedStyleChangeSet.SetSharedStyles(styleSet.styleDataId, ref styleData, (StyleId*) buffer, newStyleBuffer.size);
            }
        }

        // would be great to do this in bulk for elements
        // not sure how to create n elements from compiled templates though
        // repeat is probably the issue
        // technically a given entry point knows how many elements it will create though

        public int CreatesStyleData() {
            // todo -- this needs lovin. don't always add, keep free list index for removal
            // can be extracted into a new type: UnmanagedTable that supports adding/removal with freelist of spare indices
            return styleSetTable.Add(new StyleSetData() {
                state = StyleState2Byte.Normal,
                changeSetId = ushort.MaxValue,
            });
        }

        public StyleState2 GetState(int styleDataId) {
            if (styleDataId == -1) {
                return default;
            }

            return (StyleState2) styleSetTable[styleDataId].state;
        }

        // instanceStyleTable[id] 
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

            // data.instanceData = data.instanceData != null
            //     ? data.instanceData
            //     : CreateInstanceStyleData();
            //
            // int idx = -1;
            // InstanceStyleData* instanceStyleData = data.instanceData;
            //
            // for (int i = 0; i < instanceStyleData->totalStyleCount; i++) {
            //     if (instanceStyleData->keys[i] == propertyId) {
            //         idx = i;
            //
            //         if (instanceStyleData->data[i] == styleData) {
            //             return;
            //         }
            //
            //         instanceStyleData->data[i] = styleData;
            //         break;
            //     }
            // }
            //
            // if (idx == -1) {
            //
            //     if (instanceStyleData->totalStyleCount + 1 >= instanceStyleData->capacity) {
            //         data.instanceData = ResizeInstanceStyleData(data.instanceData, instanceStyleData->totalStyleCount + 1);
            //         instanceStyleData = data.instanceData;
            //     }
            //
            //     instanceStyleData->keys[instanceStyleData->totalStyleCount] = propertyId;
            //     instanceStyleData->data[instanceStyleData->totalStyleCount] = styleData;
            //     instanceStyleData->totalStyleCount++;
            // }
            //
            // if ((state & data.state) != 0) {
            //     // rebuild if not already rebuilding
            //     // 
            // }

        }

        private unsafe void RemoveInstanceStyle(StyleSetData data, PropertyId propertyId, StyleState2Byte state) {

            // if (data.instanceData == null) {
            //     return;
            // }
            //
            // InstanceStyleData* instanceStyleData = data.instanceData;
            //
            // int idx = -1;
            // for (int i = 0; i < instanceStyleData->totalStyleCount; i++) {
            //     if (instanceStyleData->keys[i] == propertyId) {
            //         idx = i;
            //         break;
            //     }
            // }
            //
            // if (idx == -1) return;
            //
            // if ((propertyId.typeFlags & PropertyTypeFlags.RequireDestruction) != 0) {
            //     long currentValue = instanceStyleData->data[idx];
            //     if (currentValue != 0) {
            //         IntPtr ptr = (IntPtr) currentValue;
            //         if (ptr != IntPtr.Zero) {
            //             GCHandle.FromIntPtr(ptr).Free();
            //         }
            //     }
            // }
            //
            // if ((state & data.state) != 0) {
            //     // rebuild if not already rebuilding
            // }
        }

        internal void SetInstanceStyle(StyleSet styleSet, PropertyId propertyId, long? styleData, StyleState2Byte state) {
            // StyleSetData data = styleSetTable[styleSet.styleDataId];
            //
            // propertyId.state = state;
            //
            // if (styleData.HasValue) {
            //     AddOrUpdateInstanceStyle(data, propertyId, styleData.Value, state);
            // }
            // else {
            //     RemoveInstanceStyle(data, propertyId, state);
            // }

        }


        // todo -- create in bulk where possible
        internal int CreatesStyle(string styleName) {
            // styleNameTable.Add(styleName);
            // int nameIdx = styleNameTable.size - 1;
            // Assert.AreEqual(nameIdx, styleIdx);
            // return styleIdx;
            throw new NotImplementedException();
        }

        internal unsafe void EnterState(StyleSet styleSet, StyleState2 state) {
            // StyleSetData data = styleSetTable[styleSet.styleDataId];
            // if (data.changeSetId != ushort.MaxValue) {
            //     bool hasStateStyles = false;
            //     for (int i = 0; i < data.sharedStyleCount; i++) {
            //         StyleId styleId = data.sharedStyles[i];
            //         if (styleId.DefinesState(state)) {
            //             hasStateStyles = true;
            //         }
            //     }
            //
            //     if (hasStateStyles) {
            //         CreateStyleChangeSet(styleSet.styleDataId, ref data);
            //         ref SharedStyleChangeEntry changeSetData = ref sharedStyleChangeSets.array[data.changeSetId];
            //         changeSetData.newState |= (StyleState2Byte) (state);
            //     }
            // }
        }

        internal unsafe void ExitState(StyleSet styleSet, StyleState2 state) {
            // StyleSetData data = styleSetTable[styleSet.styleDataId];
            // if (data.changeSetId != ushort.MaxValue) {
            //     bool hasStateStyles = false;
            //     for (int i = 0; i < data.sharedStyleCount; i++) {
            //         StyleId styleId = data.sharedStyles[i];
            //         if (styleId.DefinesState(state)) {
            //             hasStateStyles = true;
            //         }
            //     }
            //
            //     if (hasStateStyles) {
            //         CreateStyleChangeSet(styleSet.styleDataId, ref data);
            //         ref SharedStyleChangeEntry changeSetData = ref sharedStyleChangeSets.array[data.changeSetId];
            //         changeSetData.newState &= (StyleState2Byte) (~state);
            //     }
            // }
        }

        public static string GetStyleName(int index) {
            if (index > 0 && index < s_DebugStyleNames.size) {
                return s_DebugStyleNames.array[index];
            }

            return null;
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