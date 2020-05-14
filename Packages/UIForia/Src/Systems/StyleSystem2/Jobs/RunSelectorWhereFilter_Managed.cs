using System;
using System.Runtime.InteropServices;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace UIForia {

    public struct RunSelectorWhereFilter_Managed : IJob, IJobParallelForBatch {

        public GCHandle table_WhereFilterFuncs;
        public DataList<SelectorMatchCandidate>.Shared input_MatchCandidates;
        public PerThread<SelectorMatchList> output_MatchedElements;
        
        [NativeSetThreadIndex]
        public int threadIndex;

        public void Execute() {
            Run(0, input_MatchCandidates.size);
        }
        
        public void Execute(int start, int count) {
            Run(start, count);
        }

        private void Run(int start, int count) {
            int end = start + count;

            ref SelectorMatchList output = ref output_MatchedElements.GetForThread(threadIndex);
            
            LightList<Func<SelectorElementWrapper, SelectorFilterContext, bool>> table_SelectorWhereFilters = (LightList<Func<SelectorElementWrapper, SelectorFilterContext, bool>>) table_WhereFilterFuncs.Target;
                
            SelectorFilterContext context = new SelectorFilterContext() {
                table_SelectorWhereFilters = table_SelectorWhereFilters
            };
            
            
            // future state: run all similar filters on groups of elements
            // in production the filters should be bust compiled functions
            // give them arrays and let them do their thing
            
            for (int index = start; index < end; index++) {

                SelectorMatchCandidate candidate = input_MatchCandidates[index];

                // SelectorInfo info = table_SelectorInfo[candidate.selectorId];
                //
                // SelectorElementWrapper wrapper = new SelectorElementWrapper(candidate.targetElementId);
                //
                // if (table_SelectorWhereFilters[info.whereFilterId](wrapper, context)) {
                //     output.Add(new SelectorMatch() {
                //         selectorId = candidate.selectorId,
                //         sourceElementId = candidate.hostElementId,
                //         targetElementId = candidate.targetElementId
                //     });
                // }

            }
        }

     
    }

}