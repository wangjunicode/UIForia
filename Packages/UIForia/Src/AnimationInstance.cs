using System;
using UIForia.Style;

namespace UIForia {

    ///<summary>
    /// operates on a single element
    /// </summary> 
    internal struct AnimationInstance : IComparable<AnimationInstance> {

        /// <summary>
        /// The state enum (Pending, Running etc.) 
        /// </summary>
        public AnimationState state;

        public BlockUsageSortKey sortKey;

        public ElementId elementId;
        
        public int elapsedMS;
        
        /// <summary>
        /// Used to cancel or pause this animation.
        /// </summary>
        public int executionToken;

        public int durationMS;

        public int direction;
        public int forwardDelay;
        public int reverseDelay;
        
        public AnimationLoopType loopType;
        public int iterationCount;
        public int totalIterations;
        public int delay;
        
        public AnimationReference animationReference;

        public int CompareTo(AnimationInstance other) {
            return sortKey.value > other.sortKey.value ? 1 : -1;
        }

    }

}