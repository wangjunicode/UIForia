using UIForia.ListTypes;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace UIForia.Text {

    [BurstCompile]
    internal unsafe struct CreateLayoutSymbols : IJob {

        public DataList<TextChange>.Shared textChanges;
        public DataList<BurstTextInfo>.Shared textInfoMap;
        public DataList<TextLayoutSymbol>.Shared buffer;

        public void Execute() {
            for (int i = 0; i < textChanges.size; i++) {

                ref BurstTextInfo textInfo = ref textInfoMap[textChanges[i].textInfoId];

                buffer.size = 0;
                TextUtil.CreateLayoutSymbols(textInfo.symbolList.array, textInfo.symbolList.size, buffer);

                if (textInfo.layoutSymbolList.array == null) {
                    textInfo.layoutSymbolList = new List_TextLayoutSymbol(buffer.size, Allocator.Persistent);
                }

                textInfo.layoutSymbolList.CopyFrom(buffer.GetArrayPointer(), buffer.size);

            }
        }

    }

}