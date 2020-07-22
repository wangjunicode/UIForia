using UIForia.Rendering;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Jobs;

namespace UIForia.Systems {

    public struct EffectUsage {

        public ElementId elementId;
        public bool isForeground;

    }
    
    [BurstCompile]
    public unsafe struct GatherRenderedElements : IJob {

        public DataList<ElementList> elementLists;
        public ElementTable<ElementTraversalInfo> traversalTable;
        public ElementTable<ClipInfo> clipInfoTable;
        public ElementTable<RenderInfo> renderInfoTable;
        public DataList<RenderCallInfo>.Shared renderCallList;

        public void Execute() {
            renderCallList.size = 0;

            for (int i = 0; i < elementLists.size; i++) {

                ElementList list = elementLists[i];
                renderCallList.EnsureAdditionalCapacity(list.size);

                for (int j = 0; j < list.size; j++) {

                    ElementId elementId = list.array[j];
                    ref ClipInfo clipInfo = ref clipInfoTable[elementId];

                    // consider a 'render always' setting so culled things can still handle render target changes etc
                    if (clipInfo.isCulled || clipInfo.visibility == Visibility.Hidden) {
                        continue;
                    }

                    ref RenderInfo renderInfo = ref renderInfoTable[elementId];
                    ref ElementTraversalInfo traversalInfo = ref traversalTable[elementId];
                    renderCallList.Add(new RenderCallInfo() {
                        layer = renderInfo.layer,
                        elementId = elementId,
                        ftbIndex = traversalInfo.ftbIndex,
                        zIndex = renderInfo.zIndex,
                        renderOp = 0
                    });

                    if (renderInfo.drawForeground) {
                        renderCallList.Add(new RenderCallInfo() {
                            layer = renderInfo.layer,
                            elementId = elementId,
                            ftbIndex = traversalInfo.ftbIndex,
                            zIndex = renderInfo.zIndex,
                            renderOp = 1
                        });

                    }

                }
            }

        }

    }

}