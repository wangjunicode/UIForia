using System.Diagnostics;
using UIForia.Graphics;
using UIForia.Layout;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace UIForia.Systems {

    [BurstCompile]
    public struct ComputeElementCulling : IJob, IVertigoParallel {

        public DataList<ElementId>.Shared elementList;
        public ElementTable<ClipInfo> clipInfoTable;
        public DataList<Clipper>.Shared clipperList;
        public ElementTable<LayoutBoxInfo> layoutResultTable;

        public ParallelParams parallel { get; set; }

        // definitely culled = clipper culled || aabb doesnt overlap clipper aabb
        // let the gpu handle per pixel clipping, this is just a broadphase

        public void Execute() {
            Run(0, elementList.size);
        }

        public void Execute(int startIndex, int count) {
            Run(startIndex, startIndex + count);
        }

        private void Run(int start, int end) {

            for (int i = start; i < end; i++) {
                ElementId elementId = elementList[i];
                ref ClipInfo clipInfo = ref clipInfoTable[elementId];
                ref Clipper clipper = ref clipperList[clipInfo.clipperIndex];
                ref LayoutBoxInfo layoutResult = ref layoutResultTable[elementId];

                if (clipper.isCulled || layoutResult.actualSize.x * layoutResult.actualSize.y == 0) {
                    layoutResult.isCulled = true;
                    clipInfo.isCulled = true;
                }
                else {

                    ref OrientedBounds orientedBounds = ref clipInfo.orientedBounds;

                    float xMin = float.MaxValue;
                    float xMax = float.MinValue;
                    float yMin = float.MaxValue;
                    float yMax = float.MinValue;

                    if (orientedBounds.p0.x < xMin) xMin = orientedBounds.p0.x;
                    if (orientedBounds.p1.x < xMin) xMin = orientedBounds.p1.x;
                    if (orientedBounds.p2.x < xMin) xMin = orientedBounds.p2.x;
                    if (orientedBounds.p3.x < xMin) xMin = orientedBounds.p3.x;

                    if (orientedBounds.p0.x > xMax) xMax = orientedBounds.p0.x;
                    if (orientedBounds.p1.x > xMax) xMax = orientedBounds.p1.x;
                    if (orientedBounds.p2.x > xMax) xMax = orientedBounds.p2.x;
                    if (orientedBounds.p3.x > xMax) xMax = orientedBounds.p3.x;

                    if (orientedBounds.p0.y < yMin) yMin = orientedBounds.p0.y;
                    if (orientedBounds.p1.y < yMin) yMin = orientedBounds.p1.y;
                    if (orientedBounds.p2.y < yMin) yMin = orientedBounds.p2.y;
                    if (orientedBounds.p3.y < yMin) yMin = orientedBounds.p3.y;

                    if (orientedBounds.p0.y > yMax) yMax = orientedBounds.p0.y;
                    if (orientedBounds.p1.y > yMax) yMax = orientedBounds.p1.y;
                    if (orientedBounds.p2.y > yMax) yMax = orientedBounds.p2.y;
                    if (orientedBounds.p3.y > yMax) yMax = orientedBounds.p3.y;

                    // todo -- store aligned bounds 
                    
                    bool overlappingOrContains = xMax >= clipper.aabb.x && xMin <= clipper.aabb.z && yMax >= clipper.aabb.y && yMin <= clipper.aabb.w;

                    layoutResult.isCulled = !overlappingOrContains;
                    clipInfo.isCulled = !overlappingOrContains;
                    clipInfo.aabb = new AxisAlignedBounds2D(xMin, yMin, xMax, yMax);

                }

            }

        }

    }

}