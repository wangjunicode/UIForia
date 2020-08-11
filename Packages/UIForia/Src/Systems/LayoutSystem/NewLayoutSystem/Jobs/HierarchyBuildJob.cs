using System.Diagnostics;
using UIForia.Layout;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace UIForia.Systems {

    [BurstCompile]
    public struct HierarchyBuildJob : IJob {

        public DataList<ElementId>.Shared roots;
        public DataList<ElementId>.Shared layoutIgnoredList;

        public ElementTable<ElementMetaInfo> metaTable;
        public ElementTable<HierarchyInfo> hierarchyTable;
        public ElementTable<LayoutHierarchyInfo> layoutHierarchyTable;

        public void Execute() {

            DataList<ElementId> stack = new DataList<ElementId>(128, Allocator.Temp);
            DataList<ElementId> transclusionList = new DataList<ElementId>(32, Allocator.Temp);

            for (int i = 0; i < roots.size; i++) {
                
                ElementId rootId = roots[i];
                
                stack.Add(rootId);

                while (stack.size != 0) {

                    if(roots[0].index != 2) Debugger.Break();
                    
                    ElementId parent = stack[--stack.size];

                    ElementId ptr = hierarchyTable[parent].firstChildId;

                    ref LayoutHierarchyInfo parentLayoutInfo = ref layoutHierarchyTable[parent];

                    parentLayoutInfo.childCount = 0;
                    parentLayoutInfo.firstChildId = default;
                    parentLayoutInfo.lastChildId = default;

                    ElementId prevSibling = default;

                    while (ptr.id != default) {
                        if(roots[0].index != 2) Debugger.Break();

                        if (ElementSystem.IsDeadOrDisabled(ptr, metaTable)) {
                            ptr = hierarchyTable[ptr].nextSiblingId;
                            continue;
                        }

                        ElementId currentChild = ptr;
                        ptr = hierarchyTable[ptr].nextSiblingId;

                        ref LayoutHierarchyInfo layoutHierarchyInfo = ref layoutHierarchyTable[currentChild];

                        layoutHierarchyInfo.parentId = parent;
                        layoutHierarchyInfo.prevSiblingId = prevSibling;
                        parentLayoutInfo.childCount++;

                        if (prevSibling == default) {
                            parentLayoutInfo.firstChildId = currentChild;
                        }
                        else {
                            ref LayoutHierarchyInfo prevLayoutHierarchy = ref layoutHierarchyTable[prevSibling];
                            prevLayoutHierarchy.nextSiblingId = currentChild;
                        }
                        if(roots[0].index != 2) Debugger.Break();

                        // always set last child since we dont know if next ptr value is enabled or not
                        parentLayoutInfo.lastChildId = currentChild;

                        prevSibling = currentChild;

                        switch (layoutHierarchyInfo.behavior) {
                            // easier to setup everything and then do another teardown pass for ignored & transclude
                            // unlinking is cheap and that way the data for all the elements is totally correct
                            case LayoutBehavior.Ignored:
                                layoutIgnoredList.Add(currentChild);
                                break;

                            case LayoutBehavior.TranscludeChildren:
                                transclusionList.Add(currentChild);
                                break;
                        }

                    }

                    ptr = hierarchyTable[parent].lastChildId;
                    if(roots[0].index != 2) Debugger.Break();

                    while (ptr != default) {
                        if(roots[0].index != 2) Debugger.Break();
                        stack.Add(ptr);      
                        if(roots[0].index != 2) Debugger.Break();

                        ptr = hierarchyTable[ptr].prevSiblingId;
                    }
                }
            }

            // already in hierarchy sort order
            for (int i = 0; i < transclusionList.size; i++) {
                LayoutUtil.Transclude(transclusionList[i], layoutHierarchyTable);
            }

            // already in hierarchy sort order
            for (int i = 0; i < layoutIgnoredList.size; i++) {
                LayoutUtil.UnlinkFromParent(layoutIgnoredList[i], layoutHierarchyTable);
            }

            // link root to its parent. Roots will never be in the ignore or transclusion lists
            for (int i = 0; i < roots.size; i++) {
                layoutHierarchyTable[roots[i]].parentId = LayoutUtil.FindLayoutParent(roots[i], hierarchyTable, layoutHierarchyTable);
            }

            transclusionList.Dispose();
            stack.Dispose();
        }

    }

}