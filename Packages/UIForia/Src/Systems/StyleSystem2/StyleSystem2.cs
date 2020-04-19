// using System;
// using System.Collections.Generic;
// using UIForia.Compilers.Style;
// using UIForia.Elements;
// using UIForia.Rendering;
// using UIForia.Selectors;
// using UIForia.Util;
// using Unity.Burst;
// using Unity.Collections;
// using Unity.Jobs;
// using UnityEngine;
//
// namespace UIForia.Style {
//
//     public class UIStyleTemp { }
//
//     // [BurstCompile]
//     public struct SetupSelectorDataJob : IJob {
//
//         public void Execute() { }
//
//     }
//
//     // [BurstCompile]
//     public struct RunSelectorJob : IJob {
//
//         public NativeArray<StyleSystem2.SelectorChange> selectorChanges;
//
//         public void Execute() {
//
//             // for(int i = 0; i < )
//
//         }
//
//     }
//
//     // [BurstCompile]
//     public struct SetupJob : IJob {
//
//         public void Execute() { }
//
//     }
//
//     // [BurstCompile]
//     public struct ApplyStyleJob : IJob {
//
//         public void Execute() { }
//
//     }
//
//     /**
//      * Style System
//      *
//      *     During user code, style is given a bunch of changes to process. These get buffered in the style system until it gets a chance to run
//      * 
//      *     Changes come in 3 varieties
//      *         1. Instance style was set
//      *         2. Dynamic styles were updated
//      *         3. State changed (Hover | Focus | Active)
//      *
//      *     for every style that has pending changes we need to start flushing those changes.
//      *     We only want to actually apply style once because we need to inform other systems
//      *     as to which styles changed for which element. This means we have to run selectors
//      *     before we can write any changes into the actual elements.
//      *
//      *     So the way this works it that we first issue a job(s) that figures out the style diffs for state and group changes.
//      *     Using that result we can figure out which selectors are no longer valid and which now became valid.
//      *
//      *     Then we need to run all selectors. order should not matter if we can sort the result at the end so could probably be done
//      *     in parallel.
//      *
//      *     Now we issue all the run commands which might enqueue sounds or animations to play
//      *
//      *     At this point we apply styles to actual elements and build an initial diff list. This involves flushing the instance and inherited
//      *     style changes as well.
//      * 
//      *     At this point we can run transitions, but they won't update diff list yet because animation might be running.
//      *
//      *     Now we run animations and for any element that was animated we need to re-apply style and update diff list
//      *
//      *     Finish the frame by setting the output data for layout & rendering.
//      *
//      *     There are a few steps to styling:
//      *         The first is collecting all the style properties that have an effect on our element.
//      *         The second is finding which of those to actually apply. 
//      * 
//      */
//     public class StyleSystem2 {
//
//         public class ChangeSet {
//
//             public StyleState state;
//             public readonly StructList<StyleUsage> instanceChanges;
//             public readonly StructList<StyleGroup> groupChanges;
//             public readonly StructList<SelectorChange> selectorChanges;
//
//             public ChangeSet() {
//                 groupChanges = new StructList<StyleGroup>();
//                 instanceChanges = new StructList<StyleUsage>();
//                 selectorChanges = new StructList<SelectorChange>();
//             }
//
//         }
//
//         public struct SelectorChange {
//
//             public readonly int selectorId;
//             public readonly SelectorChangeType changeType;
//             public readonly ElementReference reference;
//
//             public SelectorChange(SelectorChangeType changeType, int selectorId, in ElementReference reference) {
//                 this.changeType = changeType;
//                 this.selectorId = selectorId;
//                 this.reference = reference;
//             }
//
//         }
//
//         public enum SelectorChangeType {
//
//             AddedToRunList,
//             RemovedFromRunList,
//             AddedToEffectList,
//             RemovedFromEffectList
//
//         }
//
//         private UIElement[] stack;
//         private Application application;
//         private LightList<StyleSet2> changeSet;
//         private StructList<StyleGroup> styleGroups;
//
//         // private NativeList<RunCommand> runCommands;
//         // private NativeList<Selector> selectorMap;
//         // private NativeList<StyleGrouping> styleGroupings;
//         // private NativeList<SelectorEffect> effects;
//         private NativeArray<StyleProperty2> persistentPropertyTable;
//
//         internal NativeArray<int> persistentStringMap;
//         internal Dictionary<string, int> persistentMapReverseLookup;
//
//         public event Action<UIElement, StructList<StyleProperty>> onStylePropertyChanged;
//         
//         public StyleSystem2(Application application) {
//             this.application = application;
//             this.stack = new UIElement[32];
//             this.changeSet = new LightList<StyleSet2>(32);
//             // this.selectorMap = new NativeList<Selector>(64, Allocator.Persistent);
//             // this.persistentStringMap = new NativeArray<int>(2048, Allocator.Persistent);
//         }
//         
//         public void Run() {
//
//             SetupJob setupJob = new SetupJob(); // handle all things initialized this frame or with dynamic style changes
//
//             JobHandle setupJobHandle = setupJob.Schedule();
//
//             RunSelectorJob selectorJob = new RunSelectorJob();
//
//             SetupSelectorDataJob setupSelectorDataJob = new SetupSelectorDataJob(); // get data in a format selectors can easily consume
//
//             JobHandle selectorDataHandle = setupSelectorDataJob.Schedule();
//
//             JobHandle selectorJobHandle = selectorJob.Schedule(JobHandle.CombineDependencies(setupJobHandle, selectorDataHandle)); // run the selectors
//
//             ApplyStyleJob applyStyleJob = new ApplyStyleJob();
//
//             JobHandle applyStyleHandle = applyStyleJob.Schedule(selectorJobHandle);
//
//             // animation
//
//             // apply again
//
//             // transitions
//
//             // diff & publish
//             JobHandle.ScheduleBatchedJobs();
//
//             applyStyleHandle.Complete();
//
//         }
//
//         public void SetInstanceProperty(StyleSet2 styleSet2, in StyleProperty property, StyleState state) {
//             if (styleSet2.changeSet == null) {
//                 styleSet2.changeSet = new ChangeSet();
//                 changeSet.Add(styleSet2);
//             }
//
//             styleSet2.changeSet.instanceChanges.Add(new StyleUsage() {
//                 property = property,
//                 priority = new StylePriority(SourceType.Instance, state)
//             });
//         }
//
//         public void SetDynamicStyles(StyleSet2 styleSet, StructList<StyleGroup> styleGroupList) {
//             if (styleSet.changeSet == null) {
//                 styleSet.changeSet = new ChangeSet(); // pool
//                 changeSet.Add(styleSet);
//             }
//
//             // styleSet.changeSet.groupChanges = styleGroupList;
//         }
//
//         public void EnterState(StyleSet2 styleSet, StyleState state) {
//             if ((styleSet.activeStates & state) != 0) {
//                 return;
//             }
//
//             if (styleSet.changeSet == null) {
//                 styleSet.changeSet = new ChangeSet();
//                 changeSet.Add(styleSet);
//             }
//
//             styleSet.changeSet.state = styleSet.activeStates | state;
//         }
//
//         public void ExitState(StyleSet2 styleSet, StyleState state) {
//             if ((styleSet.activeStates & state) == 0) {
//                 return;
//             }
//
//             if (styleSet.changeSet == null) {
//                 styleSet.changeSet = new ChangeSet();
//                 changeSet.Add(styleSet);
//             }
//
//             styleSet.changeSet.state = styleSet.activeStates & ~state;
//         }
//
//         private StructList<StyleUsage> scratch = new StructList<StyleUsage>(64);
//
//         private unsafe void ApplyStyleGroupChangesWithStateChange(StyleSet2 style, StyleState oldState, StyleState newState) {
//             int cnt = 0;
//             int* changeIds = stackalloc int[style.changeSet.groupChanges.size + style.dynamicGroups.size];
//
//             for (int i = 0; i < style.dynamicGroups.size; i++) {
//                 changeIds[cnt++] = style.dynamicGroups.array[i].id;
//             }
//
//             // todo -- for dynamic styles we can probably play with state and not add them if they originating from an active state
//             // for every removed style group, remove any properties originating from that group
//             for (int i = 0; i < style.usageCount; i++) {
//                 ref StyleUsage usage = ref style.styleUsages[i];
//
//                 for (int j = 0; j < cnt; j++) {
//                     if (usage.sourceId.id == changeIds[j]) {
//                         usage = style.styleUsages[style.usageCount - 1];
//                         // make sure we don't leak the object reference
//                         style.styleUsages[style.usageCount - 1].property.objectField = null;
//                         style.usageCount--;
//                         break;
//                     }
//                 }
//             }
//
//             // for (int i = 0; i < style.selectors.size; i++) {
//             //     Selector selector = style.selectors[i];
//             //
//             //     for (int j = 0; j < selector.resultSet.size; i++) {
//             //         selector.resultSet[j].RemoveSelector(selector.id);
//             //     }
//             //
//             //     selector.resultSet.Clear();
//             // }
//
//             style.selectors.Clear();
//
//             int addCount = 0;
//
//             for (int i = 0; i < style.changeSet.groupChanges.size; i++) {
//                 ref StyleGroup group = ref style.changeSet.groupChanges.array[i];
//
//                 addCount += group.normal.properties.Length;
//
//                 if ((newState & StyleState.Hover) != 0) {
//                     addCount += group.hover.properties.Length;
//                 }
//
//                 if ((newState & StyleState.Active) != 0) {
//                     addCount += group.active.properties.Length;
//                 }
//
//                 if ((newState & StyleState.Focused) != 0) {
//                     addCount += group.focus.properties.Length;
//                 }
//             }
//
//             if (style.usageCount + addCount >= style.styleUsages.Length) {
//                 Array.Resize(ref style.styleUsages, style.usageCount + addCount + 8);
//             }
//
//             for (int i = 0; i < style.changeSet.groupChanges.size; i++) {
//                 ref StyleGroup group = ref style.changeSet.groupChanges.array[i];
//
//                 if (group.normal.properties.Length != 0) {
//                     Array.Copy(group.normal.properties, 0, style.styleUsages, style.usageCount, group.normal.properties.Length);
//                     style.usageCount += (ushort) group.normal.properties.Length;
//                 }
//
//                 if ((newState & StyleState.Hover) != 0) {
//                     Array.Copy(group.hover.properties, 0, style.styleUsages, style.usageCount, group.hover.properties.Length);
//                     style.usageCount += (ushort) group.hover.properties.Length;
//                 }
//
//                 if ((newState & StyleState.Active) != 0) {
//                     Array.Copy(group.active.properties, 0, style.styleUsages, style.usageCount, group.active.properties.Length);
//                     style.usageCount += (ushort) group.active.properties.Length;
//                 }
//
//                 if ((newState & StyleState.Focused) != 0) {
//                     Array.Copy(group.focus.properties, 0, style.styleUsages, style.usageCount, group.focus.properties.Length);
//                     style.usageCount += (ushort) group.focus.properties.Length;
//                 }
//             }
//
//             for (int i = 0; i < style.changeSet.groupChanges.size; i++) {
//                 ref StyleGroup group = ref style.changeSet.groupChanges.array[i];
//                 if (group.selectors != null) {
//                     for (int j = 0; j < group.selectors.Length; j++) {
//                         if ((group.selectors[j].state & newState) != 0) {
//                             style.selectors.Add(group.selectors[j]);
//                         }
//                     }
//                 }
//             }
//         }
//
//         private unsafe void ApplyStyleGroupChanges(StyleSet2 style, StyleState state) {
//             // update state
//             int cnt = 0;
//             int* changeIds = stackalloc int[style.changeSet.groupChanges.size + style.dynamicGroups.size];
//
//             // todo -- for now just removing all and re-adding later. should do a better diff
//
//             for (int i = 0; i < style.dynamicGroups.size; i++) {
//                 changeIds[cnt++] = style.dynamicGroups.array[i].id;
//             }
//
//             // todo -- for dynamic styles we can probably play with state and not add them if they originating from an active state
//             // for every removed style group, remove any properties originating from that group
//             for (int i = 0; i < style.usageCount; i++) {
//                 ref StyleUsage usage = ref style.styleUsages[i];
//                 for (int j = 0; j < cnt; j++) {
//                     if (usage.sourceId.id == changeIds[j]) {
//                         usage = style.styleUsages[style.usageCount - 1];
//                         // make sure we don't leak the object reference
//                         style.styleUsages[style.usageCount - 1].property.objectField = null;
//                         style.usageCount--;
//                         break;
//                     }
//                 }
//             }
//
//             // for every added style group, add any properties originating from that group
//             int addCount = 0;
//             for (int i = 0; i < style.changeSet.groupChanges.size; i++) {
//                 ref StyleGroup addedGroup = ref style.changeSet.groupChanges.array[i];
//                 addCount += addedGroup.normal.properties.Length;
//                 addCount += addedGroup.active.properties.Length;
//                 addCount += addedGroup.hover.properties.Length;
//                 addCount += addedGroup.focus.properties.Length;
//             }
//
//             if (style.usageCount + addCount >= style.styleUsages.Length) {
//                 Array.Resize(ref style.styleUsages, style.usageCount + addCount + 8);
//             }
//         }
//
//         private void UpdateSelectorRunList(StyleSet2 styleSet2) {
//             // selector changes are changes in selectors that we push out. In this case we want to find any selectors
//             // that we no longer publish and remove their effects from other elements. We can't manipulate the target
//             // directly at this point, so we push the selector id on to the target's change set. The target might be 
//             // the publishing element in case of 'when' selectors. 
//
//             if (styleSet2.changeSet == null || styleSet2.changeSet.selectorChanges.size == 0) {
//                 return;
//             }
//
//             ElementReference reference = styleSet2.element;
//
//             for (int i = 0; i < styleSet2.changeSet.selectorChanges.size; i++) {
//                 ref SelectorChange change = ref styleSet2.changeSet.selectorChanges.array[i];
//
//                 if (change.changeType == SelectorChangeType.RemovedFromRunList) {
//
//                     styleSet2.RemoveSelector(change.selectorId);
//
//                     // find all targets that were effected by this selector and remove them. if target was 'this' element, just mark in own change set
//                     for (int j = 0; j < styleSet2.selectorEffects.size; j++) {
//                         // ref SelectorEffect effect = ref styleSet2.selectorEffects.array[i];
//                         //
//                         // UIElement target = application.ResolveElementReference(effect.elementReference);
//                         //
//                         // // if we were effecting a destroyed element, continue
//                         // if (target == null) {
//                         //     continue;
//                         // }
//                         //
//                         // if (target.styleSet2.changeSet == null) {
//                         //     target.styleSet2.changeSet = new ChangeSet(); // todo -- pool
//                         //     changeSet.Add(target.styleSet2);
//                         // }
//                         //
//                         // target.styleSet2.changeSet.selectorChanges.Add(new SelectorChange(SelectorChangeType.RemovedFromEffectList, change.selectorId, reference));
//                     }
//                 }
//                 else if (change.changeType == SelectorChangeType.AddedToRunList) {
//                     // todo -- what happens with duplicates?
//                     //styleSet2.selectors.Add(selectorMap[change.selectorId]);
//                 }
//             }
//         }
//
//         public unsafe void Update() {
//             // traversal sort all things marked for change
//             // for each element
//             // set style groups (if created this frame or using dynamics and dynamics changed)
//             // apply state changes
//
//             // simple way is to traverse the tree and just ask each element for its changes
//
//             int size = 0;
//
//             if (application.views.Count > stack.Length) {
//                 Array.Resize(ref stack, stack.Length + application.views.Count);
//             }
//
//             for (int i = 0; i < application.views.Count; i++) {
//                 stack[size++] = application.views[i].RootElement;
//             }
//
//             int* map = stackalloc int[PropertyParsers.PropertyCount];
//
//             for (int i = 0; i < changeSet.size; i++) {
//                 StyleSet2 style = changeSet.array[i];
//
//                 // if groups changed we need to swap remove all styles from removed groups. we need to recompute active styles and reprioritize anyway later on
//                 if (style.changeSet.groupChanges != null) {
//                     //      ApplyStyleGroupChanges(style);
//                 }
//
//                 if (style.changeSet.state != 0) {
//                     style.activeStates = style.changeSet.state;
//
//                     for (int j = 0; j < style.selectors.size; j++) {
//                         // if (style.selectors[j].active && (style.selectors[j].state & style.activeStates) == 0) {
//                         //     // remove
//                         // }
//                     }
//                 }
//             }
//
//             int traversalIndex = 0;
//             while (size > 0) {
//                 UIElement current = stack[--size];
//
//                 if ((current.flags & UIElementFlags.EnabledFlagSet) != UIElementFlags.EnabledFlagSet) {
//                     continue;
//                 }
//                 //
//                 // StyleSet2 styleSet2 = current.styleSet2;
//                 // styleSet2.traversalIndex = traversalIndex++;
//
//                 // if we have a state change or we have a new set of style groups to add
//                 // for each active selector that we provide to our children or self
//                 //     does this selector remain active?
//                 //     if not. go through all elements that were affected and remove 1 from their reference count. remove if count == 0
//
//                 // for each newly active selector
//                 //    add to list of active selectors
//
//                 // for each active selector
//                 // run selector
//                 // get result set
//                 // for each element in result set
//                 // if was not affected by this selector in the last frame
//                 // add to reference count
//
//                 // if (styleSet2.changeSet != null) {
//                 //     ChangeSet changeSet = styleSet2.changeSet;
//                 //
//                 //     StructList<StyleGroup> dynamicGroups = styleSet2.dynamicGroups;
//                 //
//                 //     int diff = dynamicGroups.size - changeSet.groupChanges.size;
//                 //
//                 //     if (diff == 0) {
//                 //         int index = 0;
//                 //         for (int i = 0; i < dynamicGroups.size; i++) {
//                 //             if (dynamicGroups.array[i].id != changeSet.groupChanges.array[i].id) {
//                 //                 index = i;
//                 //                 break;
//                 //             }
//                 //         }
//                 //
//                 //         // remove all groups from our array
//                 //         for (int i = index; i < dynamicGroups.size; i++) {
//                 //             ref StyleGroup group = ref dynamicGroups.array[i];
//                 //             for (int j = 0; j < styleSet2.usageCount; j++) {
//                 //                 ref StyleUsage usage = ref styleSet2.styleUsages[j];
//                 //                 if (usage.sourceId.id == group.id) {
//                 //                     StyleUsage swap = styleSet2.styleUsages[styleSet2.usageCount - 1];
//                 //                     usage = swap;
//                 //                     styleSet2.usageCount--;
//                 //                 }
//                 //             }
//                 //         }
//                 //     }
//                 //
//                 //     // flush instance & shared changes
//                 //     // add new selectors
//                 //     // remove old ones
//                 //     // when removing need to also track all affected elements by this instance of the selector
//                 //     // todo -- pool change sets
//                 //     styleSet2.changeSet = null;
//                 // }
//                 //
//                 // for (int i = 0; i < styleSet2.selectors.size; i++) {
//                 //     // run selector    
//                 //     if ((styleSet2.selectors[i].state & styleSet2.activeStates) != 0) {
//                 //         //   styleSet2.selectors[i].Run(resultSet);
//                 //     }
//                 //
//                 //     // need to write 
//                 // }
//                 //
//                 // // if changed
//                 //
//                 // for (int i = 0; i < styleSet2.splitIndex; i++) {
//                 //     ref StyleUsage usage = ref styleSet2.styleUsages[i];
//                 //     map[(int) usage.property.propertyId] = 1;
//                 // }
//
//                 // dont really need a sort actually just need to shuffle the high priority actives to the front and everything else can be in unsorted in the back
//                 // for each property keep a score in the map
//                 for (int i = 0; i < scratch.size; i++) { }
//
//                 // check for changes
//                 // new properties will have a positive value, removed will have a negative. 0 for unchanged
//                 // for (int i = 0; i < StylePropertyCount; i++) {
//                 //     if (map[i] != 0) { }
//                 // }
//
//                 // flush all property changes before running animation
//
//                 // we want to apply style changes once per frame 
//                 // except when something gets animated, then twice is ok. (or just defer those changes for that element if animating)
//
//                 // if has changes
//                 // sort style usage array
//                 // figure out what properties changed
//                 // if any was inherited, push into inherit stack
//                 // batch to notify other systems
//
//                 UIElement[] children = current.children.array;
//                 int childCount = current.children.size;
//
//                 if (size + childCount >= stack.Length) {
//                     Array.Resize(ref stack, size + childCount + 16);
//                 }
//
//                 for (int i = childCount - 1; i >= 0; i--) {
//                     stack[size++] = children[i];
//                 }
//             }
//
//             // all things with selectors
//             // all things with changes to flush
//
//             // sort so that changes happen then selectors run since selectors may or may not remain valid after flushing dynamic styles
//         }
//
//         public void UpdateStyles() {
//             // dynamic group added selector(s)            
//         }
//
//         public void UpdateSelectors() { }
//
//         public void UpdateStates() {
//             // input sets state
//
//             // bindings etc
//
//             // style changes from bindings (either batched or as part of bindings)
//
//             // selectors
//
//             // animations
//
//             // transitions
//
//             // each frame we need to know what changed
//             // to tell layout
//             // to tell rendering
//             // to tell animation (transitions)
//         }
//
//         public static int GetStringId(string data) {
//             throw new NotImplementedException();
//         }
//
//         public void OnUpdate() {
//         }
//
//
//     }
//
// }