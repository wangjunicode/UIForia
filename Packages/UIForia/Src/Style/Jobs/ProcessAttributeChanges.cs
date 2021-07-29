using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace UIForia.Style {

    [BurstCompile(DisableSafetyChecks = UIApplication.DisableJobSafety)]
    internal unsafe struct ProcessAttributeChanges : IJob {

        public CheckedArray<RuntimeTraversalInfo> meta;
        public CheckedArray<StyledAttributeChange> changes;
        public CheckedArray<AttributeMemberTable> attrTables;

        public void Execute() {

            for (int i = 0; i < attrTables.size; i++) {

                ref AttributeMemberTable table = ref attrTables.array[i];

                if (table.elementIds.array == null) continue;

                for (int elementIter = 0; elementIter < table.elementIds.size; elementIter++) {
                    if (ElementSystem.IsDeadOrDisabled(table.elementIds[elementIter], meta)) {
                        table.elementIds[elementIter--] = table.elementIds[--table.elementIds.size];
                    }
                }

            }

            for (int i = 0; i < changes.size; i++) {
                StyledAttributeChange change = changes[i];
                if (ElementSystem.IsDeadOrDisabled(change.elementId, meta)) {
                    continue;
                }

                if (change.operation == StyledAttributeOperation.Remove) {
                    ref AttributeMemberTable table = ref attrTables.array[change.tagId];
                    if (table.elementIds.array == null) {
                        continue;
                    }

                    for (int elIter = 0; elIter < table.elementIds.size; elIter++) {
                        if (table.elementIds[elIter] == change.elementId) {
                            table.elementIds[elIter] = table.elementIds[--table.elementIds.size];
                            break;
                        }
                    }

                }
                else {
                    ref AttributeMemberTable table = ref attrTables.array[change.tagId];
                    if (table.elementIds.array == null) {
                        table.elementIds = new DataList<ElementId>(8, Allocator.Persistent);
                    }

                    table.elementIds.Add(change.elementId);
                }
            }

        }

    }

}