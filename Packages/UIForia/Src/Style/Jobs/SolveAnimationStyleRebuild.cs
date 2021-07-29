using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Jobs;

namespace UIForia.Style {

    [BurstCompile(DisableSafetyChecks = UIApplication.DisableJobSafety)]
    internal unsafe struct SolveAnimationStyleRebuild : IJob {
        
        public DataList<StyleUsage>.Shared styleUsages;
        public DataList<StyleUsageQueryResult>.Shared styleUsageQueryResults;

        public DataList<BitSet> blockQueryRequirements; // from style database 

        public ElementMap rebuildStylesMap;
        
        public void Execute() {
            
            
            
        }

    }

}