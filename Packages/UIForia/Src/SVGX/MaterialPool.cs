using System.Collections.Generic;
using UIForia.Util;
using UnityEngine;

namespace SVGX {

    internal class MaterialPool {

        private readonly Material material;
        private readonly Stack<Material> pool;

        private readonly LightList<Material> releaseQueue;

        public MaterialPool(Material material) {
            this.material = material;
            this.pool = new Stack<Material>();
            this.releaseQueue = new LightList<Material>();
        }

        public Material Get() {
            if (pool.Count > 0) {
                return pool.Pop();
            }

            return new Material(material);
        }

        public void Release(Material material) {
            pool.Push(material);
        }

        public void FlushReleaseQueue() {
            for (int i = 0; i < releaseQueue.Count; i++) {
                pool.Push(releaseQueue[i]);
            }

            releaseQueue.Clear();
        }

        public void QueueForRelease(Material mat) {
            releaseQueue.Add(mat);
        }

    }

}