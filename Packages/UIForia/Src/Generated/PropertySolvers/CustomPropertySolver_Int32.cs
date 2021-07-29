using System;
using UIForia.Rendering;
using UIForia.Util.Unsafe;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace UIForia.Style {

    internal unsafe struct CustomPropertySolver_Int32 : ICustomPropertySolver {

        public DataList<PropertyUpdate> sharedProperties;
        public DataList<Transition<Int32>> transitions;
        public UnsafeHashMap<ElementId, Int32> valueMap;

        public Int32 defaultValue;

        public Int32 GetValue(ElementId elementId, Int32 defaultOverride, bool useDefault) {

            if (!valueMap.TryGetValue(elementId, out Int32 retn)) {
                retn = useDefault ? defaultOverride : defaultValue;
            }

            return retn;
        }

        public void Solve(PropertyId propertyId, ref SolverParameters parameters, ref PropertyTables propertyTables, ref CustomSolverInfo sharedSolverInfo, BumpAllocator* tempAllocator) {

            ref CheckedArray<PropertyUpdate> sharedUpdateList = ref parameters.sharedRebuildResult->updateLists[(int) propertyId];
            ref TransitionUpdateList transitionUpdateList = ref parameters.transitionUpdateResult->updateLists[(int) propertyId];
            ref InstancePropertyUpdateList instanceUpdateList = ref parameters.instanceRebuildResult->updateLists[(int) propertyId];

            ref DataList<KeyFrameResult> keyFramesProperties = ref parameters.animationResultBuffer[propertyId];

            DataList<Int32> dbValues = propertyTables.propertyTable_Int32;

            ElementMap animationMap = sharedSolverInfo.animationMap; // already cleared
            ElementMap definitionMap = sharedSolverInfo.definitionMap; // already cleared

            CheckedArray<ValueVariable> variableList = sharedSolverInfo.GetVariableList_ValueVariable();
            ref DataList<PropertyContainer> variableResolveList = ref sharedSolverInfo.variableResolveList;

            int defaultIndex = parameters.defaultValueIndices[(int) propertyId];
            defaultValue = dbValues[defaultIndex];

            // Remove dead transitions
            // todo -- still need to handle transitions being removed without a replacement
            for (int i = 0; i < transitions.size; i++) {
                if (parameters.invalidatedElementMap.Get(transitions[i].elementId)) {
                    transitions.array[i--] = transitions.array[--transitions.size];
                }
            }

            if (transitionUpdateList.size > 0) {

                for (int i = 0; i < transitionUpdateList.size; i++) {
                    ElementId elementId = transitionUpdateList.array[i].elementId;
                    int transitionId = transitionUpdateList.array[i].transitionId;

                    bool found = false;
                    for (int j = 0; j < transitions.size; j++) {
                        ref Transition<Int32> transition = ref transitions[j];

                        if (transition.elementId != elementId) {
                            continue;
                        }

                        found = true;
                        if (transition.transitionId != transitionId) {
                            // replace current w/ new one
                            transition.transitionId = transitionId;
                            if (transition.state == TransitionState.Active) {
                                // we don't know if the value will also change yet
                                // just mark it as changed and figure this out later
                                transition.state = TransitionState.Replacing;
                            }
                            else {
                                TransitionDefinition transitionDefinition = parameters.transitionDatabase[transitionId];
                                transition.bezier = transitionDefinition.bezier;
                                transition.easing = transitionDefinition.easing;
                                transition.duration = transitionDefinition.duration;
                                transition.delay = transitionDefinition.delay;
                                transition.elapsedTime = 0;

                            }
                        }

                        break;

                    }

                    if (!found) {
                        // create new transition 

                        if (!valueMap.TryGetValue(elementId, out Int32 prevValue)) {
                            prevValue = defaultValue;
                        }

                        TransitionDefinition transitionDefinition = parameters.transitionDatabase[transitionId];

                        transitions.Add(new Transition<Int32>() {
                            state = parameters.initMap.Get(elementId) ? TransitionState.Init : TransitionState.Pending,
                            prevValue = prevValue,
                            elementId = elementId,
                            duration = transitionDefinition.duration,
                            delay = transitionDefinition.delay,
                            elapsedTime = 0,
                            bezier = transitionDefinition.bezier,
                            easing = transitionDefinition.easing,
                            transitionId = transitionId,
                        });

                    }
                }

            }

            // change check for active 
            // if valueMap[elementId] != toProperty  

            // for each property we got from the shared property list, if element was disabled/ destroyed or is getting rebuilt this frame, swap remove

            for (int i = 0; i < sharedProperties.size; i++) {
                if (sharedSolverInfo.invalidOrRebuiltMap.Get(sharedProperties[i].elementId)) {
                    sharedProperties.array[i--] = sharedProperties.array[--sharedProperties.size];
                }
            }

            sharedProperties.AddRange(sharedUpdateList.array, sharedUpdateList.size);

            variableResolveList.size = 0;

            valueMap.Clear(); // this implementation kind of sucks.

            for (int i = 0; i < instanceUpdateList.size; i++) {
                ref PropertyContainer update = ref instanceUpdateList.array[i];

                definitionMap.Set(update.elementId);

                if (update.variableNameId != ushort.MaxValue) {
                    variableResolveList.Add(update);
                    continue;
                }

                Int32 value = update.Get<Int32>();
                valueMap.Add(update.elementId, value);

            }

            // this is re-solving all properties every frame. can probably skip a lot of this by checking a map
            // to see if the element had a shared changed or not. would still need to re-handle variables i guess

            for (int i = 0; i < sharedProperties.size; i++) {
                PropertyUpdate update = sharedProperties[i];

                if (!definitionMap.TrySet(update.elementId)) {
                    continue;
                }

                Int32 dbValue = dbValues[update.dbValueLocation];

                if (update.IsVariable) {

                    PropertyContainer propertyContainer = new PropertyContainer() {
                        variableNameId = update.variableNameId,
                        elementId = update.elementId
                    };

                    propertyContainer.Set(dbValue);

                    variableResolveList.Add(propertyContainer);
                }
                else {
                    valueMap.Add(update.elementId, dbValue);
                }
            }

            ResolveVariables(variableResolveList, variableList, parameters.traversalTable);

            variableResolveList.size = 0;

            ApplyAnimations(ref parameters, variableList, keyFramesProperties, dbValues, defaultIndex, animationMap);

            ApplyTransitions(ref parameters, animationMap);

        }

        private void ApplyAnimations(ref SolverParameters parameters, CheckedArray<ValueVariable> variableList, DataList<KeyFrameResult> keyFramesProperties, DataList<Int32> dbValues, int defaultIndex, ElementMap animationMap) {
            if (keyFramesProperties.size == 0) {
                return;
            }

            for (int i = 0; i < keyFramesProperties.size; i++) {
                KeyFrameResult keyFrameProp = keyFramesProperties[i];

                if (!valueMap.TryGetValue(keyFrameProp.elementId, out Int32 currentValue)) {
                    currentValue = dbValues[defaultIndex];
                }

                // todo -- if we want to support an explicit 'inherit' value we'd have to traverse the elements by depth level, but it should be possible 
                Int32 next = keyFrameProp.next == KeyFrameResult.k_Current ? currentValue : dbValues[keyFrameProp.next];
                Int32 prev = keyFrameProp.prev == KeyFrameResult.k_Current ? currentValue : dbValues[keyFrameProp.prev];

                next = ResolveAnimationVariable(keyFrameProp.nextVarId, next, parameters.traversalTable, variableList, keyFrameProp.elementId);
                prev = ResolveAnimationVariable(keyFrameProp.prevVarId, prev, parameters.traversalTable, variableList, keyFrameProp.elementId);

                Int32 result = Int32_Interpolator.Interpolate(prev, next, keyFrameProp.t);

                valueMap[keyFrameProp.elementId] = result;

                animationMap.Set(keyFrameProp.elementId);

            }
        }

        private void ApplyTransitions(ref SolverParameters parameters, ElementMap animationMap) {
            for (int i = 0; i < transitions.size; i++) {

                ref Transition<Int32> transition = ref transitions[i];

                if (parameters.initMap.Get(transition.elementId) || animationMap.Get(transition.elementId)) {
                    continue;
                }

                if (!valueMap.TryGetValue(transition.elementId, out Int32 currentValue)) {
                    currentValue = defaultValue;
                }

                switch (transition.state) {

                    case TransitionState.Init: {
                        transition.prevValue = currentValue;
                        transition.state = TransitionState.Pending;
                        break;
                    }

                    case TransitionState.Pending: {

                        if (!transition.prevValue.Equals(currentValue)) {

                            // valueMap value must be set to current
                            valueMap[transition.elementId] = transition.prevValue;

                            transition.nextValue = currentValue;
                            transition.state = TransitionState.Active;

                        }

                        break;
                    }

                    case TransitionState.Active: {
                        transition.elapsedTime += parameters.deltaMS;
                        float progress = 0;
                        Int32 output = transition.prevValue;

                        if (transition.elapsedTime >= transition.delay) {
                            progress = math.saturate((transition.elapsedTime - transition.delay) / (float) transition.duration);
                            float t = Easing.Interpolate(progress, transition.easing, ref transition.bezier);
                            output = Int32_Interpolator.Interpolate(transition.prevValue, transition.nextValue, t);
                        }

                        if (progress == 1f) {
                            transition.state = TransitionState.Pending;
                            transition.prevValue = currentValue;
                            transition.elapsedTime = 0;
                        }

                        valueMap[transition.elementId] = output;
                        break;
                    }

                    case TransitionState.Replacing: {

                        TransitionDefinition newTransition = parameters.transitionDatabase[transition.transitionId];
                        Int32 output = transition.prevValue;

                        if (transition.elapsedTime >= transition.delay) {
                            float progress = math.saturate((transition.elapsedTime - transition.delay) / (float) transition.duration);
                            float t = Easing.Interpolate(progress, transition.easing, ref transition.bezier);
                            output = Int32_Interpolator.Interpolate(transition.prevValue, transition.nextValue, t);
                        }

                        transition.delay = newTransition.delay;
                        transition.bezier = newTransition.bezier;
                        transition.easing = newTransition.easing;
                        transition.duration = newTransition.duration;
                        transition.elapsedTime = 0;
                        transition.prevValue = output;
                        transition.state = TransitionState.Active;

                        if (currentValue.Equals(transition.nextValue)) {
                            // todo -- if target value did not change maybe handle this differently?
                        }

                        i--;
                        break;
                    }

                }

            }
        }

        private Int32 ResolveAnimationVariable(ushort nextVarId, Int32 current, CheckedArray<TraversalInfo> traversalInfoTable, CheckedArray<ValueVariable> variableList, ElementId elementId) {

            if (nextVarId == ushort.MaxValue) {
                return current;
            }

            int nearestDistance = int.MaxValue;
            int targetIndex = -1;
            Int32 resolved = default;

            for (int v = 0; v < variableList.size; v++) {

                ref ValueVariable variable = ref variableList.Get(v);
                ref TraversalInfo traversalInfo = ref traversalInfoTable.Get(elementId.index);

                int depthDiff = traversalInfo.depth - variable.traversalInfo.depth;

                if (depthDiff > nearestDistance || !traversalInfo.IsSelfOrDescendentOf(variable.traversalInfo) || !variable.TryConvertToInt32(out resolved)) {
                    continue;
                }

                nearestDistance = depthDiff;
                targetIndex = v;

            }

            return targetIndex == -1
                ? current
                : resolved;
        }

        private void ResolveVariables(DataList<PropertyContainer> variableResolveList, CheckedArray<ValueVariable> variableList, CheckedArray<TraversalInfo> traversalInfoTable) {

            // todo -- some sorting might help reduce iterations here, sort by depth and move our variable search start point accordingly 

            for (int i = 0; i < variableResolveList.size; i++) {

                ref PropertyContainer variableToResolve = ref variableResolveList[i];

                int nearestDistance = int.MaxValue;
                int targetIndex = -1;
                Int32 resolved = default;

                for (int v = 0; v < variableList.size; v++) {

                    ref ValueVariable variable = ref variableList.Get(v);
                    ref TraversalInfo traversalInfo = ref traversalInfoTable.Get(variableToResolve.elementId.index);

                    int depthDiff = traversalInfo.depth - variable.traversalInfo.depth;

                    if (depthDiff > nearestDistance || !traversalInfo.IsSelfOrDescendentOf(variable.traversalInfo) || !variable.TryConvertToInt32(out resolved)) {
                        continue;
                    }

                    nearestDistance = depthDiff;
                    targetIndex = v;

                }

                valueMap.Add(variableToResolve.elementId, targetIndex == -1
                    ? variableToResolve.Get<Int32>()
                    : resolved
                );

            }
        }

        public void Initialize() {
            valueMap = new UnsafeHashMap<ElementId, Int32>(31, Allocator.Persistent);
            sharedProperties = new DataList<PropertyUpdate>(16, Allocator.Persistent);
            transitions = new DataList<Transition<Int32>>(8, Allocator.Persistent);
        }

        public void Dispose() {
            valueMap.Dispose();
            sharedProperties.Dispose();
            transitions.Dispose();
        }

    }

}