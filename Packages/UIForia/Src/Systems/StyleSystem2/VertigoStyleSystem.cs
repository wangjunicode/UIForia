using System;
using UIForia.Elements;
using UIForia.Style;
using UIForia.Util.Unsafe;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

// ReSharper disable MemberCanBePrivate.Global

namespace UIForia {

    public class VertigoStyleSystem : IDisposable {

        public const int k_MaxStyleProperties = 256;

        internal UIElement rootElement;
        
        internal StyleResultTable sharedStyleTable;
        internal StyleResultTable selectorStyleTable;
        internal StyleResultTable animatorStyleTable;

        internal PerThread<StyleRebuildResultList> sharedStyleRebuildResult;
        internal PerThread<StyleRebuildResultList> selectorStyleRebuildResult;
        internal PerThread<StyleRebuildResultList> animatorStyleRebuildResult;

        internal SharedStyleChangeSet sharedStyleChangeSet;

        internal PagedSplitBufferList<PropertyId, long> sharedPropertyTable;

        internal PagedList<StyleSetData> styleSetTable;

        public VertigoStyleSystem() {

            sharedPropertyTable = new PagedSplitBufferList<PropertyId, long>(1024, Allocator.Persistent);
            styleSetTable = new PagedList<StyleSetData>(128, Allocator.Persistent);

            sharedStyleTable = new StyleResultTable();
            selectorStyleTable = new StyleResultTable();
            animatorStyleTable = new StyleResultTable();

            sharedStyleRebuildResult = new PerThread<StyleRebuildResultList>();
            selectorStyleRebuildResult = new PerThread<StyleRebuildResultList>();
            animatorStyleRebuildResult = new PerThread<StyleRebuildResultList>();

            sharedStyleChangeSet = new SharedStyleChangeSet(256, 64, Allocator.Persistent);

        }

        public void Dispose() {
            
            sharedStyleRebuildResult.Dispose();
            selectorStyleRebuildResult.Dispose();
            animatorStyleRebuildResult.Dispose();

            sharedStyleTable.Dispose();
            selectorStyleTable.Dispose();
            animatorStyleTable.Dispose();

            sharedStyleChangeSet.Dispose();

            sharedPropertyTable.Dispose();
            styleSetTable.Dispose();
         
        }

        // the more i can do all these operations efficiently in isolation, the better the whole system gets
        // just combine the results at the end and use that as our output. memory shouldn't be a huge concern
        // focus on cache locality where it makes a difference but lean more on parallel than cache coherence 
        // for our main performance boost.

        // per-thread allocators for data
        // combine changes into persistent buffer at the end of frame when we know all the data
        // this means allocating intermediate buffers in jobs, which is good for locality anyway

        public struct GatherStyleSetIds : IJob {

            public void Execute() { }

        }
        
        // Right now this is only handling styles but will very likely be extended for the whole app. 
        //    I want to pump parallel as much as possible, having a single runner makes that easy, though likely makes for a large file
        
        // StyleJobRunner -> holds transitional (per-frame) data and responsible for all job execution and scheduling
        //        could export some handles for other systems to hook off of later on for code seperation
        
        // LayoutJobRunner
        
        // RenderJobRunner
        
        // InputJobRunner
        
        // BindingJobRunner
        
        // SystemManager can handle resizing collections that live in the same buffer and maybe orchestrating inter system events like ElementDestroyed/Disabled/Created/Whatever
        
        // Systems hold dynamic or stateful data that can change frame to frame. Exposes interfaces to user code
        // Databases hold the static data for a system. Only written to when new styles / etc are created
        
        // StyleDatabase -> holds all the static style data
        // SelectorDatabase -> holds all the static selector data, references style database
        // AnimationDatabase -> holds all static animation data
        
        // StyleSystem -> handles per element style data and change lists
        // AnimationSystem -> holds active animation data
        // AttributeSystem -> holds all the attribute info and attribute index
        // SelectorSystem -> holds active selector data, list of active selectors, mapping of selector -> style -> element etc
        // ElementSystem -> holds element meta data like traversals, enabled state, alive state, etc
        
        public void OnUpdate() {

            // JobHandle traversalIndex = new TraversalIndexJob_Managed() {
            //     rootElementHandle = GCHandle.Alloc(rootElement),
            //     traversalInfo = traversalInfo
            // }.Schedule();
            
            // selectors per element
            // targeted by selectors per element
            
            // need to know order in which selectors apply to an element
            // first sort by depth
            // then by state
            // check if full sorted, if not ->
            // then by style index if we still have ties -> list is available, just to a quick search. 
            // then by selector index if still have ties -> easy just track the index in the data, its static
            // this way we dont need extra data and dont care about expensive tracking until we actually need it
            
            // state index
            // tag index
            // style index
            // still need to be built
            
            // style index is probably easy, we know which elements were added / removed from each style
            
            // unknowns
            // element clean up / garbage collecting efficiently. systems are probably responsible for this independently
            // how to build the tag index. dont we just add entries when elements with that id are created? want to do it in bulk? buffer -> sort -> remove dead if needed -> add in bulk
            // style id indices. think this is handled
            // selector sorting. still not sure
            // animation & transition. still not sure
            // inheritance if any. still not sure, might get away with trickery
            // dead / disabled element detection, use generation table
            // managing all the data buffers split into static, transient, persistant
            // allocation scheme -> split allocators or joined? profile?
            
            PerThread<ConvertedStyleList> perThread_ConvertedStyleIds = new PerThread<ConvertedStyleList>(Allocator.TempJob);
            PerThread<StyleRebuildResultList> perThread_RebuildSharedStyles = new PerThread<StyleRebuildResultList>(Allocator.TempJob);
            UnmanagedList<ConvertedStyleId> gathered_ConvertedStyleIds = new UnmanagedList<ConvertedStyleId>(Allocator.TempJob);

            TransientData transient = TransientData.Create();

            VertigoScheduler.SchedulerStep convertStyleIdsHandle = VertigoScheduler.ParallelForRange(sharedStyleChangeSet.Size, 15, new ConvertStyleIdsToStatePairs() {
                    sharedStyleChangeSet = sharedStyleChangeSet,
                    perThreadOutput = perThread_ConvertedStyleIds
                })
                .Then(new MergePerThreadData<ConvertedStyleList, ConvertedStyleId>() {
                    perThread = perThread_ConvertedStyleIds,
                 //   gatheredOutput = gathered_ConvertedStyleIds
                });

            JobHandle sharedStyles = VertigoScheduler.Await(convertStyleIdsHandle);
                // .Then(new BuildSharedStyles() {
                //   //  convertedStyleList = gathered_ConvertedStyleIds,
                //     table_StyleInfo = default, // todo
                //     perThread_RebuiltResult = perThread_RebuildSharedStyles
                // })
                // .Then(new GatherPerThreadData<StyleRebuildResultList>() {
                //     perThread = perThread_RebuildSharedStyles,
                //     gatheredOutput = default
                // })
                // .Then(
                //      new WriteToStyleResultTable() {
                //         targetTable = sharedStyleTable,
                //         writeList = default
                //     },
                //     new GatherStyleSetIds() { }
                // );

            // for style index be sure we didnt discard styles that have no attached state, still want to select based on those
            // this job might want to work the change sets and not the converted ids
          
            // persistent data
            // table = has indexer returning data, might be a list might not be.
            // tables survive for the whole app lifecycle
            // naming should be 1-1 with the tables struct
            // disposed when app destroyed
            // cleared on app restart

            // per-frame data
            // everything here is disposed every frame

            // per-thread data
            // per thread data is cleared every frame but root is not disposed

            // persist_ActiveSelectorFreeList, just use an inline free list
            
            // transient data to lists I don't need anymore

            Table table = new Table();

            PerThreadData perThread = new PerThreadData();

            // all per-thread lists store their merged/gathered version as well
            // gather still needs to be a job i think but then data management is easier
            // split data into table / transient / per-thread / per-frame
            // per thread == per-frame except root isn't disposed, just cleared.

            // could be that my batching is better than Unity's foreach since I can allocate less, 
            // re-use lists across threads, and continue down the chain at a much more granular level
            // that said: it way over-schedules but I'm not certain thats an issue, could be ok, could be horrible.
            JobHandle selectorResults = VertigoScheduler.Await(sharedStyles)
                .Then(new DiffSharedStyleChanges() {
                    input_SharedStyleChangeList = default,
                    output_AddedStyleStateList = transient.addedStyleStateList,
                    output_RemovedStyleStateList = transient.removedStyleStateList
                })
                .Then(
                    new ResolveSelectorsJob() {
                        input_StyleStateElementIdList = transient.addedStyleStateList,
                        output_SelectorList = transient.selectorCreateList,
                        output_SelectorList_Indexed = transient.selectorCreateList_Indexed,
                        table_SelectorIdMap = table.selectorIdMap
                    },
                    new ResolveSelectorsJob() {
                        input_StyleStateElementIdList = transient.removedStyleStateList,
                        output_SelectorList = transient.selectorKillList,
                        output_SelectorList_Indexed = transient.selectorKillList_Indexed,
                        table_SelectorIdMap = table.selectorIdMap
                    }
                )
                .Then(new RemoveDeadSelectorsJob() {
                    input_SelectorKillList = default,
                    output_ActiveSelectorFreeList = default,
                    table_ActiveSelectors = table.activeSelectors,
                    table_ActiveSelectorIndexMap = default
                })
                .Then(new CreateNewSelectorsJob() {
                    input_SelectorCreateList = default,
                    input_ActiveSelectorFreeList = default,
                    table_write_ActiveSelectors = default, // do we care about active selector indices or is it just a flat list? ideally a flat list
                })
                .Then(new GatherIndexedSelectorRunInfo() {
                    output_RemoveSelectorEffectList = default,
                    output_SelectorRunInfoList = default,
                    table_ElementIndex = default,
                    table_ActiveSelectors = table.activeSelectors,
                    table_SelectorQueries = table.selectorQueries,
                    // table_TraversalInfo = table.traversalInfo,
                })
                .Then(new RunSelectorFilters() {
                    output_Matches = default,
                    output_WhereFilterCandidates = default,
                    input_SelectorRunInfoList = default,
                    inout_RemoveSelectorEffectList = default,
                    table_ElementTagTable = default
                })
                .Then(new RunSelectorWhereFilter_Managed() {
                    input_MatchCandidates = default,
                    output_MatchedElements = default,
                    table_WhereFilterFuncs = default,
                });

            JobHandle selectorStyles = VertigoScheduler.Await(selectorResults)
                .Then(new BuildSelectorStyles());

            VertigoScheduler.Await(sharedStyles, selectorStyles)
                .Then(new BuildFinalStyles());

            transient.Dispose();
            EndFrame();

            perThread_RebuildSharedStyles.Dispose();
            perThread_ConvertedStyleIds.Dispose();
            gathered_ConvertedStyleIds.Dispose(); // must be disposed after perThread_ConvertedStyleIds!

        }

        public struct TransientData : IDisposable {

            public DataList<StyleStateElementId>.Shared addedStyleStateList;
            public DataList<StyleStateElementId>.Shared removedStyleStateList;

            public DataList<SelectorIdElementId>.Shared selectorKillList;
            public DataList<SelectorIdElementId>.Shared selectorKillList_Indexed;
            public DataList<SelectorIdElementId>.Shared selectorCreateList;
            public DataList<SelectorIdElementId>.Shared selectorCreateList_Indexed;

            public static TransientData Create() {
                TransientData retn = new TransientData();
                retn.addedStyleStateList = new DataList<StyleStateElementId>.Shared(64, Allocator.TempJob);
                retn.removedStyleStateList = new DataList<StyleStateElementId>.Shared(64, Allocator.TempJob);
                retn.selectorKillList = new DataList<SelectorIdElementId>.Shared(32, Allocator.TempJob);
                retn.selectorKillList_Indexed = new DataList<SelectorIdElementId>.Shared(32, Allocator.TempJob);
                retn.selectorCreateList = new DataList<SelectorIdElementId>.Shared(32, Allocator.TempJob);
                retn.selectorCreateList_Indexed = new DataList<SelectorIdElementId>.Shared(32, Allocator.TempJob);
                return retn;
            }

            public void Dispose() {
                addedStyleStateList.Dispose();
                removedStyleStateList.Dispose();
                selectorKillList.Dispose();
                selectorKillList_Indexed.Dispose();
                selectorCreateList.Dispose();
                selectorCreateList_Indexed.Dispose();
            }

        }

        public unsafe struct PerThreadData { }

        public unsafe struct Table {

            public DataList<ActiveSelector>.Shared activeSelectors;
            public DataList<SelectorQuery>.Shared selectorQueries;
            public DataList<ElementTraversalInfo>.Shared traversalInfo;
            public IntListMap<SelectorTypeIndex> selectorIdMap;

            public static Table Create() {
                Table table = new Table();
                table.traversalInfo = new DataList<ElementTraversalInfo>.Shared(128, Allocator.Persistent);
                table.activeSelectors = new DataList<ActiveSelector>.Shared(256, Allocator.Persistent);
                table.selectorQueries = new DataList<SelectorQuery>.Shared(128, Allocator.Persistent);
                table.selectorIdMap = new IntListMap<SelectorTypeIndex>();
                return table;
            }

            public void Dispose() {
                activeSelectors.Dispose();
                selectorQueries.Dispose();
                traversalInfo.Dispose();
            }

        }

        internal void EndFrame() {

            sharedStyleRebuildResult.Clear();
            selectorStyleRebuildResult.Clear();
            animatorStyleRebuildResult.Clear();
            sharedStyleChangeSet.Clear();
        }

        // assumes at least 1 of the groups changed or order was altered in some way
        // if this gets called multiple times in a frame for 1 element we just allocate a new 
        // property range and ignore the hole since that memory is released every frame.
        public unsafe void SetSharedStyles(StyleSet styleSet, ref StackIntBuffer7 newStyleBuffer) {
            // todo -- we do need to de-dup these styles after all. but theres max 7 or so, should be fast and easy to do on-stack
            fixed (int* buffer = newStyleBuffer.array) {
                ref StyleSetData styleData = ref styleSetTable.GetReference(styleSet.styleDataId);
                // sharedStyleChangeSet.SetSharedStyles(styleSet.styleDataId, ref styleData, (StyleId*) buffer, newStyleBuffer.size);
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
                state = StyleState2.Normal,
                styleChangeIndex = ushort.MaxValue,
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

        public unsafe struct GenericElementIndex {

            // todo -- could just use a List_ElementId with allocator, might be better for locality
            public DataList<ElementId> elementIds;

            public ElementGenerationTable generationTable;
            
            public void Add(ElementId elementId) {
                if (elementIds.size + 1 > elementIds.capacity) {
                    elementIds.SetSize(generationTable.RemoveDeadElements(elementIds.GetArrayPointer(), elementIds.size));
                }
                elementIds.Add(elementId);
            }

            public void Remove(ElementId elementId) {
                for (int i = 0; i < elementIds.size; i++) {
                    if (elementIds[i] == elementId) {
                        elementIds.SwapRemove(i);
                        return;
                    }
                }
            }

        }
        
        internal unsafe void EnterState(StyleSet styleSet, StyleState2 state) {
            
            StyleSetData data = styleSetTable[styleSet.styleDataId];
            if (((int)data.state & (int)state) != 0) {
                return;
            }

            GenericElementIndex index = default;
            index.Add(styleSet.element.id);
            
            // stateIndex[(int)state].Add(styleSet.elementId);
            // 

            if (data.styleChangeIndex != ushort.MaxValue) {
                
            }
            else {
                
            }

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
        

        internal static unsafe DisposedDataList<FixedBlockAllocator>.Shared CreateStyleAllocators(bool traceMemory) {
            DisposedDataList<FixedBlockAllocator>.Shared retn = new DisposedDataList<FixedBlockAllocator>.Shared(8, Allocator.Persistent);

            // expect lots of 4 and 8 sized blocks, fewer 16s and even fewer of the rest
            int itemSize = sizeof(PropertyId) + sizeof(PropertyData);
            retn.Add(default); // the 0 index is never used but makes index math better
            retn.Add(new FixedBlockAllocator(4 * itemSize, 256, 4, traceMemory));
            retn.Add(new FixedBlockAllocator(8 * itemSize, 256, 4, traceMemory));
            retn.Add(new FixedBlockAllocator(16 * itemSize, 128, 4, traceMemory));
            retn.Add(new FixedBlockAllocator(32 * itemSize, 16, 0, traceMemory));
            retn.Add(new FixedBlockAllocator(64 * itemSize, 8, 0, traceMemory)); // used but rarely 
            retn.Add(new FixedBlockAllocator(128 * itemSize, 4, 0, traceMemory)); // probably never used
            retn.Add(new FixedBlockAllocator(256 * itemSize, 1, 0, traceMemory)); // never used 

            return retn;
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