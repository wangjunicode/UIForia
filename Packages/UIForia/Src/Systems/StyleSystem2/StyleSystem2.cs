using System;
using UIForia.Elements;
using UIForia.Rendering;
using UIForia.Util;

namespace UIForia.Systems {

    public class StyleSystem2 {

        public class ChangeSet {

            public StyleState state;
            public StructList<StyleUsage> instanceChanges;
            public LightList<StyleGroup> groupChanges;

        }

        private UIElement[] stack;
        private Application application;

        public StyleSystem2(Application application) {
            this.application = application;
            this.stack = new UIElement[32];
        }

        public void SetInstanceProperty(StyleSet2 styleSet2, in StyleProperty property, StyleState state) {
            // if (styleSet2.changeSet == null) {
            //     styleSet2.changeSet = StructList<StyleUsage>.Get();
            // }
            //
            // styleSet2.changeSet.Add(new StyleUsage() {
            //     property = property,
            //     priority = new StylePriority(SourceType.Instance, state)
            // });
        }

        public void SetDynamicStyles(StyleSet2 styleSet, StructList<StyleGroup> styleGroupList) { }

        public void EnterState(StyleSet2 styleSet, StyleState state) {
            if ((styleSet.activeStates & state) != 0) {
                return;
            }
        }

        public void ExitState(StyleSet2 styleSet, StyleState state) { }

        public unsafe void Update() {
            // traversal sort all things marked for change
            // for each element
            // set style groups (if created this frame or using dynamics and dynamics changed)
            // apply state changes

            // simple way is to traverse the tree and just ask each element for its changes

            // complex way would be to record that an element is a selector source and also walk that
            
            int size = 0;

            if (application.views.Count > stack.Length) {
                Array.Resize(ref stack, stack.Length + application.views.Count);
            }

            for (int i = 0; i < application.views.Count; i++) {
                stack[size++] = application.views[i].RootElement;
            }

            int* map = stackalloc int[(int)StylePropertyId.INVALID];

            while (size > 0) {
                UIElement current = stack[--size];

                if ((current.flags & UIElementFlags.EnabledFlagSet) != UIElementFlags.EnabledFlagSet) {
                    continue;
                }

                StyleSet2 styleSet2 = current.styleSet2;

                // if we have a state change or we have a new set of style groups to add
                // for each active selector that we provide to our children or self
                //     does this selector remain active?
                //     if not. go through all elements that were affected and remove 1 from their reference count. remove if count == 0

                // for each newly active selector
                //    add to list of active selectors

                // for each active selector
                // run selector
                // get result set
                // for each element in result set
                // if was not affected by this selector in the last frame
                // add to reference count

                if (styleSet2.changeSet != null) {

                    StructList<StyleUsage> changeSet = styleSet2.changeSet;

                    StructList<StyleGroup> dynamicGroups = styleSet2.dynamicGroups;

                    int diff = dynamicGroups.size - changeSet.newDynamicGroups.size;
                    for (int i = 0; i < dynamicGroups.size; i++) {
                            
                    }
                    
                    // flush instance & shared changes
                    // add new selectors
                    // remove old ones
                    // when removing need to also track all affected elements by this instance of the selector
                    // todo -- pool change sets
                    styleSet2.changeSet = null;
                }

                for (int i = 0; i < styleSet2.selectors.size; i++) {
                    
                    if (styleSet2.selectors[i].isActive) {
                        // run selector    
                        styleSet2.selectors[i].Run(resultSet);
                        
                    }
                    
                    // need to write 
                    
                }
                
                // if (styleSet2.selectors) {
                //     
                // }

                // flush all property changes before running animation
                // 

                // we want to apply style changes once per frame 
                // except when something gets animated, then twice is ok. (or just defer those changes for that element if animating)

                // if has changes
                // sort style usage array
                // figure out what properties changed
                // if any was inherited, push into inherit stack
                // batch to notify other systems

                UIElement[] children = current.children.array;
                int childCount = current.children.size;

                if (size + childCount >= stack.Length) {
                    Array.Resize(ref stack, size + childCount + 16);
                }

                for (int i = childCount - 1; i >= 0; i--) {
                    stack[size++] = children[i];
                }
            }

            // all things with selectors
            // all things with changes to flush

            // sort so that changes happen then selectors run since selectors may or may not remain valid after flushing dynamic styles
        }

        public void UpdateStyles() {
            // dynamic group added selector(s)            
        }

        public void UpdateSelectors() { }

        public void UpdateStates() {

            // input sets state

            // bindings etc

            // style changes from bindings (either batched or as part of bindings)

            // selectors

            // animations

            // transitions

            // each frame we need to know what changed
            // to tell layout
            // to tell rendering
            // to tell animation (transitions)

        }

    }

}