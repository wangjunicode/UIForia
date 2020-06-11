using UIForia.Systems;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Jobs;

namespace UIForia.Layout {

    [BurstCompile]
    public unsafe struct UpdatePaddingBorderMargin : IJob, IVertigoParallel {

        public ElementTable<EmValue> emTable;
        public ElementTable<PaddingBorderMargin> propertyTable;
        public ElementTable<LayoutInfo> horizontalLayoutInfo;
        public ElementTable<LayoutInfo> verticalLayoutInfo;
        public ElementTable<LayoutBoxInfo> layoutResultTable;
        public ViewParameters viewParameters;

        public DataList<ElementId>.Shared elementList;
        public ParallelParams parallel { get; set; }

        public void Execute() {
            Run(0, elementList.size);
        }

        public void Execute(int startIndex, int count) {
            Run(startIndex, startIndex + count);
        }

        private void Run(int start, int end) {

            for (int i = start; i < end; i++) {

                ElementId elementId = elementList[i];

                int index = elementId.index;

                ref PaddingBorderMargin properties = ref propertyTable.array[index];
                ref LayoutInfo verticalInfo = ref verticalLayoutInfo.array[index];
                ref LayoutInfo horizontalInfo = ref horizontalLayoutInfo.array[index];
                ref LayoutBoxInfo layoutResult = ref layoutResultTable.array[index];

                float emSize = emTable.array[index].resolvedValue;

                verticalInfo.emSize = emSize;
                horizontalInfo.emSize = emSize;

                float marginTop = MeasurementUtil.ResolveFixedLayoutSize(viewParameters, emSize, properties.marginTop);
                float marginBottom = MeasurementUtil.ResolveFixedLayoutSize(viewParameters, emSize, properties.marginBottom);
                float marginRight = MeasurementUtil.ResolveFixedLayoutSize(viewParameters, emSize, properties.marginRight);
                float marginLeft = MeasurementUtil.ResolveFixedLayoutSize(viewParameters, emSize, properties.marginLeft);

                float borderTop = MeasurementUtil.ResolveFixedLayoutSize(viewParameters, emSize, properties.borderTop);
                float borderBottom = MeasurementUtil.ResolveFixedLayoutSize(viewParameters, emSize, properties.borderBottom);
                float borderRight = MeasurementUtil.ResolveFixedLayoutSize(viewParameters, emSize, properties.borderRight);
                float borderLeft = MeasurementUtil.ResolveFixedLayoutSize(viewParameters, emSize, properties.borderLeft);

                float paddingTop = MeasurementUtil.ResolveFixedLayoutSize(viewParameters, emSize, properties.paddingTop);
                float paddingBottom = MeasurementUtil.ResolveFixedLayoutSize(viewParameters, emSize, properties.paddingBottom);
                float paddingRight = MeasurementUtil.ResolveFixedLayoutSize(viewParameters, emSize, properties.paddingRight);
                float paddingLeft = MeasurementUtil.ResolveFixedLayoutSize(viewParameters, emSize, properties.paddingLeft);

                // todo -- still doesn't handle animated case, will need to store each property twice with animation info :(

                verticalInfo.marginStart = marginTop;
                verticalInfo.marginEnd = marginBottom;
                verticalInfo.paddingBorderStart = paddingTop + borderTop;
                verticalInfo.paddingBorderEnd = paddingBottom + borderBottom;
                
                if (horizontalInfo.marginStart != marginLeft || horizontalInfo.marginEnd != marginRight) {
                    LayoutUtil.MarkForContentSizeChange(elementId, default, horizontalLayoutInfo);
                }
                
                if (verticalInfo.marginStart != marginTop || verticalInfo.marginEnd != marginBottom) {
                    LayoutUtil.MarkForContentSizeChange(elementId, default, verticalLayoutInfo);
                }

                if (horizontalInfo.paddingBorderStart != paddingLeft + borderLeft || horizontalInfo.paddingBorderEnd != paddingRight + borderRight) {
                    
                }
                
                horizontalInfo.marginStart = marginLeft;
                horizontalInfo.marginEnd = marginRight;
                horizontalInfo.paddingBorderStart = paddingLeft + borderLeft;
                horizontalInfo.paddingBorderEnd = paddingRight + borderRight;

                // if padding border changed -> content width changed
                // if margin changed -> tell parent
                
                
                // if these aren't used frequently I can skip this step and just re-compute when needed on demand
                // this is about 25% of the run time of the job
                layoutResult.margin.top = marginTop;
                layoutResult.margin.right = marginRight;
                layoutResult.margin.bottom = marginBottom;
                layoutResult.margin.left = marginLeft;
                
                layoutResult.border.top = borderTop;
                layoutResult.border.right = borderRight;
                layoutResult.border.bottom = borderBottom;
                layoutResult.border.left = borderLeft;
                
                layoutResult.padding.top = paddingTop;
                layoutResult.padding.right = paddingRight;
                layoutResult.padding.bottom = paddingBottom;
                layoutResult.padding.left = paddingLeft;

            }

        }

    }

}