using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UIForia.Graphics;
using UIForia.Rendering;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.Profiling;
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
        public ElementTable<RenderInfo> renderInfoTable; // need for layer & z-index eventually
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
                    int elementIndex = elementId.index;
                    ref ClipInfo clipInfo = ref clipInfoTable.array[elementIndex];

                    // consider a 'render always' setting so culled things can still handle render target changes etc
                    if (clipInfo.isCulled || clipInfo.visibility == Visibility.Hidden) {
                        continue;
                    }

                    // ref RenderInfo renderInfo = ref renderInfoTable[elementId];
                    ref ElementTraversalInfo traversalInfo = ref traversalTable.array[elementIndex];
                    renderCallPtr[count++] = new RenderCallInfo() {
                        elementId = elementId,
                        ftbIndex = traversalInfo.ftbIndex,
                        btfIndex = traversalInfo.btfIndex,
                        renderOp = 0
                    };
                    renderCallPtr[count++] = new RenderCallInfo() {
                        // zIndex = renderInfo.zIndex,
                        //  layer = renderInfo.layer,
                        elementId = elementId,
                        ftbIndex = traversalInfo.ftbIndex,
                        btfIndex = traversalInfo.btfIndex,
                        renderOp = 1
                    };
                }

                renderCallList.size = count;
            }
            
      //      Profiler.BeginSample("Sort");
            NativeSortExtension.Sort(renderCallList.GetArrayPointer(), renderCallList.size, new RenderCallComparer());
         //   Profiler.EndSample();
            
            // if (!printed) {
            //     printed = true;
            //     for (int i = 0; i < renderCallList.size; i++) {
            //         Debug.Log("element " + renderCallList[i].elementId.index + ": " + renderCallList[i].renderOp);
            //     }
            // }
        }

        
        private static bool printed = false;
        public static int compares;
        public struct RenderCallComparer : IComparer<RenderCallInfo> {

            public int Compare(RenderCallInfo x, RenderCallInfo y) {
                if (x.ftbIndex == y.ftbIndex) {
                    return x.renderOp - y.renderOp;
                }

                if (x.ftbIndex > y.ftbIndex && x.btfIndex > y.btfIndex) {
                    return y.renderOp == 0 ? 1 : -1;
                }

                if (y.ftbIndex > x.ftbIndex && y.btfIndex > x.btfIndex) {
                    return x.renderOp == 0 ? -1 : 1;
                }

                // if (x.renderOp == y.renderOp) {
                // // i think we're still missing a case here with compare
                // }

                return x.ftbIndex - y.ftbIndex;

            }

        }

    }

}