using UIForia.Layout;
using UIForia.Rendering;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace UIForia.Text {

    [BurstCompile]
    public unsafe struct UpdateTextLayoutJob : IJob, IVertigoParallel {

        public float viewportWidth;
        public float viewportHeight;

        internal ElementTable<EmValue> emTable;
        internal DataList<TextChange>.Shared textChanges;
        internal DataList<TextInfo> textInfoMap;
        internal DataList<FontAssetInfo>.Shared fontAssetMap;

        public ParallelParams parallel { get; set; }

        public void Execute() {
            Run(0, textChanges.size);
        }

        public void Execute(int startIndex, int count) {
            Run(startIndex, startIndex + count);
        }

        private void Run(int start, int end) {

            DataList<TextSymbol> symbolBuffer = new DataList<TextSymbol>(128, Allocator.Temp);
            DataList<TextLayoutSymbol> layoutBuffer = new DataList<TextLayoutSymbol>(128, Allocator.Temp);

            TextMeasureState measureState = new TextMeasureState(Allocator.Temp) {
                viewportWidth = viewportWidth,
                viewportHeight = viewportHeight
            };

            for (int i = start; i < end; i++) {

                ref TextInfo textInfo = ref textChanges[i].textInfo[0];

                TextInfo.ProcessWhitespace(ref textInfo, ref symbolBuffer);

            }

            for (int i = start; i < end; i++) {

                ref TextInfo textInfo = ref textChanges[i].textInfo[0];

                layoutBuffer.size = 0;
                TextInfo.CreateLayoutSymbols(ref textInfo, ref layoutBuffer);

            }

            for (int i = start; i < end; i++) {

                ElementId elementId = textChanges[i].elementId;
                ref TextInfo textInfo = ref textChanges[i].textInfo[0];

                TextInfo.ComputeSize(fontAssetMap, ref textInfo, emTable[elementId].resolvedValue, ref measureState);

            }
            
            for (int i = start; i < end; i++) {

                TextInfo.CountRenderedCharacters(ref textChanges[i].textInfo[0]);

            }
            
            symbolBuffer.Dispose();
            layoutBuffer.Dispose();
            measureState.Dispose();
        }

    }

}