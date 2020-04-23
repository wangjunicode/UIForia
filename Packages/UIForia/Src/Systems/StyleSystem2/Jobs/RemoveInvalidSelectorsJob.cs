using System;
using System.Runtime.InteropServices;
using UIForia.Elements;
using UIForia.Selectors;
using UIForia.Style;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace UIForia {

    public struct SelectorHook {

        public long selectorId;
        public int hookIndex;
        public RangeInt targetRange;

    }

    // todo -- convert [enter] etc to just be self matching selectors

    public unsafe struct AddNewSelectorsJob : IJob { // ranged batched job

        public UnsafeList<VertigoStyle> styles;
        public NativeList<int> freeList;
        public NativeList<StyleStateGroup> addedList;
        public NativeList<StyleStateGroup> appendList;
        public NativeList<SelectorId> activeSelectorList; // todo -- type is wrong 
        public NativeList<SelectorHook> enterHooks;

        public void Execute() {

            UnsafeList<StyleStateGroup> temp = new UnsafeList<StyleStateGroup>(addedList.Length, Allocator.TempJob);

            for (int addedIndex = 0; addedIndex < addedList.Length; addedIndex++) {

                if (freeList.Length > 0) { }

                StyleStateGroup added = addedList[addedIndex];

                if (added.styleId.HasSelectorsInState(added.state)) {
                    temp.AddUnchecked(added);
                }

            }

            temp.Dispose();

        }

    }

    public unsafe struct SelectorRunData {

        public SelectorId selectorId;
        public VertigoSelectorQuery query;
        public int hostElementId;

    }

    public unsafe struct TraversalInfo {

        public bool isEnabled;
        public UIElement element;
        public int descendentStartIndex;
        public int descendentEndIndex;

    }

    public unsafe struct BuildTraversalJob : IJob {

        public ElementInfo root;
        //  public UnsafeList<TraversalInfo> output;

        public void Execute() {

            // int idx = output.size;
            // // this won't tell me what elements are in the same template
            // // but will narrow the tree a lot. will have to compare the
            // // template ids to find same-template descendents
            // // for slots will also have to check (isTemplateRoot && hasSlotOverrides) before continuing
            // output.array[idx].descendentStartIndex = output.size;
            //
            // for (int i = 0; i < root.childCount; i++) {
            //     // breadth first makes a lot of sense here
            //     // so i have a range for children and a range for descendents
            //     // otherwise we need to track 'next sibling' indices, which is also maybe ok
            //     // most checks are likely to be 'does my parent have x' so keeping children and parent close is nice
            //     // int childIdx = Traverse(root.children[i]);
            //     
            // }
            //
            // output.array[idx].descendentEndIndex = output.size;
        }

    }

    public unsafe struct ElementInfo {

        public int id;
        public int tableIndex; // index into elementTable, like Id but re-used across dead elements
        public int version;
        public int depth;
        public int templateId;
        public bool isTemplateRoot;
        public int traversalIndex;
        public ElementInfo* children;
        public int childCount;
        public bool isEnabled;

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
    public unsafe struct RunSelectorsJob : IJob {

        public GCHandle filterFnHandle;

        public NativeList<SelectorKey> output;
        public NativeList<SelectorResultRange> resultRanges;

        public void Execute() {
            // if selector.isOnce -> 
            // if selector.fromTarget == Self
            // if selector.filter == null

            LightList<TraversalInfo> traversalData = new LightList<TraversalInfo>();
            LightList<SelectorRunData> selectors = new LightList<SelectorRunData>();

            // if any are descendent selectors, get all descendents in template and save that list for later
            int elementIndex = 10;

            for (int i = 0; i < selectors.size; i++) {
                if (selectors.array[i].query.targetGroup == FromTarget.Descendents) {
                    //   GetTraversal(elementIndex, traversalData);
                }
            }

            Func<UIElement, bool> filter = (Func<UIElement, bool>) filterFnHandle.Target;

            //int traversalIndex = elementData[elementIndex].traversalIndex;

            // element.template.GetActiveHierarchy();

            RangeInt range = new RangeInt(output.Length, 0);
            for (int i = 0; i < traversalData.size; i++) {
                if (traversalData[i].isEnabled && filter(traversalData[i].element)) {
                    // output.Add(new SelectorKey(elementId, selectors[i].selectorId));
                }
            }

            range.length = output.Length - range.start;
            output.Add(new SelectorKey());

            // selector output
            // selectorId + elementId & range
            // append to element id list

            // output.Sort();

        }

        public static void GetTraversal(int elementIndex, ref LightList<TraversalInfo> traversalData) {
            // ElementInfo info = traversalData[elementIndex];

        }

    }

    public struct SelectorInfo {

        public SelectorKey key;
        public RangeInt results;

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

        public readonly UnsafeList<VertigoStyle> styles;
        public readonly UnsafeList<VertigoSelector> selectors;
        public readonly UnsafeList<SelectorInfo> sortedActiveSelectors;

        [ReadOnly] public NativeList<StyleStateGroup> removedList;
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

            UnsafeList<StyleStateGroup> temp = new UnsafeList<StyleStateGroup>(removedList.Length, Allocator.TempJob);

            for (int removedIdx = 0; removedIdx < removedList.Length; removedIdx++) {

                StyleStateGroup removed = removedList[removedIdx];

                if (removed.styleId.HasSelectorsInState(removed.state)) {
                    temp.AddUnchecked(removed);
                }

            }

            // gather selectors to remove here & pump into list

            for (int removedIdx = 0; removedIdx < temp.size; removedIdx++) {

                StyleStateGroup toRemove = temp.array[removedIdx];
                VertigoStyle style = styles[toRemove.styleId.index];
                VertigoSelector* selectorPtr = selectors.array + style.selectorOffset;

                for (int i = 0; i < style.selectorCount; i++) {
                    ref VertigoSelector selector = ref selectorPtr[i];

                    if (selector.id.state != toRemove.state) {
                        continue;
                    }

                    SelectorKey key = new SelectorKey(toRemove.styleSetId, selector.id); // todo -- make styleId not contain StyleSheet data -> this is really debug data that is easily backtracked
                    deadSelectors.Add(new SelectorRemovalData() {
                        index = FindIndex(key, sortedActiveSelectors),
                        eventRange = new RangeInt(selector.eventOffset, selector.eventCount)
                    });
                }

            }

            // learnings --> 1 level of style lookup is better than two
            // can still trace back to origin sheet via index
            // nice to have properties all together as well
            // 1 big table might be implemented as pages but addressable and single entity

            // to remove effect on element we need to rebuild the element
            // since i dont store which styles were applied to elements and shouldn't
            // if stored effects per element id need to 

            temp.Dispose();

        }

        private static int FindIndex(SelectorKey key, UnsafeList<SelectorInfo> sortedSearchList) {
            int start = 0;
            int end = sortedSearchList.size - 1;

            while (start <= end) {
                int index = start + (end - start >> 1);

                if (sortedSearchList.array[index].key.longVal < key.longVal) {
                    start = index + 1;
                }
                else if (sortedSearchList.array[index].key.longVal > key.longVal) {
                    end = index - 1;
                }
                else {
                    return index;
                }

            }

            return ~start; // should never hit
        }

    }

}