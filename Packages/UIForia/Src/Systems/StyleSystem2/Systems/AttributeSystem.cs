using System;
using System.Diagnostics;
using UIForia.Util.Unsafe;
using Unity.Collections;

namespace UIForia {

    public struct AttributeInfoDebugView {

        public string key;
        public string value;

        public AttributeInfoDebugView(AttributeInfo target) {
            this.key = StringInternSystem.Debug?.GetString(target.keyIndex);
            this.value = StringInternSystem.Debug?.GetString(target.valueIndex);
        }

    }

    [DebuggerTypeProxy(typeof(AttributeInfoDebugView))]
    public struct AttributeInfo {

        public int keyIndex;
        public int valueIndex;

    }

    public unsafe class AttributeSystem : IDisposable {

        internal IntMap<List_ElementId> index;
        internal StringInternSystem internSystem;
        internal ListAllocator<ElementId> indexAllocator;
        internal ListAllocator<AttributeInfo> attrListAllocator;
        internal IntMap<List_AttributeInfo> map_ElementAttributes;
        internal ElementSystem elementSystem;
        
        public AttributeSystem(StringInternSystem internSystem, ElementSystem elementSystem) {
            this.internSystem = internSystem;
            this.elementSystem = elementSystem;
            
            FixedAllocatorDesc* attrAllocators = stackalloc FixedAllocatorDesc[3];
            attrAllocators[0] = new FixedAllocatorDesc<AttributeInfo>(4, 64, 1);
            attrAllocators[1] = new FixedAllocatorDesc<AttributeInfo>(8, 32, 0);
            attrAllocators[2] = new FixedAllocatorDesc<AttributeInfo>(16, 8, 0);

            FixedAllocatorDesc* indexAllocators = stackalloc FixedAllocatorDesc[4];
            indexAllocators[0] = new FixedAllocatorDesc<ElementId>(8, 32, 1);
            indexAllocators[1] = new FixedAllocatorDesc<ElementId>(16, 32, 1);
            indexAllocators[2] = new FixedAllocatorDesc<ElementId>(32, 8, 0);
            indexAllocators[3] = new FixedAllocatorDesc<ElementId>(64, 8, 0);

            this.attrListAllocator = ListAllocator<AttributeInfo>.Create(attrAllocators, 3);
            this.indexAllocator = ListAllocator<ElementId>.Create(indexAllocators, 4);

            this.index = new IntMap<List_ElementId>(54, Allocator.Persistent);
            this.map_ElementAttributes = new IntMap<List_AttributeInfo>(64, Allocator.Persistent);
        }

        public void CleanupElement(ElementId elementId) {

            if (map_ElementAttributes.TryRemove(elementId.id, out List_AttributeInfo attributes)) {
                // todo -- maybe enqueue the attribute ids to have their indices updated when garbage collecting, 1 list just sort to dedup, shouldn't be too large per frame
                attrListAllocator.Free(ref attributes);
            }

        }

        private void AddToIndex(ElementId elementId, int keyIndex) {

            ref List_ElementId elementIds = ref index.GetOrCreateReference(keyIndex);
            if (elementIds.size + 1 > elementIds.capacity) {
                elementIds.size = elementSystem.RemoveDeadElements(elementIds.array, elementIds.size);
            }

            indexAllocator.Add(ref elementIds, elementId);

        }

        private void RemoveFromIndex(int keyIndex, ElementId elementId) {

            if (!index.TryGetPointer(keyIndex, out List_ElementId* elementIds)) {
                return;
            }

            for (int i = 0; i < elementIds->size; i++) {
                
                if (elementIds->array[i].id != elementId.id) {
                    continue;
                }
                
                indexAllocator.Remove(elementIds, i);

                if (elementIds->size == 0) {
                    index.Remove(keyIndex);
                }

                return;
            }

        }

        public int GetAttributeCount(ElementId elementId) {

            //if (!generationTable.IsAlive(elementId)) {
            //    return default;
            //}

            if (map_ElementAttributes.TryGetPointer(elementId.id, out List_AttributeInfo* attributes)) {
                return attributes->size;
            }

            return 0;

        }

        public string GetAttribute(ElementId elementId, string key) {
            if (string.IsNullOrEmpty(key)) {
                return default;
            }

            //if (!generationTable.IsAlive(elementId)) {
            //    return default;
            //}

            int keyIdx = internSystem.GetIndex(key);

            if (keyIdx < 0) return null;

            if (map_ElementAttributes.TryGetPointer(elementId.id, out List_AttributeInfo* attributes)) {

                for (int i = 0; i < attributes->size; i++) {
                    if (attributes->array[i].keyIndex == keyIdx) {
                        return internSystem.GetString(attributes->array[i].valueIndex);
                    }
                }
            }

            return null;

        }

        public void InitializeAttributes(ElementId elementId, int attrCount) {
            if (attrCount <= 0) return;
            // todo -- allocate space for attrCount attributes
            // maybe use linked list for attributes since we almost never iterate them
        }
        
        public void SetAttribute(ElementId elementId, string key, string value) {

            if (value == null) {
                RemoveAttribute(elementId, key);
                return;
            }

            ref List_AttributeInfo attributes = ref map_ElementAttributes.GetOrCreateReference(elementId.id);

            int keyIdx = internSystem.Add(key);

            for (int i = 0; i < attributes.size; i++) {
                ref AttributeInfo attr = ref attributes.array[i];

                if (attr.keyIndex != keyIdx) {
                    continue;
                }

                if (internSystem.GetString(attr.valueIndex) != value) {
                    internSystem.Remove(attr.valueIndex);
                    attr.valueIndex = internSystem.Add(value);
                }

                return;

            }

            AttributeInfo attributeInfo = new AttributeInfo {
                keyIndex = keyIdx,
                valueIndex = internSystem.Add(value)
            };

            attrListAllocator.Add(ref attributes, attributeInfo);

            AddToIndex(elementId, keyIdx);

        }

        public void RemoveAttribute(ElementId elementId, string key) {

            if (!map_ElementAttributes.TryGetPointer(elementId.id, out List_AttributeInfo* attributes)) {
                return;
            }

            int keyIdx = internSystem.Add(key);

            for (int i = 0; i < attributes->size; i++) {
                ref AttributeInfo attr = ref attributes->array[i];

                if (attr.keyIndex != keyIdx) {
                    continue;
                }

                internSystem.Remove(attr.keyIndex);
                internSystem.Remove(attr.valueIndex);

                attrListAllocator.Remove(attributes, i);

                if (attributes->size == 0) {
                    map_ElementAttributes.Remove(elementId.id);
                }

                RemoveFromIndex(keyIdx, elementId);

                return;

            }

        }

        public void Dispose() {
            index.Dispose();
            map_ElementAttributes.Dispose();
            attrListAllocator.Dispose();
            indexAllocator.Dispose();
        }

        public BurstableAttributeDatabase GetBurstableDatabase() {
            return new BurstableAttributeDatabase() {
                burstValues = internSystem.burstValues
            };
        }

        internal List_ElementId GetIndexForAttribute(string key) {
            int keyIdx = internSystem.GetIndex(key);
            index.TryGetValue(keyIdx, out List_ElementId idList);
            return idList;
        }


    }

    // not disposed! just a wrapper around a borrowed pointer
    public struct BurstableAttributeDatabase {

        public DataList<StringHandle>.Shared burstValues;

        public StringHandle Get(int stringId) {
            return burstValues[stringId];
        }

    }

}