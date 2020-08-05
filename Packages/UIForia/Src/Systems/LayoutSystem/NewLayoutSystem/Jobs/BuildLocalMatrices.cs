using System.Diagnostics;
using SVGX;
using UIForia.Systems;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Debug = UnityEngine.Debug;

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
            // todo -- burst version is acting strange
            if (false && isBurstActive == 1) {
                for (int i = start; i < end; i++) {
                    ElementId elementId = elementList[i];
                    ElementId parentId = parentList[i];

                    ref LayoutBoxInfo layoutResult = ref layoutBoxInfoTable[elementId];
                    ref LayoutBoxInfo parentResult = ref layoutBoxInfoTable[parentId];
                    ref TransformInfo transformInfo = ref transformInfoTable[elementId];

                    float x = MeasurementUtil.ResolveTransformMeasurement(layoutResult, parentResult, viewParameters, transformInfo.positionX, layoutResult.actualSize.x);
                    float y = MeasurementUtil.ResolveTransformMeasurement(layoutResult, parentResult, viewParameters, transformInfo.positionY, layoutResult.actualSize.y);

                    float px = MeasurementUtil.ResolveTransformPivot(layoutResult.actualSize.x, viewParameters, layoutResult.emSize, transformInfo.pivotX);
                    float py = MeasurementUtil.ResolveTransformPivot(layoutResult.actualSize.y, viewParameters, layoutResult.emSize, transformInfo.pivotY);

                    float scaleX = transformInfo.scaleX;
                    float scaleY = transformInfo.scaleY;
                    
                    float rotation = transformInfo.rotation * Mathf.Deg2Rad;
                    px += layoutResult.alignedPosition.x;
                    py += layoutResult.alignedPosition.y;
                    float4x4 final = float4x4.Translate(new float3(x, y, 0));
                    final = math.mul(final, float4x4.Translate(new float3(px, -py, 0)));
                    final = math.mul(final, float4x4.RotateZ(rotation));
                    final = math.mul(final, float4x4.Scale(new float3(scaleX, scaleY, 1)));
                    localMatrices[elementId] = math.mul(final, float4x4.Translate(new float3(-px, py, 0)));

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
                // if (elementId.index == 4) {
                //     float angle = -20;
                //     SVGXMatrix m = SVGXMatrix.identity;
                //     m = m.Translate(layoutResult.alignedPosition.x, (layoutResult.alignedPosition.y));
                //     m = m.SkewX(angle);
                //     m.m4 += m.skewX * 2;
                //     localMatrices.array[elementId.index] = m.ToMatrix4x4();
                //     continue;
                // }

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

                float px = MeasurementUtil.ResolveTransformPivot(layoutResult.actualSize.x, viewParameters, layoutResult.emSize, transformInfo.pivotX);
                float py = MeasurementUtil.ResolveTransformPivot(layoutResult.actualSize.y, viewParameters, layoutResult.emSize, transformInfo.pivotY);

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

                    // todo -- when NOT using burst, these matrix operations are INCREDIBLY SLOW
                    // could cut these down to 2d if applicable

                    px += layoutResult.alignedPosition.x;
                    py += layoutResult.alignedPosition.y;
                    
                    float4x4 final = float4x4.Translate(new float3(x, y, 0));
                    final = math.mul(final, float4x4.Translate(new float3(px, -py, 0)));
                    
                    if (rotation != 0) {
                        final = math.mul(final, float4x4.RotateZ(rotation));
                    }

                    if (scaleX != 1 || scaleY != 1) {
                        final = math.mul(final, float4x4.Scale(new float3(scaleX, scaleY, 1)));
                    }

                    localMatrices[elementId] = math.mul(final, float4x4.Translate(new float3(-px, py, 0)));

                }

            }
        }

        [BurstDiscard]
        private static void CheckIsBurst(int* arg) {
            *arg = 0;
        }

    }

}