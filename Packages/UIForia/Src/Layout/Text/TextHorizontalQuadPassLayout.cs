using UIForia.Text;
using UIForia.Util;
using UIForia.Util.Unsafe;
using UnityEngine;

namespace UIForia.Layout {

    internal static unsafe class TextHorizontalQuadPassLayout {

        public static CheckedArray<LayoutBoxInfo> LayoutBottomUp(ref LayoutListBuffer listBuffer, ref LayoutAxis horizontalAxis, ref LayoutAxis verticalAxis, CheckedArray<LayoutBoxInfo> boxes, ref TextDataTable textDataTable) {

            ref TempList<LayoutBoxInfo> solveList = ref listBuffer.solveList;
            ref TempList<LayoutBoxInfo> layoutList = ref listBuffer.layoutList;
            ref TempList<LayoutBoxInfo> contentSizeList = ref listBuffer.contentSizeList;

            solveList.size = 0;
            layoutList.size = 0;
            contentSizeList.size = 0;

            // text cannot have the traditional aspect locking, at least not right now

            for (int i = 0; i < boxes.size; i++) {

                LayoutBoxInfo box = boxes.array[i];
                ref ReadyFlags readyFlags = ref horizontalAxis.readyFlags.array[box.boxIndex];

                // horizontal w/o content size needs content ready to be set...right?
                
                // if we need content width & dont have it we enqueue to compute it
                if ((readyFlags & ReadyFlags.ContentSizeResolved) == 0) {
                    readyFlags |= ReadyFlags.ContentSizeResolved | ReadyFlags.ContentReady;
                    listBuffer.contentSizeList.array[listBuffer.contentSizeList.size++] = box;
                }

                if (((readyFlags & ReadyFlags.BaseSizeResolved) == 0) && (readyFlags & LayoutUtil.k_BaseSizeResolvable) == LayoutUtil.k_BaseSizeResolvable) {

                    readyFlags |= ReadyFlags.BaseSizeResolved;
                    solveList.array[solveList.size++] = box;

                    // if not waiting for parent stretch or parent size -> we're done
                    if ((readyFlags & ReadyFlags.StretchReady) != 0) {
                        readyFlags |= ReadyFlags.FinalSizeResolved;
                    }
                }
                
                // if content is not ready but base size is resolved -> text is stretch and parent is stretch too
                // (readyFlags & ReadyFlags.ContentReady) == 0 && 
                if ((readyFlags & ReadyFlags.BaseSizeResolved) != 0) {
                    readyFlags |= ReadyFlags.ContentReady;
                }

            }

            ComputeContentWidth(ref horizontalAxis, listBuffer.contentSizeList, ref textDataTable);

            LayoutUtil.SolveSizes(listBuffer.solveList, ref horizontalAxis);
            
            listBuffer.solveList.size = 0;
            listBuffer.layoutList.size = 0;
            listBuffer.contentSizeList.size = 0;
            
            for (int i = 0; i < boxes.size; i++) {

                LayoutBoxInfo box = boxes.array[i];

                ref ReadyFlags verticalFlags = ref verticalAxis.readyFlags.array[box.boxIndex];
                ReadyFlags horizontalFlags = horizontalAxis.readyFlags[box.boxIndex];
                
                // if we need content height & dont have it & horizontal has a final size, we enqueue to compute it
                if (((horizontalFlags & ReadyFlags.FinalSizeResolved) != 0) && (verticalFlags & ReadyFlags.ContentSizeResolved) == 0) {
                    verticalFlags |= ReadyFlags.ContentSizeResolved | ReadyFlags.ContentReady;
                    listBuffer.contentSizeList.array[listBuffer.contentSizeList.size++] = box;
                }

                if (((verticalFlags & ReadyFlags.BaseSizeResolved) == 0) && (verticalFlags & LayoutUtil.k_BaseSizeResolvable) == LayoutUtil.k_BaseSizeResolvable) {

                    verticalFlags |= ReadyFlags.BaseSizeResolved;
                    solveList.array[solveList.size++] = box;

                    // if not waiting for parent stretch or parent size -> we're done
                    if ((verticalFlags & ReadyFlags.StretchReady) != 0) {
                        verticalFlags |= ReadyFlags.FinalSizeResolved;
                    }

                }

                if ((verticalFlags & LayoutUtil.k_ReadyForLayout) == LayoutUtil.k_ReadyForLayout) {
                    layoutList.array[layoutList.size++] = box; // not sure about this.. I think we only do this when both axes are complete / ready for layout 
                }
                
            }

            ComputeContentHeight(ref horizontalAxis, ref verticalAxis, listBuffer.contentSizeList, ref textDataTable);

            LayoutUtil.SolveSizes(listBuffer.solveList, ref verticalAxis);

            listBuffer.solveList.size = 0;
            listBuffer.layoutList.size = 0;
            listBuffer.contentSizeList.size = 0;
            
            for (int i = 0; i < boxes.size; i++) {

                LayoutBoxInfo box = boxes.array[i];
                ref ReadyFlags horizontalFlags = ref horizontalAxis.readyFlags.array[box.boxIndex];
                ref ReadyFlags verticalFlags = ref verticalAxis.readyFlags.array[box.boxIndex];

                if ((horizontalFlags & ReadyFlags.LayoutComplete) != 0) {
                    // should never hit this since h & v complete together and we filter out completed boxes 
                    continue;
                }
                
                if ((horizontalFlags & LayoutUtil.k_ReadyForLayout) == LayoutUtil.k_ReadyForLayout &&
                    (verticalFlags & LayoutUtil.k_ReadyForLayout) == LayoutUtil.k_ReadyForLayout) {

                    horizontalFlags |= ReadyFlags.LayoutComplete;
                    verticalFlags |= ReadyFlags.LayoutComplete;

                    float outputWidth = horizontalAxis.outputSizes[box.boxIndex];
                    float outputHeight = verticalAxis.outputSizes[box.boxIndex];
                    
                    OffsetRect padding = new OffsetRect() {
                        left = horizontalAxis.paddingBorderStart[box.boxIndex],
                        right = horizontalAxis.paddingBorderEnd[box.boxIndex],
                        top = verticalAxis.paddingBorderStart[box.boxIndex],
                        bottom = verticalAxis.paddingBorderEnd[box.boxIndex]
                    };
                    
                    textDataTable.FinalizeLayout(textDataTable.boxIndexToTextId[box.boxIndex], outputWidth, outputHeight, padding);
                }

            }

            return LayoutUtil.FilterCompletedBoxes(boxes, horizontalAxis.readyFlags, verticalAxis.readyFlags);

        }

        private static void ComputeContentWidth(ref LayoutAxis horizontalAxis, TempList<LayoutBoxInfo> measureList, ref TextDataTable textDataTable) {

            for (int i = 0; i < measureList.size; i++) {
                LayoutBoxInfo box = measureList.array[i];

                float contentWidth = textDataTable.ResolveContentWidth(textDataTable.boxIndexToTextId[box.boxIndex]);

                horizontalAxis.contentSizes[box.boxIndex] = new LayoutContentSizes() {
                    contentSize = contentWidth,
                    maxChildSize = contentWidth, // todo -- we could support min/max intrinsic size here 
                    minChildSize = contentWidth
                };
            }

        }
        
        private static void ComputeContentHeight(ref LayoutAxis horizontalAxis, ref LayoutAxis verticalAxis, TempList<LayoutBoxInfo> measureList, ref TextDataTable textDataTable) {
            
            // do we treat text with content height requirements the same way we do aspect locked axes? 
            // I'm not sure if we only do it relative to output size or if we prefer to use resolved base size 
            // I think using output size introduces awkwardness with paradox busting 
            
            for (int i = 0; i < measureList.size; i++) {
                
                LayoutBoxInfo box = measureList.array[i];

                float contentHeight = textDataTable.GetHeight(textDataTable.boxIndexToTextId[box.boxIndex], horizontalAxis.outputSizes[box.boxIndex]);

                verticalAxis.contentSizes[box.boxIndex] = new LayoutContentSizes() {
                    contentSize = contentHeight,
                    maxChildSize = contentHeight, // todo -- we could support min/max intrinsic size here 
                    minChildSize = contentHeight
                };

                verticalAxis.readyFlags[box.boxIndex] |= ReadyFlags.ContentReady;

            }
            
        }

        public static CheckedArray<LayoutBoxInfo> LayoutTopDown(ref LayoutListBuffer listBuffer, ref LayoutAxis horizontalAxis, ref LayoutAxis verticalAxis, CheckedArray<LayoutBoxInfo> boxes, ref TextDataTable textDataTable) {
            return LayoutBottomUp(ref listBuffer, ref horizontalAxis, ref verticalAxis, boxes, ref textDataTable);
        }

    }

}