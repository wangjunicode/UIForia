#define MULTIPLE_LOOPS
#define SORT_FOR_LOCALITY

using Unity.Mathematics;
// ReSharper disable once RedundantUsingDirective
using System;
// ReSharper disable once RedundantUsingDirective
using System.Diagnostics; // required!
// ReSharper disable once RedundantUsingDirective
using UIForia;
using UIForia.ListTypes;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
// ReSharper disable once RedundantUsingDirective
using Unity.Collections.LowLevel.Unsafe; // required!

namespace UIForia.Style {

    internal struct UIMeasurementWrite {

        public ElementId elementId;
        public UIMeasurement value;

    }

    internal unsafe struct PropertySolverInfo_UIMeasurement {

        public int enumTypeId;
        public bool isImplicitInherit;

        public PendingTransition* pendingTransitionList;
        public ActiveTransition_UIMeasurement* activeTransitionList;
        public PropertyUpdate* sharedPropertyList;

        public int activeTransitionCount;
        public int activeTransitionCapacity;

        public int pendingTransitionCount;
        public int pendingTransitionCapacity;

        public int sharedPropertyCapacity;
        public int sharedPropertyCount;
        public DataList<PropertySolver_UIMeasurement.AnimationValue_UIMeasurement> animationValues;

    }

    internal static unsafe class PropertySolver_UIMeasurement {

        public static void Invoke(ref PropertySolverInfo_UIMeasurement solverInfo, in SolverParameters parameters, ref PropertySolverContext context, DataList<UIMeasurement> styleDatabase, UIMeasurement* outputTargets, int solverIndex, PropertyId propertyId, BumpAllocator* tempAllocator, bool contextual) {

            tempAllocator->Clear();
            context.variableResolveList.size = 0;

            int defaultIndex = parameters.defaultValueIndices[(int) propertyId];
            ref CheckedArray<PropertyUpdate> sharedUpdateList = ref parameters.sharedRebuildResult->updateLists[(int) propertyId];
            ref TransitionUpdateList transitionUpdateList = ref parameters.transitionUpdateResult->updateLists[(int) propertyId];
            ref InstancePropertyUpdateList instanceUpdateList = ref parameters.instanceRebuildResult->updateLists[(int) propertyId];

            int longsPerElementMap = parameters.longsPerElementMap;

            // these two maps are held across frames but we store the data for them in a single allocation managed by the context
            // this init code grabs the section of the big buffer that is relevant for this solver
            ElementMap inheritMap = new ElementMap(context.inheritMap + (longsPerElementMap * solverIndex), longsPerElementMap);
            ElementMap previousDefinitionMap = new ElementMap(context.definitionMap + (longsPerElementMap * solverIndex), longsPerElementMap);

            // these per frame maps are re-computed every time we run
            ulong* mapBuffer = tempAllocator->AllocateCleared<ulong>(longsPerElementMap * 4);
            ElementMap definitionMap = new ElementMap(mapBuffer + (longsPerElementMap * 0), longsPerElementMap);
            ElementMap changeMap = new ElementMap(mapBuffer + (longsPerElementMap * 1), longsPerElementMap);
            ElementMap animationMap = new ElementMap(mapBuffer + (longsPerElementMap * 2), longsPerElementMap); // not used yet
            ElementMap scratchMap = new ElementMap(mapBuffer + (longsPerElementMap * 3), longsPerElementMap); // used for various map operations so we don't allocate a bunch of maps to use just once

            if (contextual) {
                // in order for the `current` feature and other change checks to work, 
                // we need to restore the previously set animation values to their non animated values before proceeding 
                for (int i = 0; i < solverInfo.animationValues.size; i++) {
                    ref AnimationValue_UIMeasurement value = ref solverInfo.animationValues[i];
                    outputTargets[value.elementId.index] = value.currentValue;
                }

            }

            BumpList<ActiveTransition_UIMeasurement> purgatoryTransitionList = SetupFrameData(
                ref solverInfo,
                parameters,
                tempAllocator,
                scratchMap,
                inheritMap,
                previousDefinitionMap
            );

            // any properties that were rebuilt can be safely dumped into this list now without worry about duplicates because we already removed them 
            context.listAllocator->AddToAllocList(
                ref solverInfo.sharedPropertyList,
                ref solverInfo.sharedPropertyCount,
                ref solverInfo.sharedPropertyCapacity,
                sharedUpdateList.array,
                sharedUpdateList.size
            );

            // any transitions that were rebuilt can be safely dumped into this list now without worry about duplicates because we already removed them 
            context.listAllocator->AddToAllocList(
                ref solverInfo.pendingTransitionList,
                ref solverInfo.pendingTransitionCount,
                ref solverInfo.pendingTransitionCapacity,
                transitionUpdateList.array,
                transitionUpdateList.size
            );

            // pessimistically get a write buffer that can hold all the data we need for updates this frame 
            int maxWriteBufferSize = solverInfo.sharedPropertyCount + instanceUpdateList.size + solverInfo.activeTransitionCount + solverInfo.pendingTransitionCount;
            BumpList<UIMeasurementWrite> writeBuffer = tempAllocator->AllocateList<UIMeasurementWrite>(maxWriteBufferSize);
            CheckedArray<ValueVariable> variables = SolverParameters.GetVariableList_ValueVariable(parameters);

            // write all the transition values to output. this lets other phases treat the buffer values as their base line for change checking 
            // otherwise we'd have to do a bunch of searching through the active transition list to find the right values. 
            scratchMap.Clear();
            for (int i = 0; i < solverInfo.activeTransitionCount; i++) {
                ref ActiveTransition_UIMeasurement transition = ref solverInfo.activeTransitionList[i];
                outputTargets[transition.elementId.index] = transition.next;
                scratchMap.Set(transition.elementId);
            }

            // instance properties might refer to variables which we resolve in a deferred manner for perf reasons
            // we need to resolve the instance variables here separately from the shared style variables because of how we handle transitions
            UpdateInstanceProperties(ref context, outputTargets, instanceUpdateList, definitionMap, inheritMap, changeMap, ref writeBuffer);
            ResolveVariables(solverInfo.enumTypeId, parameters.traversalTable, ref context.variableResolveList, variables, ref writeBuffer, changeMap, outputTargets);

            // active transitions & change map - animation = instance changed and we need to reset the corresponding transition
            // scratch map still contains the active transitions set from above 
            for (int i = 0; i < longsPerElementMap; i++) {
                scratchMap.map[i] = scratchMap.map[i] & changeMap.map[i] & ~animationMap.map[i];
            }

            UpdateActiveTransitions(solverInfo, parameters, animationMap, scratchMap, ref writeBuffer);

            // todo -- handle transition cancels and maybe events 

            // the shared property list contains the set of all active elements that have a style that defines this property. There is exactly one entry
            // in the sharedPropertyList per active element. Every frame we remove the inactive elements from the list in SetupFrame(). 
            HandlePropertyUpdates(
                solverInfo.sharedPropertyList,
                solverInfo.sharedPropertyCount,
                styleDatabase,
                outputTargets,
                definitionMap,
                inheritMap,
                changeMap,
                ref context.variableResolveList,
                tempAllocator,
                ref writeBuffer
            );

            ResolveVariables(solverInfo.enumTypeId, parameters.traversalTable, ref context.variableResolveList, variables, ref writeBuffer, changeMap, outputTargets);

            // any element where the property was unset needs to get its default value written into the output buffer 
            WriteDefaultValues(solverInfo, parameters, styleDatabase[defaultIndex], outputTargets, tempAllocator, previousDefinitionMap, definitionMap, changeMap);

            scratchMap.Clear();

            // pump our pending transitions into the scratch map
            for (int i = 0; i < solverInfo.pendingTransitionCount; i++) {
                scratchMap.Set(solverInfo.pendingTransitionList[i].elementId);
            }

            // toActivateMap = changes with pending transitions that were not initialized this frame
            for (int i = 0; i < longsPerElementMap; i++) {
                scratchMap.map[i] = changeMap.map[i] & scratchMap.map[i] & ~parameters.initMap.map[i];
            }

            BumpList<ElementId> changedTransitionIds = scratchMap.ToBumpList(tempAllocator);
            BumpList<UIMeasurement> transitionPrevValues = tempAllocator->AllocateList<UIMeasurement>(changedTransitionIds.size);
            transitionPrevValues.size = changedTransitionIds.size;

            // gather up all the previous values before flushing the write buffer. We will overwrite the write buffer result but that's fine
            for (int i = 0; i < changedTransitionIds.size; i++) {
                transitionPrevValues.array[i] = outputTargets[changedTransitionIds.array[i].index];
            }

            FlushWriteBuffer(parameters, outputTargets, writeBuffer);

            // copy new definition map for the next frame 
            TypedUnsafe.MemCpy(previousDefinitionMap.map, definitionMap.map, longsPerElementMap);

            RemoveExpiredTransitions(ref solverInfo, context);

            ref DataList<KeyFrameResult> keyFramesProperties = ref parameters.animationResultBuffer[propertyId];

            animationMap.Clear();

            // pending transition store prev value
            
            if (keyFramesProperties.size > 0) {

                // todo -- keyFramesResultValues & animationValues are mutually exclusive, maybe use a union / cast?

                if (contextual) {

                    ref DataList<InterpolatedStyleValue> keyFramesResultValues = ref parameters.animationValueBuffer[propertyId];
                    keyFramesResultValues.EnsureCapacity(keyFramesProperties.size);
                    keyFramesResultValues.size = keyFramesProperties.size;

                    for (int i = 0; i < keyFramesProperties.size; i++) {
                        KeyFrameResult keyFrameProp = keyFramesProperties[i];

                        UIMeasurement next = keyFrameProp.next == KeyFrameResult.k_Current ? outputTargets[keyFrameProp.elementId.index] : styleDatabase[keyFrameProp.next];
                        UIMeasurement prev = keyFrameProp.prev == KeyFrameResult.k_Current ? outputTargets[keyFrameProp.elementId.index] : styleDatabase[keyFrameProp.prev];

                        next = ResolveAnimationVariable(keyFrameProp.nextVarId, next, parameters.traversalTable, variables, keyFrameProp.elementId);
                        prev = ResolveAnimationVariable(keyFrameProp.prevVarId, prev, parameters.traversalTable, variables, keyFrameProp.elementId);

                        keyFramesResultValues[i] = InterpolatedStyleValue.FromUIMeasurement(next, prev, keyFrameProp.t);
                        animationMap.Set(keyFrameProp.elementId);

                    }
                }
                else {

                    solverInfo.animationValues.EnsureCapacity(keyFramesProperties.size, Allocator.Persistent);
                    solverInfo.animationValues.size = keyFramesProperties.size;

                    for (int i = 0; i < keyFramesProperties.size; i++) {
                        KeyFrameResult keyFrameProp = keyFramesProperties[i];

                        UIMeasurement next = keyFrameProp.next == KeyFrameResult.k_Current ? outputTargets[keyFrameProp.elementId.index] : styleDatabase[keyFrameProp.next];
                        UIMeasurement prev = keyFrameProp.prev == KeyFrameResult.k_Current ? outputTargets[keyFrameProp.elementId.index] : styleDatabase[keyFrameProp.prev];

                        next = ResolveAnimationVariable(keyFrameProp.nextVarId, next, parameters.traversalTable, variables, keyFrameProp.elementId);
                        prev = ResolveAnimationVariable(keyFrameProp.prevVarId, prev, parameters.traversalTable, variables, keyFrameProp.elementId);

                        UIMeasurement result = UIMeasurement_Interpolator.Interpolate(prev, next, keyFrameProp.t);

                        solverInfo.animationValues[i].elementId = keyFrameProp.elementId;
                        solverInfo.animationValues[i].currentValue = outputTargets[keyFrameProp.elementId.index];

                        outputTargets[keyFrameProp.elementId.index] = result;
                        animationMap.Set(keyFrameProp.elementId);

                    }
                }
            }

            if (changedTransitionIds.size != 0) {

                scratchMap.Clear();

                for (int i = 0; i < purgatoryTransitionList.size; i++) {
                    scratchMap.Set(purgatoryTransitionList.array[i].elementId);
                }

                // for each transition that we will promote from pending to active, remove from the pending list and
                // create an active transition for it or reset that transition if it was in purgatory
                if (contextual) {
                    
                    ref DataList<InterpolatedStyleValue> keyFramesResultValues = ref parameters.animationValueBuffer[propertyId];
                    
                    for (int i = 0; i < transitionPrevValues.size; i++) {

                        ElementId elementId = changedTransitionIds.array[i];
                      
                        PendingTransition pending = PropertySolver.FindAndRemovePending(elementId, solverInfo.pendingTransitionList, ref solverInfo.pendingTransitionCount);
                        
                        if (animationMap.Get(elementId)) {
                            continue;
                        }

                        ActiveTransition_UIMeasurement active = new ActiveTransition_UIMeasurement {
                            definition = parameters.transitionDatabase[pending.transitionId],
                            elementId = elementId,
                            elapsed = 0,
                            transitionId = pending.transitionId,
                            next = outputTargets[elementId.index],
                            prev = transitionPrevValues.array[i]
                        };

                        if (scratchMap.Get(elementId)) {
                            ActiveTransition_UIMeasurement prevTransition = FindInPurgatory(elementId, purgatoryTransitionList);
                            float progress = math.clamp(prevTransition.elapsed / (float) prevTransition.definition.duration, 0, 1);
                            active.prev = ActiveTransition_UIMeasurement.ResolveTransitionResetValue(solverInfo.enumTypeId, parameters, prevTransition, progress);
                        }

                        // transitions that begin this frame should put their previous value in the buffer because we transition
                        // to that value so we obviously need to start from the previous point 

                        keyFramesResultValues.Add(InterpolatedStyleValue.FromUIMeasurement(outputTargets[elementId.index], transitionPrevValues.array[i], 0));
                        
                        context.listAllocator->AddToAllocList(
                            ref solverInfo.activeTransitionList,
                            ref solverInfo.activeTransitionCount,
                            ref solverInfo.activeTransitionCapacity,
                            active
                        );

                    }
                }
                else {
                    for (int i = 0; i < transitionPrevValues.size; i++) {

                        ElementId elementId = changedTransitionIds.array[i];
                        
                        if (animationMap.Get(elementId)) continue;

                        PendingTransition pending = PropertySolver.FindAndRemovePending(elementId, solverInfo.pendingTransitionList, ref solverInfo.pendingTransitionCount);

                        ActiveTransition_UIMeasurement active = new ActiveTransition_UIMeasurement {
                            definition = parameters.transitionDatabase[pending.transitionId],
                            elementId = elementId,
                            elapsed = 0,
                            transitionId = pending.transitionId,
                            next = outputTargets[elementId.index],
                            prev = transitionPrevValues.array[i]
                        };

                        if (scratchMap.Get(elementId)) {
                            ActiveTransition_UIMeasurement prevTransition = FindInPurgatory(elementId, purgatoryTransitionList);
                            float progress = math.clamp(prevTransition.elapsed / (float) prevTransition.definition.duration, 0, 1);
                            active.prev = ActiveTransition_UIMeasurement.ResolveTransitionResetValue(solverInfo.enumTypeId, parameters, prevTransition, progress);
                        }

                        // transitions that begin this frame should put their previous value in the buffer because we transition
                        // to that value so we obviously need to start from the previous point 
                        outputTargets[elementId.index] = active.prev;

                        context.listAllocator->AddToAllocList(
                            ref solverInfo.activeTransitionList,
                            ref solverInfo.activeTransitionCount,
                            ref solverInfo.activeTransitionCapacity,
                            active
                        );
                    }
                }
            }

            if (solverInfo.isImplicitInherit) {

                for (int i = 0; i < longsPerElementMap; i++) {
                    scratchMap.map[i] = ~(definitionMap.map[i] | animationMap.map[i]) & parameters.activeMap.map[i];
                }

                scratchMap.Unset(default);

                BumpList<ElementId> inheritList = scratchMap.ToBumpList(tempAllocator);

                NativeSortExtension.Sort(inheritList.array, inheritList.size, new InheritSorter() {
                    traversalInfo = parameters.traversalTable
                });

                // we need the root element (at index 0, the parent of all elements) to be set to the default value 

                outputTargets[0] = styleDatabase[defaultIndex];

                // order matters, must do parent before children 
                for (int i = 0; i < inheritList.size; i++) {
                    ElementId elementId = inheritList.array[i];
                    ElementId parentId = parameters.elementIdToParentId[elementId.index];

                    // this is ok because we cannot have an inherited and contextual property 
                    outputTargets[elementId.index] = outputTargets[parentId.index];
                    // todo -- maybe check and see if we need to do this write or at least update the changemap 
                }

            }

        }

        public struct AnimationValue_UIMeasurement {

            public ElementId elementId;
            public UIMeasurement currentValue;

        }

        private static void WriteDefaultValues(in PropertySolverInfo_UIMeasurement solverInfo, in SolverParameters parameters, UIMeasurement defaultValue, UIMeasurement* outputTargets, BumpAllocator* tempAllocator, ElementMap previousDefinitionMap, ElementMap definitionMap, ElementMap changeMap) {

            if (solverInfo.isImplicitInherit) {
                // todo handle implicit inherits
                return;
            }

            BumpList<ElementId> defaults = PropertySolver.ComputeDefaults(previousDefinitionMap, definitionMap, parameters, tempAllocator);

            for (int i = 0; i < defaults.size; i++) {
                TypedUnsafe.CheckRange(defaults.array[i].index, parameters.maxElementId);
                UIMeasurement* dest = outputTargets + defaults.array[i].index;
                if (!dest->Equals(defaultValue)) {
                    changeMap.Set(defaults.array[i]);
                    *dest = defaultValue;
                }
            }
        }

        private static void FlushWriteBuffer(in SolverParameters parameters, UIMeasurement* outputTargets, BumpList<UIMeasurementWrite> writeBuffer) {

            // maybe sort the write buffer by elementId for better write locality?

            for (int i = 0; i < writeBuffer.size; i++) {
                ref UIMeasurementWrite entry = ref writeBuffer.array[i];
                TypedUnsafe.CheckRange(entry.elementId.index, parameters.maxElementId);
                // HUGE NOTE!!! this assumes our output for style values is BY ELEMENT ID. I think we want this to be by flattened index later on when we do layout 
                outputTargets[entry.elementId.index] = writeBuffer.array[i].value;
            }
        }

        private static void RemoveExpiredTransitions(ref PropertySolverInfo_UIMeasurement solverInfo, in PropertySolverContext context) {
            for (int i = 0; i < solverInfo.activeTransitionCount; i++) {
                ref ActiveTransition_UIMeasurement transition = ref solverInfo.activeTransitionList[i];

                if (transition.elapsed < transition.definition.duration) {
                    continue;
                }

                solverInfo.activeTransitionCount--;
                solverInfo.activeTransitionList[i--] = solverInfo.activeTransitionList[solverInfo.activeTransitionCount];

                context.listAllocator->AddToAllocList(
                    ref solverInfo.pendingTransitionList,
                    ref solverInfo.pendingTransitionCount,
                    ref solverInfo.pendingTransitionCapacity,
                    new PendingTransition() {
                        elementId = transition.elementId,
                        transitionId = transition.transitionId
                    }
                );

            }
        }

        private static BumpList<ActiveTransition_UIMeasurement> SetupFrameData(ref PropertySolverInfo_UIMeasurement solverInfo, in SolverParameters parameters, BumpAllocator* tempAllocator, ElementMap scratch, ElementMap inheritMap, ElementMap previousDefinitionMap) {
            int longsPerMap = parameters.longsPerElementMap;

            // todo -- don't re-construct this for all properties, share it across all properties and host it on the context 
            scratch.Copy(parameters.invalidatedElementMap);
            scratch.Combine(parameters.rebuildBlocksMap);

            ElementMap invalidatedElementMap = parameters.invalidatedElementMap;

            for (int i = 0; i < longsPerMap; i++) {
                // i think we only want to remove dead / disabled / newly initialized elements from prev definition map, not everything
                previousDefinitionMap.map[i] &= ~invalidatedElementMap.map[i];
                inheritMap.map[i] &= ~invalidatedElementMap.map[i];
            }

            for (int i = 0; i < solverInfo.activeTransitionCount; i++) {
                if (invalidatedElementMap.Get(solverInfo.activeTransitionList[i].elementId)) {
                    solverInfo.activeTransitionList[i--] = solverInfo.activeTransitionList[--solverInfo.activeTransitionCount];
                }
            }

            // We might want to do this in a different job since we can solve it earlier
            // todo -- could extract the removal of invalidated data into a different job since we know whats invalidated before we know what properties to add
            // todo    if that happens we need to persist the definition maps I think
            // clear out dead data from last frame

            // for each pending transition we have, figure out if the element the transition was set for is either dead, disabled, or is going to be rebuilt
            for (int i = 0; i < solverInfo.pendingTransitionCount; i++) {
                PendingTransition pending = solverInfo.pendingTransitionList[i];
                if (scratch.Get(pending.elementId)) {
                    solverInfo.pendingTransitionList[i--] = solverInfo.pendingTransitionList[--solverInfo.pendingTransitionCount];
                }
            }

            // for each property we got from the shared property list, if element was disabled/ destroyed or is getting rebuilt this frame, swap remove 
            for (int i = 0; i < solverInfo.sharedPropertyCount; i++) {
                // maybe only if invalidatedMap & hasUpdateMap != 0? 
                if (scratch.Get(solverInfo.sharedPropertyList[i].elementId)) {
                    solverInfo.sharedPropertyList[i--] = solverInfo.sharedPropertyList[--solverInfo.sharedPropertyCount];
                }
            }

            // we need to know which transitions are currently running but also going to be rebuilt. Get a list to hold all active transitions 
            BumpList<ActiveTransition_UIMeasurement> purgatoryTransitionList = tempAllocator->AllocateList<ActiveTransition_UIMeasurement>(solverInfo.activeTransitionCount);

            // for each active transition, if destroyed or rebuilt add that transition to 'purgatory' and remove it from the active list
            // we don't yet know if we will remove the transition or if it will be reset. No need to enter a pending entry since our style rebuild will do that 
            for (int i = 0; i < solverInfo.activeTransitionCount; i++) {
                if (scratch.Get(solverInfo.activeTransitionList[i].elementId)) {
                    purgatoryTransitionList.array[purgatoryTransitionList.size++] = solverInfo.activeTransitionList[i];
                    solverInfo.activeTransitionCount--;
                    solverInfo.activeTransitionList[i--] = solverInfo.activeTransitionList[solverInfo.activeTransitionCount];
                }
            }

            return purgatoryTransitionList;
        }

        private static void UpdateInstanceProperties(ref PropertySolverContext context, UIMeasurement* outputTargets, InstancePropertyUpdateList instanceUpdateList, ElementMap definitionMap, ElementMap activeTransitionMap, ElementMap changeMap, ref BumpList<UIMeasurementWrite> writeBuffer) {
            for (int i = 0; i < instanceUpdateList.size; i++) {

                ref PropertyContainer update = ref instanceUpdateList.array[i];

                if (!definitionMap.TrySet(update.elementId)) {
                    continue;
                }

                if (update.variableNameId != ushort.MaxValue) {
                    context.variableResolveList.Add(update);
                    continue;
                }

                UIMeasurement value = update.Get<UIMeasurement>();

                if (value.Equals(outputTargets[update.elementId.index])) {
                    continue;
                }

                changeMap.Set(update.elementId);

                writeBuffer.array[writeBuffer.size++] = new UIMeasurementWrite() {
                    elementId = update.elementId,
                    value = value
                };

            }

        }

        private static void UpdateActiveTransitions(in PropertySolverInfo_UIMeasurement solverInfo, in SolverParameters parameters, ElementMap animationMap, ElementMap resetTransitionMap, ref BumpList<UIMeasurementWrite> writeBuffer) {
            // we didn't know if instance styles were going to change before. If an instance changed and we had an actively running transition for that element
            // then we will need to reset the transition with new values.
            for (int i = 0; i < solverInfo.activeTransitionCount; i++) {
                ref ActiveTransition_UIMeasurement transition = ref solverInfo.activeTransitionList[i];
                if (resetTransitionMap.Get(transition.elementId)) {
                    float progress = math.clamp(transition.elapsed / (float) transition.definition.duration, 0, 1);
                    transition.prev = ActiveTransition_UIMeasurement.ResolveTransitionResetValue(solverInfo.enumTypeId, parameters, transition, progress);
                    transition.next = FindInWriteBuffer(transition.elementId, writeBuffer);
                    transition.elapsed = -parameters.deltaMS; // negative because we'll increment it shortly and we want the result to be 0 since this is our first frame
                }
            }

            // for each transition that is actively running, tick its elapsed time
            // if nothing has set the property for the corresponding element yet, update the transition value and add it to the write buffer

            // todo -- these should be rolled into animations and treated as a slightly special case of those 
            for (int i = 0; i < solverInfo.activeTransitionCount; i++) {
                ref ActiveTransition_UIMeasurement transition = ref solverInfo.activeTransitionList[i];
                transition.elapsed += parameters.deltaMS;

                // if an animation ran, do nothing 
                if (!animationMap.TrySet(transition.elementId)) {
                    continue;
                }

                // todo -- delay

                float progress = math.clamp(transition.elapsed / (float) transition.definition.duration, 0, 1);

                writeBuffer.array[writeBuffer.size++] = new UIMeasurementWrite() {
                    elementId = transition.elementId,
                    value = ActiveTransition_UIMeasurement.Interpolate(solverInfo.enumTypeId, parameters, transition, progress)
                };

            }

        }

        // value is 100% in the buffer 
        private static UIMeasurement FindInWriteBuffer(ElementId elementId, BumpList<UIMeasurementWrite> list) {
            int idx = 0;
            while (list.array[idx].elementId != elementId) {
                idx++;
            }

            return list.array[idx].value;
        }

        // value is 100% in the buffer 
        private static ActiveTransition_UIMeasurement FindInPurgatory(ElementId elementId, BumpList<ActiveTransition_UIMeasurement> list) {

            int idx = 0;
            while (list.array[idx].elementId != elementId) {
                idx++;
            }

            return list.array[idx];
        }

        private static UIMeasurement ResolveAnimationVariable(ushort variableNameId, UIMeasurement defaultValue, CheckedArray<TraversalInfo> traversalInfo, CheckedArray<ValueVariable> variables, ElementId elementId) {

            if (variableNameId == ushort.MaxValue) return defaultValue;

            TraversalInfo elementTraversalInfo = traversalInfo[elementId.index];
            int start = -1;

            for (int i = 0; i < variables.size; i++) {
                ValueVariable variable = variables[i];
                if (variable.variableNameId == variableNameId) {
                    start = i;
                    break;
                }
            }

            if (start == -1) {
                return defaultValue;
            }

            int end = start;

            for (int i = start; i < variables.size; i++) {
                ValueVariable variable = variables[i];
                end++;
                if (variable.variableNameId != variableNameId) {
                    break;
                }
            }

            UIMeasurement value = default;
            int nearestDistance = int.MaxValue;
            int targetIndex = -1;

            for (int i = start; i < end; i++) {

                ValueVariable variable = variables[i];
                TraversalInfo variableTraversal = variable.traversalInfo; // maybe pump variable traversals into their own array and share that 

                if (!elementTraversalInfo.IsSelfOrDescendentOf(variableTraversal)) {
                    continue;
                }

                // todo -- depending on the scope of the variable we may also need to ensure the templates are the same 

                int depthDiff = elementTraversalInfo.depth - variableTraversal.depth;

                if (depthDiff > nearestDistance || !variable.TryConvertToUIMeasurement(out UIMeasurement resolved)) {
                    continue;
                }

                value = resolved;
                nearestDistance = depthDiff;
                targetIndex = i;
            }

            if (targetIndex == -1) {
                // write the default value 
                return defaultValue;
            }

            return value;
        }

        private static void ResolveVariables(int enumTypeId, CheckedArray<TraversalInfo> traversalInfo, ref DataList<PropertyContainer> propertyList, CheckedArray<ValueVariable> variables, ref BumpList<UIMeasurementWrite> writeBuffer, ElementMap changeMap, UIMeasurement* outputTargets) {
            // search parents in scope (global, same template) until found variable definition or nothing left to search
            if (propertyList.size == 0) {
                return;
            }

            // todo -- sort updates by variable id, might be faster to avoid variable search, but only if we have a lot of variables to scan 

            for (int updateIdx = 0; updateIdx < propertyList.size; updateIdx++) {

                ref PropertyContainer update = ref propertyList[updateIdx];

                TraversalInfo elementTraversalInfo = traversalInfo[update.elementId.index];

                int start = -1;

                for (int i = 0; i < variables.size; i++) {
                    ValueVariable variable = variables[i];
                    if (variable.variableNameId == update.variableNameId) {
                        start = i;
                        break;
                    }
                }

                if (start == -1) {
                    // write the default value
                    writeBuffer.array[writeBuffer.size++] = new UIMeasurementWrite() {
                        elementId = update.elementId,
                        value = update.Get<UIMeasurement>(),
                    };

                    continue;
                }

                int end = start;

                for (int i = start; i < variables.size; i++) {
                    ValueVariable variable = variables[i];
                    end++;
                    if (variable.variableNameId != update.variableNameId) {
                        break;
                    }
                }

                UIMeasurement value = default;
                int nearestDistance = int.MaxValue;
                int targetIndex = -1;

                for (int i = start; i < end; i++) {

                    ValueVariable variable = variables[i];
                    TraversalInfo variableTraversal = variable.traversalInfo; // maybe pump variable traversals into their own array and share that 

                    if (variable.enumTypeId != enumTypeId || !elementTraversalInfo.IsSelfOrDescendentOf(variableTraversal)) {
                        continue;
                    }

                    // todo -- depending on the scope of the variable we may also need to ensure the templates are the same 

                    int depthDiff = elementTraversalInfo.depth - variableTraversal.depth;

                    if (depthDiff > nearestDistance || !variable.TryConvertToUIMeasurement(out UIMeasurement resolved)) {
                        continue;
                    }

                    value = resolved;
                    nearestDistance = depthDiff;
                    targetIndex = i;
                }

                if (targetIndex == -1) {
                    // write the default value 

                    if (outputTargets[update.elementId.index].Equals(value)) {
                        continue;
                    }

                    changeMap.Set(update.elementId);

                    writeBuffer.array[writeBuffer.size++] = new UIMeasurementWrite() {
                        elementId = update.elementId,
                        value = update.Get<UIMeasurement>(),
                    };
                }
                else {

                    // inheritMap.Unset(update.elementId);

                    if (outputTargets[update.elementId.index].Equals(value)) {
                        continue;
                    }

                    changeMap.Set(update.elementId);

                    writeBuffer.array[writeBuffer.size++] = new UIMeasurementWrite() {
                        elementId = update.elementId,
                        value = value
                    };
                }

            }

            propertyList.size = 0;

        }

        /// <summary>
        /// This handles all the shared properties for all elements that define the current property.
        /// We run all these checks every frame because we have so many possible change sources that
        /// its easier to just do the work than try to figure out what changed where. 
        /// </summary>
        /// <param name="updateList">list of properties to process</param>
        /// <param name="updateCount">how many items to process</param>
        /// <param name="styleDatabase">the table that contains the actual property values</param>
        /// <param name="outputTargets">where to write the results to. This is a pointer to StyleTables.xxxx</param>
        /// <param name="definitionMap">map to check if some other animation or transition or instance property already defined this property for the element</param>
        /// <param name="inheritMap">map to check if property needs to inherit, might not be needed </param>
        /// <param name="changeMap">map to check if the property value is different from last frame</param>
        /// <param name="variableResolveList">list of variables we need to resolve</param>
        /// <param name="tempAllocator">allocator for bump lists</param>
        /// <param name="writeBuffer">an intermediate buffer to write our updates to</param>
        private static void HandlePropertyUpdates(
            PropertyUpdate* updateList,
            int updateCount,
            DataList<UIMeasurement> styleDatabase,
            UIMeasurement* outputTargets,
            ElementMap definitionMap,
            ElementMap inheritMap,
            ElementMap changeMap,
            ref DataList<PropertyContainer> variableResolveList,
            BumpAllocator* tempAllocator,
            ref BumpList<UIMeasurementWrite> writeBuffer) {

#if MULTIPLE_LOOPS
            // get a list of property updates that we'll actually need to write. Pessimistically allocated
            PropertyUpdate* usedUpdates = tempAllocator->Allocate<PropertyUpdate>(updateCount);
            int usedUpdateSize = 0;

            // go through all our properties and discard any that were previously defined by animations, instances styles, or transitions since those take priority
            for (int i = 0; i < updateCount; i++) {
                PropertyUpdate update = updateList[i];

                if (!definitionMap.TrySet(update.elementId)) {
                    continue;
                }

                usedUpdates[usedUpdateSize++] = update;

            }

            // for each property we are actually going to evaluate, check if that property is a variable
            // if it is, remove it from the list and push it into the list of variables that need resolving
            for (int i = 0; i < usedUpdateSize; i++) {

                if (!usedUpdates[i].IsVariable) {
                    continue;
                }

                UIMeasurement variableDefaultValue = styleDatabase[usedUpdates[i].dbValueLocation];

                PropertyContainer propertyContainer = new PropertyContainer() {
                    variableNameId = usedUpdates[i].variableNameId,
                    elementId = usedUpdates[i].elementId
                };

                propertyContainer.Set(variableDefaultValue);

                variableResolveList.Add(propertyContainer);
                usedUpdates[i--] = usedUpdates[--usedUpdateSize];
            }

#if SORT_FOR_LOCALITY
            // sort updates by elementId for locality 
            NativeSortExtension.Sort(usedUpdates, usedUpdateSize);
#endif

            UIMeasurement* values = tempAllocator->Allocate<UIMeasurement>(usedUpdateSize);
            UIMeasurement* dbValues = tempAllocator->Allocate<UIMeasurement>(usedUpdateSize);

            for (int i = 0; i < usedUpdateSize; i++) {
                values[i] = outputTargets[usedUpdates[i].elementId.index];
                dbValues[i] = styleDatabase[usedUpdates[i].dbValueLocation];
            }

            // for each element that actually gets processed, check if the value is different from last frame
            // if it is, mark it in the change map and write to the write buffer
            for (int i = 0; i < usedUpdateSize; i++) {

                PropertyUpdate update = usedUpdates[i];

                inheritMap.Unset(update.elementId);

                if (values[i].Equals(dbValues[i])) {
                    continue;
                }

                changeMap.Set(update.elementId);
                writeBuffer.array[writeBuffer.size++] = new UIMeasurementWrite() {
                    elementId = update.elementId,
                    value = dbValues[i]
                };
            }

#else
            for (int i = 0; i < updateCount; i++) {
                PropertyUpdate update = updateList[i];

                if (!definitionMap.TrySet(update.elementId)) {
                    continue;
                }

                if (update.IsVariable) {
                    variableResolveList.Add(update);
                    continue;
                }

                UIMeasurement databaseValue = styleDatabase[update.dbValueLocation];

                inheritMap.Unset(update.elementId); // todo -- how do I handle inherit? db value? or some other place?

                UIMeasurement* current = outputTargets + update.elementId.index;

                if (!databaseValue.Equals(*current)) {
                    changeMap.Set(update.elementId);
                    writeBuffer.array[writeBuffer.size++] = new UIMeasurementWrite() {
                        elementId = update.elementId,
                        value = databaseValue
                    };
                }

            }
#endif
        }

    }

}