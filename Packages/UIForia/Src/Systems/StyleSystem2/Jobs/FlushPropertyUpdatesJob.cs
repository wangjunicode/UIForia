using System;
using UIForia.Style;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace UIForia {

    // to avoid storing all instance data for all elements, opt in to a property cache on a per element basis

    // element.style.ShouldCacheProperties(true);
    // element.style.GetPropertyCache().Get(StylePropertyId)
    // computing a style property for an element should be fairly fast (slower than currently is ok for uncached)
    // 


    public unsafe struct Property {

        public ushort id;
        public ushort flags;
        public int intData;
        public long longData;

    }

    public unsafe struct PropertyListAllocator {

        public StyleProperty2* Allocate(int count) {
            return default;
        }

    }

    public unsafe struct UpdateInstanceProperties : IJob {

        public PropertyListAllocator allocator;
        public NativeSlice<InstanceStyleChangeSet> changeSetSlice;
        public UnmanagedList<StyleSetInstanceData> instanceDataMap;

        public void Execute() {

            // todo -- 256 should be total # of style properties
            NativeArray<StyleProperty2> scratch = new NativeArray<StyleProperty2>(256, Allocator.Temp);

            UnsafeSizedBuffer<InstanceStyleChangeSet> changeSets = new UnsafeSizedBuffer<InstanceStyleChangeSet>(changeSetSlice);

            for (int changeSetIdx = 0; changeSetIdx < changeSets.size; changeSetIdx++) {

                ref InstanceStyleChangeSet changeSet = ref changeSets.array[changeSetIdx];

                StyleSetInstanceData instanceData = default; //instanceDataMap.array[changeSet.instanceDataId];

                StyleProperty2* currentProperties = instanceData.properties;

                // or we just always replace the list?

                // 1 output list
                // need to know how big it will be first
                // dont need exact size, just need approximate
                // can be oversized but not undersized

                // problem is worst case, when max style count changes

                // need 4 scratch arrays for that? technically yes :(

                // to remove we need to set a flag in propertyId.flags (PropertyFlags)
                // feels hacky but shouldn't be an issue since flags are internal todo -> make them internal
                if (currentProperties == null) {
                    // resize 

                    // allocator.Allocate();

                }
                else {
                    int requiredSize = GuessListSize(ref instanceData, ref changeSet);
                }

                // might need to reallocate data list

            }

            scratch.Dispose();
        }

        private static int GuessListSize(ref StyleSetInstanceData instanceData, ref InstanceStyleChangeSet changeSet) {

            // StyleProperty2* instanceStyles = instanceData.properties;
            //
            // if (instanceStyles != null) {
            //
            //     int count = EstimatePropertyCount(instanceData.normal, instanceStyles, changeSet.normalChangeCount, changeSet.normalChanges);
            //
            //     instanceStyles += instanceData.normal;
            //
            //     count += EstimatePropertyCount(instanceData.hover, instanceStyles, changeSet.hoverChangeCount, changeSet.hoverChanges);
            //
            //     instanceStyles += instanceData.hover;
            //
            //     count += EstimatePropertyCount(instanceData.focus, instanceStyles, changeSet.focusChangeCount, changeSet.focusChanges);
            //
            //     instanceStyles += instanceData.focus;
            //
            //     count += EstimatePropertyCount(instanceData.active, instanceStyles, changeSet.activeChangeCount, changeSet.activeChanges);
            //
            //     return count;
            // }
            // else {
            //     int count = EstimatePropertyCount(0, null, changeSet.normalChangeCount, changeSet.normalChanges);
            //     count += EstimatePropertyCount(0, null, changeSet.hoverChangeCount, changeSet.hoverChanges);
            //     count += EstimatePropertyCount(0, null, changeSet.focusChangeCount, changeSet.focusChanges);
            //     count += EstimatePropertyCount(0, null, changeSet.activeChangeCount, changeSet.activeChanges);
            //     return count;
            // }

            throw new NotImplementedException();
        }

        private static int EstimatePropertyCount(int existingStyleCount, StyleProperty2* existingStyles, int changedStyleCount, StyleProperty2* changedStyles) {

            BitBuffer512 buffer = new BitBuffer512();
            IntBoolMap map = new IntBoolMap(buffer.ptr, 16);

            for (int i = 0; i < existingStyleCount; i++) {
                ref StyleProperty2 property = ref existingStyles[i];
                map[property.propertyId.index] = true;
            }

            for (int i = 0; i < changedStyleCount; i++) {
                ref StyleProperty2 property = ref changedStyles[i];
                map[property.propertyId.index] = (property.propertyId.typeFlags & PropertyTypeFlags.Removed) == 0;
            }

            return BitUtil.CountSetBits(buffer.ptr, 16);

        }

        private static int AddProperties(StyleProperty2* updates, int count, ref NativeArray<StyleProperty2> output) {
            IntBoolMap map = default;
            int scratchSize = 0;

            for (int i = count - 1; i >= 0; i--) {
                ref StyleProperty2 property = ref updates[i];
                if (map.TrySetIndex(property.propertyId.index)) {
                    output[scratchSize++] = property;
                }
            }

            return scratchSize;
        }

    }

    public unsafe struct SelectorEffectData {

        public int count;
        public StyleProperty2* properties;

    }


    public unsafe struct FlushPropertyUpdatesJob : IJob {

        private const int k_MaxStyleStateSize = StyleSet.k_MaxSharedStyles * 4; // max styles each with max 4 states

        public UnmanagedList<InstanceStyleChangeSet> instanceChangeSetMap;
        public UnmanagedPagedList<StyleProperty2> stylePropertyTable;

        [ReadOnly]
        public UnmanagedPagedList<VertigoStyle> styleTable;
        // new StyleCache(properties);
        // cache.Update(propertyList);
        // if cache.Changed(property) {
        //    
        // }

        public void Execute() {
            int changeSetCount = 1;

            for (int changeSetIdx = 0; changeSetIdx < changeSetCount; changeSetIdx++) {
                BitBuffer256 buffer256 = default;
                IntBoolMap activeMap = new IntBoolMap(buffer256.data, 8);

                StyleSetData styleData = default;

                StyleState2 state = (StyleState2) styleData.state;
                
                UnmanagedList<StyleProperty2> outputStyles = new UnmanagedList<StyleProperty2>();

                // store currently active style set
                // would then store all inherited properties as well

                // should i store a style list per element?
                // easy to check values
                // much more memory usage
                // cache method to opt in makes a lot more sense to me
                // when setting a property i still need to see if it would apply 
                // if properties set but not for an active state -> no-op

                // build list of instance properties first
                if (styleData.instanceData != null) {
                    StyleSetInstanceData instanceData = default;
                    // old one is already sorted
                    for (int i = 0; i < instanceData.propertyCount; i++) {
                        ref StyleProperty2 property = ref instanceData.properties[i];
                        if (activeMap.TrySetIndex(property.propertyId.index)) {
                            outputStyles.Add(property);
                        }
                    }
                }

                // now add selectors
                if (styleData.selectorDataId != -1) {
                    SelectorEffectData selectorEffectData = default;
                    for (int i = 0; i < selectorEffectData.count; i++) {
                        ref StyleProperty2 property = ref selectorEffectData.properties[i];
                        if (activeMap.TrySetIndex(property.propertyId.index)) {
                            outputStyles.Add(property);
                        }
                    }
                }

                // todo -- for style properties that are 'compiled' statically we can re-arrange the buffer to read 
                // property Id's up front and skip t
                for (int sharedStyleIndex = 0; sharedStyleIndex < styleData.sharedStyleCount; sharedStyleIndex++) {
                    StyleId styleId = styleData.sharedStyles[sharedStyleIndex];
                    VertigoStyle style = styleTable[styleId.index];

                    if ((style.propertyOffset == ushort.MaxValue)) {
                        continue;
                    }

                    StyleProperty2* properties = stylePropertyTable.GetPointer(style.propertyOffset);
                    
                    if ((state & StyleState2.Active) != 0) {
                        for (int i = 0; i < style.activeCount; i++) {
                            ref StyleProperty2 property = ref properties[i];
                            if (activeMap.TrySetIndex(property.propertyId.index)) {
                                outputStyles.Add(property);
                            }
                        }
                    }
                    
                    properties += style.activeCount;
                    
                    if ((state & StyleState2.Focused) != 0) {
                        for (int i = 0; i < style.focusCount; i++) {
                            ref StyleProperty2 property = ref properties[i];
                            if (activeMap.TrySetIndex(property.propertyId.index)) {
                                outputStyles.Add(property);
                            }
                        }
                    }

                    properties += style.focusCount;
                    
                    if ((state & StyleState2.Hover) != 0) {
                        for (int i = 0; i < style.focusCount; i++) {
                            ref StyleProperty2 property = ref properties[i];
                            if (activeMap.TrySetIndex(property.propertyId.index)) {
                                outputStyles.Add(property);
                            }
                        }
                    }
                    
                    properties += style.hoverCount;
                    
                    for (int i = 0; i < style.focusCount; i++) {
                        ref StyleProperty2 property = ref properties[i];
                        if (activeMap.TrySetIndex(property.propertyId.index)) {
                            outputStyles.Add(property);
                        }
                    }

                }

                // output = per element rebuild list 

                // paged lists
                // PagedListLocation<StyleProperty2> pageLocation = outputStyleData.AddRange(oldStyles); // make sure they all fit in the same page
                // outputHeaderData.Add(new RebuildResult(styleData.changeSetId, buffer256, pageLocation));

                // now i have my last frame state (except for inherited from last frame, but maybe ignore those?)

                // can start computing next frame state at this point

                // how much work am I doing in order to change check?
                // seems like a lot
                // is it faster to just re-grab properties when anything changed && compare local?
                // ie only build a 'current' list? on change and publish that instead of diffing?
                // let user diff it or put up a caching interface instead?

                // then we only diff whats actually needed when its needed

                // can output a property map and the current state
                // then diffing is up to user
                // but the user is basically just layout + rendering
                // and text i guess but it already diffs internally
                // this kind of fixes inheritence as well since then when something cares about an inherited property change, it can just ask for it
                // diffing can still be done per system in parallel / burst
                // so instead of a diff list per element we just push out a current state list and a map of what is inside it

                // if the changes get sorted by traversal it should be easy to do inherit via polling for those who care
                // 2 lists, 1 ushort = ushort list to tell us about containment and index
                // 1 actual property list

                // inheritence can come as a seperate job that takes current state outputs, checks for inherited changes and pushes them outwards
                // 2nd traversal per inherited change

                // probably want 1 traversal with inherited state and walk down completed tree
                // if hadInheritedChanges
                //     enqueue

                // special case text
                // if text property changed
                // find all text node descendents
                // if not marked for update, mark for update
                // then we dont pay cost for non text nodes

                // except for text, only ~6 inherited properties
                // which means holding per-element inherited property list or merging with instance list might be ok
                // or even enqueing traversals for inherited changes

                // when text property changes
                // mark all text descendents to poll that property
                // which will do an upward walk on each of those to find the value to use if not defined
                // theres probably something clever I can do here. ie check if node n defines that property or is marked for query by a lower depth node
                // since inheritence is lowest priority this can be done as post step in an additive way

                // just not sure how make this safely parallel which we definitely want
                // can reserve memory for that if its really only 6 properties
                // i expect inherited to be rarely set other than text
                // maybe store pointer to parent data who defined it?
                // then unsetting somewhere kinda sucks

            }

            // for each property in change set. if state applicable 
            // if removed, remove from active set
            // if try add to active set
            //     add to output list
            // else 
            //     replace in output list

            // if state not applicable still need to update stored instance styles somehow
            // probably a 3rd list for merged output

            // we need 2 things here
            // 1 rebuild old instance styles, maybe we just ignore state applicability and do that check later when deciding if we are dirty or not
            // 2 build new instance styles

            // how can i store properties so they still sit in 16 bytes but can have flags and state on them?
            // probably can't 
            // can internally override/steal the property flags
            // but then comparing 2 property ids isn't the same
            // maybe have a different struct internally where flags are stored different?
            // then we'd have to convert when moving into dirty lists

            // instanceStyleChangeSet.activeChanges

            // output = list of changed properties (not inherited)
            // also need to update instance style data with new values
            // instance diffing can be improved with higher memory usage
            // when creating change set for properties -> copy current list as a basis 
            // saves me from doing it here twice and checking membership
            // but maybe we don't give a fuck

            // take the instance property change list (might need to treat inherited as instance for diffing) 
            // apply it to previous instance styles to get updated instance list

            // compute new styles based on that
            // 

            // var oldStyleList = styleData.sharedStyles;
            // var oldSelectorList = styleData.selectorList;
            // var oldInstanceList = instanceData.properties;
            //
            // // return a lookup table so we don't have to diff like this? maybe the diff search is ok
            // BuildStyles(oldStyles, oldInstanceList, oldStyleList, oldSelectorList);
            // BuildStyles(newStyles, newInstanceList, newStyleList, newSelectorList);

            // but then how do i get the removed ones?
            // will have to iterate something
            // lookup table stores index?  still bad
            // 2 tables?

            // need to populate a dirty list
            // to do this we need to diff this frame's styles with last frame's
            // but we dont store last frame's styles
            // so we need to instead re-construct it
            // which means for instance styles we take the old instance list
            // as a base, and then any time we have an instance overwrite we replace / remove it

            // add all instance styles to map if not already in
            //
            // for (int i = instanceData.active.start; i < instanceData.active.end ; i++) {
            //     //    ref StyleProperty2 property = 
            //     //    if(activeMap.TrySetIndex())
            // }

            // selectors have a precedence so sort them by that
            // walk through them front to back
            // if we encounter a property we didnt have in our map, add it

            // then go through styles in precedence order

        }

        private void BuildStyles(UnmanagedList<StyleProperty2> oldStyles, StyleProperty2* oldInstanceList, long* oldStyleList, object oldSelectorList) {
            // instance styles
            // selectors (need to be sorted)
            // shared styles (already sorted)
            // for each inherited
            // if not set
            // look it up (assumes ancestors are up to date, can I know this at this point?) not if we do this in parallel
        }

    }

}