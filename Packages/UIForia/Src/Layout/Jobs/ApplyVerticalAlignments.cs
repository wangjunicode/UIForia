using UIForia.Style;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace UIForia.Layout {

    [BurstCompile]
    internal unsafe struct ApplyVerticalAlignments : IJob {

        [NativeDisableUnsafePtrRestriction] public LayoutTree* layoutTree;
        [NativeDisableUnsafePtrRestriction] public StyleTables* styleTables;
        [NativeDisableUnsafePtrRestriction] public TempBumpAllocatorPool* bumpPool;
        [NativeDisableUnsafePtrRestriction] public PerFrameLayoutOutput* layoutOutput;
        [NativeDisableUnsafePtrRestriction] public PropertySolverGroup_ClippingAndTransformation* solverGroup;
        [NativeDisableUnsafePtrRestriction] public float** emTablePtr;

        public ElementMap activeMap;
        public float screenWidth;
        public float screenHeight;

        public CheckedArray<Rect> viewRects;
        public CheckedArray<int> elementIdToIndex;
        public CheckedArray<int> indexToParentIndex;
        public CheckedArray<ElementId> indexToElementId;
        public CheckedArray<ushort> elementIdToViewId;
        public float2 mousePosition;

        public void Execute() {
            // todo -- all layout values are using LAYOUT INDEX not the traversal list index, this needs to respect that or we're boned 
            ElementMap boundaryMap = solverGroup->GetDefinitionMap(PropertyId.AlignmentBoundaryY);
            ElementMap targetMap = solverGroup->GetDefinitionMap(PropertyId.AlignmentTargetY);

            ulong* resultBuffer = stackalloc ulong[boundaryMap.longCount];
            ElementMap result = new ElementMap(resultBuffer, boundaryMap.longCount);

            for (int i = 0; i < boundaryMap.longCount; i++) {
                result.map[i] = activeMap.map[i] & (boundaryMap.map[i] | targetMap.map[i]);
            }

            using TempList<ElementId> list = result.ToTempList(Allocator.Temp);

            if (list.size == 0) {
                return;
            }

            using DisposableBumpAllocator pooledAllocator = bumpPool->GetTempAllocator(out BumpAllocator* allocator);

            BumpList<AlignmentTargetInfo> targetList = allocator->AllocateList<AlignmentTargetInfo>(list.size);

            for (int i = 0; i < list.size; i++) {

                int elementIdIndex = list.array[i].index;

                targetList.array[targetList.size++] = new AlignmentTargetInfo() {
                    elementIndex = elementIdIndex,
                    boundary = styleTables->AlignmentBoundaryY[elementIdIndex],
                    direction = styleTables->AlignmentDirectionY[elementIdIndex],
                    alignmentTarget = styleTables->AlignmentTargetY[elementIdIndex],
                    layoutIndex = layoutTree->elementIdToLayoutIndex[elementIdIndex]
                };

            }

            CheckedArray<float> emTable = new CheckedArray<float>(*emTablePtr, layoutTree->elementCount);

            for (int i = 0; i < targetList.size; i++) {

                ref AlignmentTargetInfo targetInfo = ref targetList.array[i];

                UIOffset offset = styleTables->AlignmentOffsetY[targetInfo.elementIndex];
                UIOffset origin = styleTables->AlignmentOriginY[targetInfo.elementIndex];

                targetInfo.offset = OffsetSize(offset, targetInfo, ref emTable);
                targetInfo.innerOrigin = OffsetSize(origin, targetInfo, ref emTable);

            }

            for (int i = 0; i < targetList.size; i++) {

                ref AlignmentTargetInfo targetInfo = ref targetList.array[i];

                float originBase = 0;
                float originSize = 0;

                switch (targetInfo.alignmentTarget) {

                    // can happen if boundary is set but target is not
                    case AlignmentTarget.None:
                        break;

                    case AlignmentTarget.Parent: {
                        int parentIndex = layoutTree->nodeList[targetInfo.layoutIndex].parentIndex;
                        if (parentIndex == -1) {
                            originSize = viewRects[elementIdToViewId[targetInfo.elementIndex]].height;
                        }
                        else {
                            originSize = layoutOutput->sizes[parentIndex].height;
                        }

                        originBase = 0;

                        break;
                    }

                    case AlignmentTarget.LayoutBox:
                        originSize = layoutOutput->sizes[targetInfo.layoutIndex].height;
                        originBase = layoutOutput->localPositions[targetInfo.layoutIndex].y;
                        break;

                    case AlignmentTarget.ParentContentArea: {
                        int parentIndex = layoutTree->nodeList[targetInfo.layoutIndex].parentIndex;
                        if (parentIndex == -1) {
                            originSize = viewRects[elementIdToViewId[targetInfo.elementIndex]].height;
                        }
                        else {
                            // depends on direction but we want to take padding and border and add it here
                            originSize = layoutOutput->sizes[parentIndex].height;
                        }

                        originBase = 0;
                        break;
                    }

                    case AlignmentTarget.View: {

                        Rect viewRect = viewRects[elementIdToViewId[targetInfo.elementIndex]];

                        originBase = 0;

                        int ptr = layoutTree->nodeList[targetInfo.layoutIndex].parentIndex;

                        while (ptr != -1) {
                            originBase -= layoutOutput->localPositions[ptr].y;
                            ptr = layoutTree->nodeList[ptr].parentIndex;
                        }

                        originSize = viewRect.height;
                        break;
                    }

                    case AlignmentTarget.Clipper: {

                        int ptr = layoutTree->nodeList[targetInfo.layoutIndex].parentIndex;
                        originBase = 0;

                        while (ptr != -1) {

                            Overflow overflow = styleTables->OverflowX[layoutTree->elementIdList[ptr].index];

                            if (overflow == Overflow.Hidden) {
                                break;
                            }

                            originBase -= layoutOutput->localPositions[ptr].y;
                            ptr = layoutTree->nodeList[ptr].parentIndex;
                        }

                        if (ptr == -1) {
                            // compute local distance to that clipper point
                            originSize = viewRects[elementIdToViewId[targetInfo.elementIndex]].height;
                        }
                        else {
                            originSize = layoutOutput->sizes[ptr].height;
                            ElementId clipperId = layoutTree->elementIdList[ptr];
                            ClipBounds bounds = styleTables->ClipBounds[clipperId.index];

                            if (bounds == ClipBounds.ContentBox) {
                                OffsetRect borders = layoutOutput->borders[ptr];
                                OffsetRect paddings = layoutOutput->paddings[ptr];
                                originSize -= borders.bottom + paddings.bottom + borders.top + paddings.top;
                                originBase += borders.top + paddings.top;
                            }

                        }

                        break;
                    }

                    case AlignmentTarget.Mouse:
                    case AlignmentTarget.Screen: {
                        // want dist from 0 to element
                        originBase = 0;
                        int ptr = layoutTree->nodeList[targetInfo.layoutIndex].parentIndex;

                        while (ptr != -1) {
                            originBase += layoutOutput->localPositions[ptr].y;
                            ptr =  layoutTree->nodeList[ptr].parentIndex;
                        }

                        Rect viewRect = viewRects[elementIdToViewId[targetInfo.elementIndex]];
                        originBase = -viewRect.y - originBase;

                        if (targetInfo.alignmentTarget == AlignmentTarget.Screen) {
                            originSize = screenHeight;
                        }
                        else {
                            originSize = 0;
                            originBase += mousePosition.y;
                        }

                        break;
                    }

                }

                float elementHeight = layoutOutput->sizes[targetInfo.layoutIndex].height;

                float alignedY = targetInfo.direction == AlignmentDirection.End
                    ? (originBase + originSize) - (targetInfo.innerOrigin + targetInfo.offset) - elementHeight
                    : originBase + targetInfo.innerOrigin + targetInfo.offset;

                float boundsY = alignedY;
                float boundsHeight = elementHeight;

                switch (targetInfo.boundary) {

                    case AlignmentBoundary.None:
                        break;

                    case AlignmentBoundary.Screen: {

                        boundsY = 0;
                        int ptr = layoutTree->nodeList[targetInfo.layoutIndex].parentIndex;

                        while (ptr != -1) {
                            boundsY -= layoutOutput->localPositions[ptr].y;
                            ptr = layoutTree->nodeList[ptr].parentIndex;
                        }

                        Rect viewRect = viewRects[elementIdToViewId[targetInfo.elementIndex]];
                        boundsY -= viewRect.y;
                        boundsHeight = screenHeight;

                        break;
                    }

                    case AlignmentBoundary.Parent: {
                        boundsY = 0;
                        int parentIndex = layoutTree->nodeList[targetInfo.layoutIndex].parentIndex;
                        if (parentIndex == -1) {
                            boundsHeight = viewRects[elementIdToViewId[targetInfo.elementIndex]].height;
                        }
                        else {
                            boundsHeight = layoutOutput->sizes[parentIndex].height;
                        }

                        break;
                    }

                    case AlignmentBoundary.ParentContentArea: {
                        int parentIndex = layoutTree->nodeList[targetInfo.layoutIndex].parentIndex;
                        if (parentIndex == -1) {
                            boundsY = 0;
                            boundsHeight = viewRects[elementIdToViewId[targetInfo.elementIndex]].height;
                        }
                        else {
                            OffsetRect paddings = layoutOutput->paddings[parentIndex];
                            OffsetRect borders = layoutOutput->borders[parentIndex];
                            boundsY = paddings.top + borders.top;
                            boundsHeight = layoutOutput->sizes[parentIndex].height - (paddings.top + paddings.bottom + borders.bottom + borders.top);
                        }

                        break;
                    }

                    case AlignmentBoundary.Clipper: {

                        int ptr = layoutTree->nodeList[targetInfo.layoutIndex].parentIndex;

                        boundsY = 0;

                        while (ptr != -1) {

                            Overflow overflow = styleTables->OverflowY[layoutTree->nodeList[ptr].elementId.index];

                            if (overflow == Overflow.Hidden) {
                                break;
                            }

                            boundsY += layoutOutput->localPositions[ptr].y;
                            ptr = layoutTree->nodeList[ptr].parentIndex;
                        }

                        if (ptr == -1) {
                            // compute local distance to that clipper point
                            boundsHeight = viewRects[elementIdToViewId[targetInfo.elementIndex]].height;
                        }
                        else {
                            ElementId clipperId = layoutTree->nodeList[ptr].elementId;
                            ClipBounds bounds = styleTables->ClipBounds[clipperId.index];

                            boundsHeight = layoutOutput->sizes[ptr].height;

                            if (bounds == ClipBounds.ContentBox) {
                                OffsetRect borders = layoutOutput->borders[ptr];
                                OffsetRect paddings = layoutOutput->paddings[ptr];
                                boundsHeight -= borders.top + paddings.bottom + borders.top + paddings.bottom;
                                boundsY += borders.top + paddings.top;
                            }

                        }
                        break;
                    }

                    case AlignmentBoundary.View: {
                        boundsY = 0;

                        int ptr = layoutTree->nodeList[targetInfo.layoutIndex].parentIndex;

                        while (ptr != -1) {
                            boundsY -= layoutOutput->localPositions[ptr].y;
                            ptr = layoutTree->nodeList[ptr].parentIndex;
                        }

                        boundsHeight = viewRects[elementIdToViewId[targetInfo.elementIndex]].height;
                        break;
                    }

                }


                if (alignedY < boundsY) {
                    alignedY = boundsY;
                }

                if (alignedY + elementHeight > boundsHeight + boundsY) {
                    alignedY = boundsHeight + boundsY - elementHeight;
                }

                layoutOutput->localPositions.array[targetInfo.layoutIndex].y = alignedY;
            }
        }

        private float OffsetSize(UIOffset offset, AlignmentTargetInfo targetInfo, ref CheckedArray<float> emTable) {
            float offsetSize;
            switch (offset.unit) {
                default:
                case UIOffsetUnit.Unset:
                case UIOffsetUnit.Pixel:
                    offsetSize = offset.value;
                    break;

                case UIOffsetUnit.Em:
                    offsetSize = emTable[targetInfo.layoutIndex] * offset.value;
                    break;

                case UIOffsetUnit.Width:
                    offsetSize = layoutOutput->sizes[targetInfo.layoutIndex].width * offset.value;
                    break;

                case UIOffsetUnit.Height:
                    offsetSize = layoutOutput->sizes[targetInfo.layoutIndex].height * offset.value;
                    break;

                case UIOffsetUnit.ContentWidth:
                    // content extents for children
                    // do we always compute this? is this even interesting? probably cheap to compute bottom up after layout 
                    offsetSize = 0; //  math.max(0, layoutResult.ContentAreaWidth * measurement.value);
                    break;

                case UIOffsetUnit.ContentHeight:
                    offsetSize = 0; // math.max(0, layoutResult.ContentAreaHeight * measurement.value);
                    break;

                case UIOffsetUnit.ViewportWidth: {
                    int viewIndex = elementIdToViewId[targetInfo.elementIndex];
                    offsetSize = viewRects[viewIndex].width * offset.value;
                    break;
                }

                case UIOffsetUnit.ViewportHeight: {
                    int viewIndex = elementIdToViewId[targetInfo.elementIndex];
                    offsetSize = viewRects[viewIndex].height * offset.value;
                    break;
                }

                case UIOffsetUnit.ParentWidth: {
                    int parentIndex = indexToParentIndex[targetInfo.layoutIndex];
                    if (parentIndex == -1) {
                        int viewIndex = elementIdToViewId[targetInfo.elementIndex];
                        offsetSize = viewRects[viewIndex].width * offset.value;
                    }
                    else {
                        offsetSize = layoutOutput->sizes[parentIndex].width * offset.value;
                    }

                    break;
                }

                case UIOffsetUnit.ParentHeight: {
                    int parentIndex = indexToParentIndex[targetInfo.layoutIndex];
                    if (parentIndex == -1) {
                        int viewIndex = elementIdToViewId[targetInfo.elementIndex];
                        offsetSize = viewRects[viewIndex].height * offset.value;
                    }
                    else {
                        offsetSize = layoutOutput->sizes[parentIndex].height * offset.value;
                    }

                    break;
                }

                case UIOffsetUnit.ScreenWidth:
                    offsetSize = screenWidth * offset.value;
                    break;

                case UIOffsetUnit.ScreenHeight:
                    offsetSize = screenHeight * offset.value;
                    break;

            }

            return offsetSize;
        }

    }

}