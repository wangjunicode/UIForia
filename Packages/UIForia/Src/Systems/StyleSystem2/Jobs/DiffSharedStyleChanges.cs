using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace UIForia {

    [BurstCompile]
    public unsafe struct DiffSharedStyleChanges : IJob {

        public ConvertedStyleList input_SharedStyleChangeList;
        public DataList<StyleStateElementId>.Shared output_AddedStyleStateList;
        public DataList<StyleStateElementId>.Shared output_RemovedStyleStateList;

        public void Execute() {

            PagedList<ConvertedStyleId> styleList = input_SharedStyleChangeList.GetIdList();

            DataList<StyleStatePair> oldStylePairs = new DataList<StyleStatePair>(16, Allocator.Temp);

            for (int i = 0; i < styleList.pageCount; i++) {

                PagedListPage<ConvertedStyleId> page = styleList.GetPage(i);

                for (int j = 0; j < page.size; j++) {

                    ConvertedStyleId item = page.array[j];

                    StyleStatePair* oldStyles = item.oldStyles;
                    StyleStatePair* newStyles = item.newStyles;

                    SetupOldStyles(ref oldStylePairs, oldStyles, item.oldStyleCount);

                    for (int newId = 0; newId < item.newStyleCount; newId++) {

                        if (!ContainsAndRemove(newStyles[newId], ref oldStylePairs)) {
                            output_AddedStyleStateList.Add(new StyleStateElementId() {
                                styleId = newStyles[newId].styleId,
                                state = newStyles[newId].state,
                                elementId = item.elementId
                            });
                        }

                    }

                    for (int oldId = 0; oldId < oldStylePairs.size; oldId++) {
                        output_RemovedStyleStateList.Add(new StyleStateElementId() {
                            styleId = oldStylePairs[oldId].styleId,
                            state = oldStylePairs[oldId].state,
                            elementId = item.elementId
                        });
                    }

                }

            }
        }

        private static bool ContainsAndRemove(in StyleStatePair newStyle, ref DataList<StyleStatePair> oldStylePairs) {
            for (int i = 0; i < oldStylePairs.size; i++) {
                if (oldStylePairs[i].val == newStyle.val) {
                    oldStylePairs.SwapRemove(i);
                    return true;
                }
            }

            return false;
        }

        private static void SetupOldStyles(ref DataList<StyleStatePair> oldStylePairs, StyleStatePair* oldStyles, int oldStyleCount) {
            oldStylePairs.SetSize(oldStyleCount);
            for (int i = 0; i < oldStyleCount; i++) {
                oldStylePairs[i] = oldStyles[i];
            }
        }

    }

}