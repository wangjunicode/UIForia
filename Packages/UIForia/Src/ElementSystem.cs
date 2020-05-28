using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UIForia.Elements;
using UIForia.Layout;
using UIForia.Systems;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using Debug = UnityEngine.Debug;

namespace UIForia {

    public unsafe class ElementSystem : IDisposable {

        private const int k_MinFreeIndices = 1024;

        private Queue<int> indexQueue; // todo -- make a better queue, inlined single linked list w/ free list that supports bulk insert

        private int idGenerator;
        public ElementTable<ElementMetaInfo> metaTable;
        public ElementTable<HierarchyInfo> hierarchyTable;
        public ElementTable<ElementTraversalInfo> traversalTable;
        public LayoutResult[] layoutTable;

        public UIElement[] instanceTable;
        public LayoutBox[] layoutBoxes; // accept empty space and store 1-1 with element id, easier mapping

        private int elementCapacity;
        private byte* backingStore;

        public DataList<ElementId>.Shared enabledElementsThisFrame;
        public DataList<ElementId>.Shared disabledElementsThisFrame;
        
        public ElementTable<LayoutMetaData> layoutMetaDataTable;
        public ElementTable<LayoutHierarchyInfo> layoutHierarchyTable;

        // these should be per active layout box, not per element
        public ElementTable<float4x4> localMatrices;
        public ElementTable<float4x4> worldMatrices;
        public ElementTable<TransformInfo> transformInfo;
        public ElementTable<AlignmentInfo> alignmentInfoHorizontal;
        public ElementTable<AlignmentInfo> alignmentInfoVertical;
        public ElementTable<LayoutBoxFlags> layoutFlags;
        
        public ElementSystem(int initialElementCount) {
            this.idGenerator = 1; // 0 is always invalid
            this.indexQueue = new Queue<int>(k_MinFreeIndices * 2);
            ResizeBackingBuffer(128); // todo -- make this sane, bumped up due to crash with unsafetuil.malloc

            // todo -- allocate from same buffer w/ front-back load config (grow one size from top, other from bottom)
            enabledElementsThisFrame = new DataList<ElementId>.Shared(64, Allocator.Persistent);
            disabledElementsThisFrame = new DataList<ElementId>.Shared(64, Allocator.Persistent);

            Initialize();
        }

        public void FilterEnabledDisabledElements(ElementId root) {
            new FilterEnabledDisabledElementsJob() {
                metaTable = metaTable,
                enabledElements = enabledElementsThisFrame,
                disabledElements = disabledElementsThisFrame
            }.Run();
            new UpdateTraversalTable() {
                hierarchyTable = hierarchyTable,
                metaTable = metaTable,
                traversalTable = traversalTable,
                rootId = root,
            }.Run();
            
        }

        public void Initialize() {
            idGenerator = 1;
            indexQueue.Clear();
            enabledElementsThisFrame.size = 0;
            disabledElementsThisFrame.size = 0;
            Array.Clear(instanceTable, 0, instanceTable.Length);
            Array.Clear(layoutTable, 0, layoutTable.Length);
            Array.Clear(layoutBoxes, 0, layoutBoxes.Length);
            TypedUnsafe.MemClear(layoutMetaDataTable.array, elementCapacity);
            TypedUnsafe.MemClear(layoutHierarchyTable.array, elementCapacity);
            TypedUnsafe.MemClear(metaTable.array, elementCapacity);
            TypedUnsafe.MemClear(traversalTable.array, elementCapacity);
            TypedUnsafe.MemClear(hierarchyTable.array, elementCapacity);
        }

        // I'll have to see how this gets used. Ideally we push out a lot of indices at once and bulk create
        internal ElementId CreateElement(UIElement element, int depth, int templateId, int templateOriginId, UIElementFlags flags) {

            int idx;
            if (indexQueue.Count >= k_MinFreeIndices) {
                idx = indexQueue.Dequeue();
            }
            else {
                idx = idGenerator++;
                if (idx >= elementCapacity) {
                    ResizeBackingBuffer(elementCapacity * 2);
                }
            }

            if (idx >= layoutTable.Length) {
                Debug.Log("Out of bounds");
                return default;
            }

            // note: need to use generation from existing data at idx
            metaTable.array[idx].flags = flags;
            metaTable.array[idx].__padding__ = 0;

            traversalTable.array[idx] = new ElementTraversalInfo() {
                depth = depth,
                btfIndex = 0,
                ftbIndex = 0,
                //templateId =  templateId,
                //templateOriginId = (ushort) templateOriginId
            };

            hierarchyTable.array[idx] = new HierarchyInfo() {
                childCount = 0,
                firstChildId = default,
                lastChildId = default,
                nextSiblingId = default,
                prevSiblingId = default,
                parentId = default
            };

            layoutTable[idx] = new LayoutResult(element);
            instanceTable[idx] = element;

            return new ElementId(idx, metaTable.array[idx].generation);

        }

        public void AddChild(ElementId parentId, ElementId childId) {
            if (parentId.index >= elementCapacity || childId.index >= elementCapacity) {
                Debug.Log("out of bounds");
                return;
            }

            ref HierarchyInfo parentInfo = ref hierarchyTable.array[parentId.index];
            ref HierarchyInfo childInfo = ref hierarchyTable.array[childId.index];

            childInfo.parentId = parentId;

            if (parentInfo.childCount == 0) {
                parentInfo.firstChildId = childId;
                parentInfo.lastChildId = childId;
                childInfo.nextSiblingId = default;
                childInfo.prevSiblingId = default;
            }
            else {
                if (parentInfo.lastChildId.index >= elementCapacity || parentInfo.lastChildId.index >= elementCapacity) {
                    Debug.Log("out of bounds");
                    return;
                }

                ref HierarchyInfo prevSibling = ref hierarchyTable.array[parentInfo.lastChildId.index];

                childInfo.prevSiblingId = parentInfo.lastChildId;
                childInfo.nextSiblingId = default;
                parentInfo.lastChildId = childId;
                prevSibling.nextSiblingId = childId;
            }

            parentInfo.childCount++;

        }

        public void RemoveChild(ElementId parentId, ElementId childId) {

            ref HierarchyInfo parentInfo = ref hierarchyTable.array[parentId.index];
            ref HierarchyInfo childInfo = ref hierarchyTable.array[childId.index];

            if (childInfo.parentId != parentId) {
                return;
            }

            if (childId == parentInfo.firstChildId) {
                parentInfo.firstChildId = childInfo.nextSiblingId;
            }

            if (childId == parentInfo.lastChildId) {
                parentInfo.lastChildId = childInfo.prevSiblingId;
            }

            if (childInfo.prevSiblingId != default) {
                ref HierarchyInfo prevSibling = ref hierarchyTable.array[parentInfo.lastChildId.index];
                prevSibling.nextSiblingId = childInfo.nextSiblingId;
            }

            if (childInfo.nextSiblingId != default) {
                ref HierarchyInfo nextSibling = ref hierarchyTable.array[parentInfo.lastChildId.index];
                nextSibling.prevSiblingId = childInfo.prevSiblingId;
            }

            // todo -- if we decide sibling index is a good thing we need to update the ids
            // I don't expect lots of add / removal thrashing
            // unless sibling index also updates on enable | disable

            childInfo.prevSiblingId = default;
            childInfo.nextSiblingId = default;
            parentInfo.childCount--;
        }

        public void DestroyElement(ElementId elementId) {
            ref ElementMetaInfo meta = ref metaTable.array[elementId.index];
            meta.generation++;
            meta.flags = default;
            meta.__padding__ = default;
            indexQueue.Enqueue(elementId.index);
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsAlive(ElementId elementId) {
            return metaTable.array[elementId.index].generation == elementId.generation;
        }
        
        public bool IsEnabled(ElementId elementId) {
            return metaTable.array[elementId.index].generation == elementId.generation && (metaTable[elementId].flags & UIElementFlags.EnabledFlagSet) == UIElementFlags.EnabledFlagSet;;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAlive(ElementId elementId, in ElementTable<ElementMetaInfo> metaTable) {
            return metaTable.array[elementId.index].generation == elementId.generation;
        }

        public static bool IsDeadOrDisabled(ElementId elementId, ElementTable<ElementMetaInfo> metaTable) {
            return metaTable[elementId].generation != elementId.generation || (metaTable[elementId].flags & UIElementFlags.EnabledFlagSet) != UIElementFlags.EnabledFlagSet;
        }

        public int SetSiblingIndex(ElementId id, int value) {
            throw new NotImplementedException();
        }

        public void Dispose() {
            if (backingStore != null) {
                UnsafeUtility.Free(backingStore, Allocator.Persistent);
                backingStore = null;
            }

            enabledElementsThisFrame.Dispose();
            disabledElementsThisFrame.Dispose();
        }

        private void ResizeBackingBuffer(int newCapacity) {

            backingStore = TypedUnsafe.ResizeSplitBuffer(
                ref metaTable.array,
                ref traversalTable.array,
                ref hierarchyTable.array,
                ref layoutHierarchyTable.array,
                ref layoutMetaDataTable.array,
                elementCapacity,
                newCapacity,
                Allocator.Persistent,
                true
            );
            
            elementCapacity = newCapacity;
            if (instanceTable == null) {
                instanceTable = new UIElement[elementCapacity];
                layoutTable = new LayoutResult[elementCapacity];
                layoutBoxes = new LayoutBox[elementCapacity];
                
                // metaTable = new ElementTable<ElementMetaInfo>(new ElementMetaInfo[elementCapacity]);
                // traversalTable = new ElementTable<ElementTraversalInfo>(new ElementTraversalInfo[elementCapacity]);
                // hierarchyTable = new ElementTable<HierarchyInfo>(new HierarchyInfo[elementCapacity]);
                // layoutHierarchyTable = new ElementTable<LayoutHierarchyInfo>(new LayoutHierarchyInfo[elementCapacity]);
                // layoutMetaDataTable = new ElementTable<LayoutMetaData>(new LayoutMetaData[elementCapacity]);
            }
            else {
                if (instanceTable.Length <= elementCapacity) {
                    Array.Resize(ref instanceTable, elementCapacity);
                    Array.Resize(ref layoutTable, elementCapacity);
                    Array.Resize(ref layoutBoxes, elementCapacity);
                    //
                    // Array.Resize(ref metaTable.array, elementCapacity);
                    // Array.Resize(ref traversalTable.array, elementCapacity);
                    // Array.Resize(ref hierarchyTable.array, elementCapacity);
                    // Array.Resize(ref layoutHierarchyTable.array, elementCapacity);
                    // Array.Resize(ref layoutMetaDataTable.array, elementCapacity);
                    
                }
            }
        }

        public int RemoveDeadElements(ElementId* ptr, int size) {
            for (int i = 0; i < size; i++) {
                if (!IsAlive(ptr[i], metaTable)) {
                    ptr[i--] = ptr[--size];
                }
            }

            return size;
        }

        public static int RemoveDeadElements(ElementTable<ElementMetaInfo> metaTable, ElementId* ptr, int size) {

            for (int i = 0; i < size; i++) {
                if (!IsAlive(ptr[i], metaTable)) {
                    ptr[i--] = ptr[--size];
                }
            }

            return size;
        }

        [BurstCompile]
        private struct FilterEnabledDisabledElementsJob : IJob {

            public ElementTable<ElementMetaInfo> metaTable;
            public DataList<ElementId>.Shared enabledElements;
            public DataList<ElementId>.Shared disabledElements;

            public void Execute() {

                enabledElements.FilterSwapRemove(new FilterEnabled() {
                    metaTable = metaTable
                });

                disabledElements.FilterSwapRemove(new FilterDisabled() {
                    metaTable = metaTable
                });
                
                int bufferSize = enabledElements.size > disabledElements.size
                    ? enabledElements.size
                    : disabledElements.size;

                DataList<ElementId> buffer = new DataList<ElementId>(bufferSize, Allocator.Temp);
                RemoveDuplicates(enabledElements, buffer);
                RemoveDuplicates(disabledElements, buffer);

                for (int i = 0; i < enabledElements.size; i++) {
                    metaTable[enabledElements[i]].flags |= UIElementFlags.EnableStateChanged;
                }
                
                buffer.Dispose();

            }

            private static void RemoveDuplicates(DataList<ElementId>.Shared target, DataList<ElementId> buffer) {
                buffer.size = 0;
                if (target.size > 1) {

                    NativeSortExtension.Sort(target.GetArrayPointer(), target.size, new ElementIdComp());

                    for (int i = 0; i < target.size - 1; i++) {

                        if (target[i] != target[i + 1]) {
                            buffer.AddUnchecked(target[i]);
                        }

                    }

                    buffer.AddUnchecked(target[target.size - 1]);
                }

                TypedUnsafe.MemCpy(target.GetArrayPointer(), buffer.GetArrayPointer(), buffer.size);
                target.size = buffer.size;
            }

            private struct ElementIdComp : IComparer<ElementId> {

                public int Compare(ElementId x, ElementId y) {
                    return x.id - y.id;
                }

            }

            private struct FilterDisabled : IListFilter<ElementId> {

                public ElementTable<ElementMetaInfo> metaTable;

                public bool Filter(in ElementId item) {
                    return metaTable[item].generation == item.generation && (metaTable[item].flags & UIElementFlags.EnabledFlagSet) != UIElementFlags.EnabledFlagSet;
                }

            }

            private struct FilterEnabled : IListFilter<ElementId> {

                public ElementTable<ElementMetaInfo> metaTable;

                public bool Filter(in ElementId item) {
                    return metaTable[item].generation == item.generation && (metaTable[item].flags & UIElementFlags.EnabledFlagSet) == UIElementFlags.EnabledFlagSet;
                }

            }

        }

        public void CleanupFrame() {
            
            for (int i = 0; i < enabledElementsThisFrame.size; i++) {
                metaTable[enabledElementsThisFrame[i]].flags &= ~(UIElementFlags.EnableStateChanged | UIElementFlags.EnabledRoot);
            }
            
            for (int i = 0; i < disabledElementsThisFrame.size; i++) {
                metaTable[disabledElementsThisFrame[i]].flags &= ~(UIElementFlags.DisableRoot);
            }
            
            enabledElementsThisFrame.size = 0;
            disabledElementsThisFrame.size = 0;
        }

    }

}