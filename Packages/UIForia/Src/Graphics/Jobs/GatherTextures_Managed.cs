using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;

namespace UIForia.Graphics {

    public struct GatherTextures_Managed : IJob {

        public PerThreadObjectPool<RenderContext2> contextPoolHandle;
        public GCHandle<Dictionary<int, Texture>> textureMapHandle;
        
        public void Execute() {

            ThreadSafePool<RenderContext2> pool = contextPoolHandle.GetPool();
            Dictionary<int, Texture> textureMap = textureMapHandle.Get();
            
            for (int i = 0; i < pool.perThreadData.Length; i++) {
                
                if (pool.perThreadData[i] != null) {
                    RenderContext2 renderContext = pool.perThreadData[i];

                    foreach (KeyValuePair<int, Texture> kvp in renderContext.textureMap) {
                        textureMap[kvp.Key] = kvp.Value;
                    }
                        
                }       
            }
            
        }

    }

}