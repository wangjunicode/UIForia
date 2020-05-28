using System;
using UIForia.Rendering;
using UIForia.Systems;

namespace UIForia.Layout {

    public unsafe struct BurstLayoutRunner {

        public ViewParameters viewParameters;
        public LayoutSizeInfo* horizontalSizeInfoTable;
        public LayoutSizeInfo* verticalSizeInfoTable;
        public LayoutPassResult* horizontalLayoutResults;
        public LayoutBoxUnion* layoutBoxTable;

        // called every frame to init data because pointers might have changed if element system resized
        public void Setup(
            in ViewParameters viewParameters,
            ElementTable<LayoutSizeInfo> horizontalSizeInfoTable,
            ElementTable<LayoutSizeInfo> verticalSizeInfoTable,
            ElementTable<LayoutBoxUnion> layoutBoxTable,
            ElementTable<LayoutPassResult> horizontalLayoutResults
        ) {
            this.viewParameters = viewParameters;
            this.horizontalSizeInfoTable = horizontalSizeInfoTable.array;
            this.verticalSizeInfoTable = verticalSizeInfoTable.array;
            this.layoutBoxTable = layoutBoxTable.array;
            this.horizontalLayoutResults = horizontalLayoutResults.array;
        }

        public ref LayoutMetaData GetLayoutMetaData(ElementId elementId) {
            throw new NotImplementedException();
        }

        public ref LayoutSizeInfo GetHorizontalSizeInfo(ElementId elementId) {
            return ref horizontalSizeInfoTable[elementId.index];
        }

        public ref LayoutPassResult GetHorizontalLayoutResult(ElementId elementId) {
            return ref horizontalLayoutResults[elementId.index];
        }

        public void ApplyLayoutHorizontal(ElementId elementId, float localX, float alignedPosition, float size, float availableSize, in BlockSize blockSize, LayoutFit defaultFit, float parentSize) {

            ref LayoutSizeInfo layoutSizeInfo = ref GetHorizontalSizeInfo(elementId);
            ref LayoutPassResult layoutPassResult = ref GetHorizontalLayoutResult(elementId);

            LayoutFit fit = layoutSizeInfo.fit;

            if (fit == LayoutFit.Default || fit == LayoutFit.Unset) {
                fit = defaultFit;
            }

            float newWidth = size;
            switch (fit) {
                case LayoutFit.Unset:
                case LayoutFit.None:
                case LayoutFit.Default:
                    newWidth = size;
                    break;

                case LayoutFit.Grow:
                    if (availableSize > size) {
                        newWidth = availableSize;
                        alignedPosition = localX;
                    }

                    break;

                case LayoutFit.Shrink:
                    if (availableSize < size) {
                        newWidth = availableSize;
                        alignedPosition = localX;
                    }

                    break;

                case LayoutFit.Fill:
                    newWidth = availableSize;
                    alignedPosition = localX;
                    break;

                case LayoutFit.FillParent: {
                    newWidth = parentSize;
                    alignedPosition = 0; //localX;
                    break;
                }

            }

            if (newWidth != layoutPassResult.actualSize) {
                // require layout
                // other flags? invalidate something?
            }

            layoutSizeInfo.finalSize = newWidth;

            layoutPassResult.actualSize = newWidth;
            layoutPassResult.alignedPosition = alignedPosition;
            layoutPassResult.allocatedPosition = alignedPosition;
            layoutPassResult.allocatedSize = availableSize;

        }

        public void GetWidths<T>(in T parent, in BlockSize blockSize, ElementId childId, ref LayoutSize size) where T : ILayoutHandler {

            // todo -- handle animated sizes

            ref LayoutSizeInfo childSizeInfo = ref horizontalSizeInfoTable[childId.index];
            size.preferred = ResolveWidth(parent, childId, blockSize, childSizeInfo.prefSize, ref childSizeInfo);
            size.minimum = ResolveWidth(parent, childId, blockSize, childSizeInfo.minSize, ref childSizeInfo);
            size.maximum = ResolveWidth(parent, childId, blockSize, childSizeInfo.maxSize, ref childSizeInfo);
            size.marginStart = childSizeInfo.marginStart;
            size.marginEnd = childSizeInfo.marginEnd;
        }

        public float ResolveWidth<T>(in T parent, ElementId elementId, in BlockSize blockSize, in UIMeasurement measurement, ref LayoutSizeInfo sizeInfo) where T : ILayoutHandler {
            float value = measurement.value;

            switch (measurement.unit) {

                case UIMeasurementUnit.Auto: {
                    return parent.ResolveAutoWidth(elementId, measurement.value);
                }

                case UIMeasurementUnit.Content: {
                    return GetContentWidth(elementId, blockSize, measurement.value, ref sizeInfo);
                }

                case UIMeasurementUnit.Pixel:
                    return value;

                case UIMeasurementUnit.Em: {
                    float scaled = sizeInfo.emSize * value;
                    return scaled > 0 ? scaled : 0;
                }

                case UIMeasurementUnit.ViewportWidth:
                    return viewParameters.viewWidth * value;

                case UIMeasurementUnit.ViewportHeight:
                    return viewParameters.viewHeight * value;

                case UIMeasurementUnit.BlockSize: {
                    // ignored elements can use the output size of their parent since it has been resolved already
                    float scaled = blockSize.outerSize * measurement.value;
                    return scaled < 0 ? 0 : scaled;
                }

                case UIMeasurementUnit.Percentage:
                case UIMeasurementUnit.ParentContentArea: {
                    float scaled = blockSize.insetSize * measurement.value;
                    return scaled < 0 ? 0 : scaled;
                }
            }

            return 0;
        }

        private float GetContentWidth(ElementId layoutBoxId, BlockSize blockSize, float measurementValue, ref LayoutSizeInfo sizeInfo) {

            if (sizeInfo.contentCache.cachedSize >= 0 && blockSize == sizeInfo.contentCache.blockSize) {
                float val = sizeInfo.contentCache.cachedSize * measurementValue;
                return val > 0 ? val : 0;
            }

            float contentWidth = layoutBoxTable[layoutBoxId.index].ComputeContentWidth(ref this, blockSize);

            contentWidth += sizeInfo.paddingBorderStart + sizeInfo.paddingBorderEnd;
            sizeInfo.contentCache.cachedSize = contentWidth;
            sizeInfo.contentCache.blockSize = blockSize;

            contentWidth *= measurementValue;

            if (contentWidth < 0) contentWidth = 0;

            return contentWidth;
        }

    }

}