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
    internal unsafe struct SolveHorizontalSpacingSizes : IJob {

        public float appWidth;
        public float appHeight;
        public CheckedArray<Rect> viewRects;
        public CheckedArray<ushort> elementIdToViewId;

        [NativeDisableUnsafePtrRestriction] public float** emTablePtr;
        [NativeDisableUnsafePtrRestriction] public LayoutTree* layoutTree;
        [NativeDisableUnsafePtrRestriction] public StyleTables* styleTables;

        [NativeDisableUnsafePtrRestriction] public ResolvedSpacerSize** marginLeftPtr;
        [NativeDisableUnsafePtrRestriction] public ResolvedSpacerSize** marginRightPtr;

        [NativeDisableUnsafePtrRestriction] public ResolvedSpacerSize** paddingLeftPtr;
        [NativeDisableUnsafePtrRestriction] public ResolvedSpacerSize** paddingRightPtr;

        [NativeDisableUnsafePtrRestriction] public ResolvedSpacerSize** spaceBetweenPtr;

        [NativeDisableUnsafePtrRestriction] public PropertyTables* propertyTables;

        [NativeDisableUnsafePtrRestriction] public PropertySolverGroup_HorizontalSpacing* solverGroup;
        [NativeDisableUnsafePtrRestriction] public SolverParameters solverParameters;
        [NativeDisableUnsafePtrRestriction] public SpaceCollapse** spaceCollapsePtr;

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
            CheckedArray<ResolvedSpacerSize> marginRights = new CheckedArray<ResolvedSpacerSize>(*marginRightPtr, layoutTree->elementCount);
            CheckedArray<ResolvedSpacerSize> marginLefts = new CheckedArray<ResolvedSpacerSize>(*marginLeftPtr, layoutTree->elementCount);

            CheckedArray<ResolvedSpacerSize> paddingRights = new CheckedArray<ResolvedSpacerSize>(*paddingRightPtr, layoutTree->elementCount);
            CheckedArray<ResolvedSpacerSize> paddingLefts = new CheckedArray<ResolvedSpacerSize>(*paddingLeftPtr, layoutTree->elementCount);

            // todo -- likely can keep these as temp allocs
            CheckedArray<ResolvedSpacerSize> spaceBetweens = new CheckedArray<ResolvedSpacerSize>(*spaceBetweenPtr, layoutTree->elementCount);
            CheckedArray<SpaceCollapse> spaceCollapse = new CheckedArray<SpaceCollapse>(*spaceCollapsePtr, layoutTree->elementCount);

            for (int i = 0; i < layoutTree->elementCount; i++) {
                float emSize = emTable[i];
                ElementId elementId = layoutTree->nodeList[i].elementId;
                int viewIndex = elementIdToViewId[elementId.index];

                marginRights[i] = MeasurementUtil.ResolveSpacerSize(viewRects, appWidth, appHeight, viewIndex, styleTables->MarginRight[elementId.index], emSize);
                marginLefts[i] = MeasurementUtil.ResolveSpacerSize(viewRects, appWidth, appHeight, viewIndex, styleTables->MarginLeft[elementId.index], emSize);
                spaceBetweens[i] = MeasurementUtil.ResolveSpacerSize(viewRects, appWidth, appHeight, viewIndex, styleTables->SpaceBetweenHorizontal[elementId.index], emSize);

            }

            for (int i = 0; i < layoutTree->elementCount; i++) {

                float emSize = emTable[i];
                ElementId elementId = layoutTree->nodeList[i].elementId;
                int viewIndex = elementIdToViewId[elementId.index];

                paddingRights[i] = MeasurementUtil.ResolveSpacerSize(viewRects, appWidth, appHeight, viewIndex, styleTables->PaddingRight[elementId.index], emSize);
                paddingLefts[i] = MeasurementUtil.ResolveSpacerSize(viewRects, appWidth, appHeight, viewIndex, styleTables->PaddingLeft[elementId.index], emSize);
                spaceCollapse[i] = styleTables->CollapseSpaceHorizontal[elementId.index];

            }

            for (int i = 0; i < layoutTree->elementCount; i++) {
                ref ResolvedSpacerSize marginRight = ref marginRights.Get(i);
                if (marginRight.value < 0) marginRight.value = 0;
                if (marginRight.stretch < 0) marginRight.stretch = 0;
            }

            for (int i = 0; i < layoutTree->elementCount; i++) {
                ref ResolvedSpacerSize marginLeft = ref marginLefts.Get(i);
                if (marginLeft.value < 0) marginLeft.value = 0;
                if (marginLeft.stretch < 0) marginLeft.stretch = 0;
            }

            LayoutDepthLevel lastDepthLevel = layoutTree->depthLevels[layoutTree->depthLevels.size - (layoutTree->depthLevels.size <= 1 ? 1 : 2)];
            int nonLeafNodeTotal = lastDepthLevel.nodeRange.end + lastDepthLevel.ignoredRange.length;
            LayoutInfo2* layoutBuffer = TypedUnsafe.Malloc<LayoutInfo2>(nonLeafNodeTotal, Allocator.Temp); // todo -- this feels fishy

            TempList<SolveSizeUtil.LayoutTypeInfoList> layoutTypeLists = TypedUnsafe.MallocSizedTempList<SolveSizeUtil.LayoutTypeInfoList>((int) LayoutBoxType.__COUNT__, Allocator.Temp);

            SolveSizeUtil.GatherBoxesForSpacingAndCollapse(layoutTree, ref layoutTypeLists, layoutBuffer, new RangeInt(0, nonLeafNodeTotal));

            MarginCollapseInfo marginCollapseInfo = new MarginCollapseInfo() {
                marginStart = marginLefts,
                marginEnd = marginRights,
                paddingStart = paddingLefts,
                paddingEnd = paddingRights,
                betweenSpacer = spaceBetweens,
                spaceCollapse = spaceCollapse
            };

            // flex margin collapse handled here because we have no other data dependencies unlike text & grid & others
            FlexLayout.HandleMainAxisMarginCollapse(ref marginCollapseInfo, layoutTypeLists[(int) LayoutBoxType.FlexHorizontal].ToCheckedArray());
            FlexLayout.HandleCrossAxisMarginCollapse(ref marginCollapseInfo, layoutTypeLists[(int) LayoutBoxType.FlexVertical].ToCheckedArray());

            // stack margin collapse is the same as flex cross axis but on both axes
            FlexLayout.HandleCrossAxisMarginCollapse(ref marginCollapseInfo, layoutTypeLists[(int) LayoutBoxType.Stack].ToCheckedArray());

            IgnoredLayout.CollapseIgnoredMargins(layoutTree, marginCollapseInfo);

            TypedUnsafe.Dispose(layoutBuffer, Allocator.Temp);
            layoutTypeLists.Dispose();
        }


    }

}