using UIForia.ListTypes;
using UIForia.Rendering;
using UIForia.Systems;
using UnityEngine;

namespace UIForia.Layout {

    internal unsafe struct BurstLayoutRunner {

        public ViewParameters viewParameters;
        public LayoutInfo* horizontalLayoutInfoTable;
        public LayoutInfo* verticalLayoutInfoTable;
        public LayoutBoxInfo* layoutBoxInfoTable;
        public LayoutBoxUnion* layoutBoxTable;
        public List_TextLineInfo* lineInfoBuffer;
        public LayoutHierarchyInfo* layoutHierarchyTable;
        public FontAssetInfo* fontAssetMap;
        public EmValue* emTable;

        public ref LayoutHierarchyInfo GetLayoutHierarchy(ElementId elementId) {
            return ref layoutHierarchyTable[elementId.index];
        }

        public ref LayoutInfo GetHorizontalLayoutInfo(ElementId elementId) {
            return ref horizontalLayoutInfoTable[elementId.id & ElementId.ENTITY_INDEX_MASK];
        }

        public ref LayoutInfo GetVerticalLayoutInfo(ElementId elementId) {
            return ref verticalLayoutInfoTable[elementId.id & ElementId.ENTITY_INDEX_MASK];
        }

        public ref LayoutBoxInfo GetLayoutBoxInfo(ElementId elementId) {
            return ref layoutBoxInfoTable[elementId.index];
        }

        public void ApplyLayoutHorizontal(ElementId elementId, float localX, float alignedPosition, float size, float availableSize, in BlockSize blockSize, LayoutFit defaultFit, float parentSize) {
            ref LayoutInfo layoutInfo = ref GetHorizontalLayoutInfo(elementId);
            ref LayoutBoxInfo layoutBoxInfo = ref GetLayoutBoxInfo(elementId);

            LayoutFit fit = layoutInfo.fit;

            if (fit == LayoutFit.Default || fit == LayoutFit.Unset) {
                fit = defaultFit;
            }

            layoutInfo.parentBlockSize = blockSize;

            float newSize = size;
            switch (fit) {
                case LayoutFit.Unset:
                case LayoutFit.None:
                case LayoutFit.Default:
                    newSize = size;
                    break;

                case LayoutFit.Grow:
                    if (availableSize > size) {
                        newSize = availableSize;
                        alignedPosition = localX;
                    }

                    break;

                case LayoutFit.Shrink:
                    if (availableSize < size) {
                        newSize = availableSize;
                        alignedPosition = localX;
                    }

                    break;

                case LayoutFit.Fill:
                    newSize = availableSize;
                    alignedPosition = localX;
                    break;

                case LayoutFit.FillParent: {
                    newSize = parentSize;
                    alignedPosition = 0; //localX;
                    break;
                }
            }

            if (newSize != layoutInfo.finalSize) {
                layoutBoxInfo.sizeChanged = true;
            }

            layoutInfo.finalSize = newSize;

            layoutBoxInfo.actualSize.x = newSize;
            layoutBoxInfo.alignedPosition.x = alignedPosition;
            layoutBoxInfo.allocatedPosition.x = localX;
            layoutBoxInfo.allocatedSize.x = availableSize;
        }

        public void ApplyLayoutVertical(ElementId elementId, float localY, float alignedPosition, float size, float availableSize, in BlockSize blockSize, LayoutFit defaultFit, float parentSize) {
            ref LayoutInfo layoutInfo = ref GetVerticalLayoutInfo(elementId);
            ref LayoutBoxInfo layoutBoxInfo = ref GetLayoutBoxInfo(elementId);

            LayoutFit fit = layoutInfo.fit;

            if (fit == LayoutFit.Default || fit == LayoutFit.Unset) {
                fit = defaultFit;
            }

            layoutInfo.parentBlockSize = blockSize;

            float newSize = size;
            switch (fit) {
                case LayoutFit.Unset:
                case LayoutFit.None:
                case LayoutFit.Default:
                    newSize = size;
                    break;

                case LayoutFit.Grow:
                    if (availableSize > size) {
                        newSize = availableSize;
                        alignedPosition = localY;
                    }

                    break;

                case LayoutFit.Shrink:
                    if (availableSize < size) {
                        newSize = availableSize;
                        alignedPosition = localY;
                    }

                    break;

                case LayoutFit.Fill:
                    newSize = availableSize;
                    alignedPosition = localY;
                    break;

                case LayoutFit.FillParent: {
                    newSize = parentSize;
                    alignedPosition = 0; //localY;
                    break;
                }
            }

            if (newSize != layoutInfo.finalSize) {
                layoutBoxInfo.sizeChanged = true;
            }

            layoutInfo.finalSize = newSize;

            layoutBoxInfo.actualSize.y = newSize;
            layoutBoxInfo.alignedPosition.y = alignedPosition;
            layoutBoxInfo.allocatedPosition.y = localY;
            layoutBoxInfo.allocatedSize.y = availableSize;
        }

        public void GetWidths<T>(in T parent, in BlockSize blockSize, ElementId childId, out LayoutSize size) where T : ILayoutBox {
            // todo -- handle animated sizes
            // ref AnimationLayoutSizes sizes = ref animationInfo[childId.index]; // if isAnimating pref/min/max -> update accordingly with lerp data

            ref LayoutInfo childInfo = ref horizontalLayoutInfoTable[childId.index];
            size.preferred = Mathf.Max(ResolveWidth(parent, childId, blockSize, childInfo.prefSize, ref childInfo, MeasurementAxis.Preferred), 0);
            size.minimum = Mathf.Max(ResolveWidth(parent, childId, blockSize, childInfo.minSize, ref childInfo, MeasurementAxis.Min), 0);
            size.maximum = Mathf.Max(ResolveWidth(parent, childId, blockSize, childInfo.maxSize, ref childInfo, MeasurementAxis.Max), 0);
            size.marginStart = childInfo.marginStart;
            size.marginEnd = childInfo.marginEnd;
        }

        public void GetHeights<T>(in T parent, in BlockSize blockSize, ElementId childId, out LayoutSize size) where T : ILayoutBox {
            // todo -- handle animated sizes

            ref LayoutInfo childInfo = ref verticalLayoutInfoTable[childId.index];
            size.preferred = Mathf.Max(ResolveHeight(parent, childId, blockSize, childInfo.prefSize, ref childInfo), 0);
            size.minimum = Mathf.Max(ResolveHeight(parent, childId, blockSize, childInfo.minSize, ref childInfo), 0);
            size.maximum = Mathf.Max(ResolveHeight(parent, childId, blockSize, childInfo.maxSize, ref childInfo), 0);
            size.marginStart = childInfo.marginStart;
            size.marginEnd = childInfo.marginEnd;
        }

        public float ResolveWidth<T>(in T parent, ElementId elementId, in BlockSize blockSize, in UIMeasurement measurement, ref LayoutInfo info, MeasurementAxis axis) where T : ILayoutBox {
            float value = measurement.value;

            switch (measurement.unit) {
                
                case UIMeasurementUnit.IntrinsicMinimum: {
                    // todo -- consider making this real, its just a prototype for now
                    LayoutBoxUnion layoutBox = layoutBoxTable[elementId.index];
                    if (layoutBox.layoutType == LayoutBoxType.Text) {
                        return layoutBox.text.ComputeContentWidth(ref this, new BlockSize(float.MaxValue, float.MaxValue));
                    }

                    return 0;
                }

                case UIMeasurementUnit.Auto: {
                    LayoutBoxUnion layoutBox = layoutBoxTable[elementId.index];
                    if (layoutBox.layoutType == LayoutBoxType.Image) {
                        return layoutBox.image.ResolveSelfAutoWidth(ref this, parent, blockSize, axis);
                    }

                    return parent.ResolveAutoWidth(ref this, elementId, blockSize);
                }
                
                case UIMeasurementUnit.BackgroundImageWidth: {
                    return GetHorizontalLayoutInfo(elementId).bgSize * value;
                }

                case UIMeasurementUnit.BackgroundImageHeight: {
                    return GetVerticalLayoutInfo(elementId).bgSize * value;
                }

                case UIMeasurementUnit.Content: {
                    return GetContentWidth(elementId, blockSize, measurement.value, ref info);
                }

                default:
                case UIMeasurementUnit.Pixel:
                    return value;

                case UIMeasurementUnit.Em: {
                    float scaled = info.emSize * value;
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
        }

        public enum MeasurementAxis {

            Preferred,
            Min,
            Max

        }
        
        public float ResolveHeight<T>(in T parent, ElementId elementId, in BlockSize blockSize, in UIMeasurement measurement, ref LayoutInfo info) where T : ILayoutBox {
            float value = measurement.value;

            switch (measurement.unit) {
                case UIMeasurementUnit.Auto: {
                    
                    LayoutBoxUnion layoutBox = layoutBoxTable[elementId.index];
                    if (layoutBox.layoutType == LayoutBoxType.Image) {
                        return layoutBox.image.ResolveSelfAutoHeight<T>(ref this);
                    }

                    return parent.ResolveAutoHeight(ref this, elementId, blockSize);
                }

                case UIMeasurementUnit.Content: {
                    return GetContentHeight(elementId, blockSize, measurement.value, ref info);
                }

                default:
                case UIMeasurementUnit.Pixel:
                    return value;

                case UIMeasurementUnit.Em: {
                    float scaled = info.emSize * value;
                    return scaled > 0 ? scaled : 0;
                }

                case UIMeasurementUnit.BackgroundImageWidth: {
                    return GetHorizontalLayoutInfo(elementId).bgSize * value;
                }

                case UIMeasurementUnit.BackgroundImageHeight: {
                    return GetVerticalLayoutInfo(elementId).bgSize * value;
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
        }

        private float GetContentWidth(ElementId layoutBoxId, BlockSize blockSize, float measurementValue, ref LayoutInfo info) {
            // todo -- need 2 or 3 levels of content cache because of fits / constraints

            // if (info.contentCache.cachedSize >= 0 && blockSize == info.contentCache.blockSize) {
            //     float val = info.contentCache.cachedSize * measurementValue;
            //     return val > 0 ? val : 0;
            // }

            float contentWidth = layoutBoxTable[layoutBoxId.index].ComputeContentWidth(ref this, blockSize);

            contentWidth += info.paddingBorderStart + info.paddingBorderEnd;
            info.contentCache.cachedSize = contentWidth;
            info.contentCache.blockSize = blockSize;

            contentWidth *= measurementValue;

            if (contentWidth < 0) contentWidth = 0;

            return contentWidth;
        }

        private float GetContentHeight(ElementId layoutBoxId, BlockSize blockSize, float measurementValue, ref LayoutInfo info) {
            // todo -- need 2 or 3 levels of content cache because of fits / constraints
            // if (info.contentCache.cachedSize >= 0 && blockSize == info.contentCache.blockSize) {
            //     float val = info.contentCache.cachedSize * measurementValue;
            //     return val > 0 ? val : 0;
            // }

            float contentHeight = layoutBoxTable[layoutBoxId.index].ComputeContentHeight(ref this, blockSize);

            contentHeight += info.paddingBorderStart + info.paddingBorderEnd;
            info.contentCache.cachedSize = contentHeight;
            info.contentCache.blockSize = blockSize;

            contentHeight *= measurementValue;

            if (contentHeight < 0) contentHeight = 0;

            return contentHeight;
        }

        public ref FontAssetInfo GetFontAsset(int fontAssetId) {
            return ref fontAssetMap[fontAssetId];
        }

        public float GetResolvedFontSize(ElementId elementId) {
            return emTable[elementId.index].resolvedValue;
        }

    }

}