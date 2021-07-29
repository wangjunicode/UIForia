using System.Diagnostics;
using System.Linq.Expressions;
using UIForia.Elements;
using UIForia.Style;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace UIForia.Layout {

    internal struct InputQueryResult {

        public ElementId elementId;
        public bool requiresCustomHandling;
        public int zIndex;

        public InputQueryResult(ElementId elementId, int zIndex, bool requiresCustomHandling = false) {
            this.elementId = elementId;
            this.requiresCustomHandling = requiresCustomHandling;
            this.zIndex = zIndex;
        }

        public static InputQueryResult Create(ElementId elementId, in CheckedArray<TraversalInfo> traversalInfo) {
            return new InputQueryResult(elementId, traversalInfo[elementId.index].zIndex);
        }
    }

    public struct ClipInfo {

        public bool isCulled;
        public AxisAlignedBounds2D aabb;
        public OrientedBounds orientedBounds;
        public ElementId elementId; // temp for debugging

        // public int clipperIndex;
        // public Visibility visibility;
        // public PointerEvents pointerEvents;
        // public ClipBehavior clipBehavior;
        // public ClipBounds clipBounds;
        // public Overflow overflow;
        // public bool isCulled;
        // public bool fullyContainedByParentClipper;

    }

    public struct Clipper {

        public bool isCulled;
        public int flatElementIndex;
        public AxisAlignedBounds2D aabb;
        public int parentIndex;
        public ElementId elementId;
        public OrientedBounds orientedBounds;

    }

    [BurstCompile(DisableSafetyChecks = UIApplication.DisableJobSafety)]
    internal unsafe struct InputQuery : IJob {

        public float2 point;
        public int maxElementId;
        public CheckedArray<ElementId> elementIdByActiveIndex;
        public CheckedArray<TraversalInfo> traversalTable;
        public CheckedArray<ElementId> elementIdToParentId;
        [NativeDisableUnsafePtrRestriction] public StyleTables* styleTables;
        [NativeDisableUnsafePtrRestriction] public LayoutTree* layoutTree;
        [NativeDisableUnsafePtrRestriction] public PerFrameLayoutOutput* perFrameLayoutOutput;
        [NativeDisableUnsafePtrRestriction] public LockedBumpAllocator* lockedBumpAllocator;

        public void Execute() {

            Debug.Log($"x,y: {point.x}, {point.y}");
            
            CheckedArray<Clipper> clippers = perFrameLayoutOutput->clippers;
            if (clippers.size < 2) {
                // todo -- might need to set some data still in this case 
                return;
            }

            bool* containsPoint = stackalloc bool[clippers.size];

            containsPoint[0] = true; // never clipper
            containsPoint[1] = true; // screen clipper

            for (int i = 0; i < clippers.size; i++) {
                ref Clipper clipper = ref clippers.array[i];
                containsPoint[i] = !clipper.isCulled && PolygonUtil.PointInOrientedBounds(point, clipper.orientedBounds);
            }

            int candidateCount = 0;
            for (int i = 0; i < layoutTree->elementCount; i++) {
                
                ElementId elementId = elementIdByActiveIndex[i];
                int layoutIndex = layoutTree->elementIdToLayoutIndex[elementId.index];

                ushort clipperIndex = perFrameLayoutOutput->clipperIndex[layoutIndex];

                if (!clippers[clipperIndex].isCulled && containsPoint[clipperIndex]) {
                    candidateCount++;
                }

            }

            TempList<int> elementsWithinClipBounds = TypedUnsafe.MallocUnsizedTempList<int>(candidateCount, Allocator.Temp);

            // find all the non clipped elements
            for (int i = 0; i < layoutTree->elementCount; i++) {
                ElementId elementId = elementIdByActiveIndex[i];
                if (styleTables->PointerEvents[elementId.index] == PointerEvents.None) {
                    continue;
                }
                
                int layoutIndex = layoutTree->elementIdToLayoutIndex[elementId.index];
                ushort clipperIndex = perFrameLayoutOutput->clipperIndex[layoutIndex];

                if (!clippers[clipperIndex].isCulled && containsPoint[clipperIndex]) {
                    elementsWithinClipBounds.array[elementsWithinClipBounds.size++] = layoutIndex;
                }
            }

            // return here if nothing got any input
            if (elementsWithinClipBounds.size == 0) {
                elementsWithinClipBounds.Dispose();
                return;
            }

            // NativeSortExtension.Sort(elementsWithinClipBounds.array, elementsWithinClipBounds.size);

            TempList<ElementId> elementsMouseOver = TypedUnsafe.MallocUnsizedTempList<ElementId>(elementsWithinClipBounds.size, Allocator.Temp);

            // check each non clipped element to see if it contains the query point or has special handling 
            for (int i = elementsWithinClipBounds.size - 1; i >= 0; i--) {

                int layoutIndex = elementsWithinClipBounds[i];

                ref ClipInfo clipInfo = ref perFrameLayoutOutput->clipInfos.array[layoutIndex];

                ElementId elementId = layoutTree->nodeList[layoutIndex].elementId;

                // todo -- re-implement this, the flag is set when it shouldn't be 
                // if((metaTable.Get(elementId.index).flags &= UIElementFlags.HasCustomInputHandling) != 0) {
                //     retn.array[retn.size++] = new InputQueryResult(elementId, true);
                //     continue;
                // }

                if (PolygonUtil.PointInOrientedBounds(point, clipInfo.orientedBounds)) {
                    elementsMouseOver.array[elementsMouseOver.size++] = elementId;
                }
            }

            if (elementsMouseOver.size == 0) {
                elementsWithinClipBounds.Dispose();
                elementsMouseOver.Dispose();
                return;
            }

            int mapSize = LongBoolMap.GetMapSize(maxElementId);
            ulong* buffer = stackalloc ulong[mapSize];
            ElementMap elementsInHierarchy = new ElementMap(buffer, mapSize);

            for (int i = 0; i < elementsMouseOver.size; i++) {
            
                if (elementsInHierarchy.TrySet(elementsMouseOver[i])) {
                    // walk up parents
                    var parent = elementIdToParentId[elementsMouseOver[i].index];
                    while (parent != default) {
                        if (!elementsInHierarchy.TrySet(parent)) {
                            break;
                        }
                        parent = elementIdToParentId[parent.index];
                    }
                }
                
            }

            int elementsInHierarchyCount = elementsInHierarchy.PopCount();
            DataList<ElementId> elementIdsInHierarchy = new DataList<ElementId>(elementsInHierarchyCount, Allocator.Temp);
            // elementsInHierarchy.ToTempList()
            for (int i = 0; i < elementsMouseOver.size; i++) {
                elementIdsInHierarchy.Add(elementsMouseOver[i]);
            }

            NativeSortExtension.Sort(elementIdsInHierarchy.array, elementIdsInHierarchy.size, new HierarchySorter() {
                traversalInfo = traversalTable
            });

            TempList<InputQueryResult> filteredResult = TypedUnsafe.MallocUnsizedTempList<InputQueryResult>(elementIdsInHierarchy.size, Allocator.Temp);
            
            // at this point the first element is the one the mouse is definitely over
            TraversalInfo first = traversalTable[elementIdsInHierarchy[0].index];
            filteredResult.array[filteredResult.size++] = InputQueryResult.Create(elementIdsInHierarchy[0], in traversalTable);

            // we filter out all the elements that are not in the same branch as the first element
            for (int i = 1; i < elementIdsInHierarchy.size; i++) {
                TraversalInfo current = traversalTable[elementIdsInHierarchy[i].index];
                if (current.IsAncestorOf(first)) {
                    filteredResult.array[filteredResult.size++] = InputQueryResult.Create(elementIdsInHierarchy[i], in traversalTable);
                }
            }

            InputQueryResult* memory = lockedBumpAllocator->Allocate<InputQueryResult>(filteredResult.size);
            perFrameLayoutOutput->mouseQueryResults = new CheckedArray<InputQueryResult>(memory, filteredResult.size);
            TypedUnsafe.MemCpy(perFrameLayoutOutput->mouseQueryResults.array, filteredResult.array, filteredResult.size);

            elementsMouseOver.Dispose();
            elementsWithinClipBounds.Dispose();
            filteredResult.Dispose();
        }
    }

}