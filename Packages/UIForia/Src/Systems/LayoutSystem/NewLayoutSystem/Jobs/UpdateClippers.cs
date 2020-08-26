using UIForia.Extensions;
using UIForia.Graphics;
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
    internal unsafe struct UpdateClippers : IJob {

        public DataList<Clipper>.Shared clipperList;
        public ElementTable<ClipInfo> clipInfoTable;
        public ElementTable<LayoutBoxInfo> layoutResultTable;
        public DataList<float2>.Shared intersectionResults;
        public DataList<AxisAlignedBounds2D>.Shared clipperBoundsList;
        
        public float screenWidth;
        public float screenHeight;
        
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

            clipperBoundsList.size = 0;
            // todo -- i think this is wrong. 1 for screen, 1 for never
            
            clipperBoundsList.Add(new AxisAlignedBounds2D(float.MinValue, float.MinValue, float.MaxValue, float.MaxValue));
            clipperBoundsList.Add(new AxisAlignedBounds2D(0, 0, screenWidth, screenHeight));
            
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

                ElementId parentElementId = clipperList[clipper.parentIndex].elementId;
                ref ClipInfo parentClipInfo = ref clipInfoTable[parentElementId];

                clipper.aabb = new AxisAlignedBounds2D();
                float xMin = float.MaxValue;
                float xMax = float.MinValue;
                float yMin = float.MaxValue;
                float yMax = float.MinValue;

                if (bounds.p0.x < xMin) xMin = bounds.p0.x;
                if (bounds.p1.x < xMin) xMin = bounds.p1.x;
                if (bounds.p2.x < xMin) xMin = bounds.p2.x;
                if (bounds.p3.x < xMin) xMin = bounds.p3.x;

                if (bounds.p0.x > xMax) xMax = bounds.p0.x;
                if (bounds.p1.x > xMax) xMax = bounds.p1.x;
                if (bounds.p2.x > xMax) xMax = bounds.p2.x;
                if (bounds.p3.x > xMax) xMax = bounds.p3.x;

                if (bounds.p0.y < yMin) yMin = bounds.p0.y;
                if (bounds.p1.y < yMin) yMin = bounds.p1.y;
                if (bounds.p2.y < yMin) yMin = bounds.p2.y;
                if (bounds.p3.y < yMin) yMin = bounds.p3.y;

                if (bounds.p0.y > yMax) yMax = bounds.p0.y;
                if (bounds.p1.y > yMax) yMax = bounds.p1.y;
                if (bounds.p2.y > yMax) yMax = bounds.p2.y;
                if (bounds.p3.y > yMax) yMax = bounds.p3.y;

                clipper.aabb = AxisAlignedBounds2D.Intersect(clipperList[clipper.parentIndex].aabb, new AxisAlignedBounds2D(xMin, yMin, xMax, yMax));
                if (clipper.aabb.Width <= 0 || clipper.aabb.Height <= 0) {
                    clipper.isCulled = true;
                    clipper.aabb = default;
                }
                clipperBoundsList.Add(clipper.aabb);
                // int rangeStart = intersectionResults.size;
                // outputBuffer.size = 0;
                //
                //
                // if (parentClipInfo.orientedBounds.ContainsPoint(bounds.p0) &&
                //     parentClipInfo.orientedBounds.ContainsPoint(bounds.p1) &&
                //     parentClipInfo.orientedBounds.ContainsPoint(bounds.p2) &&
                //     parentClipInfo.orientedBounds.ContainsPoint(bounds.p3)) {
                //     clipInfo.fullyContainedByParentClipper = true;
                //     clipInfo.isCulled = false;
                //     clipper.intersectionRange = new RangeInt(rangeStart, 4);
                //     intersectionResults.EnsureAdditionalCapacity(4);
                //     intersectionResults.Add(bounds.p0);
                //     intersectionResults.Add(bounds.p1);
                //     intersectionResults.Add(bounds.p2);
                //     intersectionResults.Add(bounds.p3);
                //     
                //     clipper.aabb = PolygonUtil.GetBounds2D(intersectionResults.GetArrayPointer() + clipper.intersectionRange.start, clipper.intersectionRange.length);
                //
                //     clipperBoundsList.Add(clipper.aabb);
                //     continue;
                // }
                //    
                // RangeInt parentIntersectionRange = clipperList[clipper.parentIndex].intersectionRange;
                //
                // SutherlandHodgman.GetIntersectedPolygon(
                //     subjectBuffer,
                //     intersectionResults.GetArrayPointer() + parentIntersectionRange.start,
                //     parentIntersectionRange.length,
                //     ref outputBuffer,
                //     ref pointBuffer,
                //     ref edgeBuffer
                // );
                //
                // if (outputBuffer.size > 0) {
                //     intersectionResults.EnsureAdditionalCapacity(outputBuffer.size);
                //     TypedUnsafe.MemCpy(intersectionResults.GetArrayPointer() + intersectionResults.size, outputBuffer.GetArrayPointer(), outputBuffer.size);
                //     
                //     // if (outputBuffer.size == 3) {
                //     //     // if the clip output is a triangle 
                //     //     clipper.overflowBoundsIndex = clipperBoundsList.size;
                //     //     clipperBoundsList.Add(new OverflowBounds() {
                //     //         p0 = outputBuffer[0],
                //     //         p1 = outputBuffer[1],
                //     //         p2 = outputBuffer[2],
                //     //         p3 = outputBuffer[1],
                //     //     });
                //     // }
                //     // else if (outputBuffer.size == 4) {
                //     //     clipper.overflowBoundsIndex = clipperBoundsList.size;
                //     //     clipperBoundsList.Add(new OverflowBounds() {
                //     //         p0 = outputBuffer[0],
                //     //         p1 = outputBuffer[1],
                //     //         p2 = outputBuffer[2],
                //     //         p3 = outputBuffer[3],
                //     //     });
                //     // }
                // }
                //
                // clipper.intersectionRange = new RangeInt(rangeStart, outputBuffer.size);
                // clipper.isCulled = clipper.intersectionRange.length <= 0;
                // if (clipper.isCulled) {
                //     clipper.aabb = default;
                //     continue;
                // }
                //
                // clipper.aabb = PolygonUtil.GetBounds2D(intersectionResults.GetArrayPointer() + clipper.intersectionRange.start, clipper.intersectionRange.length);

            }

            pointBuffer.Dispose();
            edgeBuffer.Dispose();
            subjectBuffer.Dispose();
        }

    }

}