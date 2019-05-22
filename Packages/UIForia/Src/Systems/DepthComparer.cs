using System.Collections.Generic;
using UIForia.Layout.LayoutTypes;

namespace UIForia.Systems {

    public class DepthComparer : IComparer<LayoutBox> {

        public int Compare(LayoutBox a, LayoutBox b) {
            if (a.layer != b.layer) {
                return a.layer > b.layer ? -1 : 1;
            }

            if (a.viewDepthIdx != b.viewDepthIdx) {
                return a.viewDepthIdx > b.viewDepthIdx ? -1 : 1;
            }

            if (a.zIndex != b.zIndex) {
                return a.zIndex > b.zIndex ? -1 : 1;
            }

            return a.traversalIndex > b.traversalIndex ? -1 : 1;

        }

    }

}