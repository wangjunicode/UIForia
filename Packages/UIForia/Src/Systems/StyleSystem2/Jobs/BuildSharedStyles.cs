using UIForia.Style;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace UIForia {

    public unsafe struct BuildSharedStyles : IJobParallelForBatch {

        // Inputs
        public UnmanagedList<RebuildInfo> rebuildTable;
        public UnmanagedList<SharedStyleChange> changedSharedStyles;
        public UnmanagedPagedList<VertigoStylePropertyInfo> styleTable;
        
        public PagedSplitBufferList<PropertyId, long>.PerThread perThreadStyleOutput;
        public PagedSplitBufferList<PropertyId, long> stylePropertyTable;

        [NativeSetThreadIndex] public int threadIndex;

        // todo -- profile doing all gathers then all property lookups, cache locality should be better at the expense of memory usage
        
        public void Execute(int startIndex, int count) {
            // this job will gather all the properties for each updated shared style set

            PagedSplitBufferList<PropertyId, long> propertyOutputList = perThreadStyleOutput.GetListForThread(threadIndex);

            UnmanagedList<RangeInt> ranges = new UnmanagedList<RangeInt>(7 * 4, Allocator.TempJob);
            UnmanagedList<PropertyId> idBuffer = new UnmanagedList<PropertyId>(256, Allocator.TempJob); // todo - use real style count
            UnmanagedList<long> dataBuffer = new UnmanagedList<long>(256, Allocator.TempJob);
            
            VertigoStylePropertyInfo* buffer = stackalloc VertigoStylePropertyInfo[7]; // max shared style count is currently 7 (without crunching)

            int endIndex = startIndex + count;

            for (int i = startIndex; i < endIndex; i++) {

                BitBuffer256 bitBuffer = new BitBuffer256();
                IntBoolMap map = new IntBoolMap(bitBuffer.ptr, bitBuffer.bitCount);

                ranges.size = 0;
                
                SharedStyleChange current = changedSharedStyles[startIndex + i];

                // do all the de-referencing up front and store results in buffer
                for (int j = 0; j < current.count; j++) {
                    buffer[j] = styleTable[current.sharedStyles[j]];
                }

                if ((current.state & StyleState2.Active) != 0) {
                    for (int j = 0; j < current.count; j++) {
                        ref VertigoStylePropertyInfo styleInfo = ref buffer[j];
                        if (styleInfo.activeCount > 0) {
                            ranges.Add(new RangeInt(styleInfo.propertyOffset, styleInfo.activeCount));
                        }
                    }
                }

                if ((current.state & StyleState2.Focused) != 0) {
                    for (int j = 0; j < current.count; j++) {
                        ref VertigoStylePropertyInfo styleInfo = ref buffer[j];
                        if (styleInfo.focusCount > 0) {
                            ranges.Add(new RangeInt(styleInfo.propertyOffset + styleInfo.activeCount, styleInfo.focusCount));
                        }
                    }
                }

                if ((current.state & StyleState2.Hover) != 0) {
                    for (int j = 0; j < current.count; j++) {
                        ref VertigoStylePropertyInfo styleInfo = ref buffer[j];
                        if (styleInfo.hoverCount > 0) {
                            ranges.Add(new RangeInt(styleInfo.propertyOffset + styleInfo.activeCount + styleInfo.focusCount, styleInfo.hoverCount));
                        }
                    }
                }

                for (int j = 0; j < current.count; j++) {
                    ref VertigoStylePropertyInfo styleInfo = ref buffer[j];
                    if (styleInfo.normalCount > 0) {
                        ranges.Add(new RangeInt(styleInfo.propertyOffset + styleInfo.activeCount + styleInfo.focusCount + styleInfo.hoverCount, styleInfo.normalCount));
                    }
                }

                

                // once all ranges are gathered we walk through them in priority order (active -> focus -> hover -> normal) and apply styles if they haven't been set before
                
                int idx = 0;
                
                for (int j = 0; j < ranges.size; j++) {

                    RangeInt range = ranges[j];

                    stylePropertyTable.GetPointers(range.start, out PropertyId* keyPtr, out long* dataPtr);

                    for (int k = 0; k < range.length; k++) {
                        if (map.TrySetIndex(keyPtr[k].index)) {
                            idBuffer.array[idx] = keyPtr[k];
                            dataBuffer[idx] = dataPtr[k];
                            idx++;
                        }
                    }

                }

                // fill the rebuild table with the data we just gathered. then write the output to the thread's property buffer.
                // this will be copied to a permanent location later, before the frame ends. This happens so we can keep the data
                // as local as we can for cache coherence. 
                
                ref SharedStyleRebuildInfo rebuildInfo = ref rebuildTable.array[current.styleSetId].sharedStyles;
                rebuildInfo.splitBufferBase = propertyOutputList.GetRawPointer();
                rebuildInfo.location = propertyOutputList.AddRange(idBuffer.array, dataBuffer.array, idx);;
            }

            ranges.Dispose();
            idBuffer.Dispose();
            dataBuffer.Dispose();

        }

    }

}