using UIForia.Layout;
using UIForia.Rendering;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace UIForia.Systems {

    [BurstCompile]
    public struct ConstructClippersJob : IJob {

        public ElementTable<LayoutHierarchyInfo> layoutHierarchyTable;

        public ElementTable<ClipInfo> clipInfoTable;
        public DataList<ElementId>.Shared viewRootIds;
        public DataList<Clipper>.Shared clipperOutputList;

        public struct ClipPair {

            public ElementId elementId;
            public int inheritedClipperIndex;

        }

        public void Execute() {

            DataList<ClipPair> stack = new DataList<ClipPair>(256, Allocator.Temp);

            for (int i = 0; i < viewRootIds.size; i++) {

                int viewClipperIndex = clipperOutputList.size;
                
                clipperOutputList.Add(new Clipper() {
                    elementId = viewRootIds[i],
                    parentIndex = 1, // 1 is the screen clipper
                });

                stack.Add(new ClipPair() {
                    elementId = viewRootIds[i],
                    inheritedClipperIndex = viewClipperIndex
                });

                while (stack.size != 0) {

                    ClipPair clipPair = stack[--stack.size];

                    int currentClipperIndex = clipPair.inheritedClipperIndex;

                    ref ClipInfo clipInfo = ref clipInfoTable[clipPair.elementId];

                    switch (clipInfo.clipBehavior) {

                        case ClipBehavior.Never:
                            clipInfo.clipperIndex = 0;
                            break;

                        default:
                        case ClipBehavior.Normal:
                            clipInfo.clipperIndex = currentClipperIndex;
                            break;

                        case ClipBehavior.View:
                            clipInfo.clipperIndex = viewClipperIndex;
                            break;

                        case ClipBehavior.Screen:
                            clipInfo.clipperIndex = 1;
                            break;

                    }

                    if (clipInfo.overflow != Overflow.Visible) {
                        int prevIndex = currentClipperIndex;
                        currentClipperIndex = clipperOutputList.size;
                        clipperOutputList.Add(new Clipper() {
                            parentIndex = prevIndex,
                            elementId = clipPair.elementId,
                        });
                    }

                    ref LayoutHierarchyInfo hierarchyInfo = ref layoutHierarchyTable[clipPair.elementId];

                    ElementId childPtr = hierarchyInfo.lastChildId;

                    while (childPtr != default) {

                        stack.Add(new ClipPair() {
                            elementId = childPtr,
                            inheritedClipperIndex = currentClipperIndex
                        });

                        childPtr = layoutHierarchyTable[childPtr].prevSiblingId;

                    }

                }

                clipperOutputList[viewClipperIndex].subClipperCount = clipperOutputList.size - viewClipperIndex;

            }

            stack.Dispose();
        }

    }

}