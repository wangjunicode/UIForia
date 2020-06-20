using System.Collections.Generic;
using UIForia.Systems;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace UIForia.Graphics {

    internal struct RenderCallComparer : IComparer<DrawInfo> {

        public DataList<RenderCallInfo> renderCallInfo;

        public RenderCallComparer(in DataList<RenderCallInfo> renderCallInfo) {
            this.renderCallInfo = renderCallInfo;
        }

        public int Compare(DrawInfo x, DrawInfo y) {

            if (x.renderCallIdx == y.renderCallIdx) {
                return x.localDrawIdx - y.localDrawIdx;
            }

            ref RenderCallInfo a = ref renderCallInfo[x.renderCallIdx];
            ref RenderCallInfo b = ref renderCallInfo[y.renderCallIdx];

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

    [BurstCompile]
    internal unsafe struct SortOpaqueDrawCalls : IJob {

        public DataList<DrawInfo> opaqueList;
        public DataList<RenderCallInfo> renderCall;

        public void Execute() {

            NativeSortExtension.Sort(opaqueList.GetArrayPointer(), opaqueList.size, new RenderCallComparer(renderCall));

        }

    }

}