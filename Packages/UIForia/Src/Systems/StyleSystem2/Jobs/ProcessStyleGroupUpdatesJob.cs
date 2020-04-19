using UIForia.Util.Unsafe;
using Unity.Collections;
using Unity.Jobs;

namespace UIForia {

    public struct StyleStateGroup {

        public readonly int index;
        public readonly StyleId styleId;
        public readonly StyleState2 state;

        public StyleStateGroup(int index, StyleState2 state, StyleId styleId) {
            this.index = index;
            this.state = state;
            this.styleId = styleId;
        }

    }

    public unsafe struct ProcessSharedStyleUpdatesJob : IJob {

        public SharedListPointer<StyleStateGroup> addedListPointer;
        public SharedListPointer<StyleStateGroup> removedListPointer;
        public UnsafeSpan<SharedStyleChangeSet> changeSets;

        private UnsafeList<StyleStateGroup> addedStateGroups;
        private UnsafeList<StyleStateGroup> removedStateGroups;
        
        public void Execute() {

            addedStateGroups = addedListPointer.GetList();
            removedStateGroups = removedListPointer.GetList();
            
            for (int changeSetIdx = 0; changeSetIdx < changeSets.size; changeSetIdx++) {

                SharedStyleChangeSet changeSet = changeSets.array[changeSetIdx];
                
                StyleId* oldStyles = changeSet.oldStyles;
                StyleId* newStyles = changeSet.newStyles;

                for (int i = 0; i < changeSet.oldStyleCount; i++) {
                    StyleId styleId = oldStyles[i];
                    MaybeAddStyleGroup(ref removedStateGroups, changeSet.originalState, StyleState2.Normal, styleId);
                    MaybeAddStyleGroup(ref removedStateGroups, changeSet.originalState, StyleState2.Hover, styleId);
                    MaybeAddStyleGroup(ref removedStateGroups, changeSet.originalState, StyleState2.Focused, styleId);
                    MaybeAddStyleGroup(ref removedStateGroups, changeSet.originalState, StyleState2.Active, styleId);
                }

                for (int i = changeSet.oldStyleCount; i < changeSet.newStyleCount; i++) {
                    StyleId styleId = changeSet.styles[i];
                    MaybeAddStyleGroup(ref addedStateGroups, changeSet.newState, StyleState2.Normal, styleId);
                    MaybeAddStyleGroup(ref addedStateGroups, changeSet.newState, StyleState2.Hover, styleId);
                    MaybeAddStyleGroup(ref addedStateGroups, changeSet.newState, StyleState2.Focused, styleId);
                    MaybeAddStyleGroup(ref addedStateGroups, changeSet.newState, StyleState2.Active, styleId);
                }
            }
            
            addedListPointer.SetList(addedStateGroups);
            removedListPointer.SetList(removedStateGroups);

        }

        // change set is still burstable if we store the base stylelist somewhere
        // shared styles is an int array
        // int points to global id (per application) of a style
        // can probably be 2byte2byte scheme for stylesheet -> index of style in style sheet

        // change set burstable for style groups but maybe not properties since properties need an object field
        // 2 arrays then, 1 array<array<int>> for recording the shared styles + state? state at index 0?
        //                1 array<stylepropertydata> for recording instance styles

        private void Run(in SharedStyleChangeSet changeSet) {

            StackLongBuffer64 searchList = default;

           

            rebuildList.Add();

        }

        private static void MaybeAddStyleGroup(ref UnsafeList<StyleStateGroup> list, StyleState2 checkState, StyleState2 targetState, StyleId styleId) {
            if ((checkState & targetState) != 0 && styleId.DefinesState(targetState)) {
                list.array[list.size++] = new StyleStateGroup(list.size, targetState, styleId);
            }
        }

    }

}