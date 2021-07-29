using System.Runtime.InteropServices;
using UIForia;
using UIForia.Style;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace UIForia {


    [BurstCompile(DisableSafetyChecks = true)]
    internal unsafe struct RunAnimationJob : IJob {

        public DataList<DataList<KeyFrameResult>> resultBuffer;
        public DataList<AnimationOptions> animationOptionsTable;

        public DataList<AnimationKeyFrameRange> keyFrameRanges;
        public DataList<PropertyKeyFrames> animationProperties;
        public DataList<KeyFrameInfo> animationKeyFrames;

        [NativeDisableUnsafePtrRestriction] public DataList<AnimationCommand>* commandsThisFrame;
        [NativeDisableUnsafePtrRestriction] public DataList<AnimationInstance>* animationList;

        public int propertyTypeCount;
        public int frameDelta;
        public int usedPropertyMapSize;

        public void Execute() {

            for (int i = 0; i < commandsThisFrame->size; i++) {

                AnimationCommand cmd = commandsThisFrame->array[i];

                switch (cmd.type) {

                    case AnimationCommandType.Play:

                        bool isActive = false;
                        for (int animIdx = 0; animIdx < animationList->size; animIdx++) {
                            if (animationList->array[animIdx].elementId == cmd.elementId && animationList->array[animIdx].animationReference == cmd.animationReference) {
                                isActive = true;
                                break;
                            }
                        }

                        if (!isActive) {
                            animationList->Add(new AnimationInstance() {
                                elapsedMS = -frameDelta, // will cancel out to 0, removes special cases later on 
                                elementId = cmd.elementId,
                                state = AnimationState.StartDelay,
                                animationReference = cmd.animationReference,
                                executionToken = cmd.executionToken
                            });
                        }

                        break;

                    case AnimationCommandType.Stop: // stop

                        // for (int animIdx = 0; animIdx < playingAnimations.size; animIdx++) {
                        //     AnimationInstance animation = default;
                        //     if (animation.executionToken == cmd.executionToken) {
                        //         // remove
                        //         // add to dead list 
                        //     }
                        // }

                        break;
                }
            }

            // sorting by precedence 
            NativeSortExtension.Sort(animationList->array, animationList->size);

            ulong* map = stackalloc ulong[usedPropertyMapSize];
            LongBoolMap usedPropertyMap = new LongBoolMap(map, usedPropertyMapSize);

            int usedPropertyCount = FindUsedProperties(usedPropertyMap);

            int* usedPropertyBuffer = stackalloc int[usedPropertyCount];
            CheckedArray<int> usedProperties = usedPropertyMap.FillBuffer(usedPropertyBuffer, usedPropertyCount);

            int propertyMapSize = LongBoolMap.GetMapSize(GetPropertyMapSize());

            ulong* propertyMemory = TypedUnsafe.MallocCleared<ulong>(usedProperties.size * propertyMapSize, Allocator.Temp);
            TempList<ElementMap> propertyMaps = TypedUnsafe.MallocSizedTempList<ElementMap>(propertyTypeCount, Allocator.Temp);

            for (int i = 0; i < usedProperties.size; i++) {
                propertyMaps[usedProperties[i]] = new ElementMap(propertyMemory + (propertyMapSize * i), propertyMapSize);
            }

            UpdateAnimations(propertyMaps);

            propertyMaps.Dispose();
            TypedUnsafe.Dispose(propertyMemory, Allocator.Temp);

        }

        private int GetPropertyMapSize() {
            int maxElementId = 1;
            for (int i = 0; i < animationList->size; i++) {
                ref AnimationInstance anim = ref animationList->Get(i);
                if (anim.elementId.index > maxElementId) {
                    maxElementId = anim.elementId.index;
                }
            }

            return maxElementId;
        }

        private int FindUsedProperties(LongBoolMap usedPropertyMap) {
            for (int i = 0; i < animationList->size; i++) {

                ref AnimationInstance anim = ref animationList->Get(i);
                ref AnimationKeyFrameRange keyFrameRange = ref keyFrameRanges[anim.animationReference.animationId.id];
                CheckedArray<PropertyKeyFrames> propertyKeyFrames = animationProperties.Slice(keyFrameRange.start, keyFrameRange.Count);

                // todo -- could also check if we already examined this animation id and bail if we did

                for (int p = 0; p < propertyKeyFrames.size; p++) {
                    usedPropertyMap.Set(propertyKeyFrames[p].propertyId.index);
                }

            }

            return usedPropertyMap.PopCount();
        }

        private void UpdateAnimations(TempList<ElementMap> propertyMaps) {
            for (int i = 0; i < animationList->size; i++) {

                ref AnimationInstance anim = ref animationList->Get(i);
                ref AnimationOptions options = ref animationOptionsTable[anim.animationReference.optionId.id];

                switch (anim.state) {

                    case AnimationState.StartDelay: {

                        if (anim.elapsedMS + frameDelta >= options.startDelay.AsMillisecondsOrValue) { 
                            anim.elapsedMS = -frameDelta;
                            anim.state = AnimationState.Running;

                            // todo -- resolve and use variables for options

                            anim.durationMS = options.duration.AsMillisecondsOrValue;
                            anim.totalIterations = options.iterations;
                            anim.direction = options.direction == AnimationDirection.Forward ? 1 : -1;
                            anim.forwardDelay = options.forwardStartDelay.AsMillisecondsOrValue;
                            anim.reverseDelay = options.reverseStartDelay.AsMillisecondsOrValue;
                            anim.loopType = options.loopType;
                            anim.iterationCount = 0;
                            i--; // look at this element again 
                            break;
                        }

                        anim.elapsedMS += frameDelta;

                        break;
                    }

                    case AnimationState.ForwardDelay: {

                        if (anim.elapsedMS + frameDelta >= options.forwardStartDelay.AsMillisecondsOrValue) { 
                            anim.elapsedMS = -frameDelta;
                            anim.state = AnimationState.Running;
                            i--; // look at this element again 
                            break;
                        }

                        ref AnimationKeyFrameRange frameRange = ref keyFrameRanges.array[anim.animationReference.animationId.id];
                        CheckedArray<PropertyKeyFrames> propertyKeyFrames = animationProperties.Slice(frameRange.start, frameRange.Count);

                        for (int p = 0; p < propertyKeyFrames.size; p++) {

                            PropertyKeyFrames propertyKeyFrame = propertyKeyFrames[p];
                            int propertyIndex = (int) propertyKeyFrame.propertyId;
                            KeyFrameInfo frame = animationKeyFrames[propertyKeyFrame.keyFrames.start];
                            ElementMap propertyMap = propertyMaps[propertyIndex];

                            if (propertyMap.TrySet(anim.elementId)) {

                                resultBuffer[propertyIndex].Add(new KeyFrameResult() {
                                    prev = frame.startValueId,
                                    next = frame.endValueId,
                                    elementId = anim.elementId,
                                    nextVarId = frame.endVarId,
                                    prevVarId = frame.startVarId,
                                    t = 0f
                                });
                            }

                        }

                        anim.elapsedMS += frameDelta;
                        break;
                    }

                    case AnimationState.ReverseDelay: {
                        if (anim.elapsedMS + frameDelta >= options.reverseStartDelay.AsMillisecondsOrValue) { 
                            anim.elapsedMS = -frameDelta;
                            anim.state = AnimationState.Running;
                            i--; // look at this element again 
                            break;
                        }

                        ref AnimationKeyFrameRange frameRange = ref keyFrameRanges.array[anim.animationReference.animationId.id];
                        CheckedArray<PropertyKeyFrames> propertyKeyFrames = animationProperties.Slice(frameRange.start, frameRange.Count);

                        for (int p = 0; p < propertyKeyFrames.size; p++) {

                            PropertyKeyFrames propertyKeyFrame = propertyKeyFrames[p];

                            CheckedArray<KeyFrameInfo> keyFrames = animationKeyFrames.Slice(propertyKeyFrame.keyFrames);

                            int propertyIndex = (int) propertyKeyFrame.propertyId;

                            KeyFrameInfo frame = keyFrames[keyFrames.size - 1];
                            ElementMap propertyMap = propertyMaps[propertyIndex];

                            if (propertyMap.TrySet(anim.elementId)) {

                                resultBuffer[propertyIndex].Add(new KeyFrameResult() {
                                    prev = frame.startValueId,
                                    next = frame.endValueId,
                                    elementId = anim.elementId,
                                    nextVarId = frame.endVarId,
                                    prevVarId = frame.startVarId,
                                    t = 1f
                                });
                            }

                        }

                        anim.elapsedMS += frameDelta;
                        break;
                    }

                    case AnimationState.Running: {

                        anim.elapsedMS += frameDelta;

                        if (anim.elapsedMS > anim.durationMS) anim.elapsedMS = anim.durationMS;

                        float completionPercent = math.clamp((float) anim.elapsedMS / (float) anim.durationMS, 0f, 1f);

                        ref AnimationKeyFrameRange frameRange = ref keyFrameRanges.array[anim.animationReference.animationId.id];
                        CheckedArray<PropertyKeyFrames> propertyKeyFrames = animationProperties.Slice(frameRange.start, frameRange.Count);

                        if (anim.direction == 1) {
                            for (int p = 0; p < propertyKeyFrames.size; p++) {

                                PropertyKeyFrames propertyKeyFrame = propertyKeyFrames[p];

                                CheckedArray<KeyFrameInfo> keyFrames = animationKeyFrames.Slice(propertyKeyFrame.keyFrames);

                                int propertyIndex = (int) propertyKeyFrame.propertyId;
                                int frameIndex = FindKeyFrameIndex(keyFrames, completionPercent, anim.durationMS);

                                KeyFrameInfo frame = keyFrames[frameIndex];

                                ElementMap propertyMap = propertyMaps[propertyIndex];

                                if (propertyMap.TrySet(anim.elementId)) {

                                    int start = frame.startTime.ToMilliseconds(anim.durationMS);
                                    int end = frame.endTime.ToMilliseconds(anim.durationMS);

                                    float t = MathUtil.PercentOfRange(anim.elapsedMS, start, end);

                                    resultBuffer[propertyIndex].Add(new KeyFrameResult() {
                                        next = frame.endValueId,
                                        prev = frame.startValueId,
                                        elementId = anim.elementId,
                                        nextVarId = frame.endVarId,
                                        prevVarId = frame.startVarId,
                                        t = t
                                    });
                                }

                            }
                        }
                        else {
                            for (int p = 0; p < propertyKeyFrames.size; p++) {

                                PropertyKeyFrames propertyKeyFrame = propertyKeyFrames[p];

                                CheckedArray<KeyFrameInfo> keyFrames = animationKeyFrames.Slice(propertyKeyFrame.keyFrames);

                                int propertyIndex = (int) propertyKeyFrame.propertyId;
                                int frameIndex = FindKeyFrameIndexReverse(keyFrames, completionPercent, anim.durationMS);

                                KeyFrameInfo frame = keyFrames[frameIndex];
                                // todo add EasingFunctions here

                                ElementMap propertyMap = propertyMaps[propertyIndex];

                                if (propertyMap.TrySet(anim.elementId)) {

                                    int start = frame.startTime.ToMilliseconds(anim.durationMS);
                                    int end = frame.endTime.ToMilliseconds(anim.durationMS);

                                    float t = MathUtil.PercentOfRange(anim.durationMS - anim.elapsedMS, start, end);

                                    resultBuffer[propertyIndex].Add(new KeyFrameResult() {
                                        prev = frame.startValueId,
                                        next = frame.endValueId,
                                        elementId = anim.elementId,
                                        nextVarId = frame.endVarId,
                                        prevVarId = frame.startVarId,
                                        t = t,
                                    });
                                }

                            }
                        }

                        if (completionPercent == 1f) {
                            anim.iterationCount++;
                            anim.elapsedMS = 0;

                            if (anim.loopType == AnimationLoopType.PingPong) {
                                anim.direction *= -1;
                            }

                            // infinite or not yet done w/ iterations
                            if (anim.totalIterations <= 0 || anim.iterationCount < anim.totalIterations) {
                                anim.state = anim.direction == 1 ? AnimationState.ForwardDelay : AnimationState.ReverseDelay;
                                // emit event?
                            }
                            else {
                                // end, swap remove since order doesn't matter & precidence handled elsewhere 
                                animationList->RemoveAt(i--);
                                // emit event? 
                            }

                        }

                        break;

                    }
                }

            }
        }

        private static int FindKeyFrameIndex(CheckedArray<KeyFrameInfo> frames, float completionPercent, int durationMS) {

            for (int i = frames.size - 1; i >= 0; i--) {
                if (completionPercent >= frames[i].startTime.ToPercentage(durationMS)) {
                    return i;
                }
            }

            return 0;
        }

        private static int FindKeyFrameIndexReverse(CheckedArray<KeyFrameInfo> frames, float completionPercent, int durationMS) {

            completionPercent = 1f - completionPercent;

            for (int i = 0; i < frames.size; i++) {
                if (completionPercent <= frames[i].endTime.ToPercentage(durationMS)) {
                    return i;
                }
            }

            return 0;
        }

    }

}