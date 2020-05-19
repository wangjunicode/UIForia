using System;
using UIForia.Util.Unsafe;
using Unity.Collections;
using Unity.Jobs;

namespace UIForia {

    public class StyleJobRunner : IDisposable {

        public StyleSystem2 styleSystem;
        public ElementSystem elementSystem;
        public SelectorSystem selectorSystem;

        public StyleDatabase styleDatabase;
        public SelectorDatabase selectorDatabase;

        private PerThreadData perThreadData;
        private PersistentTransientData persistentData;

        public StyleJobRunner(StyleSystem2 styleSystem, ElementSystem elementSystem, SelectorSystem selectorSystem) {
            this.styleSystem = styleSystem;
            this.elementSystem = elementSystem;
            this.selectorSystem = selectorSystem;
            this.styleDatabase = styleSystem.styleDatabase;
            // this.selectorDatabase = selectorSystem.s
            perThreadData = PerThreadData.Create();
            persistentData = PersistentTransientData.Create();
        }

        public unsafe void Run() {

            // first we want to schedule the traversal job so selectors can wait on it

            // selectors only work downstream so disabled elements have no effect and I dont need to update them at all
            // we'll implicitly create change sets when elements are re-enabled so they just go through the normal flow.

            int* sharedStyleRebuildCount = &persistentData.deferredCountBuffer->rebuildSharedStyleListCount;

            // don't love this being single threaded but it runs super fast so whatever
            new CullDeadAndDisabledElementChanges() {
                metaTable = elementSystem.metaTable,
                sharedChangeSets = styleSystem.sharedChangeSets
            }.Run();
            
            JobHandle styleDiff = VertigoScheduler.Parallel(new DiffStyles() {
                parallel = new ParallelParams(styleSystem.sharedChangeSets.size, 32),
                sharedChangeSets = styleSystem.sharedChangeSets,
                perThread_StyleIndexUpdater = perThreadData.styleIndexUpdater
            });

            JobHandle createStylePairs = VertigoScheduler.Parallel(new CreateStyleStatePairs() {
                parallel = new ParallelParams(styleSystem.sharedChangeSets.size, 32),
                sharedChangeSets = styleSystem.sharedChangeSets,
                perThread_styleStatePairList = perThreadData.styleStatePairList
            });

            JobHandle writeSharedStyleUpdates = VertigoScheduler.Await(createStylePairs)
                .Then(new MergeStyleStatePairs() {
                    perThread_styleStatePairList = perThreadData.styleStatePairList,
                    stylePairsList = persistentData.currentStylePairsList,
                    updateType = StylePairUpdateType.Current,
                    listSizeResult = sharedStyleRebuildCount
                })
                .ThenDeferParallel(new BuildSharedStyles() {
                    defer = new ParallelParams.Deferred(sharedStyleRebuildCount, 16),
                    styleDatabase = styleDatabase.GetBurstable(),
                    styleUpdates = persistentData.currentStylePairsList,
                    perThread_RebuildContainer = perThreadData.sharedStyleRebuild
                })
                .Then(new WriteStyleResults() {
                    allocator = styleSystem.stylePropertyListAllocator,
                    styleResultTable = styleSystem.sharedResults,
                    perThread_RebuildContainer = perThreadData.sharedStyleRebuild
                });

            JobHandle styleIndexUpdate = VertigoScheduler.Await(styleDiff).Then(new UpdateStyleIndex() {
                styleIndex = styleSystem.styleIndex,
                elementMetaInfo = elementSystem.metaTable,
                styleIndexAllocator = default,
                perThread_StyleIndexUpdater = perThreadData.styleIndexUpdater
            });

            JobHandle mergeAddedStylePairs = VertigoScheduler.Await(createStylePairs).Then(new MergeStyleStatePairs() {
                perThread_styleStatePairList = perThreadData.styleStatePairList,
                stylePairsList = persistentData.addedStylePairsList,
                updateType = StylePairUpdateType.Add,
            });

            JobHandle mergeRemovedStylePairs = VertigoScheduler.Await(createStylePairs).Then(new MergeStyleStatePairs() {
                perThread_styleStatePairList = perThreadData.styleStatePairList,
                stylePairsList = persistentData.removedStylePairsList,
                updateType = StylePairUpdateType.Remove
            });

            VertigoScheduler.Await(mergeRemovedStylePairs).Then(new ResolveSelectorsJob2() {
                
            });

            JobHandle writeFinalStyles = VertigoScheduler.Await(writeSharedStyleUpdates);

            writeFinalStyles.Complete();

            persistentData.Clear();
            perThreadData.Clear();

            styleSystem.EndFrame();
            
        }

        public void Dispose() {
            perThreadData.Dispose();
            persistentData.Dispose();
        }

        ///<summary>
        /// data that is used for communicating between jobs that doesn't belong to a particular system
        /// data from this type is expected to have a short lifetime and not be re-used between frames.
        /// </summary>
        public unsafe struct TransientData { }

        ///<summary>
        /// data that is used for communicating between jobs that doesn't belong to a particular system
        /// data from this type is expected to have a long lifetime and be re-used between frames.
        /// mostly useful for larger arrays that are the result of merging per-thread data
        /// </summary>
        public unsafe struct PersistentTransientData : IDisposable {

            public DataList<StylePairUpdate>.Shared addedStylePairsList;
            public DataList<StylePairUpdate>.Shared removedStylePairsList;
            public DataList<StylePairUpdate>.Shared currentStylePairsList;

            public DeferredCountBuffer* deferredCountBuffer;

            public static PersistentTransientData Create() {
                return new PersistentTransientData() {
                    addedStylePairsList = new DataList<StylePairUpdate>.Shared(128, Allocator.Persistent),
                    removedStylePairsList = new DataList<StylePairUpdate>.Shared(128, Allocator.Persistent),
                    currentStylePairsList = new DataList<StylePairUpdate>.Shared(128, Allocator.Persistent),
                    deferredCountBuffer = TypedUnsafe.Malloc<DeferredCountBuffer>(1, Allocator.Persistent)
                };
            }

            public int* rebuildSharedStyleListCount {
                get => &deferredCountBuffer->rebuildSharedStyleListCount;
            }

            public void Clear() {
                addedStylePairsList.size = 0;
                removedStylePairsList.size = 0;
                currentStylePairsList.size = 0;
                deferredCountBuffer->Clear();
            }

            public void Dispose() {
                TypedUnsafe.Dispose(deferredCountBuffer, Allocator.Persistent);
                addedStylePairsList.Dispose();
                removedStylePairsList.Dispose();
                currentStylePairsList.Dispose();
            }

            public struct DeferredCountBuffer {

                public int rebuildSharedStyleListCount;

                public void Clear() {
                    this = default;
                }

            }

        }

        /// <summary>
        /// data that is used for communicating between jobs that doesn't belong to a particular system
        /// data from this type is generated per-thread every frame. We keep the outer allocations around
        /// and clear them every frame instead of disposing and re-creating them
        /// </summary>
        public struct PerThreadData : IDisposable {

            public PerThread<StyleIndexUpdateSet> styleIndexUpdater;
            public PerThread<StyleStatePairList> styleStatePairList;
            public PerThread<StyleRebuildContainer> sharedStyleRebuild;

            public static PerThreadData Create() {
                return new PerThreadData {
                    styleIndexUpdater = new PerThread<StyleIndexUpdateSet>(Allocator.Persistent),
                    styleStatePairList = new PerThread<StyleStatePairList>(Allocator.Persistent),
                    sharedStyleRebuild = new PerThread<StyleRebuildContainer>(Allocator.Persistent)
                };
            }

            public void Clear() {
                styleIndexUpdater.Clear();
                styleStatePairList.Clear();
                sharedStyleRebuild.Clear();
            }

            public void Dispose() {
                styleIndexUpdater.Dispose();
                styleStatePairList.Dispose();
                sharedStyleRebuild.Dispose();
            }

        }

    }

}