using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Jobs;

namespace UIForia.Text {

    [BurstCompile]
    internal unsafe struct ProcessWhitespaceJob : IJob {

        public DataList<TextChange>.Shared textChanges;
        public DataList<BurstTextInfo>.Shared textInfoMap;
        public DataList<TextSymbol>.Shared buffer;

        public void Execute() {
            // for (int i = 0; i < textChanges.size; i++) {
            //
            //     ref BurstTextInfo textInfo = ref textInfoMap[textChanges[i].textInfoId];
            //
            //     buffer.size = 0;
            //
            //     TextUtil.ProcessWhiteSpace(textInfo.textStyle.whitespaceMode, textInfo.symbolList.array, textInfo.symbolList.size, ref buffer);
            //
            //     if (buffer.size != textInfo.symbolList.size) {
            //         textInfo.symbolList.CopyFrom(buffer.GetArrayPointer(), buffer.size);
            //     }
            //
            // }
        }

    }

}