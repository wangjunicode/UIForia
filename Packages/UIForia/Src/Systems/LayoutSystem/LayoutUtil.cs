using System;
using UIForia.Layout;

namespace UIForia.Systems {

    public static class LayoutUtil {

        public static void UnlinkFromParent(ElementId elementId, ElementTable<LayoutHierarchyInfo> layoutHierarchyTable) {
            ref LayoutHierarchyInfo layoutInfo = ref layoutHierarchyTable[elementId];

            if (layoutInfo.nextSiblingId != default) {
                layoutHierarchyTable[layoutInfo.nextSiblingId].prevSiblingId = layoutInfo.prevSiblingId;
            }

            if (layoutInfo.prevSiblingId != default) {
                layoutHierarchyTable[layoutInfo.prevSiblingId].nextSiblingId = layoutInfo.nextSiblingId;
            }

            if (layoutInfo.parentId != default) {
                ref LayoutHierarchyInfo parentLayoutInfo = ref layoutHierarchyTable[layoutInfo.parentId];
                parentLayoutInfo.childCount--;
                if (parentLayoutInfo.firstChildId == elementId) {
                    parentLayoutInfo.firstChildId = layoutInfo.nextSiblingId;
                }

                if (parentLayoutInfo.lastChildId == elementId) {
                    parentLayoutInfo.lastChildId = layoutInfo.prevSiblingId;
                }
            }

            // leave parent id intact since we'll still need it for matrix operations etc
            layoutInfo.prevSiblingId = default;
            layoutInfo.nextSiblingId = default;
        }

        public static void Insert(ElementId parentId, ElementId elementId, ElementTable<ElementTraversalInfo> traversalTable, ElementTable<LayoutHierarchyInfo> layoutHierarchyTable) {

            ElementId ptr = layoutHierarchyTable[parentId].firstChildId;

            ElementTraversalInfo traversalInfo = traversalTable[elementId];

            if (ptr == default) {
                SetLastChild(parentId, elementId, layoutHierarchyTable);
                return;
            }

            while (ptr != default) {
                if (traversalTable[ptr].IsLaterInHierarchy(traversalInfo)) {
                    break;
                }

                ptr = layoutHierarchyTable[ptr].nextSiblingId;
            }

            if (ptr == default) {
                SetLastChild(parentId, elementId, layoutHierarchyTable);
            }
            else {
                BecomePreviousSibling(elementId, ptr, layoutHierarchyTable);
            }
        }

        public static void SetLastChild(ElementId parentId, ElementId toInsert, ElementTable<LayoutHierarchyInfo> layoutHierarchyTable) {
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

        private static void BecomePreviousSibling(ElementId toInsert, ElementId target, ElementTable<LayoutHierarchyInfo> layoutHierarchyTable) {

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

        // start at first enabled element
        // while this element has a higher ftb traversal index
        // continue
        // insert point found
        // unlink children from their enclosingn layout siblings
        // link to first and last child
        public static void Untransclude(ElementId elementId, ElementTable<ElementTraversalInfo> traversalTable, ElementTable<HierarchyInfo> elementHierarchyTable, ElementTable<LayoutHierarchyInfo> layoutHierarchyTable) {
            ref HierarchyInfo elementHierarchyInfo = ref elementHierarchyTable[elementId];

            ElementId parentId = elementHierarchyInfo.parentId;

            // find the layout parent element. while parent is transcluded, get parent's parent
            while (layoutHierarchyTable[parentId].behavior == LayoutBehavior.TranscludeChildren) {
                parentId = elementHierarchyTable[parentId].parentId;
            }

            ref LayoutHierarchyInfo parentLayoutHierarchyInfo = ref layoutHierarchyTable[parentId];
            ref LayoutHierarchyInfo layoutHierarchyInfo = ref layoutHierarchyTable[elementId];

            layoutHierarchyInfo = default;
            layoutHierarchyInfo.behavior = LayoutBehavior.Normal;

            ElementTraversalInfo elementTraversalInfo = traversalTable[elementId];

            ElementId forwardPtr = parentLayoutHierarchyInfo.firstChildId;
            while (forwardPtr != default) {

                if (traversalTable[forwardPtr].IsDescendentOf(elementTraversalInfo)) {
                    break;
                }

                forwardPtr = layoutHierarchyTable[forwardPtr].nextSiblingId;
            }

            if (forwardPtr != default) {
                ElementId reversePtr = parentLayoutHierarchyInfo.lastChildId;
                while (reversePtr != default) {
                    if (traversalTable[reversePtr].IsDescendentOf(elementTraversalInfo)) {
                        break;
                    }

                    reversePtr = layoutHierarchyTable[reversePtr].prevSiblingId;
                }

                layoutHierarchyInfo.parentId = parentId;
                layoutHierarchyInfo.firstChildId = forwardPtr;
                layoutHierarchyInfo.lastChildId = reversePtr;

                ref LayoutHierarchyInfo forwardInfo = ref layoutHierarchyTable[forwardPtr];
                ref LayoutHierarchyInfo reverseInfo = ref layoutHierarchyTable[reversePtr];

                if (forwardInfo.prevSiblingId != default) {
                    layoutHierarchyInfo.prevSiblingId = forwardInfo.prevSiblingId;
                    layoutHierarchyTable[forwardInfo.prevSiblingId].nextSiblingId = elementId;
                }

                if (reverseInfo.nextSiblingId != default) {
                    layoutHierarchyInfo.nextSiblingId = reverseInfo.nextSiblingId;
                    layoutHierarchyTable[reverseInfo.nextSiblingId].prevSiblingId = elementId;
                }

                forwardInfo.prevSiblingId = default;
                reverseInfo.nextSiblingId = default;

                int count = 1;
                ElementId ptr = forwardPtr;
                layoutHierarchyTable[forwardPtr].parentId = elementId;
                layoutHierarchyTable[reversePtr].parentId = elementId;
                while (ptr != reversePtr) {
                    count++;
                    layoutHierarchyTable[ptr].parentId = elementId;
                    ptr = layoutHierarchyTable[ptr].nextSiblingId;
                }

                parentLayoutHierarchyInfo.childCount += (-count + 1);
                layoutHierarchyInfo.childCount = count;

            }
            else {
                // no descendents are currently enabled in the parent, we'll find the first place we can slot in based on element hierarchy instead
                ElementId ptr = parentLayoutHierarchyInfo.firstChildId;

                while (ptr != default) {
                    if (traversalTable[ptr].IsLaterInHierarchy(elementTraversalInfo)) {
                        break;
                    }

                    ptr = layoutHierarchyTable[ptr].nextSiblingId;
                }

                if (ptr != null) {
                    BecomePreviousSibling(elementId, ptr, layoutHierarchyTable);
                }
                else {
                    SetLastChild(elementId, parentId, layoutHierarchyTable);
                }

            }

        }

        public static void TranscludeUnattached(ElementId elementId, ElementTable<LayoutHierarchyInfo> layoutHierarchyTable, ElementTable<ElementTraversalInfo> traversalTable) {
            ref LayoutHierarchyInfo hierarchyInfo = ref layoutHierarchyTable[elementId];

            if (hierarchyInfo.childCount == 0) {
                hierarchyInfo = default;
                hierarchyInfo.behavior = LayoutBehavior.TranscludeChildren;
                return;
            }

            // assumes parent is already setup correctly
            ElementId ptr = hierarchyInfo.firstChildId;

            while (ptr != default) {
                layoutHierarchyTable[ptr].parentId = hierarchyInfo.parentId;
                ptr = layoutHierarchyTable[ptr].nextSiblingId;
            }
            
            ref LayoutHierarchyInfo parentHierarchyInfo = ref layoutHierarchyTable[hierarchyInfo.parentId];

            ElementTraversalInfo elementTraversalInfo = traversalTable[elementId];

            if (parentHierarchyInfo.childCount == 0) {
                parentHierarchyInfo.firstChildId = hierarchyInfo.firstChildId;
                parentHierarchyInfo.lastChildId = hierarchyInfo.lastChildId;
                parentHierarchyInfo.childCount = hierarchyInfo.childCount;
            }
            else {

                ElementId forwardPtr = parentHierarchyInfo.firstChildId;
                while (forwardPtr != default) {

                    if (elementTraversalInfo.IsLaterInHierarchy(traversalTable[forwardPtr])) {
                        break;
                    }

                    forwardPtr = layoutHierarchyTable[forwardPtr].nextSiblingId;
                }

                ref LayoutHierarchyInfo firstChildInfo = ref layoutHierarchyTable[hierarchyInfo.firstChildId];
                ref LayoutHierarchyInfo lastChildInfo = ref layoutHierarchyTable[hierarchyInfo.lastChildId];

                if (forwardPtr == default) {
                    // set last child
                    ref LayoutHierarchyInfo parentLastChild = ref layoutHierarchyTable[parentHierarchyInfo.lastChildId];
                    parentLastChild.nextSiblingId = hierarchyInfo.firstChildId;
                    firstChildInfo.prevSiblingId = parentHierarchyInfo.lastChildId;
                    parentHierarchyInfo.childCount += hierarchyInfo.childCount;
                    parentHierarchyInfo.lastChildId = hierarchyInfo.lastChildId;
                   
                }
                else {
                    ref LayoutHierarchyInfo prevSibling = ref layoutHierarchyTable[forwardPtr];

                    firstChildInfo.prevSiblingId = forwardPtr;
                    lastChildInfo.nextSiblingId = prevSibling.nextSiblingId;

                    if (prevSibling.nextSiblingId == default) {
                        parentHierarchyInfo.lastChildId = hierarchyInfo.lastChildId;
                    }
                    else {
                        layoutHierarchyTable[prevSibling.nextSiblingId].prevSiblingId = hierarchyInfo.lastChildId;
                    }

                    prevSibling.nextSiblingId = hierarchyInfo.firstChildId;

                    parentHierarchyInfo.childCount += hierarchyInfo.childCount;
                }
            }

            hierarchyInfo = default;
            hierarchyInfo.behavior = LayoutBehavior.TranscludeChildren;

        }

        public static void Transclude(ElementId elementId, ElementTable<LayoutHierarchyInfo> layoutHierarchyTable) {
            ref LayoutHierarchyInfo hierarchyInfo = ref layoutHierarchyTable[elementId];

            if (hierarchyInfo.prevSiblingId == default) {
                layoutHierarchyTable[hierarchyInfo.parentId].firstChildId = hierarchyInfo.firstChildId;
            }
            else {
                // become previous sibling
                layoutHierarchyTable[hierarchyInfo.prevSiblingId].nextSiblingId = hierarchyInfo.firstChildId;
            }

            if (hierarchyInfo.nextSiblingId == default) {
                layoutHierarchyTable[hierarchyInfo.parentId].lastChildId = hierarchyInfo.lastChildId;
            }
            else {
                layoutHierarchyTable[hierarchyInfo.nextSiblingId].prevSiblingId = hierarchyInfo.lastChildId;
            }

            layoutHierarchyTable[hierarchyInfo.firstChildId].prevSiblingId = hierarchyInfo.prevSiblingId;
            layoutHierarchyTable[hierarchyInfo.lastChildId].nextSiblingId = hierarchyInfo.nextSiblingId;

            layoutHierarchyTable[hierarchyInfo.parentId].childCount += hierarchyInfo.childCount - 1; // -1 to account for 

            // assumes parent is already setup correctly
            ElementId ptr = hierarchyInfo.firstChildId;

            while (ptr != default) {
                layoutHierarchyTable[ptr].parentId = hierarchyInfo.parentId;
                ptr = layoutHierarchyTable[ptr].nextSiblingId;
            }

            hierarchyInfo = default; // at this point all values in here are junk, we'll need to re-compute the hierarchy if the behavior changes
            hierarchyInfo.behavior = LayoutBehavior.TranscludeChildren;

        }

        public static ElementId FindLayoutParent(ElementId elementId, ElementTable<HierarchyInfo> hierarchyTable, ElementTable<LayoutHierarchyInfo> layoutHierarchyTable) {
            ref HierarchyInfo hierarchyInfo = ref hierarchyTable[elementId];

            ElementId parentId = hierarchyInfo.parentId;

            // find the layout parent element. while parent is transcluded, get parent's parent
            while (layoutHierarchyTable[parentId].behavior == LayoutBehavior.TranscludeChildren) {
                parentId = hierarchyTable[parentId].parentId;
            }

            return parentId;
        }

    }

}