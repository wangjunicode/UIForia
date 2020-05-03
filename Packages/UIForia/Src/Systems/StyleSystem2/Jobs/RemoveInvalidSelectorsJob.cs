using System.Runtime.InteropServices;
using UIForia.Elements;
using UIForia.Style;
using UIForia.Util.Unsafe;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine;

namespace UIForia {

    public unsafe struct SelectorRunData {

        public SelectorId selectorId;
        public VertigoSelectorQuery query;
        public int hostElementId;

    }


    [StructLayout(LayoutKind.Explicit)]
    public struct SelectorKey {

        [FieldOffset(0)] public readonly long longVal;
        [FieldOffset(0)] public readonly int styleSetId;
        [FieldOffset(4)] public readonly int selectorId;

        public SelectorKey(int styleSetId, int selectorId) {
            this.longVal = 0;
            this.styleSetId = styleSetId;
            this.selectorId = selectorId;
        }

    }

    public struct SelectorResultRange {

        public int elementId;
        public int selectorId;
        public RangeInt targetRange;

    }

    // todo -- can optimize this later on 

    public struct SelectorInfo {

        public SelectorKey key;

    }

    public struct SelectorRemovalData {

        public int styleSetId;
        public RangeInt eventRange;
        public RangeInt targetRange;
        public int index;

    }

    // somewhere need to store unique selector effects for elementId + selectorId
    // and need to write it back 
    // id isn't a good key

    // search maybe?
    // lastFrameEffects sort by selectorId -> element 
    // and also elementId -> selectorId
    // key = long value = long

    // key = elementId + unique selector id as long

    // allocate? then know selector/element -> target
    // allocate so each host has list of targets & selectorIds
    // still doesnt tell me wht is affecting element x
    // for that we probably do want to copy + sort
    // so we keep 2 lists, 1 for selectors -> targets this frame
    // and 1 for elements -> selectors effected
    public unsafe struct RemoveInvalidSelectorsJob : IJob { // ranged batched job

        public readonly UnmanagedPagedList<VertigoStyle> styles;
        public readonly UnmanagedPagedList<VertigoSelector> selectors;
        public readonly BufferList<SelectorKey> sortedActiveSelectors;

        [ReadOnly] public NativeList<StyleStatePair> removedList;
        [WriteOnly] public NativeList<SelectorRemovalData> deadSelectors;

        // 3 approaches ->
        // double MultiHashMap per job 
        //        1 for elementId -> List<selectorId>
        //        1 for selectorId -> List<elementId>
        //        merge step 
        // double MultiHashMap.Parallel
        //        same as above, no merge step
        //        convert back to binary searchable list for reading
        // 1 list of pair elementId + selector Id
        //    clone + sort by elementid for effect list

        public void Execute() {

            BufferList<StyleStatePair> temp = new BufferList<StyleStatePair>(removedList.Length, Allocator.TempJob);

            for (int removedIdx = 0; removedIdx < removedList.Length; removedIdx++) {

                StyleStatePair removed = removedList[removedIdx];

                // if (removed.styleId.HasSelectorsInState(removed.state)) {
                //     temp.AddUnchecked(removed);
                // }

            }

            // gather selectors to remove here & pump into list

            for (int removedIdx = 0; removedIdx < temp.size; removedIdx++) {

                StyleStatePair toRemove = temp.array[removedIdx];
                VertigoStyle style = styles[toRemove.styleId.index];
                VertigoSelector* selectorPtr = default; //selectors.array + style.selectorOffset;

                for (int i = 0; i < style.selectorCount; i++) {
                    ref VertigoSelector selector = ref selectorPtr[i];
                    //
                    // if (selector.id.state != toRemove.state) {
                    //     continue;
                    // }

                    // SelectorKey key = new SelectorKey(toRemove.styleSetId, selector.id); // todo -- make styleId not contain StyleSheet data -> this is really debug data that is easily backtracked
                    // deadSelectors.Add(new SelectorRemovalData() {
                    //     index = FindIndex(key, sortedActiveSelectors),
                    //     eventRange = new RangeInt(selector.eventOffset, selector.eventCount)
                    // });
                }

            }

            temp.Dispose();

        }

        // keys in one array
        // data in another 
        private static int FindIndex(SelectorKey key, BufferList<SelectorKey> sortedSearchList) {
            int elementsPerCacheLine =  JobsUtility.CacheLineSize / sizeof(SelectorKey);
            int start = elementsPerCacheLine;
            int end = sortedSearchList.size - 1;
            
            // this literally free since array start is already in cache, no miss here
            for (int i = 0; i < elementsPerCacheLine; i++) {
                if (sortedSearchList.array[i].longVal == key.longVal) {
                    return i;
                }
            }

            // first hit of binary search will look at the middle element, which will load that cache line anyway
            // we can just linearly search here for free since this cache line will definitely get loaded
            int mid = end >> 1;
            for (int i = mid; i < mid + elementsPerCacheLine; i++) {
                if (sortedSearchList.array[i].longVal == key.longVal) {
                    return i;
                }
            }
            
            // this is normal binary search from here on out but we've reduced the hop count (probably) because of above checks
            while (start <= end) {
                int index = start + (end - start >> 1);

                if (sortedSearchList.array[index].longVal < key.longVal) {
                    start = index + 1;
                }
                else if (sortedSearchList.array[index].longVal > key.longVal) {
                    end = index - 1;
                }
                else {
                    return index;
                }

            }

            return ~start; // should never hit this because I know the target is in the search list
        }

    }

}