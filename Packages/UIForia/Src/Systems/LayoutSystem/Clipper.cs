using UIForia.Graphics;
using UnityEngine;

namespace UIForia.Layout {

    public struct Clipper {

        public bool isCulled;
        public RangeInt intersectionRange;
        public AxisAlignedBounds2D aabb;
        public int parentIndex;
        public ElementId elementId;

        /// <summary>
        /// Only used for view clippers, lets me find the parallel segments of the clipper list by view
        /// </summary>
        public int subClipperCount;


    }

}