using UIForia.Style;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace UIForia.Layout {

    [BurstCompile(DisableSafetyChecks = UIApplication.DisableJobSafety)]
    internal unsafe struct SolveVerticalSpacingSizes : IJob {

        public float appWidth;
        public float appHeight;
        public CheckedArray<Rect> viewRects;
        public CheckedArray<ushort> elementIdToViewId;

        [NativeDisableUnsafePtrRestriction] public float** emTablePtr;
        [NativeDisableUnsafePtrRestriction] public LayoutTree* layoutTree;
        [NativeDisableUnsafePtrRestriction] public StyleTables* styleTables;

        [NativeDisableUnsafePtrRestriction] public ResolvedSpacerSize** marginTopPtr;
        [NativeDisableUnsafePtrRestriction] public ResolvedSpacerSize** marginBottomPtr;

        [NativeDisableUnsafePtrRestriction] public ResolvedSpacerSize** paddingTopPtr;
        [NativeDisableUnsafePtrRestriction] public ResolvedSpacerSize** paddingBottomPtr;
        [NativeDisableUnsafePtrRestriction] public SpaceCollapse** spaceCollapsePtr;

        [NativeDisableUnsafePtrRestriction] public ResolvedSpacerSize** spaceBetweenPtr;

        [NativeDisableUnsafePtrRestriction] public PropertyTables* propertyTables;

        [NativeDisableUnsafePtrRestriction] public PropertySolverGroup_VerticalSpacing* solverGroup;
        [NativeDisableUnsafePtrRestriction] public SolverParameters solverParameters;

        public void Execute() {
            // job is small enough that we can just use the temp allocator 
            // todo -- find a better metric than consuming all of temp 
            BumpAllocator bumpAllocator = new BumpAllocator(TypedUnsafe.Kilobytes(15), Allocator.Temp);
            solverGroup->Invoke(solverParameters, styleTables, propertyTables, &bumpAllocator);
            bumpAllocator.Dispose();
            Run();
        }

        private void Run() {

            CheckedArray<float> emTable = new CheckedArray<float>(*emTablePtr, layoutTree->elementCount);
            CheckedArray<ResolvedSpacerSize> marginTops = new CheckedArray<ResolvedSpacerSize>(*marginTopPtr, layoutTree->elementCount);
            CheckedArray<ResolvedSpacerSize> marginBottoms = new CheckedArray<ResolvedSpacerSize>(*marginBottomPtr, layoutTree->elementCount);

            CheckedArray<ResolvedSpacerSize> paddingTops = new CheckedArray<ResolvedSpacerSize>(*paddingTopPtr, layoutTree->elementCount);
            CheckedArray<ResolvedSpacerSize> paddingBottoms = new CheckedArray<ResolvedSpacerSize>(*paddingBottomPtr, layoutTree->elementCount);

            CheckedArray<ResolvedSpacerSize> spaceBetweens = new CheckedArray<ResolvedSpacerSize>(*spaceBetweenPtr, layoutTree->elementCount);
            CheckedArray<SpaceCollapse> spaceCollapse = new CheckedArray<SpaceCollapse>(*spaceCollapsePtr, layoutTree->elementCount);

            for (int i = 0; i < layoutTree->elementCount; i++) {
                float emSize = emTable[i];
                ElementId elementId = layoutTree->nodeList[i].elementId;
                int viewIndex = elementIdToViewId[elementId.index];

                marginTops[i] = MeasurementUtil.ResolveSpacerSize(viewRects, appWidth, appHeight, viewIndex, styleTables->MarginTop[elementId.index], emSize);
                marginBottoms[i] = MeasurementUtil.ResolveSpacerSize(viewRects, appWidth, appHeight, viewIndex, styleTables->MarginBottom[elementId.index], emSize);
                spaceBetweens[i] = MeasurementUtil.ResolveSpacerSize(viewRects, appWidth, appHeight, viewIndex, styleTables->SpaceBetweenVertical[elementId.index], emSize);

            }

            for (int i = 0; i < layoutTree->elementCount; i++) {

                float emSize = emTable[i];
                ElementId elementId = layoutTree->nodeList[i].elementId;
                int viewIndex =  elementIdToViewId[elementId.index];

                paddingTops[i] = MeasurementUtil.ResolveSpacerSize(viewRects, appWidth, appHeight, viewIndex, styleTables->PaddingTop[elementId.index], emSize);
                paddingBottoms[i] = MeasurementUtil.ResolveSpacerSize(viewRects, appWidth, appHeight, viewIndex, styleTables->PaddingBottom[elementId.index], emSize);
                spaceCollapse[i] = styleTables->CollapseSpaceVertical[elementId.index];

            }

            for (int i = 0; i < layoutTree->elementCount; i++) {
                ref ResolvedSpacerSize marginTop = ref marginTops.Get(i);
                if (marginTop.value < 0) marginTop.value = 0;
                if (marginTop.stretch < 0) marginTop.stretch = 0;
            }

            for (int i = 0; i < layoutTree->elementCount; i++) {
                ref ResolvedSpacerSize marginBottom = ref marginBottoms.Get(i);
                if (marginBottom.value < 0) marginBottom.value = 0;
                if (marginBottom.stretch < 0) marginBottom.stretch = 0;
            }

            LayoutDepthLevel lastDepthLevel = layoutTree->depthLevels[layoutTree->depthLevels.size - (layoutTree->depthLevels.size <= 1 ? 1 : 2)];
            int nonLeafNodeTotal = lastDepthLevel.nodeRange.end + lastDepthLevel.ignoredRange.length;

            LayoutInfo2* layoutBuffer = TypedUnsafe.Malloc<LayoutInfo2>(nonLeafNodeTotal, Allocator.Temp);

            TempList<SolveSizeUtil.LayoutTypeInfoList> layoutTypeLists = TypedUnsafe.MallocSizedTempList<SolveSizeUtil.LayoutTypeInfoList>((int) LayoutBoxType.__COUNT__, Allocator.Temp);

            SolveSizeUtil.GatherBoxesForSpacingAndCollapse(layoutTree, ref layoutTypeLists, layoutBuffer, new RangeInt(0, nonLeafNodeTotal));

            MarginCollapseInfo marginCollapseInfo = new MarginCollapseInfo() {
                marginStart = marginTops,
                marginEnd = marginBottoms,
                paddingStart = paddingTops,
                paddingEnd = paddingBottoms,
                betweenSpacer = spaceBetweens,
                spaceCollapse = spaceCollapse
            };
            
            FlexLayout.HandleMainAxisMarginCollapse(ref marginCollapseInfo, layoutTypeLists[(int) LayoutBoxType.FlexVertical].ToCheckedArray());
            FlexLayout.HandleCrossAxisMarginCollapse(ref marginCollapseInfo, layoutTypeLists[(int) LayoutBoxType.FlexHorizontal].ToCheckedArray());
            
            // stack margin collapse is the same as flex cross axis but on both axes
            FlexLayout.HandleCrossAxisMarginCollapse(ref marginCollapseInfo, layoutTypeLists[(int) LayoutBoxType.Stack].ToCheckedArray());
            
            IgnoredLayout.CollapseIgnoredMargins(layoutTree, marginCollapseInfo);

            TypedUnsafe.Dispose(layoutBuffer, Allocator.Temp);
            layoutTypeLists.Dispose();

        }

    }

}