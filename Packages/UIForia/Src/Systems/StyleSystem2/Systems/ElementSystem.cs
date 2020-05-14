using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UIForia.Elements;
using UIForia.Util.Unsafe;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UIForia {

    // todo -- would be awesome to get per-application metrics so we know how big a lot of the lists we create need to be

    // maybe keep this structure in parallel with the local children array
    // useful for burst traversal

    public struct TemplateId {

        public ushort templateIndex;
        public ushort lexicalTemplateIndex;

    }

    public struct HierarchyInfo {

        public int childCount;
        public ElementId parentId;
        public ElementId firstChildId;
        public ElementId lastChildId;
        public ElementId nextSiblingId;
        public ElementId prevSiblingId;

    }

    public unsafe class ElementSystem : IDisposable {

        private const int k_MinFreeIndices = 1024;

        private Queue<int> indexQueue; // todo -- make a better queue, inlined single linked list w/ free list that supports bulk insert

        private int idGenerator;
        public ElementTable<ElementMetaInfo> metaTable;
        public ElementTable<HierarchyInfo> hierarchyTable;
        public ElementTable<ElementTraversalInfo> traversalTable;

        private int elementCapacity;
        private byte* backingStore;

        // how can i handle parent / children relations easily and in a burstable way?
        // building traversal index is really all i need for this, traversal isnt that slow tho even in managed 20k nodes takes 1.7ms
        // traversal only needs updating when enabling elements, disabling is free because indices are still valid
        // 

        public ElementSystem(int initialElementCount) {
            // initially all elements have generation 0 but maybe it should generation 255 so it wraps on create?
            // or set all flags to dead?
            this.indexQueue = new Queue<int>(k_MinFreeIndices * 2);

            ResizeBackingBuffer(initialElementCount);

        }

        // I'll have to see how this gets used. Ideally we push out a lot of indices at once and bulk create
        internal ElementId CreateElement(int depth, int templateId, int templateOriginId, UIElementFlags flags) {

            int idx;
            if (indexQueue.Count >= k_MinFreeIndices) {
                idx = indexQueue.Dequeue();
            }
            else {
                idx = ++idGenerator;
                if (idx > elementCapacity) {
                    ResizeBackingBuffer(elementCapacity * 2);
                }
            }

            // note: need to use generation from existing data at idx
            metaTable[idx].flags = (UIElementFlags2) flags; // todo -- make this better
            metaTable[idx].someFlags = 0;

            traversalTable[idx] = new ElementTraversalInfo() {
                depth = depth,
                btfIndex = 0,
                ftbIndex = 0,
                templateId = (ushort) templateId,
                templateOriginId = (ushort) templateOriginId
            };

            hierarchyTable[idx] = new HierarchyInfo() {
                childCount = 0,
                firstChildId = 0,
                lastChildId = 0,
                nextSiblingId = 0,
                prevSiblingId = 0,
                parentId = 0
            };

            return new ElementId(idx, metaTable[idx].generation);

        }

        public void AddChild(ElementId parentId, ElementId childId) {
            ref HierarchyInfo parentInfo = ref hierarchyTable[parentId.index];
            ref HierarchyInfo childInfo = ref hierarchyTable[childId.index];

            childInfo.parentId = parentId;
//
            if (parentInfo.childCount == 0) {
                parentInfo.firstChildId = childId;
                parentInfo.lastChildId = childId;
                childInfo.nextSiblingId = 0;
                childInfo.prevSiblingId = 0;
            }
            else {
                childInfo.prevSiblingId = parentInfo.lastChildId;
                childInfo.nextSiblingId = 0;
                parentInfo.lastChildId = childId;
            }

            parentInfo.childCount++;

        }

        public void DestroyElement(ElementId elementId) {
            ref ElementMetaInfo meta = ref metaTable[elementId.index];
            meta.generation++;
            meta.flags = default;
            meta.someFlags = default;
            indexQueue.Enqueue(elementId.index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsAlive(ElementId elementId) {
            return metaTable[elementId.index].generation == elementId.generation;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAlive(ElementId elementId, in ElementTable<ElementMetaInfo> metaTable) {
            return metaTable[elementId.index].generation == elementId.generation;
        }

        public static bool IsDeadOrDisabled(ElementId elementId, ElementTable<ElementMetaInfo> metaTable) {
            return metaTable[elementId].generation != elementId.generation || (metaTable[elementId].flags & UIElementFlags2.EnabledFlagSet) != UIElementFlags2.EnabledFlagSet;
        }

        public int SetSiblingIndex(ElementId id, int value) {
            throw new System.NotImplementedException();
        }

        public void Dispose() {
            if (backingStore != null) {
                UnsafeUtility.Free(backingStore, Allocator.Persistent);
                backingStore = null;
            }
        }

        private void ResizeBackingBuffer(int newCapacity) {

            backingStore = TypedUnsafe.ResizeSplitBuffer(
                ref metaTable.array,
                ref traversalTable.array,
                ref hierarchyTable.array,
                elementCapacity,
                newCapacity,
                Allocator.Persistent,
                true
            );
            elementCapacity = newCapacity;
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

    }

}