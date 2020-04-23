using System;
using System.Runtime.InteropServices;
using UIForia.Elements;
using UIForia.Selectors;
using UIForia.Util;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace UIForia {

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

}