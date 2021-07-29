using System;
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

    [BurstCompile(DisableSafetyChecks = UIApplication.DisableJobSafety)]
    internal unsafe struct ApplyHorizontalAlignments : IJob {

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
        public CheckedArray<ushort> elementIdToViewId;

        public float2 mousePosition;

        public void Execute() {
            // todo -- all layout values are using LAYOUT INDEX not the traversal list index, this needs to respect that or we're boned 
            ElementMap boundaryMap = solverGroup->GetDefinitionMap(PropertyId.AlignmentBoundaryX);
            ElementMap targetMap = solverGroup->GetDefinitionMap(PropertyId.AlignmentTargetX);

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
                    boundary = styleTables->AlignmentBoundaryX[elementIdIndex],
                    direction = styleTables->AlignmentDirectionX[elementIdIndex],
                    alignmentTarget = styleTables->AlignmentTargetX[elementIdIndex],
                    layoutIndex = layoutTree->elementIdToLayoutIndex[elementIdIndex],
                };

            }

            CheckedArray<float> emTable = new CheckedArray<float>(*emTablePtr, layoutTree->elementCount);

            for (int i = 0; i < targetList.size; i++) {

                ref AlignmentTargetInfo targetInfo = ref targetList.array[i];

                UIOffset offset = styleTables->AlignmentOffsetX[targetInfo.elementIndex];
                UIOffset origin = styleTables->AlignmentOriginX[targetInfo.elementIndex];

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
                            originSize = viewRects[elementIdToViewId[targetInfo.elementIndex]].width;
                        }
                        else {
                            originSize = layoutOutput->sizes[parentIndex].width;
                        }

                        originBase = 0;

                        break;
                    }

                    case AlignmentTarget.LayoutBox:
                        originSize = layoutOutput->sizes[targetInfo.layoutIndex].width;
                        originBase = layoutOutput->localPositions[targetInfo.layoutIndex].x;
                        break;

                    case AlignmentTarget.ParentContentArea: {
                        int parentIndex = layoutTree->nodeList[targetInfo.layoutIndex].parentIndex;
                        if (parentIndex == -1) {
                            originSize = viewRects[elementIdToViewId[targetInfo.elementIndex]].width;
                        }
                        else {
                            // depends on direction but we want to take padding and border and add it here
                            originSize = layoutOutput->sizes[parentIndex].width;
                        }

                        originBase = 0;
                        break;
                    }

                    case AlignmentTarget.View: {

                        Rect viewRect = viewRects[elementIdToViewId[targetInfo.elementIndex]];

                        originBase = 0;

                        int ptr = layoutTree->nodeList[targetInfo.layoutIndex].parentIndex;

                        while (ptr != -1) {
                            originBase -= layoutOutput->localPositions[ptr].x;
                            ptr = layoutTree->nodeList[ptr].parentIndex;
                        }

                        originSize = viewRect.width;
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

                            originBase -= layoutOutput->localPositions[ptr].x;
                            ptr = layoutTree->nodeList[ptr].parentIndex;
                        }

                        if (ptr == -1) {
                            // compute local distance to that clipper point
                            originSize = viewRects[elementIdToViewId[targetInfo.elementIndex]].width;
                        }
                        else {
                            originSize = layoutOutput->sizes[ptr].width;
                            ElementId clipperId = layoutTree->elementIdList[ptr];
                            ClipBounds bounds = styleTables->ClipBounds[clipperId.index];

                            if (bounds == ClipBounds.ContentBox) {
                                OffsetRect borders = layoutOutput->borders[ptr];
                                OffsetRect paddings = layoutOutput->paddings[ptr];
                                originSize -= borders.right + paddings.right + borders.left + paddings.left;
                                originBase += borders.left + paddings.left;
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
                            originBase += layoutOutput->localPositions[ptr].x;
                            ptr =  layoutTree->nodeList[ptr].parentIndex;
                        }

                        Rect viewRect = viewRects[elementIdToViewId[targetInfo.elementIndex]];
                        originBase = -viewRect.x - originBase;

                        if (targetInfo.alignmentTarget == AlignmentTarget.Screen) {
                            originSize = screenWidth;
                        }
                        else {
                            originSize = 0;
                            originBase += mousePosition.x;
                        }

                        break;
                    }

                }

                float elementWidth = layoutOutput->sizes[targetInfo.layoutIndex].width;

                float alignedX = targetInfo.direction == AlignmentDirection.End
                    ? (originBase + originSize) - (targetInfo.innerOrigin + targetInfo.offset) - elementWidth
                    : originBase + targetInfo.innerOrigin + targetInfo.offset;

                float boundsX = alignedX;
                float boundsWidth = elementWidth;

                switch (targetInfo.boundary) {

                    case AlignmentBoundary.None:
                        break;

                    case AlignmentBoundary.Screen: {

                        boundsX = 0;
                        int ptr = layoutTree->nodeList[targetInfo.layoutIndex].parentIndex;

                        while (ptr != -1) {
                            boundsX -= layoutOutput->localPositions[ptr].x;
                            ptr = layoutTree->nodeList[ptr].parentIndex;
                        }

                        Rect viewRect = viewRects[elementIdToViewId[targetInfo.elementIndex]];
                        boundsX -= viewRect.x;
                        boundsWidth = screenWidth;

                        break;
                    }

                    case AlignmentBoundary.Parent: {
                        boundsX = 0;
                        int parentIndex = layoutTree->nodeList[targetInfo.layoutIndex].parentIndex;
                        if (parentIndex == -1) {
                            boundsWidth = viewRects[elementIdToViewId[targetInfo.elementIndex]].width;
                        }
                        else {
                            boundsWidth = layoutOutput->sizes[parentIndex].width;
                        }

                        break;
                    }

                    case AlignmentBoundary.ParentContentArea: {
                        int parentIndex = layoutTree->nodeList[targetInfo.layoutIndex].parentIndex;
                        if (parentIndex == -1) {
                            boundsX = 0;
                            boundsWidth = viewRects[elementIdToViewId[targetInfo.elementIndex]].width;
                        }
                        else {
                            OffsetRect paddings = layoutOutput->paddings[parentIndex];
                            OffsetRect borders = layoutOutput->borders[parentIndex];
                            boundsX = paddings.left + borders.left;
                            boundsWidth = layoutOutput->sizes[parentIndex].width - (paddings.left + paddings.right + borders.right + borders.left);
                        }

                        break;
                    }

                    case AlignmentBoundary.Clipper: {
                        
                        int ptr = layoutTree->nodeList[targetInfo.layoutIndex].parentIndex;
                        
                        boundsX = 0;

                        while (ptr != -1) {

                            Overflow overflow = styleTables->OverflowX[layoutTree->nodeList[ptr].elementId.index];

                            if (overflow == Overflow.Hidden) {
                                break;
                            }

                            boundsX += layoutOutput->localPositions[ptr].x;
                            ptr = layoutTree->nodeList[ptr].parentIndex;
                        }

                        if (ptr == -1) {
                            // compute local distance to that clipper point
                            boundsWidth = viewRects[elementIdToViewId[targetInfo.elementIndex]].width;
                        }
                        else {
                            ElementId clipperId = layoutTree->nodeList[ptr].elementId;
                            ClipBounds bounds = styleTables->ClipBounds[clipperId.index];
                            
                            boundsWidth = layoutOutput->sizes[ptr].width;
                            
                            if (bounds == ClipBounds.ContentBox) {
                                OffsetRect borders = layoutOutput->borders[ptr];
                                OffsetRect paddings = layoutOutput->paddings[ptr];
                                boundsWidth -= borders.right + paddings.right + borders.left + paddings.left;
                                boundsX += borders.left + paddings.left;
                            }

                        }
                        break;
                    }

                    case AlignmentBoundary.View: {
                        boundsX = 0;
                        
                        int ptr = layoutTree->nodeList[targetInfo.layoutIndex].parentIndex;

                        while (ptr != -1) {
                            boundsX -= layoutOutput->localPositions[ptr].x;
                            ptr = layoutTree->nodeList[ptr].parentIndex;
                        }
                        
                        boundsWidth = viewRects[elementIdToViewId[targetInfo.elementIndex]].width;   
                        break;
                    }
                    
                }


                if (alignedX < boundsX) {
                    alignedX = boundsX;
                }

                if (alignedX + elementWidth > boundsWidth + boundsX) {
                    alignedX = boundsWidth + boundsX - elementWidth;
                }

                layoutOutput->localPositions.array[targetInfo.layoutIndex].x = alignedX;
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
                    int parentIndex =  layoutTree->nodeList[targetInfo.layoutIndex].parentIndex;
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
                    int parentIndex =  layoutTree->nodeList[targetInfo.layoutIndex].parentIndex;
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

    internal struct AlignmentTargetInfo {

        public int layoutIndex;
        public int elementIndex;
        public AlignmentTarget alignmentTarget;
        public AlignmentBoundary boundary;
        public AlignmentDirection direction;
        public float innerOrigin;
        public float offset;

    }


}