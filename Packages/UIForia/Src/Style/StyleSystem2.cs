using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using UIForia.Animation;
using UIForia.ListTypes;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace UIForia.Style {

    // public struct StylePart { }

    public unsafe struct BurstStyleDatabase {

        //public List_StylePart stylePartList;
        public StylePartList* stylePartsList;

    }

    public unsafe struct StylePartList {

        public StylePart* parts;
        public int partCount;

    }

    public struct StylePropertyInfo {

        public PropertyId propertyId;
        public PropertyData propertyData;

        public bool isStatic { get; set; }

    }

    public enum ContributorType {

        Selector,
        When,
        StyleBlock,
        AttrBlock,
        Animation

    }

    public unsafe struct Contributor {

        public ContributorType type;
        public int contributorId; // used for adding / removing / updating
        public ushort depthDiff; // higher is more influential
        public ushort partIndex; // higher is more influential
        public StylePropertyInfo* data;
        public int propertyCount;
        public void* ptr;

        // public fixed int map[sizeof(int) / 8]; set this to # of properties rounded up to sizeof long

    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct StylePartRef {

        public int contributorIndex;
        public StylePart* part;

    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct StylePart {

        public ushort localIdx;
        public bool isFullyStatic;
        public bool hasStaticProperties;
        public bool needsVariables; // maybe store which ids and which types and how many

        public int propertyCount;
        public StyleState2 state;
        public StylePropertyInfo* styleProperties;

    }

    public unsafe class StyleSystem2 {

        private ElementTable<StyleSetData> styleDataTable;
        private ElementTable<StyleInfo> styleInfoTable;
        private DataList<SharedStyleChangeEntry> entries;
        private List_StyleId buffer;

        // state will never change from bindings
        internal void SetStyleList_Binding(ElementId elementId, StyleId[] styleIds, int count) {
            ref StyleSetData styleData = ref styleDataTable.array[elementId.index];
            if (styleData.styleCount == count) {
                fixed (StyleId* styleIdPtr = styleIds) {
                    if (UnsafeUtility.MemCmp(styleIdPtr, styleData.styleIds, sizeof(StyleId) * count) == 0) {
                        return;
                    }
                }
            }

            RangeInt styleIdRange = new RangeInt(buffer.size, count);
            fixed (StyleId* styleIdPtr = styleIds) {
                buffer.AddRange(styleIdPtr, count);
            }

            if (styleData.styleChangeIndex == 0) {
                styleData.styleChangeIndex = (ushort) entries.size;
                entries.Add(new SharedStyleChangeEntry(elementId, (StyleState2Byte) styleData.state, styleIdRange));
            }
            else {
                entries[styleData.styleChangeIndex] = new SharedStyleChangeEntry(elementId, (StyleState2Byte) styleData.state, styleIdRange);
            }

        }

        public void EnterState(ElementId elementId, StyleState2 state) { }

        public void ExitState(ElementId elementId, StyleState2 state) { }

        public struct StyleIndexUpdate {

            public ElementId elementId;
            public StyleId styleId;

        }

        internal static unsafe void Animate(ref StyleInfo styleInfo) {

            if (!styleInfo.HasAnimations) {
                return;
            }

            // list is already sorted by importance
            for (int contribIdx = 0; contribIdx < styleInfo.styleContributors.size; contribIdx++) {
                ref Contributor contribution = ref styleInfo.styleContributors.array[contribIdx];

                if (contribution.type != ContributorType.Animation) {
                    continue;
                }

                AnimationInfo * animationInfo = (AnimationInfo*)contribution.ptr;
                
                // we compute a t value so we can emit events and fire triggers
                // animation output should be full properties or just values and a t?
                // values + t for layout properties, maaaybe always do this? 

                for (int j = 0; j < animationInfo->animatedProperties.size; j++) {

                    int propertyId = animationInfo->animatedProperties.array[j];

                    // will always be found
                    int idx = BinarySearch(styleInfo.styleProperties, propertyId);

                    int contribId = -1; // contributor id for style info
                    // if property not contributed by the animator, we don't bother animating this property
                    if (contribId != contribution.contributorId) {
                        continue;
                    }

                    AnimKeyFrame prev = FindPreviousKeyFrame();
                    AnimKeyFrame next = FindNextKeyFrame();

                    if((prev.flags & AnimKeyFrame.Current) != 0) {
                        // if uses variable -> resolve variable
                        
                        // ResolveCurrent(); -> for all other contributors that are not animations, find the value. if not found and inherited, use inherited value, if still not found or not inherited use default for property 
                    }
                    else if ((prev.flags & AnimKeyFrame.Inherit) != 0) {
                        // inherit, if not found -> use default
                    }
                    
                    styleInfo.styleProperties.array[idx].propertyData = default; // new AnimationResult(dataIdx);

                }
                
                // do all properties get a keyframe set? yes but only the ones they use
                // 
                // run()
                
                // need to know how to resolve 'current' and 'inherit' values
                
                // resolveKeyFrames();
                
                // i think we drop values we arent using on the floor here
                // so its advantageous to check for each animated property, if this animation is the source of that property or not
                

            }


        }

        private static AnimKeyFrame FindNextKeyFrame() {
            throw new System.NotImplementedException();
        }

        private static AnimKeyFrame FindPreviousKeyFrame() {
            throw new System.NotImplementedException();
        }

        private static int BinarySearch(AllocatedList_StylePropertyInfo styleInfoStyleProperties, int propertyId) {
            throw new System.NotImplementedException();
        }

        public struct AnimKeyFrame {

            public float time;
            public int flags;
            public PropertyData value;

            public const int Inherit = 1;
            public const int Current = 2;
            public const int Variable = 3;

        }

        internal struct AnimationInfo {

            public AnimKeyFrame* keyFrames;
            // public AnimationOptions options;
            public List_Int32 animatedKeyFrame;
            public List_Int32 animatedProperties;

        }
        
        internal static void Inherit(ref StyleInfo styleInfo) {
           
            
            // we can hold a set of defined properties in an intbool map
            // can quickly decide if any of the inherited properties need to be taken from the current inherit context
            
            // need a stack? of changes?
            // or linked list?
            
            // how do i know when to pop?
            
            
        }
        
        [BurstCompile]
        // [MonoPInvokeCallback()]

        internal static void RebuildStyles(ref StyleInfo styleInfo) {
            
            // don't unset the flag here, we'll do that when we perform the diff
            
            if (!styleInfo.needsStyleRebuild) {
                return;
            }
            
            DataList<StylePropertyInfo> propertyBuffer = new DataList<StylePropertyInfo>();

            uint* mapBuffer = stackalloc uint[8]; // is a guess, need 1 bit per property rounded up to next int
            uint* staticMapBuffer = stackalloc uint[8]; // is a guess, need 1 bit per property rounded up to next int

            IntBoolMap map = new IntBoolMap(mapBuffer, sizeof(uint) * 8);
            IntBoolMap staticMap = new IntBoolMap(staticMapBuffer, sizeof(uint) * 8);

            for (int contribIdx = 0; contribIdx < styleInfo.styleContributors.size; contribIdx++) {
                
                ref Contributor contributor = ref styleInfo.styleContributors.array[contribIdx];
                
                for (int i = 0; i < contributor.propertyCount; i++) {

                    ref StylePropertyInfo propertyInfo = ref contributor.data[i];

                    bool isStatic = propertyInfo.isStatic;
                    int index = propertyInfo.propertyId.index;
                    
                    // if is 'current' or 'inherit' maybe we mark it as such
                    
                    if (map.TrySetIndex(index)) {
                        
                        propertyBuffer.Add(propertyInfo);
                        
                        if (isStatic) {
                            staticMap[index] = true;
                        }
                        
                    }
                    else if (propertyInfo.isStatic && staticMap.TrySetIndex(index)) {
                        // replace, its definitely in the buffer
                        for (int j = 0; j < propertyBuffer.size; j++) {
                            if (propertyBuffer[j].propertyId.index == index) {
                                propertyBuffer[j] = propertyInfo;
                            }
                        }
                    }

                }

            }

            // propertyBuffer.Sort();
            // rebuiltStyles.Add(new RebuildStyle() { propertyBuffer, elementId});
        }

        [BurstCompile]
        // [MonoPInvokeCallback()]
        public static void DiffStyles() {

            DataList<SharedStyleChangeEntry> changeList = new DataList<SharedStyleChangeEntry>();

            // at this point we 100% know they are different because we checked already
            // alternative might be to just defer the check until now in hopes that burst is better at it than c# callback in a binding
            // if the order of the styles change but they are essentially the same styles, we treat that as changed
            // this list is usually 0-5 entries, so n^2 runtimes are perfectly fine
            // we *could* optimize for the usual case of adding/removing just last style but I'm not going to do that yet

            DataList<StyleIndexUpdate> addedList = new DataList<StyleIndexUpdate>();
            DataList<StyleIndexUpdate> removedList = new DataList<StyleIndexUpdate>();
            ElementTable<StyleInfo> styleInfoTable = new ElementTable<StyleInfo>();

            for (int changeIdx = 0; changeIdx < changeList.size; changeIdx++) {
                ref SharedStyleChangeEntry change = ref changeList[changeIdx];

                ref StyleInfo styleInfo = ref styleInfoTable[change.elementId];

                //      styleInfo.needsPartUpdate = true;

                // do i bother to differentiate between state | attribute and style list for part collection?
                // saves me the style lookup ... maybe

                StyleState2 containedStates = StyleState2.Normal;

                for (int i = 0; i < change.newStyleCount; i++) {

                    //  int count = styleDatabase.GetStyleParts(change.pNewStyles[i], out partList);

                    //for (int j = 0; j < count; j++) {
                    // containedStates |= part.state;
                    // if part is applicable()
                    // partBuffer.Add(part);
                    // }

                }

                // diff the part list with the previous one
                // if its the same (in cases where state changed but nobody cared), bail out, no need to update

                if (change.hadStyleListUpdate) {
                    for (int i = 0; i < change.newStyleCount; i++) { }

                    for (int i = 0; i < change.oldStyleCount; i++) { }
                }

            }

            for (int changeIdx = 0; changeIdx < changeList.size; changeIdx++) {
                ref SharedStyleChangeEntry change = ref changeList[changeIdx];

                if (change.hadStateUpdate) {
                    // state index.update...maybe this should happen immediately so user queries can also handle interact with the updated index
                }

            }

            for (int changeIdx = 0; changeIdx < changeList.size; changeIdx++) {
                ref SharedStyleChangeEntry change = ref changeList[changeIdx];

                if (change.hadStyledAttributeUpdate) { }

            }
            // sort added list by style id
            // sort removed list by style id

            for (int i = 0; i < removedList.size; i++) {
                // styleIndex.Remove();
            }

            for (int i = 0; i < addedList.size; i++) {
                // styleIndex.Add();
            }

        }

        public void HandleElementDisabled(DataList<ElementId>.Shared disabledElements) {
            // if had change set, remove it
        }

        public void HandleElementEnabled(DataList<ElementId>.Shared enabledElements) {

            // styleInfo -> where the real data is
            // changeSet -> where the updates live temporarily
            
            for (int i = 0; i < enabledElements.size; i++) {
                // if doesnt have a change set, create it
                ElementId elementId = enabledElements[i];
                ref StyleInfo styleInfo = ref styleInfoTable[elementId];
                styleInfo.updateFlags = StyleInfoUpdate.Initialize;
                if (styleInfo.changeSetId == 0) {
                    entries.Add(new SharedStyleChangeEntry() {
                        elementId = elementId,
                        oldState = 0,
                        newState = (StyleState2Byte) styleInfo.state,
                        pStyles = null // styleInfo.styleList,
                    });
                }
            }
            
        }

        internal static void RebuildContributors(ref StyleInfo styleInfo) {
            
            DataList<Contributor> contributorBuffer = new DataList<Contributor>();
            
            // add instance if present

            if (styleInfo.instanceStyles != null) {
                
            }

            // for each style
            
            // for each selector / when
            
            
        }

        public static void ProcessChanges(DataList<SharedStyleChangeEntry> changes) {
            for (int changeIdx = 0; changeIdx < changes.size; changeIdx++) {
                
                ref SharedStyleChangeEntry change = ref changes[changeIdx];
                
                // figure out if we need to mark for contributor rebuild or not
                
                // update style index
                
                // update style state index
                
                // ideally we figure out if the change op is a no-op or value only (instance change)
                
                // figure out which style parts to remove
                    // * which selectors to remove
                    // * which whens to remove
                    // * which variables to remove
                    // * which transitions to remove
                    
                    // maybe this comes by diffing contributor lists?
                    
                // contributors come in two phases, local and selected
                    // * need to know which selectors to apply
                    // * injecting selector contributors 
                
                // figuring out which style parts to add comes from recomputing contributors, which we need to do anyway if state changed, attr changed, styles changed, anything but instance
                
            }
        }

        [Flags]
        public enum StyleInfoUpdate {

            Initialize = 1 << 0

        }

        public void Update() {

            // first step is locate all the basic contributors
            // when disabling we remove all contributors except an instance one if it exists
            
            // how does the system store data?
            // change set & styleInfo make sense as element tables
            // what about array data?
            // aggressively over allocate?
            // block allocator per style and nuke on disable? requires some kind of thread lock or we are just single threaded always
            // unity allocator and stuff all data into it at some fixed block size? makes a lot of sense to keep all data for an element contiguous
            // kind of forced into single thread scenario
            // buddy alloc maybe makes sense for this, or blocks
            // i need to hang on to the style list even when disabled
            // so i probably have a seperate allocation for the other data on style info

            // if disabled
                
                // if had change set, copy the style buffer for when it gets enabled 
                // clear data, all children are disabled or destroyed too no need to clean up since they are in the list too
                
            // if destroyed
                // clear everything
                
            // if enabled
            
            // for each style in change sets
            // if dead or disabled?
            //    for each style
            //        remove from style index
            //    clean up style data, selectors can only target ancestors / descendents so we dont need to clean those up explicitly i think
            // else
            //     if has style changes
            //        diff styles
            //           add new ones to index
            //           remove old ones from index
            //     add to part update list if
            //         styles changed
            //         state changed (and any style in new style list is state sensitive for entered or exited states
            //         attributes changed (and isStyled) if not already on it
            //     add to style rebuild list if
            //        parts are different
            //        had instance style change for active state
            //     add to style recompute list if
            //        variable changed?

        }

        public void AddStyle(ElementId elementId, StyleId styleId) {
            
            ref StyleSetData styleData = ref styleDataTable.array[elementId.index];

        }

        public void SetStyleList(ElementId elementId, IList<StyleId> styleList) {

            ref StyleSetData styleData = ref styleDataTable.array[elementId.index];

            buffer.size = 0;

            if (styleList.Count > buffer.Capacity) {
                buffer.EnsureCapacity(buffer.size + styleList.Count);
            }

            for (int i = 0; i < styleList.Count; i++) {
                buffer.array[i] = styleList[i];
            }

            UnsafeUtility.MemCmp(buffer.array, styleData.sharedStyles.array, sizeof(StyleId) * styleList.Count);

            if (styleData.styleChangeIndex == 0) {
                styleData.styleChangeIndex = (ushort) entries.size;
                entries.Add(new SharedStyleChangeEntry(elementId, (StyleState2Byte) styleData.state, default));
            }

        }

    }

}