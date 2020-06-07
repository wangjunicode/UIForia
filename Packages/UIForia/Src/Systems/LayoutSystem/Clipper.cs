using Unity.Mathematics;
using UnityEngine;

namespace UIForia.Layout {

    public struct Clipper {

        public bool isCulled;
        public RangeInt intersectionRange;
        public float4 aabb;
        public int parentIndex;
        public ElementId elementId;

        /// <summary>
        /// Only used for view clippers, lets me find the parallel segments of the clipper list by view
        /// </summary>
        public int subClipperCount;

    }

}