using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace UIForia.Layout {

    [BurstCompile(DisableSafetyChecks = UIApplication.DisableJobSafety)]
    internal unsafe struct ComputeMatrices : IJob {

        [NativeDisableUnsafePtrRestriction] public LayoutTree* layoutTree;
        public CheckedArray<float2> localPositions;
        public CheckedArray<float4x4> worldMatrices;
        public CheckedArray<float4x4> localMatrices;
        public CheckedArray<Rect> viewRects;

        public void Execute() {

            for (int i = 0; i < layoutTree->elementCount; i++) {
                
                float px = 0;
                float py = 0;
                float rotation = 0;
                float scaleX = 1;
                float scaleY = 1;

                float2 position = localPositions[i];
                float4x4 final = float4x4.Translate(new float3(position.x, -position.y, 0));
                final = math.mul(final, float4x4.Translate(new float3(px, -py, 0)));
                final = math.mul(final, float4x4.RotateZ(rotation));
                final = math.mul(final, float4x4.Scale(new float3(scaleX, scaleY, 1)));
                localMatrices[i] = math.mul(final, float4x4.Translate(new float3(-px, py, 0)));

            }

            RangeInt rootRange = layoutTree->depthLevels[0].nodeRange;

            int viewIdx = 0;
            for (int i = rootRange.start; i < rootRange.end; i++) {
                Rect viewRect = viewRects[viewIdx++];
                
                float4x4 viewMat = float4x4.Translate(new float3(viewRect.x, -viewRect.y, 0));
                
                worldMatrices[i] = math.mul(viewMat, localMatrices[i]);
            }

            for (int d = 1; d < layoutTree->depthLevels.size; d++) {

                RangeInt range = layoutTree->depthLevels[d].nodeRange;
                range.length += layoutTree->depthLevels[d].ignoredRange.length;

                for (int i = range.start; i < range.end; i++) {
                    int parentIndex = layoutTree->nodeList[i].parentIndex;
                    ref float4x4 localMatrix = ref localMatrices.array[i];
                    ref float4x4 parentWorldMatrix = ref worldMatrices.array[parentIndex];
                    worldMatrices[i] = math.mul(parentWorldMatrix, localMatrix);
                }

            }

        }

    }

}