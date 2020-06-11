using UIForia.Layout;
using UIForia.Rendering;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace UIForia.Systems {

    [BurstCompile]
    public unsafe struct UpdateClippers : IJob {

        public DataList<Clipper>.Shared clipperList;
        public ElementTable<ClipInfo> clipInfoTable;
        public ElementTable<LayoutBoxInfo> layoutResultTable;
        public DataList<float2>.Shared intersectionResults;

        public void Execute() {
            Run(2, clipperList.size);
        }

        private void Run(int start, int end) {

            // todo -- compute oriented bounds for clippers in this job so it can run in parallel with building world matrices
            // we duplicate work but who cares, parallel benefit is big
            // will take longer but parallel trade off gains us ~0.2ms by not waiting on the world matrix job
            
            // skip screen and never clippers
            start = start < 2 ? 2 : start;

            DataList<float2> pointBuffer = new DataList<float2>(16, Allocator.Temp);
            DataList<float2> outputBuffer = new DataList<float2>(16, Allocator.Temp);
            DataList<SutherlandHodgman.Edge> edgeBuffer = new DataList<SutherlandHodgman.Edge>(16, Allocator.Temp);

            DataList<float2> subjectBuffer = new DataList<float2>(4, Allocator.Temp);

            for (int i = start; i < end; i++) {

                ref Clipper clipper = ref clipperList[i];

                ElementId elementId = clipper.elementId;

                ref ClipInfo clipInfo = ref clipInfoTable[elementId];
                clipper.isCulled = false;
                clipper.intersectionRange = default;
                clipper.aabb = default;

                if (clipperList[clipper.parentIndex].isCulled) {
                    clipper.isCulled = true;
                    continue;
                }

                ref LayoutBoxInfo layoutBoxInfo = ref layoutResultTable[elementId];

                if (layoutBoxInfo.actualSize.x * layoutBoxInfo.actualSize.y == 0) {
                    clipper.isCulled = true;
                    continue;
                }

                OrientedBounds bounds = clipInfo.orientedBounds;
                subjectBuffer[0] = bounds.p0;
                subjectBuffer[1] = bounds.p1;
                subjectBuffer[2] = bounds.p2;
                subjectBuffer[3] = bounds.p3;
                subjectBuffer.size = 4;

                RangeInt parentIntersectionRange = clipperList[clipper.parentIndex].intersectionRange;

                int rangeStart = intersectionResults.size;
                outputBuffer.size = 0;

                SutherlandHodgman.GetIntersectedPolygon(
                    subjectBuffer,
                    intersectionResults.GetArrayPointer() + parentIntersectionRange.start,
                    parentIntersectionRange.length,
                    ref outputBuffer,
                    ref pointBuffer,
                    ref edgeBuffer
                );

                if (outputBuffer.size > 0) {
                    intersectionResults.EnsureAdditionalCapacity(outputBuffer.size);
                    TypedUnsafe.MemCpy(intersectionResults.GetArrayPointer() + intersectionResults.size, outputBuffer.GetArrayPointer(), outputBuffer.size);
                }

                clipper.intersectionRange = new RangeInt(rangeStart, outputBuffer.size);
                clipper.isCulled = clipper.intersectionRange.length <= 0;
                if (clipper.isCulled) {
                    clipper.aabb = default;
                    continue;
                }

                clipper.aabb = PolygonUtil.GetBounds(intersectionResults.GetArrayPointer() + clipper.intersectionRange.start, clipper.intersectionRange.length);

            }

            pointBuffer.Dispose();
            edgeBuffer.Dispose();
            subjectBuffer.Dispose();
        }

    }

}