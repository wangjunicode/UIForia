using System.Collections.Generic;
using UIForia.Systems;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace UIForia.Graphics {

    [BurstCompile]
    internal unsafe struct GetRenderRangesJobs : IJob {

        public DataList<int>.Shared drawOrder;
        public DataList<DrawInfo>.Shared drawList;
        public DataList<RenderCallInfo>.Shared renderCallList;
        public DataList<RangeInt>.Shared renderRanges;

        public void Execute() {

            for (int i = 0; i < drawList.size; i++) {
                drawOrder[i] = i;
            }

            drawOrder.size = drawList.size;

            NativeSortExtension.Sort(drawOrder.GetArrayPointer(), drawOrder.size, new DrawInfoComp(renderCallList, drawList));

            int start = 0;

            for (int i = 0; i < drawOrder.size; i++) {
                ref DrawInfo drawInfo = ref drawList[drawOrder[i]];

                if (drawInfo.type == DrawType.SetRenderTarget) {
                    renderRanges.Add(new RangeInt(start, i - start));
                    start = i;
                }

            }

            renderRanges.Add(new RangeInt(start, drawOrder.size - start));

        }

        public struct DrawInfoComp : IComparer<int> {

            public DataList<RenderCallInfo>.Shared renderCallInfo;
            public DataList<DrawInfo>.Shared drawInfoList;

            public DrawInfoComp(in DataList<RenderCallInfo>.Shared renderCallInfo, in DataList<DrawInfo>.Shared drawInfoList) {
                this.renderCallInfo = renderCallInfo;
                this.drawInfoList = drawInfoList;
            }

            public int Compare(int x, int y) {

                ref DrawInfo drawInfoA = ref drawInfoList[x];
                ref DrawInfo drawInfoB = ref drawInfoList[y];

                if (drawInfoA.renderCallId == drawInfoB.renderCallId) {
                    return drawInfoA.localDrawIdx - drawInfoB.localDrawIdx;
                }

                ref RenderCallInfo a = ref renderCallInfo[drawInfoA.renderCallId];
                ref RenderCallInfo b = ref renderCallInfo[drawInfoB.renderCallId];

                if (a.renderOp != b.renderOp) {
                    return a.renderOp - b.renderOp;
                }

                if (a.layer != b.layer) {
                    return a.layer - b.layer;
                }

                if (a.zIndex != b.zIndex) {
                    return a.zIndex - b.zIndex;
                }

                return a.ftbIndex - b.ftbIndex;
            }

        }

    }

}