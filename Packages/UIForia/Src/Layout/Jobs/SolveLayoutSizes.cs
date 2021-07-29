using System;
using UIForia.Style;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace UIForia.Layout {

    [BurstCompile(DisableSafetyChecks = UIApplication.DisableJobSafety)]
    internal unsafe struct SolveLayoutSizes : IJob {

        public float appWidth;
        public float appHeight;

        public CheckedArray<Rect> viewRects;
        public CheckedArray<ushort> elementIdToViewId;
        public CheckedArray<TemplateInfo> templateInfo;

        [NativeDisableUnsafePtrRestriction] public float** emTablePtr;
        [NativeDisableUnsafePtrRestriction] public float** lineHeightTablePtr;

        // todo -- convert these to checked arrays 
        [NativeDisableUnsafePtrRestriction] public SolvedSize** solvedWidthPtr;
        [NativeDisableUnsafePtrRestriction] public SolvedSize** solvedHeightPtr;
        [NativeDisableUnsafePtrRestriction] public SolvedConstraint** solvedMinWidthPtr;
        [NativeDisableUnsafePtrRestriction] public SolvedConstraint** solvedMaxWidthPtr;
        [NativeDisableUnsafePtrRestriction] public SolvedConstraint** solvedMinHeightPtr;
        [NativeDisableUnsafePtrRestriction] public SolvedConstraint** solvedMaxHeightPtr;

        [NativeDisableUnsafePtrRestriction] public LayoutTree* layoutTree;
        [NativeDisableUnsafePtrRestriction] public StyleTables* styleTables;
        [NativeDisableUnsafePtrRestriction] public PropertyTables* propertyTables;
        [NativeDisableUnsafePtrRestriction] public SolverParameters solverParameters;
        [NativeDisableUnsafePtrRestriction] public PropertySolverGroup_LayoutSizes* solverGroup;

        public void Execute() {
            // job is small enough that we can just use the temp allocator 
            // todo -- find a better metric than consuming all of temp 
            BumpAllocator bumpAllocator = new BumpAllocator(TypedUnsafe.Kilobytes(15), Allocator.Temp);
            solverGroup->Invoke(solverParameters, styleTables, propertyTables, &bumpAllocator);
            bumpAllocator.Dispose();
            Run();
        }

        private void Run() {

            // this job could be parallel since we never look at any parent data, order doesn't matter

            UIMeasurement* prefWidths = styleTables->PreferredWidth;
            UIMeasurement* prefHeights = styleTables->PreferredHeight;

            UISizeConstraint* minWidths = styleTables->MinWidth;
            UISizeConstraint* maxWidths = styleTables->MaxWidth;
            UISizeConstraint* minHeights = styleTables->MinHeight;
            UISizeConstraint* maxHeights = styleTables->MaxHeight;

            AspectRatio* aspectRatios = styleTables->AspectRatio;

            SolvedSize* solvedWidths = *solvedWidthPtr;
            SolvedSize* solvedHeights = *solvedHeightPtr;

            float* emTable = *emTablePtr;
            
            CheckedArray<float> lineHeightTable = new CheckedArray<float>(*lineHeightTablePtr, layoutTree->elementCount);
            
            int mapSize = LongBoolMap.GetMapSize(layoutTree->elementCount);

            ulong* mapBuffer = TypedUnsafe.MallocCleared<ulong>(mapSize * 4, Allocator.Temp);
            LongBoolMap horizontalParentBased = new LongBoolMap(mapBuffer + (mapSize * 0), mapSize);
            LongBoolMap horizontalChildBased = new LongBoolMap(mapBuffer + (mapSize * 1), mapSize);
            LongBoolMap verticalParentBased = new LongBoolMap(mapBuffer + (mapSize * 2), mapSize);
            LongBoolMap verticalChildBased = new LongBoolMap(mapBuffer + (mapSize * 3), mapSize);

            DataList<int> bgLookups = new DataList<int>(32, Allocator.Temp);
            DataList<int> viewLookups = new DataList<int>(32, Allocator.Temp);

            CheckedArray<SolvedConstraint> solvedMinWidths = new CheckedArray<SolvedConstraint>(*solvedMinWidthPtr, layoutTree->elementCount);
            CheckedArray<SolvedConstraint> solvedMaxWidths = new CheckedArray<SolvedConstraint>(*solvedMaxWidthPtr, layoutTree->elementCount);
            CheckedArray<SolvedConstraint> solvedMinHeights = new CheckedArray<SolvedConstraint>(*solvedMinHeightPtr, layoutTree->elementCount);
            CheckedArray<SolvedConstraint> solvedMaxHeights = new CheckedArray<SolvedConstraint>(*solvedMaxHeightPtr, layoutTree->elementCount);

            ResolveMeasurements(prefWidths, solvedWidths, emTable, lineHeightTable, ref bgLookups, ref viewLookups, horizontalParentBased, horizontalChildBased, PropertyId.PreferredWidth);
            ResolveConstraints(minWidths, *solvedMinWidthPtr, emTable, ref bgLookups, horizontalParentBased, horizontalChildBased);
            ResolveConstraints(maxWidths, *solvedMaxWidthPtr, emTable, ref bgLookups, horizontalParentBased, horizontalChildBased);
            CollapseParentChildParadoxes(solvedWidths, solvedMinWidths, solvedMaxWidths, horizontalParentBased, horizontalChildBased);

            ResolveMeasurements(prefHeights, solvedHeights, emTable, lineHeightTable, ref bgLookups, ref viewLookups, verticalParentBased, verticalChildBased, PropertyId.PreferredHeight);
            ResolveConstraints(minHeights, *solvedMinHeightPtr, emTable, ref bgLookups, verticalParentBased, verticalChildBased);
            ResolveConstraints(maxHeights, *solvedMaxHeightPtr, emTable, ref bgLookups, verticalParentBased, verticalChildBased);
            CollapseParentChildParadoxes(solvedHeights, solvedMinHeights, solvedMaxHeights, verticalParentBased, verticalChildBased);

            bgLookups.Dispose();
            viewLookups.Dispose();

            AdjustAspectRatios(aspectRatios, solvedWidths, solvedHeights);

            TypedUnsafe.Dispose(mapBuffer, Allocator.Temp);
        }

        private void AdjustAspectRatios(AspectRatio* aspectRatios, SolvedSize* solvedWidths, SolvedSize* solvedHeights) {
            for (int i = 0; i < layoutTree->elementCount; i++) {
                ElementId elementId = layoutTree->elementIdList[i];

                if (templateInfo[elementId.index].typeClass == ElementTypeClass.Text) {
                    // we ignore text aspect ratios 
                    continue;
                }

                AspectRatio aspect = aspectRatios[elementId.index];

                switch (aspect.mode) {

                    case AspectRatioMode.None:
                        break;

                    case AspectRatioMode.WidthControlsHeight: {
                        // todo -- detect if we are in a circle of doom 
                        if (solvedWidths[i].IsFixed) {
                            solvedHeights[i].unit = SolvedSizeUnit.Pixel;
                            solvedHeights[i].value = solvedWidths[i].value / aspect.Ratio;
                        }
                        else {
                            solvedHeights[i].unit = SolvedSizeUnit.Controlled;
                            solvedHeights[i].value = aspect.Ratio;
                        }

                        break;
                    }

                    case AspectRatioMode.HeightControlsWidth: {
                        if (solvedHeights[i].IsFixed) {
                            solvedWidths[i].unit = SolvedSizeUnit.Pixel;
                            solvedWidths[i].value = solvedHeights[i].value * aspect.Ratio;
                        }
                        else {
                            solvedWidths[i].unit = SolvedSizeUnit.Controlled;
                            solvedWidths[i].value = aspect.Ratio;
                        }

                        break;

                    }
                }

            }
        }

        private void CollapseParentChildParadoxes(SolvedSize* solvedSize, CheckedArray<SolvedConstraint> solvedMinSizes, CheckedArray<SolvedConstraint> solvedMaxSizes, LongBoolMap parentBased, LongBoolMap childBased) {

            for (int d = layoutTree->depthLevels.size - 1; d >= 1; d--) {

                RangeInt nodeRange = layoutTree->depthLevels[d].nodeRange; // todo -- ignored also?

                for (int i = nodeRange.start; i < nodeRange.end; i++) {
                    ref LayoutNode node = ref layoutTree->nodeList.Get(i);

                    if (!parentBased.Get(i) || !childBased.Get(node.parentIndex)) {
                        continue;
                    }

                    parentBased.Unset(i);

                    SolvedSizeUnit prefUnit = solvedSize[i].unit;
                    SolvedConstraintUnit minUnit = solvedMinSizes[i].unit;
                    SolvedConstraintUnit maxUnit = solvedMaxSizes[i].unit;

                    if (prefUnit == SolvedSizeUnit.ParentSize || prefUnit == SolvedSizeUnit.Percent) {
                        solvedSize[i] = new SolvedSize(0, SolvedSizeUnit.Pixel);
                    }

                    if (minUnit == SolvedConstraintUnit.ParentSize || minUnit == SolvedConstraintUnit.Percent) {
                        solvedMinSizes.array[i] = new SolvedConstraint(0, SolvedConstraintUnit.Pixel);
                    }

                    if (maxUnit == SolvedConstraintUnit.ParentSize || maxUnit == SolvedConstraintUnit.Percent) {
                        solvedMaxSizes.array[i] = new SolvedConstraint(float.MaxValue, SolvedConstraintUnit.Pixel);
                    }

                }

            }
        }

        private void ResolveConstraints(UISizeConstraint* constraints, SolvedConstraint* solved, float* emTable, ref DataList<int> bgLookups, LongBoolMap parentBased, LongBoolMap childBased) {
            bgLookups.size = 0;
            CheckedArray<float> lineHeightTable = new CheckedArray<float>(*lineHeightTablePtr, layoutTree->elementCount);

            for (int i = 0; i < layoutTree->elementCount; i++) {
                ElementId elementId = layoutTree->elementIdList[i];
                UISizeConstraint constraint = constraints[elementId.index];

                ref SolvedConstraint solvedConstraint = ref solved[i];
                solvedConstraint.unit = SolvedConstraintUnit.Pixel;

                switch (constraint.unit) {
                    // these are all directly convertible
                    case 0:
                    case UISizeConstraintUnit.Pixel:
                        solvedConstraint.unit = SolvedConstraintUnit.Pixel;
                        solvedConstraint.value = constraint.value;
                        break;

                    case UISizeConstraintUnit.Content:
                    case UISizeConstraintUnit.MaxChild:
                    case UISizeConstraintUnit.MinChild:
                        childBased.Set(i);
                        solvedConstraint.unit = (SolvedConstraintUnit) (int) constraint.unit;
                        solvedConstraint.value = constraint.value;
                        break;

                    case UISizeConstraintUnit.Em:
                        solvedConstraint.value = emTable[i] * constraint.value;
                        break;
                    case UISizeConstraintUnit.LineHeight:
                        solvedConstraint.value = lineHeightTable[i] * constraint.value;
                        break;

                    case UISizeConstraintUnit.BackgroundImageWidth:
                    case UISizeConstraintUnit.BackgroundImageHeight:
                        bgLookups.Add(i);
                        break;

                    case UISizeConstraintUnit.ApplicationWidth:
                        solvedConstraint.value = appWidth * constraint.value;
                        break;

                    case UISizeConstraintUnit.ApplicationHeight:
                        solvedConstraint.value = appHeight * constraint.value;
                        break;

                    case UISizeConstraintUnit.ViewportWidth:
                    case UISizeConstraintUnit.ViewportHeight: {
                        int viewIndex = elementIdToViewId[elementId.index];
                        if (constraint.unit == UISizeConstraintUnit.ViewportWidth) {
                            solvedConstraint.value = viewRects[viewIndex].width * constraint.value;
                            solvedConstraint.unit = SolvedConstraintUnit.Pixel;
                        }
                        else if (constraint.unit == UISizeConstraintUnit.ViewportHeight) {
                            solvedConstraint.value = viewRects[viewIndex].height * constraint.value;
                            solvedConstraint.unit = SolvedConstraintUnit.Pixel;
                        }

                        break;
                    }

                    case UISizeConstraintUnit.ParentSize:
                        parentBased.Set(i);
                        solvedConstraint.unit = SolvedConstraintUnit.ParentSize;
                        solvedConstraint.value = constraint.value;
                        break;

                    case UISizeConstraintUnit.Percent:
                        parentBased.Set(i);
                        solvedConstraint.unit = SolvedConstraintUnit.Percent;
                        solvedConstraint.value = constraint.value;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            for (int i = 0; i < layoutTree->elementCount; i++) {
                if (solved[i].value < 0) solved[i].value = 0;
            }

        }

        private void ResolveMeasurements(UIMeasurement* measurements, SolvedSize* solved, float* emTable, CheckedArray<float> lineHeightTable, ref DataList<int> bgLookups, ref DataList<int> viewLookups, LongBoolMap parentBased, LongBoolMap childBased, PropertyId propertyId) {
            bgLookups.size = 0;
            viewLookups.size = 0;
            for (int i = 0; i < layoutTree->elementCount; i++) {
                ElementId elementId = layoutTree->elementIdList[i];
                // todo -- profile breaking this data dependency 
                UIMeasurement measurement = measurements[elementId.index];

                ref SolvedSize solvedSize = ref solved[i];

                // is element animated?  -> measurement value interpreted as index into lerpSize buffer index

                switch (measurement.unit) {

                    case UIMeasurementUnit.ViewportWidth:
                    case UIMeasurementUnit.ViewportHeight: {
                        int viewIndex = elementIdToViewId[elementId.index];

                        if (measurement.unit == UIMeasurementUnit.ViewportWidth) {
                            solvedSize.value = viewRects[viewIndex].width * measurement.value;
                            solvedSize.unit = SolvedSizeUnit.Pixel;
                        }
                        else if (measurement.unit == UIMeasurementUnit.ViewportHeight) {
                            solvedSize.value = viewRects[viewIndex].height * measurement.value;
                            solvedSize.unit = SolvedSizeUnit.Pixel;
                        }

                        break;
                    }

                    case UIMeasurementUnit.FillRemaining:
                        solvedSize.value = measurement.value;
                        solvedSize.unit = SolvedSizeUnit.FillRemaining;
                        break;

                    case UIMeasurementUnit.LineHeight:
                        solved->unit = SolvedSizeUnit.Pixel;
                        solvedSize.value = lineHeightTable[i] * measurement.value;
                        solvedSize.unit = SolvedSizeUnit.Pixel;
                        break;
                    
                    case UIMeasurementUnit.Em:
                        solved->unit = SolvedSizeUnit.Pixel;
                        solvedSize.value = emTable[i] * measurement.value;
                        solvedSize.unit = SolvedSizeUnit.Pixel;
                        break;

                    case UIMeasurementUnit.BackgroundImageWidth:
                    case UIMeasurementUnit.BackgroundImageHeight:
                        bgLookups.Add(i); // todo -- I'm not sure these values are present yet since I need to solve bg textures and do a lookup and maybe also handle bg uvs?
                        break;

                    case UIMeasurementUnit.ApplicationWidth:
                        solvedSize.value = appWidth * measurement.value;
                        solvedSize.unit = SolvedSizeUnit.Pixel;
                        break;

                    case UIMeasurementUnit.ApplicationHeight:
                        solvedSize.value = appHeight * measurement.value;
                        solvedSize.unit = SolvedSizeUnit.Pixel;
                        break;

                    case UIMeasurementUnit.Percent:
                    case UIMeasurementUnit.ParentSize:
                        if (layoutTree->nodeList[i].parentIndex == -1) {
                            solvedSize.unit = SolvedSizeUnit.Pixel;
                            solvedSize.value = 0;
                        }
                        else {
                            solvedSize.unit = (SolvedSizeUnit) (int) measurement.unit;
                            solvedSize.value = measurement.value;
                            parentBased.Set(i);
                        }

                        break;

                    // these are all directly convertible
                    case UIMeasurementUnit.Unset:
                    case UIMeasurementUnit.Pixel:
                    case UIMeasurementUnit.Stretch:
                        solvedSize.unit = (SolvedSizeUnit) (int) measurement.unit;
                        solvedSize.value = measurement.value;
                        break;

                    case UIMeasurementUnit.Content:
                    case UIMeasurementUnit.FitContent:
                    case UIMeasurementUnit.MaxChild:
                    case UIMeasurementUnit.MinChild:
                    case UIMeasurementUnit.StretchContent:
                        childBased.Set(i);
                        solvedSize.unit = (SolvedSizeUnit) (int) measurement.unit;
                        solvedSize.value = measurement.value;
                        break;
                
                }

            }

            // for all animation values, we unset the data previously solved and overwrite it w/ the animation data

            ref DataList<InterpolatedStyleValue> animValueList = ref solverParameters.animationValueBuffer[propertyId];
            
            for (int i = 0; i < animValueList.size; i++) {
                
                ElementId elementId = solverParameters.animationResultBuffer[propertyId][i].elementId;
                int layoutIndex = layoutTree->elementIdToLayoutIndex[elementId.index];
                ref InterpolatedMeasurement measurement = ref animValueList[i].measurement;
                
                SolvedSize prev = default;
                SolvedSize next = default;
                
                parentBased.Unset(layoutIndex);
                childBased.Unset(layoutIndex);
                
                UIMeasurement unsolvedNext = new UIMeasurement(measurement.nextValue, measurement.nextUnit_unsolved);
                UIMeasurement unsolvedPrev = new UIMeasurement(measurement.prevValue, measurement.prevUnit_unsolved);
                
                ref SolvedSize solvedSize = ref solved[layoutIndex];
                
                SolveAnimated(elementId, layoutIndex, unsolvedNext, ref next, emTable, lineHeightTable, parentBased, childBased);
                SolveAnimated(elementId, layoutIndex, unsolvedPrev, ref prev, emTable, lineHeightTable, parentBased, childBased);
                
                measurement.nextValue = next.value;
                measurement.nextUnit_solved = next.unit;
                measurement.prevValue = prev.value;
                measurement.prevUnit_solved = prev.unit;
                
                solvedSize.unit = SolvedSizeUnit.Animation;
                solvedSize.value = i;
                
            }

            for (int i = 0; i < layoutTree->elementCount; i++) {
                if (solved[i].value < 0) solved[i].value = 0;
            }

        }

        private void SolveAnimated(ElementId elementId, int layoutIndex, UIMeasurement measurement, ref SolvedSize solvedSize, float * emTable, CheckedArray<float> lineHeightTable, LongBoolMap parentBased, LongBoolMap childBased) {
            
            switch (measurement.unit) {

                    case UIMeasurementUnit.ViewportWidth:
                    case UIMeasurementUnit.ViewportHeight: {
                        int viewIndex = elementIdToViewId[elementId.index];

                        if (measurement.unit == UIMeasurementUnit.ViewportWidth) {
                            solvedSize.value = viewRects[viewIndex].width * measurement.value;
                            solvedSize.unit = SolvedSizeUnit.Pixel;
                        }
                        else if (measurement.unit == UIMeasurementUnit.ViewportHeight) {
                            solvedSize.value = viewRects[viewIndex].height * measurement.value;
                            solvedSize.unit = SolvedSizeUnit.Pixel;
                        }

                        break;
                    }

                    case UIMeasurementUnit.FillRemaining:
                        solvedSize.value = measurement.value;
                        solvedSize.unit = SolvedSizeUnit.FillRemaining;
                        break;

                    case UIMeasurementUnit.LineHeight:
                        solvedSize.value = lineHeightTable[layoutIndex] * measurement.value;
                        solvedSize.unit = SolvedSizeUnit.Pixel;
                        break;
                    
                    case UIMeasurementUnit.Em:
                        solvedSize.value = emTable[layoutIndex] * measurement.value;
                        solvedSize.unit = SolvedSizeUnit.Pixel;
                        break;

                    case UIMeasurementUnit.BackgroundImageWidth:
                    case UIMeasurementUnit.BackgroundImageHeight:
                        //bgLookups.Add(i); // todo -- I'm not sure these values are present yet since I need to solve bg textures and do a lookup and maybe also handle bg uvs?
                        break;

                    case UIMeasurementUnit.ApplicationWidth:
                        solvedSize.value = appWidth * measurement.value;
                        solvedSize.unit = SolvedSizeUnit.Pixel;
                        break;

                    case UIMeasurementUnit.ApplicationHeight:
                        solvedSize.value = appHeight * measurement.value;
                        solvedSize.unit = SolvedSizeUnit.Pixel;
                        break;

                    case UIMeasurementUnit.Percent:
                    case UIMeasurementUnit.ParentSize:
                        if (layoutTree->nodeList[layoutIndex].parentIndex == -1) {
                            solvedSize.unit = SolvedSizeUnit.Pixel;
                            solvedSize.value = 0;
                        }
                        else {
                            solvedSize.unit = (SolvedSizeUnit) (int) measurement.unit;
                            solvedSize.value = measurement.value;
                            parentBased.Set(layoutIndex);
                        }

                        break;

                    // these are all directly convertible
                    case UIMeasurementUnit.Unset:
                    case UIMeasurementUnit.Pixel:
                    case UIMeasurementUnit.Stretch:
                        solvedSize.unit = (SolvedSizeUnit) (int) measurement.unit;
                        solvedSize.value = measurement.value;
                        break;

                    case UIMeasurementUnit.Content:
                    case UIMeasurementUnit.FitContent:
                    case UIMeasurementUnit.MaxChild:
                    case UIMeasurementUnit.MinChild:
                    case UIMeasurementUnit.StretchContent:
                        childBased.Set(layoutIndex);
                        solvedSize.unit = (SolvedSizeUnit) (int) measurement.unit;
                        solvedSize.value = measurement.value;
                        break;
                
                }
        }

    }

}