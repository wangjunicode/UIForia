using System;
using UIForia.Layout;
using UIForia.Unsafe;
using UIForia.Util.Unsafe;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UIForia.Text {

    internal unsafe struct TextShapeCache : IDisposable {

        private const int k_InvalidFreeIndex = int.MaxValue;

        private UnsafeHashMap<ShapeCacheKey, int> shapeResultCache;

        [NativeDisableUnsafePtrRestriction] private State* state;

        public static TextShapeCache Create(int baseFrameId, int cacheLifeTime) {

            // todo -- when we gc the string tagger, we need to gc this too or we get mis matches in tag ids 
            return new TextShapeCache() {
                shapeResultCache = new UnsafeHashMap<ShapeCacheKey, int>(256, Allocator.Persistent),
                state = TypedUnsafe.Malloc(new State() {
                    allocator = ListAllocatorSized.CreateAllocator(32, 256, TypedUnsafe.Kilobytes(64)),
                    nextFreeIndex = k_InvalidFreeIndex,
                    shapeValues = new DataList<ShapeCacheValue>(256, Allocator.Persistent),
                }, Allocator.Persistent)
            };
        }

        public bool TouchEntry(in ShapeCacheKey cacheKey, out int cacheIndex, out float width) {
            if (shapeResultCache.TryGetValue(cacheKey, out int index)) {
                width = state->shapeValues[index].advanceWidth;
                cacheIndex = index;
                return true;
            }

            int nextId = state->nextFreeIndex;
            if (nextId == k_InvalidFreeIndex) {
                state->shapeValues.Add(new ShapeCacheValue());
                nextId = state->shapeValues.size - 1;
            }
            else {
                state->nextFreeIndex = state->shapeValues[nextId].nextFreeIndex;
                state->shapeValues[nextId] = new ShapeCacheValue();
            }

            shapeResultCache.Add(cacheKey, nextId);
            width = -1;
            cacheIndex = nextId;
            return false;
        }

        public void GarbageCollect(LongBoolMap removalMap) {

            // This isn't perfect collection but should be good enough. We only collect when a string tag is no longer in use
            // This means some cache key combo might not be in use but we won't release it yet because the string tag is used.
            // This is fine in almost all cases though and its faster / less error prone than the 'perfect' case. 

            UnsafeHashMap<ShapeCacheKey, int>.Enumerator enumerator = shapeResultCache.GetEnumerator();

            DataList<CollectionEntry> toCollect = new DataList<CollectionEntry>(removalMap.PopCount(), Allocator.Temp);

            while (enumerator.MoveNext()) {

                KeyValue<ShapeCacheKey, int> kvp = enumerator.Current;
                if (removalMap.Get(kvp.Key.stringTag)) {
                    // if (kvp.Key.stringTag != kvp.Value) {
                        // Debug.Log("values are different");
                    // }
                    // Debug.Log($"getting {kvp.Key.stringTag} and deleting {kvp.Value}, cache key: {kvp.Key}");
                    toCollect.Add(new CollectionEntry() {
                        nodeIndex = kvp.Value,
                        cacheKey = kvp.Key
                    });
                }

            }

            enumerator.Dispose();

            for (int i = 0; i < toCollect.size; i++) {
                ref CollectionEntry entry = ref toCollect.Get(i);
                // Debug.Log($"freeing node {entry.nodeIndex}");
                ref ShapeCacheValue node = ref state->shapeValues.Get(entry.nodeIndex);
                // we store both glyphInfos and glyphPositions in a single buffer that we allocate as bytes
                // the pointers are then computed off that base pointer. The capacity we store (and must free!) is in bytes
                state->allocator.Free(node.BasePointer, node.capacityInBytes);

                node = default;
                node.nextFreeIndex = state->nextFreeIndex;
                state->nextFreeIndex = entry.nodeIndex;
                
                shapeResultCache.Remove(entry.cacheKey);

            }

            toCollect.Dispose();

        }

        public void Dispose() {
            shapeResultCache.Dispose();
            if (state != null) {
                state->Dispose();
            }

            this = default;
        }

        private struct CollectionEntry {

            public int nodeIndex;
            public ShapeCacheKey cacheKey;

        }

        private struct State : IDisposable {

            public DataList<ShapeCacheValue> shapeValues;
            public ListAllocatorSized allocator;
            public int nextFreeIndex;

            public void Dispose() {
                allocator.Dispose();
                shapeValues.Dispose();
                this = default;
            }

        }

        public void SetEntry(in ShapeCacheKey cacheKey, in ShapeResult result, GlyphInfo* glyphInfos, GlyphPosition* glyphPositions) {

            if (!shapeResultCache.TryGetValue(cacheKey, out int nodeIndex)) {
#if DEBUG
                throw new Exception("Should have had a shape cache entry");
#endif
            }

            SetEntryAt(nodeIndex, result, glyphInfos, glyphPositions);

        }

        public void SetEntryAt(int shapeIndex, in ShapeResult result, GlyphInfo* glyphInfos, GlyphPosition* glyphPositions) {

            ref ShapeCacheValue node = ref state->shapeValues.Get(shapeIndex);

#if DEBUG
            if (!node.IsFree) {
                throw new Exception("Shape result was not free");
            }
#endif

            int glyphCount = (int) result.glyphCount;

            AllocatedList<byte> allocate = state->allocator.Allocate<byte>(
                (glyphCount * sizeof(GlyphPosition)) +
                (glyphCount * sizeof(GlyphInfo))
            );

            node.glyphCount = glyphCount;
            node.glyphInfos = (GlyphInfo*) (allocate.array);

            node.advanceWidth = result.advanceWidth;
            node.capacityInBytes = allocate.capacity;
            
            // we store both glyphInfos and glyphPositions in a single buffer that we allocate as bytes
            // the pointers are then computed off that base pointer. The capacity we store (and must free!) is in bytes

            TypedUnsafe.MemCpy(node.glyphInfos, glyphInfos, glyphCount);
            TypedUnsafe.MemCpy(node.Positions.array, glyphPositions, glyphCount);

        }

        public ShapeCacheValue GetEntryAt(int shapeResultIndex) {
            return state->shapeValues[shapeResultIndex];
        }

    }

    internal unsafe struct ShapeCacheValue {

        public GlyphInfo* glyphInfos;
        public int glyphCount;
        public float advanceWidth;
        public int capacityInBytes;

        public int nextFreeIndex;

        public bool IsFree => glyphInfos == null;

        public CheckedArray<GlyphInfo> Infos => new CheckedArray<GlyphInfo>(glyphInfos, glyphCount);

        public CheckedArray<GlyphPosition> Positions => new CheckedArray<GlyphPosition>((GlyphPosition*) (glyphInfos + glyphCount), glyphCount);
        
        public byte* BasePointer => (byte*)glyphInfos;

    }

}