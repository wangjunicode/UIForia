using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UIForia.Elements;
using UIForia.Layout;
using UIForia.Util.Unsafe;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

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

        private int elementCapacity;
        private byte* backingStore;

        public ElementSystem(int initialElementCount) {
            this.idGenerator = 1; // 0 is always invalid
            this.indexQueue = new Queue<int>(k_MinFreeIndices * 2);
            ResizeBackingBuffer(initialElementCount);
            Initialize();
        }

        public void Initialize() {
            idGenerator = 1;
            indexQueue.Clear();
            Array.Clear(instanceTable, 0, instanceTable.Length);
            Array.Clear(layoutTable, 0, layoutTable.Length);
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
                idx = ++idGenerator;
                if (idx > elementCapacity) {
                    ResizeBackingBuffer(elementCapacity * 2);
                }
            }

            if (idx > elementCapacity) {
                Debug.Log("Out of bounds");
                return default;
            }

            // note: need to use generation from existing data at idx
            metaTable.array[idx].flags = (ushort) flags; // todo -- make this better
            metaTable.array[idx].someFlags = 0;

            
            traversalTable.array[idx] = new ElementTraversalInfo() {
                depth = depth,
                btfIndex = 0,
                ftbIndex = 0,
                templateId = (ushort) templateId,
                templateOriginId = (ushort) templateOriginId
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
            meta.someFlags = default;
            indexQueue.Enqueue(elementId.index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsAlive(ElementId elementId) {
            return metaTable.array[elementId.index].generation == elementId.generation;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAlive(ElementId elementId, in ElementTable<ElementMetaInfo> metaTable) {
            return metaTable.array[elementId.index].generation == elementId.generation;
        }

        public static bool IsDeadOrDisabled(ElementId elementId, ElementTable<ElementMetaInfo> metaTable) {
            return metaTable[elementId].generation != elementId.generation || (metaTable[elementId].flags & (ushort) UIElementFlags.EnabledFlagSet) != (ushort) UIElementFlags.EnabledFlagSet;
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
            if (instanceTable == null) {
                instanceTable = new UIElement[elementCapacity];
                layoutTable = new LayoutResult[elementCapacity];
            }
            else {
                if (instanceTable.Length < elementCapacity) {
                    Array.Resize(ref instanceTable, elementCapacity);
                    Array.Resize(ref layoutTable, elementCapacity);
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

    }

}