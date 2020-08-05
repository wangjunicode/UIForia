using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UIForia.Rendering;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Debug = UnityEngine.Debug;

namespace UIForia.Systems {

    public struct EffectUsage {

        public ElementId elementId;
        public bool isForeground;

    }

    [BurstCompile]
    public unsafe struct GatherRenderedElements : IJob {

        public DataList<ElementList> elementLists;
        public ElementTable<ElementTraversalInfo> traversalTable;
        public ElementTable<ClipInfo> clipInfoTable;
        public ElementTable<RenderInfo> renderInfoTable;
        public DataList<RenderCallInfo>.Shared renderCallList;

        public void Execute() {
            renderCallList.size = 0;

            for (int i = 0; i < elementLists.size; i++) {

                ElementList list = elementLists[i];
                renderCallList.EnsureAdditionalCapacity(list.size * 2);
                RenderCallInfo* renderCallPtr = renderCallList.GetArrayPointer();
                int count = renderCallList.size;

                for (int j = 0; j < list.size; j++) {

                    ElementId elementId = list.array[j];
                    ref ClipInfo clipInfo = ref clipInfoTable[elementId];

                    // consider a 'render always' setting so culled things can still handle render target changes etc
                    if (clipInfo.isCulled || clipInfo.visibility == Visibility.Hidden) {
                        continue;
                    }

                    ref RenderInfo renderInfo = ref renderInfoTable[elementId];
                    ref ElementTraversalInfo traversalInfo = ref traversalTable[elementId];
                    renderCallPtr[count++] = new RenderCallInfo() {
                        layer = renderInfo.layer,
                        elementId = elementId,
                        traversalInfo = traversalInfo,
                        zIndex = renderInfo.zIndex,
                        renderOp = 0
                    };

                    renderCallPtr[count++] = new RenderCallInfo() {
                        layer = renderInfo.layer,
                        elementId = elementId,
                        traversalInfo = traversalInfo,
                        zIndex = renderInfo.zIndex,
                        renderOp = 1
                    };

                }

                renderCallList.size = count;
            }

            NativeSortExtension.Sort(renderCallList.GetArrayPointer(), renderCallList.size, new RenderCallComparer());

            // if (!printed) {
            //     printed = true;
            //     for (int i = 0; i < renderCallList.size; i++) {
            //         Debug.Log("element " + renderCallList[i].elementId.index + ": " + renderCallList[i].renderOp);
            //     }
            // }
        }

        private static bool printed = false;
       
        public struct RenderCallComparer : IComparer<RenderCallInfo> {

            public int Compare(RenderCallInfo x, RenderCallInfo y) {

                if (x.traversalInfo.ftbIndex == y.traversalInfo.ftbIndex) {
                    return x.renderOp - y.renderOp;
                }

                if (x.traversalInfo.IsDescendentOf(y.traversalInfo)) {
                    return y.renderOp == 0 ? 1 : -1;
                }

                if (y.traversalInfo.IsDescendentOf(x.traversalInfo)) {
                    return x.renderOp == 0 ? -1 : 1;
                }

                // if (x.renderOp == y.renderOp) {
                // // i think we're still missing a case here with compare
                // }
                
                return x.traversalInfo.ftbIndex - y.traversalInfo.ftbIndex;

            }

        }

    }

}