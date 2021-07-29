using System;
using UIForia.ListTypes;
using UIForia.Unsafe;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace UIForia.Text {

    internal unsafe struct StringTagger : IEquatable<StringTagger> {

        [NativeDisableUnsafePtrRestriction] private StringTaggerState* state;

        private struct StringTaggerState : IDisposable {

            public int freeListIndex;
            public int frameId;
            public ListAllocatorSized stringAllocator;
            public DataList<StringTagInfo> nodes;
            public int cacheLifeTime;
            public UnsafeHashMap<UnsafeString, int> map;

            public void Dispose() {
                map.Dispose();
                stringAllocator.Dispose();
                nodes.Dispose();
                this = default;
            }

        }

        public void Serialize(ref ManagedByteBuffer writer) {

            writer.Write(state->freeListIndex);
            writer.Write(state->nodes.size);
            // todo -- if using cache life time marker, probably need to do something about it 
            for (int i = 0; i < state->nodes.size; i++) {
                ref StringTagInfo node = ref state->nodes[i];
                writer.Write(node.length);
                writer.Write(node.str, node.length);
                // writer.Write(node.lastTouched); // maybe dont bother serializing this 
            }

        }

        public static StringTagger Deserialize(ref ManagedByteBuffer reader) {

            StringTaggerState* state = TypedUnsafe.Malloc(new StringTaggerState(), Allocator.Persistent);

            reader.Read(out int freeListIndex);
            reader.Read(out int size);

            state->map = new UnsafeHashMap<UnsafeString, int>(128, Allocator.Persistent);
            state->nodes = new DataList<StringTagInfo>(size, Allocator.Persistent, NativeArrayOptions.ClearMemory);
            state->stringAllocator = ListAllocatorSized.CreateAllocator(4 * sizeof(char), 128 * sizeof(char));
            state->freeListIndex = freeListIndex;
            state->cacheLifeTime = int.MaxValue;
            state->frameId = 0;

            state->nodes.size = size;

            for (int i = 0; i < size; i++) {

                reader.Read(out int length);

                if (length == 0) continue;

                AllocatedList<char> allocated = state->stringAllocator.Allocate<char>(length);
                reader.Read(allocated.array, length);
                UnsafeString addNode = new UnsafeString(allocated.array, length);
                state->map.Add(addNode, i);
                state->nodes[i] = new StringTagInfo(0, allocated.array, length, allocated.capacity);
            }

            return new StringTagger() {
                state = state
            };
            
        }

        public static StringTagger Create(int baseFrameId = 0, int cacheLifeTime = 1) {
            return new StringTagger {
                state = TypedUnsafe.Malloc(new StringTaggerState() {
                    map = new UnsafeHashMap<UnsafeString, int>(256, Allocator.Persistent),
                    nodes = new DataList<StringTagInfo>(256, Allocator.Persistent),
                    freeListIndex = int.MaxValue,
                    cacheLifeTime = cacheLifeTime,
                    frameId = math.max(0, baseFrameId),
                    stringAllocator = ListAllocatorSized.CreateAllocator(4 * sizeof(char), 128 * sizeof(char))
                }, Allocator.Persistent),
            };
        }

        // Creates a new tag id, reusing old ones if we have any to recycle
        private int CreateTagId(char* str, int strLen, int capacity) {
            if (state->freeListIndex == int.MaxValue) {
                state->nodes.Add(new StringTagInfo(state->frameId, str, strLen, capacity));
                return state->nodes.size - 1;
            }

            int nextFreeIndex = state->nodes[state->freeListIndex].NextFreeIndex;
            int retn = state->freeListIndex;
            state->nodes[state->freeListIndex] = new StringTagInfo(state->frameId, str, strLen, capacity);
            state->freeListIndex = nextFreeIndex;
            return retn;
        }

        public int TagString(string str) {
            fixed (char* cbuffer = str) {
                return TagString(cbuffer, str.Length);
            }
        }

        public int TagString(char* str, RangeInt range) {
            return TagString(str + range.start, range.length);
        }

        public string GetString(int tagId) {

            if (tagId >= 0 && tagId < state->nodes.size) {
                return new string(state->nodes[tagId].str, 0, state->nodes[tagId].length);
            }

            return null;

        }

        public int TagString(char* str, int length) {
            // the check node can just use the pointer we are given
            UnsafeString node = new UnsafeString(str, length);

            if (!state->map.TryGetValue(node, out int tagIndex)) {
                // if target string was not already in the map we need to add it and allocate some memory to hold the data
                AllocatedList<char> charList = state->stringAllocator.Allocate<char>(length);
                UnsafeString addNode = new UnsafeString(charList.array, length);
                // the add node needs to make a copy of the input string 
                TypedUnsafe.MemCpy(charList.array, str, length);
                tagIndex = CreateTagId(charList.array, length, charList.capacity);
                state->map.Add(addNode, tagIndex);
            }
            else {
                state->nodes[tagIndex].lastTouched = state->frameId;
            }

            return tagIndex;
        }

        /// <summary>
        /// Removes any tags from the system that are older than `collectionThreshold` frames
        /// </summary>
        /// <param name="frameId">Current Frame Id</param>
        /// <param name="collectionThreshold">How many frames old a tag must be before we remove it from the system</param>
        public void GarbageCollect(int frameId) {

            state->frameId = frameId;

            int minAlloc = state->stringAllocator.minAllocSize / 2; // division accounts for size of char

            for (int i = 0; i < state->nodes.size; i++) {

                ref StringTagInfo tagNode = ref state->nodes.Get(i);

                if (tagNode.IsFree) continue;

                if (tagNode.lastTouched < frameId - state->cacheLifeTime) {

                    state->map.Remove(new UnsafeString(tagNode.str, tagNode.length));
                    state->stringAllocator.Free(tagNode.str, math.max(minAlloc, math.ceilpow2(tagNode.length)));
                    tagNode.str = null;
                    tagNode.lastTouched = 0;
                    tagNode.NextFreeIndex = state->freeListIndex;
                    state->freeListIndex = i;

                }

            }

        }

        public int GetRemovalMapSize() {
            return LongBoolMap.GetMapSize(state->nodes.size);
        }
        
        public void GarbageCollectWithRemovalMap(int frameId, LongBoolMap removalMap) {

            state->frameId = frameId;

            for (int i = 0; i < state->nodes.size; i++) {

                ref StringTagInfo tagNode = ref state->nodes.Get(i);

                if (tagNode.IsFree) continue;

                if (tagNode.lastTouched < frameId - state->cacheLifeTime) {
                    removalMap.Set(i);
                    state->map.Remove(new UnsafeString(tagNode.str, tagNode.length));
                    // Debug.Log($"Collecting string id:{i}, `{new UnsafeString(tagNode.str, tagNode.length)}`");
                    state->stringAllocator.Free(tagNode.str, tagNode.capacity); // math.ceilpow2(tagNode.length));
                    tagNode.str = null;
                    tagNode.lastTouched = 0;
                    tagNode.NextFreeIndex = state->freeListIndex;
                    state->freeListIndex = i;

                }

            }

        }
        public void Dispose() {
            state->Dispose();
            this = default;
        }

        public int ActiveTagCount() {
            return state->map.Count();
        }

        public void RefreshTag(int tagIndex) {
            state->nodes[tagIndex].lastTouched = state->frameId;
        }

        public int GetTagId(string str) {
            fixed (char* cbuffer = str) {
                UnsafeString node = new UnsafeString(cbuffer, str.Length);

                if (state->map.TryGetValue(node, out int tagIndex)) {
                    return tagIndex;
                }

                return -1;
            }
        }

        public int GetTagId(char* cbuffer, int length) {
            UnsafeString node = new UnsafeString(cbuffer, length);
            if (state->map.TryGetValue(node, out int tagIndex)) {
                return tagIndex;
            }

            return -1;
        }

        public int GetTagId(char[] buffer, int length) {
            fixed (char* cbuffer = buffer) {
                UnsafeString node = new UnsafeString(cbuffer, length);

                if (state->map.TryGetValue(node, out int tagIndex)) {
                    return tagIndex;
                }

                return -1;
            }
        }

        public int TagString(char[] buffer, int size) {
            fixed (char* cbuffer = buffer) {
                return TagString(cbuffer, size);
            }
        }

        public void Clear() {
            state->map.Clear();
            state->nodes.size = 0;
            state->freeListIndex = int.MaxValue;
            state->stringAllocator.Clear();
        }

        public void GetTaggedStrings(LightList<string> tags) {

            for (int i = 0; i < state->nodes.size; i++) {
                ref StringTagInfo tagNode = ref state->nodes.Get(i);
                if (tagNode.IsFree) continue;
                tags.Add(new string(tagNode.str, 0, tagNode.length));
            }

        }

        public int GetTagId(CharSpan span) {
            return GetTagId(span.data + span.rangeStart, span.Length);
        }

        public bool Equals(StringTagger other) {
            if (other.state->nodes.size != state->nodes.size) return false;
            for (int i = 0; i < state->nodes.size; i++) {
                StringTagInfo node = state->nodes[i];
                StringTagInfo cmpNode = other.state->nodes[i];
                if (node.IsFree != cmpNode.IsFree) {
                    return false;
                }

                if(node.IsFree) continue;
                if (node.length != cmpNode.length) return false;
                if (!TypedUnsafe.MemCmp(node.str, cmpNode.str, node.length)) {
                    return false;
                }
            }

            return true;
        }

        public override bool Equals(object obj) {
            return obj is StringTagger other && Equals(other);
        }

        public override int GetHashCode() {
            return unchecked((int) (long) state);
        }

    }

}