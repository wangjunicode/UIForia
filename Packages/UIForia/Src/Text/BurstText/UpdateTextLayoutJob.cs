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
        internal DataList<TextInfo>.Shared textInfoMap;
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

                ref TextInfo textInfo = ref textInfoMap[textChanges[i].textInfoId];

                TextInfo.ProcessWhitespace(ref textInfo, ref symbolBuffer);

            }

            for (int i = start; i < end; i++) {

                ref TextInfo textInfo = ref textInfoMap[textChanges[i].textInfoId];

                layoutBuffer.size = 0;
                TextInfo.CreateLayoutSymbols(ref textInfo, ref layoutBuffer);

            }

            for (int i = start; i < end; i++) {

                ElementId elementId = textChanges[i].elementId;
                ref TextInfo textInfo = ref textInfoMap[textChanges[i].textInfoId];

                TextInfo.ComputeSize(fontAssetMap, ref textInfo, emTable[elementId].resolvedValue, ref measureState);

            }
            
            for (int i = 0; i < end; i++) {
                ElementId elementId = textChanges[i].elementId;
                ref TextInfo textInfo = ref textInfoMap[textChanges[i].textInfoId];

                // need to find the right time to run this
                // ill need to run this when text changes i guess
                // but also set textures / font assets on change
                // if its a span and it change we currently re-run everything anyway
                // i dont want to keep it that way though. should be able to 
                // programatically grab spans and change styles, add text, whatever
                // probably also use different rich processors 
                // right now i care about getting data setup to render it. so lets just rebuild in the painter every frame for now
                
                // span != rich text
                // span is accessible via code
                // rich text is not, just runs a traversal to build a text range
                // i think this is easy to support, just push a material info + fontInfo for spans

                
            }
            
            symbolBuffer.Dispose();
            layoutBuffer.Dispose();
            measureState.Dispose();
        }

    }

}