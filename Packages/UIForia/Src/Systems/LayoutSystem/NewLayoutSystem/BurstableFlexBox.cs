using System;
using System.Runtime.InteropServices;
using UIForia.Elements;
using UIForia.Rendering;
using UIForia.Systems;
using UIForia.Util.Unsafe;
using Unity.Collections;

namespace UIForia.Layout {

    public struct FlexLayoutInfo {

        public LayoutDirection direction;
        public LayoutWrap layoutWrap;
        public float alignVertical;
        public float alignHorizontal;
        public LayoutFit fitVertical;
        public LayoutFit fitHorizontal;
        public SpaceDistribution horizontalDistribution;
        public SpaceDistribution verticalDistribution;

    }

    public struct HorizontalLayoutInfo {

        public float width;
        public float paddingBorderStart;
        public float paddingBorderEnd;
        public float marginStart;
        public float marginEnd;
        public int childCount;

    }

    public struct FlexItem {

        public LayoutBox.LayoutSize widthData;
        public LayoutBox.LayoutSize heightData;
        public int growPieces;
        public int shrinkPieces;
        public float baseWidth;
        public float baseHeight;
        public float availableSize;
        public bool grew;
        public bool shrunk;
        public int nextSiblingId;

    }

    public struct HorizontalSizeInfo { }

    public struct VerticalSizeInfo { }

    // layout child data
    // linked list owned by system
    // or per-box array (allocator helps here when I get one that works, block allocator is probably fine)
    // cannot collect on use because we need some style info

    // store size results somewhere

    // combine all size results & matcies into layoutresult at the end, or at least give it pointers to the data it needs

    public interface ILayoutHandler {

        float ResolveAutoWidth(LayoutBoxId elementId, UIMeasurement measurement);

    }

    // 1 per view makes sense to me
    // cannot share allocators though while running in paralle
    // but if all our lists allocator based anyway thats fine
    // we only need to alloc on children change which is single threaded anyway
    public struct BurstLayoutRunner {

        public ViewParameters viewParameters;

        public float ResolveWidth<T>(in T parent, LayoutBoxId elementId, float blockSize, in UIMeasurement measurement) where T : ILayoutHandler {
            float value = measurement.value;

            switch (measurement.unit) {
                case UIMeasurementUnit.Auto: {
                    return parent.ResolveAutoWidth(elementId, measurement.value);
                }

                case UIMeasurementUnit.Content: {
                    return GetContentWidth(elementId, measurement.value, blockSize);
                }

                case UIMeasurementUnit.Pixel:
                    return value;

                case UIMeasurementUnit.Em:
                    return 0; //element.style.GetResolvedFontSize() * value;

                case UIMeasurementUnit.ViewportWidth:
                    return viewParameters.viewWidth * value;

                case UIMeasurementUnit.ViewportHeight:
                    return viewParameters.viewHeight * value;

                case UIMeasurementUnit.BlockSize: {
                    // ignored elements can use the output size of their parent since it has been resolved already
                    return ComputeBlockWidth(measurement.value);
                }

                case UIMeasurementUnit.Percentage:
                case UIMeasurementUnit.ParentContentArea: {
                    return ComputeBlockContentAreaWidth(measurement.value);
                }
            }

            return 0;
        }

        private float ComputeBlockContentAreaWidth(float measurementValue) {
            throw new NotImplementedException();
            return 0;
        }

        private float ComputeBlockWidth(float measurementValue) {
            return 0;
        }

        private float GetContentWidth(LayoutBoxId layoutBoxId, float value, float measurementValue) {

            // if (contentWidthCache[elementId]) { }
            //
            // switch (layoutBoxId.layoutBoxType) {
            //     case LayoutType.Unset:
            //     case LayoutType.Flex:
            //         flexLayouts[layoutBoxId.instanceId].GetContentWidth(measurementValue) * value;
            //         break;
            //
            //     case LayoutType.Grid:
            //         break;
            //
            //     case LayoutType.Radial:
            //         break;
            //
            //     case LayoutType.Stack:
            //         break;
            //
            //     case LayoutType.Image:
            //         break;
            //
            //     case LayoutType.ScrollView:
            //         break;
            //
            //     case LayoutType.Text:
            //         break;
            //
            //     default:
            //         throw new ArgumentOutOfRangeException();
            // }
            return default;
        }

    }

    // public DataList<FlexItem> flexItems;
    // public DataList<FlexTracks> flexTracks;

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct FlexLayoutBoxBurst {

        // MUST BE FIRST OFFSET!!!!!!!!
        public LayoutType layoutType;

        public LayoutDirection direction;
        public LayoutWrap layoutWrap;

        public float alignHorizontal;
        public LayoutFit fitHorizontal;
        public SpaceDistribution horizontalDistribution;

        public LayoutFit fitVertical;
        public float alignVertical;
        public SpaceDistribution verticalDistribution;

        public ElementId elementId;
        public BurstLayoutRunner* burstLayoutSystem;
        public LayoutBoxId layoutBoxId;

        // public List_FlexItem flexItems;

        public void RunHorizontal() {
            // if (flexItems.size == 0) return;
            if (layoutWrap == LayoutWrap.WrapHorizontal) {
                // RunLayoutHorizontalStep_HorizontalDirection_Wrapped(frameId);
            }
            else {
                RunLayoutHorizontalStep_HorizontalDirection();
            }

        }

        // where is shared layout info stored? layout system makes sense to own this

        public void RunLayoutHorizontalStep_HorizontalDirection() { }

        public static void Initialize_Managed(ref FlexLayoutBoxBurst layoutInfo, UIElement element) {
            layoutInfo.elementId = element.id;
            layoutInfo.direction = element.style.FlexLayoutDirection;
            layoutInfo.layoutWrap = element.style.FlexLayoutWrap;
            layoutInfo.alignHorizontal = element.style.AlignItemsHorizontal;
            layoutInfo.alignVertical = element.style.AlignItemsVertical;
            layoutInfo.fitHorizontal = element.style.FitItemsHorizontal;
            layoutInfo.fitVertical = element.style.FitItemsVertical;
            layoutInfo.horizontalDistribution = element.style.DistributeExtraSpaceHorizontal;
            layoutInfo.verticalDistribution = element.style.DistributeExtraSpaceVertical;
        }

        public static void OnChildrenChanged_Managed(ref FlexLayoutBoxBurst layoutInfo, ElementId elementId, LayoutSystem layoutSystem) {

            ref LayoutHierarchyInfo layoutHierarchyInfo = ref layoutSystem.elementSystem.layoutHierarchyTable[elementId];

            int firstChildId = 1;
            int childCount = layoutHierarchyInfo.childCount;

            int ptr = firstChildId;

            // resize list if we need it
            // gather children into list

            while (ptr != 0) {

                // ptr = flexInfoDatabase.itemDataTable[ptr].nextSiblingId;

            }

        }

    }

}