using System.Collections.Generic;
using UIForia.Elements;
using UIForia.Util.Unsafe;

namespace UIForia {

    public struct ElementMetaInfo {

        public byte generation;
        public byte someFlags;
        public UIElementFlags2 flags;

    }
    
    public unsafe struct ElementManager {

        private int size;
        private int capacity;
        private ListHandle queue;
        private ListHandle generationHandle;

        private const int k_MinFreeIndices = 1024;

        public ElementGenerationTable generationTable;

        // this should hold all of the per-element tables
        public Queue<int> indexQueue;

        private int maxIdx;
        private int idGenerator;
        public DataList<ElementMetaInfo>.Shared metaInfo;

        public ElementId CreateElement() {

            // how do we clear dead data out of indices / systems?
            // ideally destroying elements doesn't cause stop the world clean up
            // that said we do need to handle on destroy and on disable events so we'll traverse anyway

            // indices should check when adding
            // dont want to reverse search indices to clear them
            // its cheap enough to check are you alive? as the last phase of selector match for example
            // when do we have synchronous down time?
            // between frames?
            // when system is finished and next one runs
            // can schedule a cleanup job for a system while others are running and complete it lazily

            // attributes
            // selector targets
            // selector targeted by
            // animations
            // shared style
            // instance style
            // tag id
            // state
            // indices (attribute, state, style, tag, templateId, lexical templateId)

            // have list of dead elements per frame
            // have list of which indices dead elements were in (can compute) 
            // n map lookups per attribute per dead element, totally unordered
            // selectors need to check for dead & disabled elements somewhere
            // 

            int idx;
            if (indexQueue.Count >= k_MinFreeIndices) {
                idx = indexQueue.Dequeue();
            }
            else {
                idx = ++idGenerator;
                metaInfo.Add(default);
            }

            metaInfo[idx].flags = default;
            metaInfo[idx].someFlags = default;

            return new ElementId(idx, metaInfo[idx].generation);

        }

        public void DestroyElements(ElementId* elementIds, int count) {
            for (int i = 0; i < count; i++) {
                ref ElementMetaInfo meta = ref metaInfo[elementIds[i].index];
                meta.generation++;
                meta.flags = default;
                meta.someFlags = default;
                // todo -- figure out how to bulk add these
                indexQueue.Enqueue(elementIds[i].index);
            }

        }

        public void DestroyElement(ElementId elementId) {
            ref ElementMetaInfo meta = ref metaInfo[elementId.index];
            meta.generation++;
            meta.flags = default;
            meta.someFlags = default;
            indexQueue.Enqueue(elementId.index);
        }

        public void GarbageCollect() { }

        public bool IsAlive(ElementId elementId) {
            return ((byte*) generationHandle.array)[elementId.index] == elementId.generation;
        }

    }

}