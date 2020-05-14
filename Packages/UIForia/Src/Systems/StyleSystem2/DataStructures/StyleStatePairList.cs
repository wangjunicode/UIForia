using System;
using UIForia.Util.Unsafe;
using Unity.Collections;

namespace UIForia {

    public unsafe struct StyleStatePairList : IPerThreadCompatible {

        private StyleStatePair* scratchBuffer;
        private PagedListState* stylePairBuffer;
        private PagedListState* styleStateUpdates;

        // these may need to be in an allocated buffer
        public int currentPairElementCount;
        public int addedPairElementCount;
        public int removedPairElementCount;
        
        public StyleStatePairList(Allocator allocator) : this() {
            InitializeForThread(allocator);
        }

        public void InitializeForThread(Allocator allocator) {
            scratchBuffer = TypedUnsafe.Malloc<StyleStatePair>(128, Allocator.TempJob); // 16 max styles * 4 max states * 2 (new and old both live in this buffer)
            styleStateUpdates = new PagedList<StylePairUpdate>(128, allocator).GetStatePointer();
            stylePairBuffer = new PagedList<StylePairUpdate>(512, allocator).GetStatePointer();
        }

        public bool IsInitialized {
            get => scratchBuffer != null
                   && styleStateUpdates != null
                   && stylePairBuffer != null;
        }

        public int GetElementCountForType(StylePairUpdateType updateType) {
            switch (updateType) {

                case StylePairUpdateType.Add:
                    return addedPairElementCount;

                case StylePairUpdateType.Remove:
                    return removedPairElementCount;

                case StylePairUpdateType.Current:
                    return currentPairElementCount;

                default:
                    return 0;
            }    
        }
        
        public StyleStatePair* GetScratchBuffer() {
            return scratchBuffer;
        }

        public PagedList<StylePairUpdate> GetStyleStateUpdateList() {
            return new PagedList<StylePairUpdate>(styleStateUpdates);
        }
        
        public void SetCurrentPairs(ElementId elementId, StyleStatePair* addedPairs, int count) {
            if (count == 0) return;
            currentPairElementCount += count;
            StyleStatePair* ptr = stylePairBuffer->AddRange(addedPairs, count);
            styleStateUpdates->AddItem(new StylePairUpdate(elementId, StylePairUpdateType.Current, ptr, count));
        }

        public void SetAddedPairs(ElementId elementId, StyleStatePair* addedPairs, int count) {
            if (count == 0) return;
            addedPairElementCount += count;
            StyleStatePair* ptr = stylePairBuffer->AddRange(addedPairs, count);
            styleStateUpdates->AddItem(new StylePairUpdate(elementId, StylePairUpdateType.Add, ptr, count));
        }

        public void SetRemovedPairs(ElementId elementId, StyleStatePair* removedPairs, int count) {
            if (count == 0) return;
            removedPairElementCount += count;
            StyleStatePair* ptr = stylePairBuffer->AddRange(removedPairs, count);
            styleStateUpdates->AddItem(new StylePairUpdate(elementId, StylePairUpdateType.Remove, ptr, count));
        }

        public void Dispose() {
            TypedUnsafe.Dispose(scratchBuffer, Allocator.TempJob);
            styleStateUpdates->Dispose();
            stylePairBuffer->Dispose();
            this = default;
        }

      

    }

}