using UIForia.Text;
using Unity.Burst;
using Unity.Jobs;

namespace UIForia.Layout {

    [BurstCompile(DisableSafetyChecks = UIApplication.DisableJobSafety)]
    internal unsafe struct GarbageCollectTextData : IJob {

        public int frameId;
        public TextShapeCache shapeCache;
        public StringTagger tagger;
        
        public void Execute() {
            int longCount = tagger.GetRemovalMapSize();
            ulong * mapStorage = stackalloc ulong[longCount];
            LongBoolMap removalMap = new LongBoolMap(mapStorage, longCount);
            tagger.GarbageCollectWithRemovalMap(frameId + 1, removalMap);
            shapeCache.GarbageCollect(removalMap);
        }

    }

}