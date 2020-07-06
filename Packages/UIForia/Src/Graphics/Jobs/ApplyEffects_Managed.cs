using System;
using UIForia.Graphics;
using UIForia.Rendering;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace UIForia.Systems {

    /// <summary>
    /// An effect is similar to a painter except it modifies the results of a painter.
    /// It cannot add new draw calls, but may change materials or modify geometry (including adding new vertices)
    /// </summary>
    internal struct ApplyEffects_Managed : IJob, IVertigoParallel {

        public ParallelParams parallel { get; set; }
        public DataList<EffectUsage>.Shared effectUsageList;
        public GCHandleArray<RenderBox> renderBoxTableHandle;

        public void Execute() {
            Run(0, effectUsageList.size);
        }

        public void Execute(int startIndex, int count) {
            Run(startIndex, startIndex + count);
        }

        private void Run(int start, int end) {

            RenderBox[] renderBoxTable = renderBoxTableHandle.Get();

            for (int i = start; i < end; i++) {
                RenderBox box = renderBoxTable[effectUsageList[i].elementId.index];

                string painterName = box.GetName();

                if (effectUsageList[i].isForeground) { }
                else {
                    RenderContext2 ctx = box.bgRenderContext;
                    RangeInt drawRange = box.bgRenderRange;
                    for (int e = drawRange.start; e < drawRange.end; e++) {
                        MaterialWriter materialInfo = ctx.GetMaterialWriter(e);
                        GeometryWriter geometryInfo = ctx.GetGeometryInfo(e);
                        try {
                            box.effect.ApplyBackground(painterName, ref materialInfo, ref geometryInfo);
                            if (geometryInfo.changed) {
                                // ctx.UpdateGeometryInfo(geometryInfo, i);
                            }
                        }
                        catch (Exception exception) {
                            Debug.Log(exception);
                        }
                    }
                }
            }

        }

    }

}