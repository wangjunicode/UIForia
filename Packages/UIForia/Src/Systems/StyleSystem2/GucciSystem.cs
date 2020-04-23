using System;
using System.Threading;
using UIForia.Elements;
using UIForia.Selectors;
using UIForia.Style;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace UIForia {

 

}
//     
//     internal struct SelectorTarget {
//
//         public int selectorId;
//         public ElementReference elementRef;
//
//     }
//
//     internal struct StylePropertyUpdate {
//
//         public StyleState2 state;
//         public StyleProperty2 property;
//
//     }
//
//     public struct StyleRunCommandSet {
//
//         public Action<UIElement> enter;
//         public Action<UIElement> exit;
//
//     }
//
//     // style needs an array of its properties but that array can be shared + use ranges
//     // current parser has a shared array + 4 ints to use as ranges for states
//     // single array approach is fine but to support crunching or runtime style generation we'll want an array per style group i think
//
//     public class GucciSystem {
//
//         private int nextChangeSetId;
//         private SizedArray<ChangeSet> changeSets;
//
//         public void Update() {
//
//             for (int i = 0; i < nextChangeSetId; i++) {
//                 ref ChangeSet changeSet = ref changeSets.array[i];
//                 // figure out where we can make this parallel, probably want to sort by depth and bucket it somehow
//                 // any existing change set objects can be safely used in parallel but newly created ones will not be if they come from the same array 
//
//                 ProcessUpdate(changeSet);
//             }
//
//             nextChangeSetId = 0;
//
//         }
//
//         private void ProcessUpdate(in ChangeSet changeSet) {
//
//             if (changeSet.sharedStyles == null || changeSet.sharedStyles.size == 0) {
//                 RebuildStylesWithSameState(changeSet);
//             }
//             else if (changeSet.state != changeSet.styleSet.state) { }
//         }
//
//         private void RunSelectors(UIElement element, SizedArray<Selector> selectors) {
//             Selector selector = selectors.array[0];
//
//             LightList<UIElement> targets = new LightList<UIElement>();
//
//             // selector dependencies
//             // element added / removed from template (counts for enable/disable also)
//             // element moved (repeat only)
//             // attribute add / remove / change
//             // state of descendent changed
//             // maybe dont cache when using where clause cause that could be anything
//             // descendent had styles change
//             // where clause isn't cachable probably
//             selector.Run(element, targets);
//
//             // int selectorSourceId = GetSelectorSourceId(); // run once when starting to apply, needs to be computed if changed
//             // charStringBuilder.AppendInt(selector.id);
//             // charStringBuilder.AppendChar(':');
//             // charStringBuilder.AppendInt(elementId);
//             // charStringBuilder.AppendChar(':');
//             // charStringBuilder.AppendInt(groupId);
//             // charStringBuilder.AppendChar(':');
//             // charStringBuilder.AppendChar(groupType);
//             // charStringBuilder.AppendChar(':');
//             // charStringBuilder.AppendChar(groupIndex);
//             // id = map.CreateId(charStringBuilder.ToString()); // can also get char array from pool and just use that ofc
//
//             // RegisterSelector(id, selector);
//
//             for (int i = 0; i < targets.size; i++) {
//                 // maybe also need to track source of selector and include that in the pairing since order is important for which styles win
//                 // if was previously affected by this selector / element pair we dont need to do anything
//                 // if it was not previously affected then we need to add the selector to the change set
//
//             }
//
//         }
//
//         // idea: query + effect = selector, but query can be referenced in code and executed with user supplied effect
//
//         // changing groups causes a rebuild
//         // first figure out what is our active set of groups, we don't care about their style properties yet
//         // for each removed group
//         //     if it had exit run commands invoke them
//         //     if it had selectors
//         //        find all effected elements that are still alive
//         //        add a group change to remove the selector -- still not sure what this looks like, element.selectorEffects.remove?
//         //                                                     do we just rebuild and get results as part of that?
//         //                                                     if selector had exit hook (if we support that) run it on the element
//         // for each new group
//         // if it has enter commands invoke them
//         // if it has selectors
//         // enqueue them -> create id, allocate target list, etc
//
//         // where is the effect list stored? on the system, on the element? 
//         //     element can store just the list of affected selector id
//         //     since the same selector produces a different id when its index changes, this should be fine
//         //     if most elements are effected by selectors then on element makes sense, if its less common then in system is better imo
//         // where is the available group list stored? implicit + shared in single list on style probably, might not make sense to put on system, most elements will have shared styles i think
//         // effects are basically groups, for debugging will want to back track to source
//
//         // where do we store list of selector effects on elements? they run at different times depending on hierarchy so 
//         // when adding / removing a selector effect we only want to rebuild target element if index changed / added / removed
//         // what happens when we have 'const' selectors that are not updated every frame? how are they written?
//         // or maybe we really do add / remove and just sort if we had any changes
//         // sort key can be some combination of depth, selector index in host, 
//
//         // group sequence
//         // instance -> state (active -> focus -> hover)
//         // instance -> normal
//         // selector -> lowest depth diff (0 == self) + selector index in host selector list (state sorted, so [active] selector 0 beats [normal] selector 4)
//         // shared -> state
//         // shared -> normal
//         // implicit -> state
//         // implicit -> normal 
//         // inherited
//
//         private readonly CharStringBuilder idBuilder = new CharStringBuilder(64);
//
//         private struct OldStyleIndex {
//
//             public int index;
//             public StyleId styleId;
//
//             public OldStyleIndex(int index, StyleId styleId) {
//                 this.index = index;
//                 this.styleId = styleId;
//             }
//
//         }
//
//         private enum UpdateType {
//
//             Added,
//             Removed,
//             IndexChanged
//
//         }
//
//         private struct StyleGroupUpdate {
//
//             public readonly int index;
//             public readonly UpdateType updateType;
//
//             public StyleGroupUpdate(UpdateType updateType, int index) {
//                 this.updateType = updateType;
//                 this.index = index;
//             }
//
//         }
//
//         // phase 1 need to register all group changes (at binding time), this will create a change set for all elements that need one
//         // phase 2 (parallel -- at update time)
//         // split workload into n jobs give each job an output array
//         // run RebuildStyleGroups on all change sets
//         // output per thread work -> selector changes & run hooks to call (all data via ids, no access to change sets, unless we copied from binding time array)
//         // phase 3 sort output lists and merge back into 1 array (main thread / single threaded)
//         // phase 4 (maybe parallel) process output list and call enter / exit handlers as needed. this will likely do animation work so maybe that all has to be done on 1 thread
//         // phase 5 update selectors based on added/removed selectors (parallel, change sets need to be given per thread). mark effected elements as changed (probably in an intermediate per thread list)
//         // sort output lists by element id merge them back into 1 while deduplicating
//         // phase 6 execute selectors (parallel -- will need another gather phase to apply results)
//         // output pair of computedSelectorId -> elementId
//         // phase 7 apply selector result
//         // sort previous result list by element id
//         // convert to range list
//         // process range list in parallel
//         // this is where style is actually computed
//
//         // todo maybe have an animator / sound player per thread also -> might let us do a better job of managing styles without merging back results
//
//         // can do last frame diffing while invoking enter/exit hooks probably
//
//         // change sets are initialized in binding phase
//
//         // when styles update
//         //     for every styleset with a change set
//         //         if groups changed (can be because of state change or because of shared styles changing)
//         //            find the ones that were added -> push into list
//         //            find the ones that were removed -> push into list
//         //            we dont care about index changing at the moment but probably also push to list if has selectors
//         //     for every group that was removed
//         //        if group had selectors and was targeting elements
//         //            add those selectors to the removed selector list
//         //     for every group that was added
//         //        if group has selectors
//         //            add those selectors to the added selector list
//         //     for every selector in the removed list
//         //        get the elements it affected last frame from stored list via range
//         //            if has exit hook add (hook, elementId) to a list to be flushed later 
//         //            add each of those to the group rebuild list if not already there, duplicates are fine we can sort later
//         //     for every selector in added list and in the active list
//         //         we need to run this selector now
//         //            can certainly be parallel but not burst because we invoke function pointers for filtering
//         //     for every element with style changes or selector effect updates
//         //        new selectors should invoke enter hook if needed
//         //        exited selectors should invoke exit hook if needed
//         //     for every element with style changes or selector effect updates
//         //        run animations // inherit first? we know which properties will change due to animation
//         //     for every element with style changes or selector effect updates
//         //        run transitions if needed
//
//         private unsafe void Phase1() {
//
//             int totalChangeSets = nextChangeSetId;
//
//             // every 10 = 1 job? kinda arbitrary but why not?
//             // if n < 10 -> 1 job per 5
//             // if n > 10 -> 1 job per 10
//
//             int jobCount = 1;
//
//             StructList<ProcessSharedStyleUpdatesJob> jobs = new StructList<ProcessSharedStyleUpdatesJob>();
//
//             NativeArray<JobHandle> handles = new NativeArray<JobHandle>(1, Allocator.Temp);
//
//             int batchSize = 1;
//             int lastSize = 0;
//
//             for (int i = 0; i < jobCount; i += batchSize) {
//                 jobs.array[i] = new ProcessSharedStyleUpdatesJob() {
//                     changeSets = new UnsafeSpan<SharedStyleChangeSet>(0, sharedStyleChangeSets.size, sharedStyleChangeSets.array),
//                     addedList = new UnsafeList<StyleUpdate>(128, Allocator.TempJob),
//                     removedList = new UnsafeList<StyleUpdate>(128, Allocator.TempJob),
//                 };
//                 lastSize += batchSize;
//             }
//
//             for (int i = 0; i < jobCount; i++) {
//                 handles[i] = jobs.array[i].Schedule();
//             }
//
//             for (int i = 0; i < jobCount; i++) {
//                 jobs.array[i].addedList.Dispose();
//                 jobs.array[i].removedList.Dispose();
//             }
//
//         }
//
//         // have list of added / removed styles for elements (already per-job data, should need to sort/merge for this)
//         // now need to figure out based on state which stategroups to add / remove
//         // where are the shared styles stored? I think on the element is fine
//         // maybe it would be better to allocate a block and then sub-allocate out of that
//         // can add that later though 
//
//         public struct StyleStateGroup {
//
//             public readonly int index;
//             public readonly StyleId styleId;
//             public readonly StyleState2 state;
//
//             public StyleStateGroup(int index, StyleState2 state, StyleId styleId) {
//                 this.index = index;
//                 this.state = state;
//                 this.styleId = styleId;
//             }
//
//         }
//
//         public unsafe struct HandleStyleStateGroupUpdateJob : IJob {
//
//             public IntPtr listPointer;
//             public UnsafeList<StyleStateGroup> previousStateGroups;
//             public UnsafeList<StyleStateGroup> newStateGroups;
//             public UnsafeList<int> rebuildList;
//
//             public void Execute() {
//
//                 // cache persistent and resuse, 32 is huge for this
//                 previousStateGroups = new UnsafeList<StyleStateGroup>(32, Allocator.TempJob);
//
//                 UnsafeSpan<SharedStyleChangeSet> styleChangeSets = default;
//
//                 for (int i = 0; i < styleChangeSets.size; i++) {
//                     SharedStyleChangeSet changeSet = styleChangeSets.array[i];
//
//                     if (changeSet.newState == changeSet.originalState) { }
//
//                 }
//
//             }
//
//             private static void MaybeAddStyleGroup(ref UnsafeList<StyleStateGroup> list, StyleState2 checkState, StyleState2 targetState, StyleId styleId) {
//                 if ((checkState & targetState) != 0 && styleId.DefinesState(targetState)) {
//                     list.array[list.size++] = new StyleStateGroup(list.size, targetState, styleId);
//                 }
//             }
//
//             // if style group exists -> styleId encodes which states it has?
//
//             private unsafe void Run(in SharedStyleChangeSet changeSet) {
//
//                 previousStateGroups.EnsureAdditionalCapacity(4 * changeSet.oldStyleCount);
//
//                 for (int i = 0; i < changeSet.oldStyleCount; i++) {
//                     StyleId styleId = changeSet.styles[i];
//                     MaybeAddStyleGroup(ref previousStateGroups, changeSet.originalState, StyleState2.Active, styleId);
//                     MaybeAddStyleGroup(ref previousStateGroups, changeSet.originalState, StyleState2.Focused, styleId);
//                     MaybeAddStyleGroup(ref previousStateGroups, changeSet.originalState, StyleState2.Hover, styleId);
//                     MaybeAddStyleGroup(ref previousStateGroups, changeSet.originalState, StyleState2.Normal, styleId);
//                 }
//
//                 for (int i = changeSet.oldStyleCount; i < changeSet.newStyleCount; i++) {
//                     StyleId styleId = changeSet.styles[i];
//                     MaybeAddStyleGroup(ref newStateGroups, changeSet.newState, StyleState2.Active, styleId);
//                     MaybeAddStyleGroup(ref newStateGroups, changeSet.newState, StyleState2.Focused, styleId);
//                     MaybeAddStyleGroup(ref newStateGroups, changeSet.newState, StyleState2.Hover, styleId);
//                     MaybeAddStyleGroup(ref newStateGroups, changeSet.newState, StyleState2.Normal, styleId);
//                 }
//
//                 for (int i = 0; i < newStateGroups.size; i++) {
//
//                     StyleStateGroup newGroup = newStateGroups[i];
//
//                     bool wasPresent = false;
//
//                     for (int j = 0; j < previousStateGroups.size; j++) {
//                         StyleStateGroup group = previousStateGroups.array[i];
//                         if (group.state == newGroup.state && group.styleId == newGroup.styleId) {
//                             previousStateGroups.SwapRemoveAt(i);
//                             wasPresent = true;
//                             break;
//                         }
//                     }
//
//                     if (wasPresent) { }
//
//                 }
//
//                 // output = list of added groups for element & list of removed groups for element
//                 // order doesn't matter. could be that a state change causes no updates
//
//                 // if added is empty and removed is empty and no indices changed -> no op
//
//                 // otherwise we need to mark this element for changing. 
//                 // should be threadsafe to use same changeset assuming dependencies are correctly setup
//                 // is that the right way?
//                 // hold a different changeset array for this? maybe doesn't matter
//
//                 // instance styles also need to be processed somehow -> maybe its only a change flag though and do the data changing at binding update time 
//
//                 rebuildList.Add(changeSet.elementId); // at this point its guaranteed not to contain duplicates, this is not true in the selector phase
//
//                 UnsafeList<StyleStateGroup> removedGroups = default;
//                 UnsafeList<ActiveSelector> activeSelectors = default;
//                 UnsafeList<int> freeIndices = default;
//                 UnsafeList<int> selectorTargetedElements = default;
//                 UnsafeList<ElementData> elementData = default;
//
//                 for (int i = 0; i < removedGroups.size; i++) {
//                     StyleStateGroup stateGroup = removedGroups[i];
//                     if (stateGroup.styleId.HasSelectorsInState(stateGroup.state)) {
//
//                         int index = FindActiveSelectorIndex(activeSelectors, new SelectorReference());
//
//                         if (index == -1) {
//                             continue;
//                         }
//
//                         activeSelectors.array[index].isActive = false;
//                         freeIndices.Add(index);
//                         int start = activeSelectors.array[index].effectedElementRange.start;
//                         int end = activeSelectors.array[index].effectedElementRange.end;
//
//                         for (int j = start; j < end; j++) {
//                             int elementId = selectorTargetedElements[i];
//                             if (Interlocked.Add(ref elementData.array[0].dirtyFlag, 1) == 1) {
//                                 // add to dirty list    
//                             }
//                         }
//                         
//                         // mark as inactive
//                         // mark all effected elements as no longer effected
//                         // retain id for added groups to get shoved into (will sort later anyway but this way there isn't a removal while multiple threads might be reading out of this array)
//
//                     }
//                 }
//
//             }
//
//         }
//
//         public struct ElementData {
//
//             public int dirtyFlag;
//
//         }
//         public struct SelectorReference {
//
//             public int elementId;
//             public StyleId styleId; // style id not needed if selector id is globally unique (which i think it should be)
//             public int selectorId;
//
//         }
//
//         public struct ActiveSelector {
//
//             public int elementId;
//             public StyleId styleId; // style id not needed if selector id is globally unique (which i think it should be)
//             public int selectorId;
//             public bool isActive;
//             public RangeInt effectedElementRange;
//
//         }
//
//         /// <param name="activeSelectors">list of active selectors sorted by elementId from which the selector originates</param>
//         /// <param name="selectorReference">the selector we are trying to find the index of</param>
//         /// <returns>index in activeSelectors the target selector reference is located at</returns>
//         public static unsafe int FindActiveSelectorIndex(in UnsafeList<ActiveSelector> activeSelectors, in SelectorReference selectorReference) {
//             int start = 0;
//             int end = activeSelectors.size - 1;
//             ActiveSelector* array = activeSelectors.array;
//
//             while (start <= end) {
//                 int index = start + (end - start >> 1);
//
//                 int cmp = array[index].elementId - selectorReference.elementId;
//
//                 if (cmp == 0) {
//                     // look to start if not the same selector id
//                     if (selectorReference.selectorId == array[index].selectorId) {
//                         return index;
//                     }
//
//                     // if not found look forward until found or element id is different
//                     for (int i = index + 1; i < activeSelectors.size; i++) {
//                         if (array[i].elementId != selectorReference.elementId) {
//                             break;
//                         }
//
//                         if (array[i].selectorId == selectorReference.selectorId) {
//                             return i;
//                         }
//                     }
//
//                     // look backwards until found or element id is different
//                     for (int i = index - 1; i >= 0; i--) {
//                         if (array[i].elementId != selectorReference.elementId) {
//                             break;
//                         }
//
//                         if (array[i].selectorId == selectorReference.selectorId) {
//                             return i;
//                         }
//                     }
//
//                     // if not found it ain't there
//                     return -1;
//                 }
//
//                 if (cmp < 0) {
//                     start = index + 1;
//                 }
//                 else {
//                     end = index - 1;
//                 }
//             }
//
//             return -1;
//         }
//
//         private struct RemoveInactiveSelectorsJob : IJob {
//
//             public UnsafeList<SelectorReference> selectorsToRemove;
//
//             public void Execute() { }
//
//         }
//
//         // build a 2nd change set -> still job maybe?
//         // trade off is latency for worker to pick up work
//         private struct RemoveStateGroupJob : IJob {
//
//             // mark for style rebuild -> static lookup table for each element? list that allows duplicates + sort/merge?
//             // if any removed style had selectors -> remove them -> still need to figure out what this means exactly
//             // if any removed style had exit hooks -> run them
//             public void Execute() {
//                 // for each selector in last frame active list
//                 //    if no longer active -> because element disabled or selector removed
//                 //        swap remove that selector (can mutate list sections here safely at this point, compress at the end & maybe stitch up pointers if needed)
//                 //        for each effected element -> if element active/alive
//                 //            mark element for rebuild
//                 //            invoke exit hook if present
//                 //    if still active
//                 //        run it
//                 //        for each affected element
//                 //            if was affected last frame -> do nothing
//                 //            if was not affected last frame -> call enter hook if present
//                 //                                           -> mark element for rebuild
//             }
//
//         }
//
//         private struct RunCommandAction {
//
//             public UIElement element;
//             public Action<UIElement> action;
//
//         }
//
//         // element.style.lastFrameSelectorRange
//         // element.style.lastFrameStaticSelectorRange;
//
//         // some selectors are basically static -> self selectors with a triggered query instead of where
//
//         // where do i store which selectors were previously affecting an element?
//         // on element itself -> lots of tiny arrays
//         // in big array on system -> re-allocating sucks, search space much larger
//         // in big(ger) array on template root data -> was selector from inside expanded template root or outside? reallocating not so bad, search space much smaller, need to make sure we are threadsafe 
//
//         // template data makes sense except for slots which are physically in a different template
//
//         // traverse last frame effect list for element in sorted order
//         // compare to this frame effect list for element
//         // where does the exit hook live? on the style of course, can back trace from selector style id 
//
//         public struct SelectorEffect {
//
//             public int styleId;
//             public int selectorId;
//             public int computedSelectorId;
//
//             public int elementId;
//             // flag for has enter/exit hook?
//
//         }
//
//         // order doesn't matter at all
//         private struct GatherSelectorsToRunJob : IJob {
//
//             // master list of selectors to run -> copy of last frame's selector to run list
//             // order doesn't matter at this point i guess
//
//             // build a removal list(s) -> easy / done
//             // if iterate and skip/compress empties would be nice to have other work to do at the same time
//             // compress by section + merge? why not just run at that point? profile?
//             // free list per job while removing? probably level 2 of optimization
//
//             public void Execute() {
//                 // run new ones
//                 // run triggered ones
//                 // run continual ones
//                 // how do we remove old ones?
//                 // iterate & skip if removed? feels kind bad and not parallel
//                 // activeSelectors free list based on unique id?
//             }
//
//         }
//
//         private struct RunSelectorsJob : IJob {
//
//             public void Execute() {
//
//                 Selector selector = default;
//                 UIElement element = default;
//
//                 StructList<SelectorEffect> effects = StructList<SelectorEffect>.Get();
//
//                 int computedSelectorId = -1;
//                 selector.Run(element, computedSelectorId, effects);
//
//                 // effect = computedId, selectorId, elementId, styleId
//                 // effect list per job
//                 // isolate + divide and conquer sort feels good on a per job batch level
//
//                 // for effected element if last frame effect order is the same and didnt have group/state changes then no work needs to be done with updating properties
//                 // large list for last frame effects
//                 // style set has range pointer
//
//                 // end of frame swap buffers
//
//             }
//
//         }
//
//         private struct ApplySelectorEffectsJob : IJob {
//
//             public void Execute() {
//                 // for (int i = 0; i < effects.size; i++) {
//                 //     element[targets].changeSet.RebuildStyles();
//                 // }
//
//             }
//
//         }
//
//         private struct RebuildStylesJob : IJob {
//
//             public void Execute() {
//
//                 UIElement element = default;
//                 ChangeSet changeSet = default;
//
//                 // previous selector list
//                 // new selector list
//
//                 // foreach new selector -> run hook if needed
//                 // foreach removed selector -> run hook if needed
//
//                 // animate
//
//                 // inherit
//
//                 // sort selectors
//                 // for each selector applicable to element
//                 // add styles
//
//             }
//
//         }
//
//         private struct RemoveDeadSelectorsJob : IJob {
//
//             public RangeInt range;
//             public StructList<int> removedSelectorIds;
//
//             // gather removed selectors into a list
//             // retain list of last update applied selectors + range of effected elements (in chunked list probably)
//
//             // it is easier to diff than remove
//             // if previous affected set contains selector id that is in the removed list -> remove
//             // already have a pretty decent way to removing selectors, can back trace their action list
//             // so really just need to add targeted elements to change list if not there already (not caring bout order or duplicates because of sorting later)
//
//             public void Execute() {
//                 int end = range.end;
//
//                 for (int i = range.start; i < end; i++) {
//
//                     // for each removed group
//                     // if group had selectors
//                     // enqueue to remove those selectors
//
//                     // when we have all of those:
//                     // for each element in effected range
//                     // mark as requiring update. that update should invoke the old selector exit hook if needed
//                 }
//
//             }
//
//         }
//
//         public StructList<SelectorEffect> lastFrameSelectorEffects;
//         public StructList<SelectorEffectedElement> lastFrameEffectedElements;
//
//         public struct SelectorEffectedElement {
//
//             public int elementId;
//             public int computedSelectorId;
//
//         }
//
//         private struct MergeUpdateJob : IJob {
//
//             public void Execute() {
//                 throw new NotImplementedException();
//             }
//
//         }
//
//         private void Phase4() {
//
//             LightList<RunCommandAction> enterList = new LightList<RunCommandAction>();
//             LightList<RunCommandAction> exitList = new LightList<RunCommandAction>();
//
//             for (int i = 0; i < exitList.size; i++) {
//                 exitList[i].action.Invoke(exitList[i].element);
//             }
//
//             for (int i = 0; i < enterList.size; i++) {
//                 enterList[i].action.Invoke(enterList[i].element);
//             }
//
//         }
//
//         // global list / array of styles by id? likely makes sense then a style is really just the global id
//
//         public struct StyleUpdate {
//
//             public int elementId;
//             public StyleId styleId;
//             private int padding;
//
//             public StyleUpdate(int elementId, StyleId styleId) {
//                 this.padding = 0;
//                 this.elementId = elementId;
//                 this.styleId = styleId;
//             }
//
//         }
//
//         private unsafe struct ProcessSharedStyleUpdatesJob : IJob {
//
//             public UnsafeList<StyleUpdate> addedList;
//             public UnsafeList<StyleUpdate> removedList;
//             public UnsafeSpan<SharedStyleChangeSet> changeSets;
//             private NativeArray<OldStyleIndex> searchList;
//
//             public void Execute() {
//
//                 searchList = new NativeArray<OldStyleIndex>(32, Allocator.TempJob); // maybe make persistent
//
//                 for (int i = 0; i < changeSets.size; i++) {
//                     Run(changeSets.array[i]);
//                 }
//
//                 searchList.Dispose();
//
//             }
//
//             // change set is still burstable if we store the base stylelist somewhere
//             // shared styles is an int array
//             // int points to global id (per application) of a style
//             // can probably be 2byte2byte scheme for stylesheet -> index of style in style sheet
//
//             // change set burstable for style groups but maybe not properties since properties need an object field
//             // 2 arrays then, 1 array<array<int>> for recording the shared styles + state? state at index 0?
//             //                1 array<stylepropertydata> for recording instance styles
//
//             private void Run(in SharedStyleChangeSet changeSet) {
//
//                 // this handles diffing shared style groups but does NOT handle state changes that add/remove groupings
//
//                 if (changeSet.styles == default || changeSet.newStyleCount == 0) {
//                     return;
//                 }
//
//                 for (int i = 0; i < changeSet.oldStyleCount; i++) {
//                     searchList[i] = new OldStyleIndex(i, changeSet.styles[i]);
//                 }
//
//                 int totalStyleCount = changeSet.oldStyleCount + changeSet.newStyleCount;
//                 int searchListSize = changeSet.oldStyleCount;
//                 int elementId = changeSet.elementId;
//
//                 addedList.EnsureAdditionalCapacity(changeSet.newStyleCount);
//
//                 for (int i = changeSet.oldStyleCount; i < totalStyleCount; i++) {
//
//                     StyleId targetId = changeSet.styles[i];
//
//                     int previousIndex = IndexOf(searchList, targetId, ref searchListSize);
//
//                     if (previousIndex == -1) {
//                         addedList[addedList.size++] = new StyleUpdate(elementId, targetId);
//                     }
//                     else if (previousIndex == i) {
//                         // unchanged
//                     }
//                     else {
//                         // index changed
//                         // outputData.array[outputData.size++] = new StyleGroupUpdate(UpdateType.IndexChanged, i);
//                     }
//
//                 }
//
//                 removedList.EnsureAdditionalCapacity(searchListSize);
//
//                 // anything still in searchList was removed
//                 for (int i = 0; i < searchListSize; i++) {
//                     removedList.array[removedList.size++] = new StyleUpdate(elementId, searchList[i].styleId);
//                 }
//
//             }
//
//             private static int IndexOf(NativeArray<OldStyleIndex> searchList, StyleId targetId, ref int searchListSize) {
//                 for (int i = 0; i < searchListSize; i++) {
//                     if (searchList[i].styleId == targetId) {
//                         int idx = searchList[i].index;
//                         searchList[i] = searchList[searchListSize--];
//                         return idx;
//                     }
//                 }
//
//                 return -1;
//             }
//
//         }
//
//         // for each removed group
//         // if group had selectors
//         // remove those from the list (set index to 0)
//         // record indices that are free
//
//         // for each added group
//         // if group has selectors
//         // if has free index in index list, set it
//         // else add to append list
//         // merge at end
//         // sort so selector list is binary searchable
//         // or keep map of selectorId -> array index but maybe thats dumb
//
//         private void HandleAddedStyleGroups(in ChangeSet changeSet, StructList<StyleGroupUpdate> groupUpdates) {
//             StyleState2 state = changeSet.styleSet.state;
//             UIElement element = changeSet.styleSet.element;
//             StructList<StyleGroup2> sharedStyles = changeSet.sharedStyles;
//
//             for (int i = 0; i < groupUpdates.size; i++) {
//
//                 ref StyleGroupUpdate update = ref groupUpdates.array[i];
//
//                 if (update.updateType != UpdateType.Added) {
//                     continue;
//                 }
//
//                 ref StyleGroup2 group = ref sharedStyles.array[update.index];
//
//                 // if has run commands that apply, enqueue them
//
//                 // if has selectors that apply to state, enqueue them
//
//                 for (int j = 0; j < group.selectorDefinitions.Length; j++) {
//                     ref SelectorDefinition selectorDefinition = ref group.selectorDefinitions[j];
//
//                     //if ((selectorDefinition.selector.state & state) != 0) {
//                     // enqueue selector / element to run
//                     // output.Add(new SelectorAction(Action.Remove, element.id, selectorDefinition.selector.id, group.id));
//                     //}
//
//                 }
//             }
//
//         }
//
//         private void HandleRemovedStyleGroups(in ChangeSet changeSet, StructList<StyleGroupUpdate> groupUpdates) {
//             //
//             // StyleState state = changeSet.styleSet.state;
//             // UIElement element = changeSet.styleSet.element;
//             // StructList<StyleGroup2> previousStyles = changeSet.styleSet.sharedStyles;
//             //
//             // SelectorHostInfo hostInfo = hostInfoList.array[changeSet.styleSet.selectorHostDataId];
//             //
//             // // call exit commands and remove selectors
//             // for (int i = 0; i < groupUpdates.size; i++) {
//             //
//             //     ref StyleGroupUpdate update = ref groupUpdates.array[i];
//             //
//             //     if (update.updateType != UpdateType.Removed) {
//             //         continue;
//             //     }
//             //
//             //     ref StyleGroup2 group = ref previousStyles.array[update.index];
//             //
//             //     group.normalRunCommands.exit?.Invoke(element);
//             //
//             //     if ((state & StyleState.Active) != 0) {
//             //         group.activeRunCommands.exit?.Invoke(element);
//             //     }
//             //
//             //     if ((state & StyleState.Focused) != 0) {
//             //         group.focusRunCommands.exit?.Invoke(element);
//             //     }
//             //
//             //     if ((state & StyleState.Hover) != 0) {
//             //         group.hoverRunCommands.exit?.Invoke(element);
//             //     }
//             //
//             //     if (group.selectorDefinitions == null) {
//             //         continue;
//             //     }
//             //
//             //     for (int j = 0; j < group.selectorDefinitions.Length; j++) {
//             //
//             //         ref SelectorDefinition selectorDefinition = ref group.selectorDefinitions[j];
//             //
//             //         if ((selectorDefinition.state & state) == 0) {
//             //             continue;
//             //         }
//             //
//             //         selectorDefinition.runCommands.exit?.Invoke(element);
//             //
//             //         // foreach effected target
//             //         // if effected by selector id
//             //         // if reference is live
//             //         // get change set
//             //         // mark as removed
//             //         // remove from effected list (order doesn't matter, can swap remove)
//             //         LightList<ElementReference> targets = LightList<ElementReference>.Get();
//             //         // find elements this selector was effecting (that aren't == to element)
//             //
//             //         // todo -- remove from style set eventually, its a waste of memory atm
//             //         changeSet.styleSet.GetSelectorTargets(selectorDefinition.selector.id, targets);
//             //
//             //         if (changeSet.styleSet.selectorHostDataId == -1) {
//             //             continue;
//             //         }
//             //
//             //         int selectorEffectId = hostInfo.GetSelectorId(selectorDefinition.selector.id);
//             //
//             //         for (int k = 0; k < targets.size; k++) {
//             //
//             //             if (TryResolveReference(targets.array[k], out UIElement target)) {
//             //                 GetChangeSet(target.styleSet2).RemoveSelectorEffect(selectorEffectId);
//             //             }
//             //
//             //         }
//             //
//             //     }
//             // }
//
//         }
//
//         internal struct IdToId {
//
//             public int key;
//             public int value;
//
//         }
//
//         internal struct SelectorTargetInfo {
//
//             public SizedArray<int> targetedByList;
//
//         }
//
//         internal struct SelectorHostInfo {
//
//             public SizedArray<IdToId> selectorIdMap;
//             public SizedArray<SelectorTarget> targetedElements;
//
//             public int GetSelectorId(int selectorId) {
//                 for (int i = 0; i < selectorIdMap.size; i++) {
//                     if (selectorId == selectorIdMap.array[i].key) {
//                         return selectorIdMap.array[i].value;
//                     }
//                 }
//
//                 return -1; // shouldn't happen
//             }
//
//         }
//
//         internal StructList<SelectorHostInfo> hostInfoList = new StructList<SelectorHostInfo>(32);
//
//         private bool TryResolveReference(in ElementReference reference, out UIElement element) {
//             element = default;
//             return true;
//         }
//
//         private static int IndexOf(int targetId, StructList<OldStyleIndex> searchList) {
//             for (int i = 0; i < searchList.size; i++) {
//                 if (searchList.array[i].styleId == targetId) {
//                     int idx = searchList.array[i].index;
//                     searchList.SwapRemoveAt(i);
//                     return idx;
//                 }
//             }
//
//             return -1;
//         }
//
//         private void RebuildStylesWithSameState(in ChangeSet changeSet) {
//             // var activeSelectors = changeSet.styleSet.activeSelectors;
//             // var matches = changeSet.styleSet.selectorMatches;
//             // all matches will be lower in the tree
//             // all matches are done in the same thread 
//             // so should be safe to manipulate result here
//
//             // selector id -> result map would make sense
//
//             StructList<StyleProperty2> properties = StructList<StyleProperty2>.Get();
//
//             StyleState2 state = changeSet.state;
//
//            // changeSet.styleSet.state = state; // maybe dont need to set this? just track original state
//
//             // if (styleSet.flags & HasImplicitStyles) != 0
//             changeSet.styleSet.GetImplicitStyles(changeSet.state, properties);
//
//             if (changeSet.instanceUpdates != null) { }
//
//             StructList<StyleGroupSource> sources = StructList<StyleGroupSource>.Get();
//
//             // todo -- dumb!
//             StructList<StyleProperty2> result = new StructList<StyleProperty2>(135);
//
//             // kinda needs to happen after selectors!
//             // won't need to sort groups if I always add them in the proper order
//             // would then only need to sort selectors possibly, not the whole list
//
//             // get all currently selectors matches
//             // for each selector see if same selector is active with new group set
//             // if not add to removal list
//
//             for (int i = 0; i < changeSet.sharedStyles.size; i++) { }
//
//             for (int i = 0; i < changeSet.sharedStyles.size; i++) {
//
//                 ref StyleGroup2 groups = ref changeSet.sharedStyles.array[i];
//
//                 if (groups.normalProperties != null && groups.normalProperties.Length != 0) {
//                     sources.Add(new StyleGroupSource() {
//                         priority = 1,
//                         properties = groups.normalProperties,
//                         state = StyleState2.Normal
//                     });
//                 }
//
//                 if (groups.hoverProperties != null && groups.hoverProperties.Length != 0 && (state & StyleState2.Hover) != 0) {
//                     sources.Add(new StyleGroupSource() {
//                         priority = 1,
//                         properties = groups.hoverProperties,
//                         state = StyleState2.Hover
//                     });
//
//                 }
//
//                 if (groups.activeProperties != null && groups.activeProperties.Length != 0 && (state & StyleState2.Active) != 0) {
//                     sources.Add(new StyleGroupSource() {
//                         priority = 1,
//                         properties = groups.activeProperties,
//                         state = StyleState2.Active
//                     });
//                 }
//
//                 if (groups.focusProperties != null && groups.focusProperties.Length != 0 && (state & StyleState2.Focused) != 0) {
//                     sources.Add(new StyleGroupSource() {
//                         priority = 1,
//                         properties = groups.focusProperties,
//                         state = StyleState2.Focused
//                     });
//                 }
//
//             }
//
//             unsafe {
//
//                 // currently 135 properties
//                 // can profile whether stackalloc is faster / slower than static array
//                 int storageSize = BitUtil.NextMultipleOf32(135);
//                 uint* mapStorage = stackalloc uint[storageSize];
//                 uint* removedMapStorage = stackalloc uint[storageSize];
//                 IntBoolMap map = new IntBoolMap(mapStorage, storageSize);
//                 IntBoolMap removedMap = new IntBoolMap(removedMapStorage, storageSize);
//
//                 // this is the final step of phase 1 (before animation) the result is our intermediate diff list
//                 for (int i = 0; i < sources.size; i++) {
//                     ref StyleGroupSource source = ref sources.array[i];
//
//                     StyleProperty2[] styleProperties = source.properties;
//
//                     for (int j = 0; j < source.size; j++) {
//                         ref StyleProperty2 property = ref styleProperties[j];
//                         if (map.TrySetIndex(property.propertyId.index)) {
//                             result.Add(property);
//                         }
//                     }
//
//                 }
//
//                 // map.Clear();
//                 // now diff this with the current style properties to build an initial diff list
//
//                 SizedArray<StyleProperty2> activeStyles = changeSet.styleSet.activeStyles;
//                 result.Sort((a, b) => a.propertyId.index - b.propertyId.index);
//
//                 StructList<StyleProperty2> changes = new StructList<StyleProperty2>(); // todo -- dumb
//
//                 for (int i = 0; i < activeStyles.size; i++) {
//                     // if active style is in result map
//                     ref StyleProperty2 previous = ref activeStyles.array[i];
//                     if (map[previous.propertyId.index]) {
//
//                         StyleProperty2 newValue = BinarySearchStyleProperty(result.array, result.size, previous.propertyId);
//
//                         // add to diff list
//                         if (newValue != previous) {
//                             changes.Add(newValue);
//                         }
//
//                         if ((newValue.propertyId.flags & PropertyFlags.Inherited) != 0) {
//                             // push into inherited map
//
//                         }
//
//                     }
//                     else {
//                         // property was removed, we need to maybe inherit from parent now or send default value
//                         changes.Add(changeSet.styleSet.GetDefaultOrInherited(previous.propertyId));
//                     }
//                 }
//
//             }
//
//         }
//
//         private static StyleProperty2 BinarySearchStyleProperty(StyleProperty2[] array, int size, PropertyId propertyId) {
//             int start = 0;
//             int end = size - 1;
//
//             while (start <= end) {
//                 int index1 = start + (end - start >> 1);
//
//                 int cmp = array[index1].propertyId.index - propertyId.index;
//
//                 if (cmp == 0) {
//                     return array[index1];
//                 }
//
//                 if (cmp < 0) {
//                     start = index1 + 1;
//                 }
//                 else {
//                     end = index1 - 1;
//                 }
//             }
//
//             // should never hit this, we only search in an array we already know contains our value
//             return default;
//         }
//
//         public struct StyleGroupSource {
//
//             public int priority;
//             public int size; // this will wrap a style group and instance properties (stored in growable list) so we want to reference both sets of data in a uniform way
//             public StyleState2 state;
//             public StyleProperty2[] properties;
//
//         }
//
//         public unsafe struct SharedStyleChangeSet {
//
//             // the first {oldStyleCount} elements in *styles are old, the rest are new
//             public ushort newStyleCount;
//             public ushort oldStyleCount;
//             public StyleState2 newState;
//             public StyleState2 originalState;
//             public StyleId* styles;
//             public int elementId;
//
//         }
//
//         internal UnsafeList<SharedStyleChangeSet> sharedStyleChangeSets;
//
//         // assumes at least 1 of the groups changed or order was altered in some way
//         public unsafe void SetSharedStyleGroups(StyleSet styleSet, StructList<StyleGroup2> newStyles) {
//
//             if (styleSet.stateAndSharedStyleChangeSetId == ushort.MaxValue) {
//                 styleSet.stateAndSharedStyleChangeSetId = (ushort) nextChangeSetId++;
//                 sharedStyleChangeSets.EnsureAdditionalCapacity(1);
//                 sharedStyleChangeSets.array[styleSet.stateAndSharedStyleChangeSetId].elementId = styleSet.element.id;
//                 sharedStyleChangeSets.array[styleSet.stateAndSharedStyleChangeSetId].newState = styleSet.state;
//             }
//
//             ref SharedStyleChangeSet changeSet = ref sharedStyleChangeSets.array[styleSet.stateAndSharedStyleChangeSetId];
//
//             if (changeSet.styles != default && changeSet.newStyleCount < newStyles.size) {
//                 UnsafeUtility.Free(changeSet.styles, Allocator.Temp);
//             }
//
//             // todo -- hold own persistent int buffer for this instead of using malloc for every allocation / free
//             long size = sizeof(int) * (newStyles.size + styleSet.sharedStyles.size);
//             changeSet.styles = (StyleId*) UnsafeUtility.Malloc(size, 4, Allocator.Temp);
//             changeSet.newStyleCount = (ushort) newStyles.size;
//             changeSet.oldStyleCount = (ushort) styleSet.sharedStyles.size;
//
//             int idx = 0;
//             for (int i = 0; i < changeSet.oldStyleCount; i++) {
//                 changeSet.styles[idx++] = styleSet.sharedStyles.array[i];
//             }
//
//             for (int i = 0; i < newStyles.size; i++) {
//                 changeSet.styles[idx++] = newStyles.array[i].id;
//             }
//
//         }
//
//         private unsafe void FreeStyleChangeSets() {
//
//             // todo -- might be better to keep a large in buffer to use as the temp allocator rather than lots of small malloc / free calls
//             for (int i = 0; i < nextChangeSetId; i++) {
//                 ref SharedStyleChangeSet changeSet = ref sharedStyleChangeSets.array[i];
//                 if (changeSet.styles != default) {
//                     UnsafeUtility.Free(changeSet.styles, Allocator.Temp);
//                 }
//
//                 changeSet = default;
//             }
//
//             nextChangeSetId = 0;
//         }
//
//         public unsafe void EnterState(StyleSet styleSet, StyleState2 state) {
//
//             if (styleSet.stateAndSharedStyleChangeSetId == ushort.MaxValue) {
//                 styleSet.stateAndSharedStyleChangeSetId = (ushort) nextChangeSetId++;
//                 sharedStyleChangeSets.EnsureAdditionalCapacity(1);
//                 sharedStyleChangeSets.array[styleSet.stateAndSharedStyleChangeSetId].elementId = styleSet.element.id;
//                 sharedStyleChangeSets.array[styleSet.stateAndSharedStyleChangeSetId].newState = styleSet.state;
//             }
//
//             sharedStyleChangeSets.array[styleSet.stateAndSharedStyleChangeSetId].newState |= state;
//
//         }
//
//         public unsafe void ExitState(StyleSet styleSet, StyleState2 state) {
//             if (styleSet.stateAndSharedStyleChangeSetId == ushort.MaxValue) {
//                 styleSet.stateAndSharedStyleChangeSetId = (ushort) nextChangeSetId++;
//                 sharedStyleChangeSets.array[styleSet.stateAndSharedStyleChangeSetId].elementId = styleSet.element.id;
//                 sharedStyleChangeSets.array[styleSet.stateAndSharedStyleChangeSetId].newState = styleSet.state;
//             }
//
//             sharedStyleChangeSets.array[styleSet.stateAndSharedStyleChangeSetId].newState &= ~state;
//         }
//
//         // assumes value changed from previous instance value, regardless if active or not
//         public void SetProperty(StyleSet styleSet, in StyleProperty2 property, StyleState2 state) {
//
//             ref ChangeSet changeSet = ref GetChangeSet(styleSet);
//
//             // changeSet.SetInstanceProperty(property, state);
//
//             // this code is probably handled when flushing since we care about output properties that changed, not intermediate ones
//             // if ((property.propertyId.flags & PropertyFlags.Inherited) != 0) {
//             //     UIElement element = styleSet.element;
//             //     for (int i = 0; i < element.children.size; i++) {
//             //         UIElement child = element.children.array[i];
//             //         if (child.isEnabled) { // if not enabled then inheritance is handled via Initialize
//             //             GetChangeSet(child.styleSet2).EnqueueInheritedProperty(property, state);
//             //         }
//             //     }                                
//             // }
//         }
//
//         private ref ChangeSet GetChangeSet(StyleSet styleSet) {
//             // if (styleSet.changeSetId == -1) {
//             //     int id = nextChangeSetId++;
//             //     styleSet.changeSetId = id;
//             //     if (id >= changeSets.array.Length - 1) {
//             //         Array.Resize(ref changeSets.array, changeSets.array.Length * 2);
//             //     }
//             //
//             //     changeSets.array[id].state = styleSet.state;
//             //     changeSets.array[id].styleSet = styleSet;
//             // }
//
//             return ref changeSets.array[styleSet.changeSetId];
//         }
//
//     }
//
// }