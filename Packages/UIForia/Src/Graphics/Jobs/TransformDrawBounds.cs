using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;

namespace UIForia.Graphics {

    // I ended up moving this work into ProcessClipping since it was a requirement for that anyway and this job shouldnt take much time compared to the scheduling overhead 
    
    // [BurstCompile]
    // internal unsafe struct TransformDrawBounds : IJob {
    //
    //     public DataList<DrawInfo2>.Shared drawList;
    //     public DataList<AxisAlignedBounds2D> transformedBounds;
    //
    //     public void Execute() {
    //
    //         for (int i = 0; i < drawList.size; i++) {
    //
    //             // note: assumes all drawInfos have a valid matrix pointer, even if its bogus
    //             
    //             ref DrawInfo2 drawInfo = ref drawList[i];
    //             AxisAlignedBounds2D aabb = drawInfo.localBounds;
    //
    //             float3 p0 = math.transform(*drawInfo.matrix, new float3(aabb.xMin, aabb.yMin, 0));
    //             float3 p1 = math.transform(*drawInfo.matrix, new float3(aabb.xMax, aabb.yMin, 0));
    //             float3 p2 = math.transform(*drawInfo.matrix, new float3(aabb.xMax, aabb.yMax, 0));
    //             float3 p3 = math.transform(*drawInfo.matrix, new float3(aabb.xMin, aabb.yMax, 0));
    //
    //             float xMin = float.MaxValue;
    //             float xMax = float.MinValue;
    //             float yMin = float.MaxValue;
    //             float yMax = float.MinValue;
    //
    //             if (p0.x < xMin) xMin = p0.x;
    //             if (p1.x < xMin) xMin = p1.x;
    //             if (p2.x < xMin) xMin = p2.x;
    //             if (p3.x < xMin) xMin = p3.x;
    //
    //             if (p0.x > xMax) xMax = p0.x;
    //             if (p1.x > xMax) xMax = p1.x;
    //             if (p2.x > xMax) xMax = p2.x;
    //             if (p3.x > xMax) xMax = p3.x;
    //
    //             if (p0.y < yMin) yMin = p0.y;
    //             if (p1.y < yMin) yMin = p1.y;
    //             if (p2.y < yMin) yMin = p2.y;
    //             if (p3.y < yMin) yMin = p3.y;
    //
    //             if (p0.y > yMax) yMax = p0.y;
    //             if (p1.y > yMax) yMax = p1.y;
    //             if (p2.y > yMax) yMax = p2.y;
    //             if (p3.y > yMax) yMax = p3.y;
    //
    //             transformedBounds[i] = new AxisAlignedBounds2D(xMin, yMin, xMax, yMax);
    //
    //         }
    //
    //     }
    //
    // }

}