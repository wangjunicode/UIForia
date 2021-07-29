using UIForia.Style;
using UIForia.Util;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace UIForia.Layout {

    [BurstCompile(DisableSafetyChecks = UIApplication.DisableJobSafety)]
    internal unsafe struct SolveBorderSize : IJob {

        [NativeDisableUnsafePtrRestriction] public LayoutTree* layoutTree;
        [NativeDisableUnsafePtrRestriction] public StyleTables* styleTables;

        [NativeDisableUnsafePtrRestriction] public float** emTablePtr;

        [NativeDisableUnsafePtrRestriction] public float** borderTopPtr;
        [NativeDisableUnsafePtrRestriction] public float** borderRightPtr;
        [NativeDisableUnsafePtrRestriction] public float** borderBottomPtr;
        [NativeDisableUnsafePtrRestriction] public float** borderLeftPtr;

        public void Execute() {

            float* emTable = *emTablePtr;

            CheckedArray<float> borderTops = new CheckedArray<float>(*borderTopPtr, layoutTree->elementCount);
            CheckedArray<float> borderRights = new CheckedArray<float>(*borderRightPtr, layoutTree->elementCount);
            CheckedArray<float> borderBottoms = new CheckedArray<float>(*borderBottomPtr, layoutTree->elementCount);
            CheckedArray<float> borderLefts = new CheckedArray<float>(*borderLeftPtr, layoutTree->elementCount);

            for (int i = 0; i < layoutTree->elementCount; i++) {

                float emSize = emTable[i];
                ElementId elementId = layoutTree->elementIdList[i];

                UIFixedLength borderTop = styleTables->BorderTop[elementId.index];
                UIFixedLength borderRight = styleTables->BorderRight[elementId.index];
                UIFixedLength borderBottom = styleTables->BorderBottom[elementId.index];
                UIFixedLength borderLeft = styleTables->BorderLeft[elementId.index];

                borderTops[i] = ResolveOffsetLength(borderTop, emSize);
                borderRights[i] = ResolveOffsetLength(borderRight, emSize);
                borderBottoms[i] = ResolveOffsetLength(borderBottom, emSize);
                borderLefts[i] = ResolveOffsetLength(borderLeft, emSize);
            }

        }

        private static float ResolveOffsetLength(UIFixedLength length, float emSize) {
            return length.unit != UIFixedUnit.Em ? length.value : length.value * emSize;
        }

    }

}