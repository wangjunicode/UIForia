namespace UIForia.Layout {

    // stack is basically a flex where both axes are considered to be the cross axis 
    // note -- either axis can be controlled and we need to handle both cases 

    internal static class StackLayout {

        public static CheckedArray<LayoutBoxInfo> LayoutBottomUp(ref LayoutListBuffer listBuffer, ref LayoutAxis horizontalAxis, ref LayoutAxis verticalAxis, CheckedArray<LayoutBoxInfo> boxes) {

            listBuffer.controlledList.size = 0;

            LayoutUtil.UpdateAxisFlags(ref listBuffer, ref horizontalAxis, ref verticalAxis, boxes, FlexLayout.CrossAxis);

            FlexLayout.ComputeContentSizes(ref horizontalAxis, listBuffer.contentSizeList, FlexLayout.CrossAxis);

            LayoutUtil.SolveSizes(listBuffer.solveList, ref horizontalAxis);

            FlexLayout.LayoutCrossAxis(ref horizontalAxis, listBuffer.layoutList);

            LayoutUtil.UpdateAxisFlags(ref listBuffer, ref verticalAxis, ref horizontalAxis, boxes, FlexLayout.MainAxis); // main ensures we don't defer any axis locked boxes

            FlexLayout.ComputeContentSizes(ref verticalAxis, listBuffer.contentSizeList, FlexLayout.CrossAxis);

            LayoutUtil.SolveSizes(listBuffer.solveList, ref verticalAxis);

            FlexLayout.LayoutCrossAxis(ref verticalAxis, listBuffer.layoutList);

            if (listBuffer.controlledList.size != 0) {

                LayoutUtil.UpdateAxisFlagsDeferred(ref listBuffer, ref verticalAxis, ref horizontalAxis, listBuffer.controlledList.ToCheckedArray());

                FlexLayout.LayoutCrossAxis(ref horizontalAxis, listBuffer.layoutList);

            }

            return LayoutUtil.FilterCompletedBoxes(boxes, horizontalAxis.readyFlags, verticalAxis.readyFlags);

        }

        public static CheckedArray<LayoutBoxInfo> LayoutTopDown(ref LayoutListBuffer listBuffer, ref LayoutAxis horizontalAxis, ref LayoutAxis verticalAxis, CheckedArray<LayoutBoxInfo> boxes) {
            return LayoutBottomUp(ref listBuffer, ref horizontalAxis, ref verticalAxis, boxes);
        }

    }

}