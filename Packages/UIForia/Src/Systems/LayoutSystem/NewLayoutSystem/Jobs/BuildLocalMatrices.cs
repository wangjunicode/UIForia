using UIForia.Systems;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace UIForia.Layout {

    [BurstCompile]
    internal unsafe struct BuildLocalMatrices : IJob, IVertigoParallel {

        public ViewParameters viewParameters;
        public ElementTable<float4x4> localMatrices;
        public ElementTable<LayoutBoxInfo> layoutBoxInfoTable;
        public ElementTable<TransformInfo> transformInfoTable;
        public DataList<ElementId>.Shared elementList;
        public DataList<ElementId>.Shared parentList;

        public ParallelParams parallel { get; set; }

        public void Execute() {
            Run(0, elementList.size);
        }

        public void Execute(int startIndex, int count) {
            Run(startIndex, startIndex + count);
        }

        private void Run(int start, int end) {

            int isBurstActive = 1;
            CheckIsBurst(&isBurstActive);
            if (isBurstActive == 1) {
                for (int i = start; i < end; i++) {
                    ElementId elementId = elementList[i];
                    ElementId parentId = parentList[i];

                    ref LayoutBoxInfo layoutResult = ref layoutBoxInfoTable[elementId];
                    ref LayoutBoxInfo parentResult = ref layoutBoxInfoTable[parentId];
                    ref TransformInfo transformInfo = ref transformInfoTable[elementId];

                    float x = MeasurementUtil.ResolveTransformMeasurement(layoutResult, parentResult, viewParameters, transformInfo.positionX, layoutResult.actualSize.x);
                    float y = MeasurementUtil.ResolveTransformMeasurement(layoutResult, parentResult, viewParameters, transformInfo.positionY, layoutResult.actualSize.y);

                    float px = MeasurementUtil.ResolveFixedLayoutSize(viewParameters, layoutResult.emSize, transformInfo.pivotX);
                    float py = MeasurementUtil.ResolveFixedLayoutSize(viewParameters, layoutResult.emSize, transformInfo.pivotY);

                    float rotation = transformInfo.rotation * Mathf.Deg2Rad;

                    math.sincos(rotation, out float s, out float c);

                    float scaleX = transformInfo.scaleX;
                    float scaleY = transformInfo.scaleY;

                    float4x4 local = new float4x4(
                        new float4(c * scaleX, -s * scaleX, 0, 0),
                        new float4(s * scaleY, c * scaleY, 0, 0),
                        new float4(0, 0, 1, 0),
                        new float4(x + layoutResult.alignedPosition.x, -(y + layoutResult.alignedPosition.y), 0, 1)
                    );

                    // todo -- i have no idea how to get rotation around a pivot working
                    localMatrices[elementId] = local; // math.mul(pivot, math.mul(local, math.inverse(pivot)));

                }
            }
            else {
                NonBurstExecute(start, end);
            }
        }

        // when NOT using burst, these matrix operations are INCREDIBLY SLOW, I shaved ~15ms off of 3000 elements by inlining this for mono
        // trying really hard here to squeak every bit of performance that I can out of this since this is the one of the most expensive parts of layout 
        // but only in managed. burst makes this basically free.
        private void NonBurstExecute(int start, int end) {
            ElementId* idArray = (ElementId*) elementList.state->array;
            for (int i = start; i < end; i++) {

                ElementId elementId = idArray[i];

                ref TransformInfo transformInfo = ref transformInfoTable.array[elementId.index];
                ref LayoutBoxInfo layoutResult = ref layoutBoxInfoTable.array[elementId.index];

                if (transformInfo.positionX == 0 && transformInfo.positionY == 0 && transformInfo.rotation == 0 && transformInfo.scaleX == 1 && transformInfo.scaleY == 1) {
                    localMatrices.array[elementId.index] = new float4x4(
                        new float4(1, 0, 0, 0),
                        new float4(0, 1, 0, 0),
                        new float4(0, 0, 1, 0),
                        new float4(layoutResult.alignedPosition.x, -(layoutResult.alignedPosition.y), 0, 1)
                    );
                    continue;
                }

                ElementId parentId = parentList[i];

                ref LayoutBoxInfo parentResult = ref layoutBoxInfoTable.array[parentId.index];

                float x = MeasurementUtil.ResolveTransformMeasurement(layoutResult, parentResult, viewParameters, transformInfo.positionX, layoutResult.actualSize.x);
                float y = MeasurementUtil.ResolveTransformMeasurement(layoutResult, parentResult, viewParameters, transformInfo.positionY, layoutResult.actualSize.y);

                float px = MeasurementUtil.ResolveFixedLayoutSize(viewParameters, layoutResult.emSize, transformInfo.pivotX);
                float py = MeasurementUtil.ResolveFixedLayoutSize(viewParameters, layoutResult.emSize, transformInfo.pivotY);

                float rotation = transformInfo.rotation * Mathf.Deg2Rad;

                float scaleX = transformInfo.scaleX;
                float scaleY = transformInfo.scaleY;

                if (rotation == 0 && scaleX == 1 && scaleY == 1) {
                    localMatrices.array[elementId.index] = new float4x4(
                        new float4(scaleX, 0, 0, 0),
                        new float4(0, scaleY, 0, 0),
                        new float4(0, 0, 1, 0),
                        new float4(x + layoutResult.alignedPosition.x, -(y + layoutResult.alignedPosition.y), 0, 1)
                    );
                }
                else {
                    math.sincos(rotation, out float s, out float c);
                    // todo -- when NOT using burst, these matrix operations are INCREDIBLY SLOW
                    float4x4 pivot = float4x4.Translate(new float3(px, py, 0));

                    float4x4 localMatrix = new float4x4(
                        new float4(c * scaleX, -s * scaleX, 0, 0),
                        new float4(s * scaleY, c * scaleY, 0, 0),
                        new float4(0, 0, 1, 0),
                        new float4(x + layoutResult.alignedPosition.x, -(y + layoutResult.alignedPosition.y), 0, 1)
                    );

                    float4x4 inversePivot = math.inverse(pivot);

                    float4x4 localxPivot = new float4x4(
                        localMatrix.c0 * pivot.c0.x + localMatrix.c1 * pivot.c0.y + localMatrix.c2 * pivot.c0.z + localMatrix.c3 * pivot.c0.w,
                        localMatrix.c0 * pivot.c1.x + localMatrix.c1 * pivot.c1.y + localMatrix.c2 * pivot.c1.z + localMatrix.c3 * pivot.c1.w,
                        localMatrix.c0 * pivot.c2.x + localMatrix.c1 * pivot.c2.y + localMatrix.c2 * pivot.c2.z + localMatrix.c3 * pivot.c2.w,
                        localMatrix.c0 * pivot.c3.x + localMatrix.c1 * pivot.c3.y + localMatrix.c2 * pivot.c3.z + localMatrix.c3 * pivot.c3.w
                    );

                    localMatrices[elementId] = new float4x4(
                        inversePivot.c0 * localxPivot.c0.x + inversePivot.c1 * localxPivot.c0.y + inversePivot.c2 * localxPivot.c0.z + inversePivot.c3 * localxPivot.c0.w,
                        inversePivot.c0 * localxPivot.c1.x + inversePivot.c1 * localxPivot.c1.y + inversePivot.c2 * localxPivot.c1.z + inversePivot.c3 * localxPivot.c1.w,
                        inversePivot.c0 * localxPivot.c2.x + inversePivot.c1 * localxPivot.c2.y + inversePivot.c2 * localxPivot.c2.z + inversePivot.c3 * localxPivot.c2.w,
                        inversePivot.c0 * localxPivot.c3.x + inversePivot.c1 * localxPivot.c3.y + inversePivot.c2 * localxPivot.c3.z + inversePivot.c3 * localxPivot.c3.w
                    );
                }

            }
        }

        [BurstDiscard]
        private static void CheckIsBurst(int* arg) {
            *arg = 0;
        }

    }

}