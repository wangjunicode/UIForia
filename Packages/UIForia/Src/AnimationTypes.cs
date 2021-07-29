using UIForia.Rendering;
using UIForia.Style;
using UnityEngine;

namespace UIForia {

    internal enum AnimationState {
        StartDelay,
        Running,
        Paused,
        Cancelled,
        Completed,
        ForwardDelay,
        ReverseDelay
    }


    // AnimatedPropertys property = elementId, prev, next, curve, t
    // animation system needs to output AnimatedProperties 
    // all keyframe transitions need to have been handled already when passed to property solvers

    // sequence has n Animations
    internal struct AnimationOptions2 {

        public UITimeMeasurement delay;
        public UITimeMeasurement duration;

        // public int iterations;
        // public UITimeMeasurement loopTime;
        // public UITimeMeasurement forwardStartDelay;
        // public UITimeMeasurement reverseStartDelay;
        // public AnimationDirection direction;
        // public AnimationLoopType loopType;
        // public EasingFunction timingFunction; // todo add Easing.Custom?
        // public AnimationFillMode fillMode;
        // public int startTriggerId;
        // public int completeTriggerId;


    }

    
    internal struct AnimationKeyFrameRange {

        public int start;
        public int end;

        public AnimationKeyFrameRange(int start, int length) {
            this.start = start;
            this.end = start + length;
        }

        public int Count => end - start;

    }

    /// <summary>
    /// Contains all keyframes of a property
    /// </summary>
    internal struct PropertyKeyFrames {

        public PropertyId propertyId;

        /// <summary>
        /// Sorted by time.
        /// Range into StyleDatabase.animationKeyFrames.
        /// - array of KeyFrameInfo
        /// </summary>
        public RangeInt keyFrames;
    }

    public struct KeyFrameInfo {

        public UITimeMeasurement startTime;
        public UITimeMeasurement endTime;
        public int endValueId; // point at styledb_data_table
        public int startValueId; // point at styledb_data_table
        public ushort startVarId;
        public ushort endVarId;

    }

    internal struct KeyFrameResult {

        public int prev;
        public int next;

        public ushort nextVarId;
        public ushort prevVarId;
        
        // easing function has been applied already
        public float t;
        public ElementId elementId;
        public OptionId optionId;
        
        public const int k_Initial = -1;
        public const int k_Current = -2;

    }

}