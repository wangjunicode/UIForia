using UIForia.Elements;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Jobs;

namespace UIForia.Systems {

    /// <summary>
    /// This job takes all the elements that were enabled or created this frame and finds
    /// the highest (lowest depth) element for all hierarchies.
    /// </summary>
    [BurstCompile]
    public unsafe struct FindHierarchyRootElements : IJob {

        public DataList<ElementId>.Shared elements;
        public ElementTable<ElementTraversalInfo> traversalTable;
        public ElementTable<ElementMetaInfo> metaTable;
        public DataList<ElementId>.Shared roots;
        public UIElementFlags mask;

        public void Execute() {

            for (int i = 0; i < elements.size; i++) {

                if ((metaTable[elements[i]].flags & mask) != 0) {
                    roots.Add(elements[i]);
                }

            }

            DataList<ElementId> buffer = new DataList<ElementId>(roots.size, TypedUnsafe.GetTemporaryAllocatorLabel<ElementId>(roots.size));

            int bufferSize = 0;

            buffer[bufferSize++] = roots[0];

            for (int i = 1; i < roots.size; i++) {

                ref ElementTraversalInfo element = ref traversalTable[roots[i]];

                bool add = true;
                for (int j = 0; j < bufferSize; j++) {
                    ElementId target = buffer[j];
                    // if what is in the buffer is a descendent of 'this', replace the thing in the buffer
                    if (traversalTable[target].IsDescendentOf(element)) {
                        element = ref traversalTable[target];
                        buffer[j] = target;
                        add = false;
                    }
                    else if (traversalTable[target].IsAncestorOf(element)) {
                        add = false;
                    }
                }

                if (add) {
                    buffer[bufferSize++] = roots[i];
                }
            }
            
            roots.CopyFrom(buffer.GetArrayPointer(), bufferSize);

            buffer.Dispose();
        }

    }

}