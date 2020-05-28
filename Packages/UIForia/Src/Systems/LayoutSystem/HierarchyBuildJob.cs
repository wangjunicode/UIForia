using UIForia.Layout;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace UIForia.Systems {

    [BurstCompile]
    public struct HierarchyBuildJob : IJob {

        public DataList<ElementId>.Shared roots;
        public DataList<ElementId>.Shared layoutIgnoredList;

        public ElementTable<ElementMetaInfo> metaTable;
        public ElementTable<HierarchyInfo> hierarchyTable;
        public ElementTable<LayoutMetaData> layoutMetaTable;
        public ElementTable<LayoutHierarchyInfo> layoutHierarchyTable;

        public void Execute() {

            DataList<ElementId> stack = new DataList<ElementId>(128, Allocator.Temp);
            DataList<ElementId> transclusionList = new DataList<ElementId>(32, Allocator.Temp);

            for (int i = 0; i < roots.size; i++) {
                stack.Add(roots[i]);

                while (stack.size != 0) {

                    ElementId parent = stack[--stack.size];

                    ElementId ptr = hierarchyTable[parent].firstChildId;

                    ref LayoutHierarchyInfo parentData = ref layoutHierarchyTable[parent];

                    parentData.childCount = 0;
                    parentData.firstChildId = default;
                    parentData.lastChildId = default;

                    ElementId prevSibling = default;

                    while (ptr.id != 0) {

                        if (ElementSystem.IsDeadOrDisabled(ptr, metaTable)) {
                            ptr = hierarchyTable[ptr].nextSiblingId;
                            continue;
                        }

                        ElementId currentChild = ptr;
                        ptr = hierarchyTable[ptr].nextSiblingId;

                        ref LayoutMetaData metaData = ref layoutMetaTable[currentChild];
                        ref LayoutHierarchyInfo layoutHierarchyInfo = ref layoutHierarchyTable[currentChild];

                        layoutHierarchyInfo.parentId = parent;
                        layoutHierarchyInfo.prevSiblingId = prevSibling;
                        parentData.childCount++;

                        if (prevSibling == default) {
                            parentData.firstChildId = currentChild;
                        }
                        else {
                            ref LayoutHierarchyInfo prevLayoutHierarchy = ref layoutHierarchyTable[prevSibling];
                            prevLayoutHierarchy.nextSiblingId = currentChild;
                        }

                        // always set last child since we dont know if next ptr value is enabled or not
                        parentData.lastChildId = currentChild;

                        prevSibling = currentChild;

                        if (metaData.layoutBehavior == LayoutBehavior.Ignored) {
                            // easier to setup everything and then do another teardown pass for ignored & transcluded I think
                            // unlinking is cheap and that way the data for all the elements is totally correct
                            layoutIgnoredList.Add(currentChild);
                        }
                        else if (metaData.layoutBehavior == LayoutBehavior.TranscludeChildren) {
                            // adjust for transculsion in a 2nd pass
                            // i still need to track first and last child pointers in transcluded element in case it later becomes untranscluded
                            // child count can be 0 though to signify that we skip this element in layout? or just keep it setup and skip it
                            // i still have the element linkages if I need them to resolve the transclude parent 
                            transclusionList.Add(currentChild);
                        }

                    }

                    ptr = hierarchyTable[parent].lastChildId;

                    while (ptr != default) {
                        stack.Add(ptr);
                        ptr = hierarchyTable[ptr].prevSiblingId;
                    }
                }
            }

            // link root to its parent
            for (int i = 0; i < roots.size; i++) {

                AttachRootToParent(roots[i]);

            }

            transclusionList.Dispose();
            stack.Dispose();
        }

        private void AttachRootToParent(ElementId currentId) {

            ElementId hierarchyParentId = hierarchyTable[currentId].parentId;
            ElementId nextEnabledSiblingId = hierarchyTable[currentId].nextSiblingId;

            // find next enabled sibling in hierarchy
            while (nextEnabledSiblingId != default) {

                if (ElementSystem.IsDeadOrDisabled(nextEnabledSiblingId, metaTable)) {
                    nextEnabledSiblingId = hierarchyTable[nextEnabledSiblingId].nextSiblingId;
                }
                else {
                    break;
                }

            }

            if (nextEnabledSiblingId != default) {
                // get its layout box, become the previous sibling
                BecomePreviousSibling(currentId, nextEnabledSiblingId);
            }
            else {
                BecomeLastChild(currentId, hierarchyParentId);
            }

        }

        private void BecomeLastChild(ElementId toInsert, ElementId parentId) {
            ref LayoutHierarchyInfo parentLayoutInfo = ref layoutHierarchyTable[parentId];
            ref LayoutHierarchyInfo toInsertLayoutInfo = ref layoutHierarchyTable[toInsert];

            ElementId prevLastChild = parentLayoutInfo.lastChildId;

            if (prevLastChild != default) {
                layoutHierarchyTable[prevLastChild].nextSiblingId = toInsert;
            }

            toInsertLayoutInfo.parentId = parentId;
            toInsertLayoutInfo.nextSiblingId = default;
            toInsertLayoutInfo.prevSiblingId = prevLastChild;

            parentLayoutInfo.childCount++;
            parentLayoutInfo.lastChildId = toInsert;

            if (parentLayoutInfo.firstChildId == default) {
                parentLayoutInfo.firstChildId = toInsert;
            }

        }

        private void BecomePreviousSibling(ElementId toInsert, ElementId target) {
            ref LayoutHierarchyInfo nextSiblingLayoutInfo = ref layoutHierarchyTable[target];
            ref LayoutHierarchyInfo toInsertLayoutInfo = ref layoutHierarchyTable[toInsert];
            ref LayoutHierarchyInfo parentLayoutInfo = ref layoutHierarchyTable[nextSiblingLayoutInfo.parentId];

            ElementId lastPrevSibling = nextSiblingLayoutInfo.prevSiblingId;

            if (lastPrevSibling != default) {
                layoutHierarchyTable[lastPrevSibling].nextSiblingId = toInsert;
            }

            nextSiblingLayoutInfo.prevSiblingId = toInsert;

            parentLayoutInfo.childCount++;

            if (parentLayoutInfo.firstChildId == target) {
                parentLayoutInfo.firstChildId = toInsert;
            }

            toInsertLayoutInfo.parentId = nextSiblingLayoutInfo.parentId;
            toInsertLayoutInfo.nextSiblingId = target;
            toInsertLayoutInfo.prevSiblingId = lastPrevSibling;

        }

    }

}