using System;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace UIForia.Style {

    [BurstCompile(DisableSafetyChecks = UIApplication.DisableJobSafety)]
    internal unsafe struct SolveSharedStyleRebuild : IJob {

        [NativeDisableUnsafePtrRestriction] public DataList<StyleBlockChanges>* blockChanges;
        [NativeDisableUnsafePtrRestriction] public DataList<AnimationCommand>* animationCommands;

        public DataList<StyleBlock> styleBlocks;
        public DataList<StateHook> stateHooks;
        public DataList<StyleUsage>.Shared styleUsages;
        public DataList<StyleUsageQueryResult>.Shared styleUsageQueryResults;

        public DataList<BitSet> blockQueryRequirements; // from style database 

        public HeapAllocated<int> executionTokenGenerator;

        public ElementMap rebuildStylesMap;
        public ElementMap recomputeBlocksMap; // this is the output

        public void Execute() {
            Run(0, styleUsages.size);
        }

        private void Run(int start, int end) {

            // store results in a temp element map so we don't worry about threading contention
            ulong* buffer = TypedUnsafe.MallocCleared<ulong>(recomputeBlocksMap.longCount, Allocator.Temp);

            ElementMap tempRebuildMap = new ElementMap(buffer, recomputeBlocksMap.longCount);
            
            blockChanges->size = 0;
            
            for (int i = start; i < end; i++) {

                ElementId elementId = styleUsages[i].elementId;
                ref StyleUsageQueryResult styleUsageResult = ref styleUsageQueryResults[i];

                bool explicitRebuild = rebuildStylesMap.Get(elementId);

                if (!explicitRebuild && styleUsageResult.currResults == styleUsageResult.prevResults) {
                    continue;
                }

                BitSet styleQueryResults = styleUsageResult.currResults;
                BitSet currentBlocks = styleUsageResult.appliedBlocks;
                int blockOffset = styleUsageResult.styleId.blockOffset;
                int blockCount = styleUsageResult.styleId.blockCount;

                BitSet appliedBlocks = default;

                for (int b = 0; b < blockCount; b++) {
                    if (styleQueryResults.ContainsAll(blockQueryRequirements[blockOffset + b])) {
                        appliedBlocks.Set(b);
                    }
                }

                if (explicitRebuild) {
                    // explicit rebuild = style list changed | init this frame
                    // if init this frame do we handle differently?

                    //  Debug.Log("elementId: " + elementId);
                    // Debug.Log("init blocks: " + appliedBlocks.DebugDisplayList());

                    blockChanges->Add(new StyleBlockChanges() {
                        currentBlocks = currentBlocks.value,
                        prevBlocks = appliedBlocks.value,
                        elementId = elementId,
                        styleId = styleUsageResult.styleId
                    });

                    styleUsageResult.appliedBlocks = appliedBlocks;
                    tempRebuildMap.Set(elementId);
                }
                else if (appliedBlocks != currentBlocks) {

                    // ulong addedBlocks = appliedBlocks.value & ~currentBlocks.value;
                    // ulong removedBlocks = currentBlocks.value & ~appliedBlocks.value;

                    blockChanges->Add(new StyleBlockChanges() {
                        currentBlocks = currentBlocks.value,
                        prevBlocks = appliedBlocks.value,
                        elementId = elementId,
                        styleId = styleUsageResult.styleId
                    });
                    //
                    // Debug.Log("elementId: " + elementId);
                    // Debug.Log("blocks: " + appliedBlocks.DebugDisplayList());
                    // Debug.Log("added blocks: " + new BitSet(addedBlocks).DebugDisplayList());
                    // Debug.Log("removed blocks: " + new BitSet(removedBlocks).DebugDisplayList());

                    styleUsageResult.appliedBlocks = appliedBlocks;
                    tempRebuildMap.Set(elementId);
                }

            }

            recomputeBlocksMap.CombineThreadSafe(tempRebuildMap);
            TypedUnsafe.Dispose(buffer, Allocator.Temp);

            HandleStateChanges();

        }

        public void HandleStateChanges() {
            
            for (int i = 0; i < blockChanges->size; i++) {
                StyleBlockChanges change = blockChanges->array[i];
                ulong addedBlocks = change.prevBlocks & ~change.currentBlocks;
                ulong removedBlocks = change.currentBlocks & ~change.prevBlocks;
                ulong keptBlocks = change.currentBlocks & change.prevBlocks;
                ulong changedBlocks = addedBlocks | removedBlocks;

                long value = (long) changedBlocks;
                
                // added blocks &= style.enterBlocks;
                // removedBlocks blocks &= style.exitBlocks;

                StyleBlock* blocks = styleBlocks.array + change.styleId.blockOffset;

                while (value != 0) {
                    // x & -x returns an integer with only the lsb of the value set to 1
                    long t = value & -value;
                    int tzcnt = math.tzcnt((ulong) value); // counts the number of trailing 0s to lsb
                    value ^= t; // toggle the target bit off

                    // todo -- bit stuff likely wrong here

                    HookEvent targetEvent = (((ulong) 1 << tzcnt & addedBlocks) != 0) ? HookEvent.Enter : HookEvent.Exit;

                    RangeInt hookrange = new RangeInt(blocks[tzcnt].stateHookStart, blocks[tzcnt].stateHookCount);

                    for (int h = hookrange.start; h < hookrange.end; h++) {
                        ref StateHook hook = ref stateHooks[h];
                        if (hook.hookEvent != targetEvent) {
                            continue;
                        }

                        switch (hook.hookType) {

                            case HookType.PlayAnimation: {
                                int token = executionTokenGenerator.Get();
                                executionTokenGenerator.Set(token + 1);
                                animationCommands->Add(new AnimationCommand() {
                                    elementId = change.elementId,
                                    type = AnimationCommandType.Play,
                                    animationReference = hook.animationReference,
                                    executionToken = token,
                                });
                                break;
                            }

                            case HookType.StopAnimation:
                                break;

                            case HookType.ResetAnimation:
                                break;

                            case HookType.PauseAnimation:
                                break;

                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }

            }
        }

    }

    [Flags]
    internal enum HookEvent : byte {

        Enter = 1 << 0,
        Exit = 1 << 1,
        Update = 1 << 2

    }

    [Flags]
    internal enum HookType : byte {

        PlayAnimation,
        StopAnimation,
        ResetAnimation,
        PauseAnimation,

    }

    internal struct StateHook {

        public HookEvent hookEvent;
        public HookType hookType;
        public long data;
        public AnimationReference animationReference;

    }

    internal struct StyleBlockChanges {

        public ElementId elementId;
        public StyleId styleId;
        public ulong currentBlocks;
        public ulong prevBlocks;

    }

}