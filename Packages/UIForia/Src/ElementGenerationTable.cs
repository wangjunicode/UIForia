using UIForia.Util.Unsafe;

namespace UIForia {

    public unsafe struct ElementGenerationTable {

        public DataList<byte>.Shared table;

        public bool IsAlive(ElementId elementId) {
            return table[elementId.index] == elementId.generation;
        }

        public int RemoveDeadElements(ElementId* elementIds, int count) {
            for (int i = 0; i < count; i++) {

                if (table[elementIds[i].index] != elementIds[i].generation) {
                    elementIds[i--] = elementIds[--count];
                }

            }

            return count;
        }

    }

}