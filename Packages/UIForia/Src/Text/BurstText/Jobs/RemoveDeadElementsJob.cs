using UIForia.Util.Unsafe;
using Unity.Jobs;

namespace UIForia.Text {

    internal struct RemoveDeadElementsJob : IJob {

        public ElementTable<ElementMetaInfo> metaTable;
        public DataList<TextChange>.Shared textChanges;

        public void Execute() {
            for (int i = 0; i < textChanges.size; i++) {

                // ids were already recycled on element destruction
                if (ElementSystem.IsDeadOrDisabled(textChanges[i].elementId, metaTable)) {
                    textChanges[i--] = textChanges[--textChanges.size];
                }

            }

        }

    }

}