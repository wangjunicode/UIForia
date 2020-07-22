using UIForia.ListTypes;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Jobs;

namespace UIForia.Graphics {

    [BurstCompile]
    internal unsafe struct BuildUIForiaMaterialBuffer : IJob {

        public DataList<DrawInfo2>.Shared drawList;

        public List_Int32 materialIdList;
        public DataList<UIForiaMaterialInfo>.Shared materialBuffer;

        public void Execute() {

            for (int i = 0; i < drawList.size; i++) {

                ref DrawInfo2 drawInfo = ref drawList[i];

                if (drawInfo.drawType == DrawType2.UIForiaElement) {
                    materialIdList[i] = materialBuffer.size;
                    materialBuffer.EnsureAdditionalCapacity(1);

                    ElementMaterialSetup* elementMaterialInfo = (ElementMaterialSetup*) drawInfo.materialData;
                    UIForiaMaterialInfo* ptr = materialBuffer.GetPointer(materialBuffer.size);
                    ptr->elementElementMaterial = elementMaterialInfo->materialInfo;
                    materialBuffer.size++;
                }
                else if (drawInfo.drawType == DrawType2.UIForiaText) {
                    materialIdList[i] = materialBuffer.size;
                    materialBuffer.EnsureAdditionalCapacity(1);
                    
                    TextMaterialSetup* elementMaterialInfo = (TextMaterialSetup*) drawInfo.materialData;
                    UIForiaMaterialInfo* ptr = materialBuffer.GetPointer(materialBuffer.size);
                    ptr->textMaterial = elementMaterialInfo->materialInfo;
                    materialBuffer.size++;

                }
                else if (drawInfo.drawType == DrawType2.UIForiaGeometry) {
                    materialIdList[i] = materialBuffer.size;
                    materialBuffer.EnsureAdditionalCapacity(1);

                }

            }

        }

    }

}