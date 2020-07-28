using UIForia.ListTypes;
using UIForia.Systems;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace UIForia.Graphics {
    
    [BurstCompile]
    internal unsafe struct BuildMaterialPermutations : IJob {

        public List_Int32 materialPermutationIdList; // assume proper size already setup
        public DataList<DrawInfo2>.Shared drawList;
        public DataList<MaterialPermutation>.Shared permutationList;

        public void Execute() {

            IntMap<int> idxMap = new IntMap<int>(64, Allocator.Temp);
            DataList<MaterialOverrideSet> overrides = new DataList<MaterialOverrideSet>(32, Allocator.Temp);

            // use 0 index as valid but empty set
            overrides.Add(default);

            for (int i = 0; i < drawList.size; i++) {
                ref DrawInfo2 drawInfo = ref drawList[i];

                if (drawInfo.IsNonRendering() || drawInfo.drawType == DrawType2.UIForiaElement || drawInfo.drawType == DrawType2.UIForiaText) {
                    continue;
                }

                // for uiforia elements and text we wont use material properties and only need to different permutation
                // ids based on textures, so we can just skip this step for those and compare using texture ids directly

                // basic idea is to get a unique integer to use as an identifier for materialId paired with a unique override set

                // check if we have the given combination of overrides
                if (!TryGetPropertyOverrideId(ref overrides, drawInfo.materialOverrideCount, drawInfo.materialOverrideValues, out int overrideId)) {
                    overrideId = overrides.size;
                    overrides.Add(new MaterialOverrideSet() {
                        propertyCount = drawInfo.materialOverrideCount,
                        propertyOverrides = drawInfo.materialOverrideValues
                    });
                }

                int permutationId = BitUtil.SetHighLowBits(drawInfo.materialId.index, overrideId);

                // see if we have this id in our map, add it if we don't
                if (!idxMap.TryGetValue(permutationId, out int listIndex)) {
                    listIndex = permutationList.size;
                    permutationList.Add(new MaterialPermutation() {
                        materialId = drawInfo.materialId,
                        overrides = drawInfo.materialOverrideValues,
                        overrideCount = drawInfo.materialOverrideCount
                    });
                }

                // todo -- i want to be able to batch Elements and Text that have no texture bound along with any that do have it
                // how can I do that? flag on permutation id? check shape type
                // i know that the only material overrides will be textures
                // so i could just compare permutation ids and if high bits are equal and low bits are 0 on one of them then that could work
                materialPermutationIdList.array[i] = listIndex;

            }

            overrides.Dispose();
            idxMap.Dispose();
        }

        private struct MaterialOverrideSet {

            public int propertyCount;
            public MaterialPropertyOverride* propertyOverrides;

        }

        private bool TryGetPropertyOverrideId(ref DataList<MaterialOverrideSet> overrideList, int cnt, MaterialPropertyOverride* propertyOverrides, out int idx) {

            if (cnt == 0 || propertyOverrides == null) {
                idx = 0;
                return false;
            }

            MaterialOverrideSet* array = overrideList.GetArrayPointer();
            int size = overrideList.size;

            // maybe a map is better for this for large inputs
            // backwards since I expect multiple draws to use same material settings for immediate mode
            for (int i = size - 1; i >= 0; i--) {
                if (array[i].propertyOverrides == propertyOverrides) {
                    idx = i;
                    return true;
                }
            }

            NativeSortExtension.Sort(propertyOverrides, cnt);

            for (int i = 0; i < size; i++) {
                if (array[i].propertyCount == cnt && UnsafeUtility.MemCmp(propertyOverrides, array[i].propertyOverrides, sizeof(MaterialPropertyOverride) * cnt) == 0) {
                    idx = i;
                    return true;
                }
            }

            idx = -1;
            return false;

        }

    }

}