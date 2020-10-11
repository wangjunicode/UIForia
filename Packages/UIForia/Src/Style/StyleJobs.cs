using AOT;
using UIForia.ListTypes;
using UIForia.Rendering;
using UIForia.Sound;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UIForia.Style {

    public struct StyleTreeInput {

        public ElementTable<ElementMetaInfo> metaTable;
        public ElementTable<ElementTraversalInfo> traversalTable;
        public ElementTable<HierarchyInfo> hierarchyTable;

    }

    public struct StyleTreeOutput { }

    public delegate float TraverseTreeDelegate(StyleTreeInput input, out StyleTreeOutput output);

    [BurstCompile]
    internal static class StyleJobs {

        [BurstCompile]
        [MonoPInvokeCallback(typeof(TraverseTreeDelegate))]
        public static unsafe void TraverseTree(StyleTreeInput input, out StyleTreeOutput output) {
            ElementId root = default;

            ElementTable<ElementMetaInfo> metaTable = input.metaTable;
            ElementTable<ElementTraversalInfo> traversalTable = input.traversalTable;
            ElementTable<HierarchyInfo> hierarchyTable = input.hierarchyTable;

            List_ElementId stack = new List_ElementId(512, Allocator.TempJob);

            stack.Add(root);

            while (stack.size != 0) {
                ElementId elementId = stack[--stack.size];

                // if (isPop) {
                //     inheritStack.Pop();
                //     continue;
                // }
                
                if (ElementSystem.IsDeadOrDisabled(elementId, metaTable)) {
                    continue;
                }

                Animate();
                
                Inherit(); // need to build the list of properties we set that need to be pushed to children
                           // also need to pull values? probably not unless some value is explicitly set to 'inherit'
                           // for inheritance of values we transition but do not store,
                           // i guess we still want to run the transition and emit to change set with our value not the inherited one
                           
                Transition(); // also works with inherited properties without storing those values or pushing them along
                              // values are only in the inherit list if they changed
                EmitChangeList();

                ElementId ptr = hierarchyTable[elementId].firstChildId;

                while (ptr != default) {
                    stack.Add(ptr);
                    ptr = hierarchyTable[ptr].nextSiblingId;
                }

                // if (pushInheritedProperties) {
                //     stack.Add(new ElementId(-1));
                // }

            }

        }

        private static void EmitChangeList() {
            
            StyleInfo styleInfo = new StyleInfo();

            if (!styleInfo.HasPropertyChanges) {
                return;
            }

            DataList<StylePropertyInfo> changeList = new DataList<StylePropertyInfo>();
            
            // if has value only changes this is easy because the lists will be same length and we just diff the values
            if (styleInfo.HasOnlyValueChanges) {
                for (int i = 0; i < styleInfo.styleProperties.size; i++) {
                    // if styleInfo.previousProperties[i] != styleInfo.styleProperties[i]
                    // changeList.Add(styleProperty);
                }
                
                // also need to emit inheritance changes somehow...
                // for each inhertance entry
                // if not in property map
                // add to change list
                // but how do i know it actually changed?
                // maybe false positives are ok in some cases?
                // remove a property but then inherit the same value == changed but shouldn't
                // worst case? layout but theres not much there i think, mostly text that matters
                
            }
            else {
                // need to figure out what properties were added and removed and changed
                // 
            }
            
        }

        private static unsafe void Transition() {
            
            StyleInfo styleInfo = new StyleInfo();

            if (!styleInfo.HasTransitions) {
                return;
            }
            
            // foreach transition
                // maybe we've already built this list from contributors
            
            // how do we know which transitions win?
            // store value always?, nope, we know the time and can reconstruct the value
            
            // need to know if a property was set
            // we assume we dont run a transition that if it was just added
            // transitions effect properties set later

            // we know which contributors provide transitions and which ones
            // can't i just use the same idea with inheriting them
            
            for (int i = 0; i < styleInfo.transitionList.size; i++) {
                
                // if transition needs init
                //    init()
                
                ref StyleTransition transition = ref styleInfo.transitionList.array[i];
                
                int idx = BinarySearch(styleInfo.styleProperties, transition.propertyId);

                if (idx == -1) {
                    // remove any state we have? do we transition to default / inherited?
                    // or try to transition from inherited list?
                    // that would make us a provider which we arent
                }
                else {
                    // if new value set this frame
                    // if property at index is sourced from an animation, stop.
                    // we only want 1 transition per property id, should be handled when recomputing contributors
                    // since selectors can probably add transitions
                    if (transition.value.to.value != styleInfo.styleProperties.array[idx].propertyData.value) {
                        // reset state and start update
                    }
                    else {
                        // tick delay or tick duration
                        // if completed set flag to skip transitions 
                    }
                }
                
            }
            
            // how do we handle properties that in the inherited list?
            // need to store transition data for them & emit to change list
            // push to inherited output? maybe not since we aren't really the provider of the property
            // but 

        }

        private static int BinarySearch(AllocatedList_StylePropertyInfo styleInfoStyleProperties, PropertyId transitionPropertyId) {
            throw new System.NotImplementedException();
        }

        internal struct StyleTransition {

            public PropertyId propertyId;
            public UITimeMeasurement duration;
            public UITimeMeasurement delay;
            public EasingFunction easingFunction;
            public TransitionValue value;

        }

        public enum TransitionState {

            Initial,
            Complete,
            Delay,
            Update

        }
        
        public struct TransitionValue {

            public TransitionState state;
            public float remainingDuration;
            public float remainingDelay;
            public PropertyData to;
            public PropertyData from;

        }

        private static void Diff() {
            
            StyleInfo styleInfo = new StyleInfo();

            if (!styleInfo.needsStyleRebuild) {
                return;
            }

            styleInfo.needsStyleRebuild = false;
            
            // we have two cases here
            // one where we need to diff which styles are applied
            // and one where we just need to check the values
            // we already know they are different, we dont yet know which are different
            
            throw new System.NotImplementedException();
            
        }

        private static void Animate() {
            throw new System.NotImplementedException();
        }

        private static void Inherit() {
            throw new System.NotImplementedException();
        }

        [BurstCompile]
        [MonoPInvokeCallback(typeof(DiffStylePartsDelegate))]
        public static unsafe void DiffStyleParts() {

            DataList<SharedStyleChangeEntry> changes = default;
            BurstStyleDatabase* pStyleDatabase = default;

            ref BurstStyleDatabase styleDatabase = ref UnsafeUtility.AsRef<BurstStyleDatabase>(pStyleDatabase);

            List_Int32 stylePartBuffer = new List_Int32(128, Allocator.Temp);

            for (int changeIdx = 0; changeIdx < changes.size; changeIdx++) {
                ref SharedStyleChangeEntry change = ref changes[changeIdx];

                stylePartBuffer.size = 0;

                // gather the style parts for each style
                // we need to sort them then compare with the currently applied set

                ElementId elementId = change.elementId;
                StyleState2 state = (StyleState2) change.newState;

                // check if has styled attributes and store in flag, skip attr checks if not set
                bool hasStyledAttributes = false;

                // if has styled attributes
                // get the attributes and store in a buffer to be checked

                for (int i = 0; i < change.newStyleCount; i++) {

                    ref StylePartList partList = ref styleDatabase.stylePartsList[change.pStyles[i].index];

                    int partCount = partList.partCount;

                    for (int j = 0; j < partCount; j++) {
                        ref StylePart part = ref partList.parts[j];

                        if (IsPartApplicable(elementId, hasStyledAttributes, part, state)) {
                            // stylePartBuffer[stylePartBuffer.size++] = part.id;
                        }
                    }
                }

                // if (stylePartBuffer.size == styleInfo.partCount) {
                //     stylePartBuffer.Sort();
                //     if (UnsafeUtility.MemCmp(stylePartBuffer.array, styleInfo.partList, sizeof(StylePart) * stylePartBuffer.size) != 0) {
                //         // rebuildList.Add(elementId);
                //         // pendingRebuildList.AddRange(stylePartBuffer);
                //     }
                // }
                // else {
                //     // rebuildList.Add(elementId);
                //     // pendingRebuildList.AddRange(stylePartBuffer);
                // }

                // diff old and new part list
                // this will tell us which selectors to run 
                // once we find all the selectors to apply we can see if the styles actually need updating
                // variables need to be resolved here somehow
                // step 1 is to run the selectors
                // later we resolve the styles they would apply
                // that way we can diff the part list, selector list to see if we need to apply new ones

            }

        }

        private static bool IsPartApplicable(ElementId elementId, bool hasStyledAttributes, in StylePart part, StyleState2 styleState2) {
            return true;
        }

        private static bool IsPartApplicable() {
            return true;
        }

        public delegate float DiffStylePartsDelegate();

    }

}