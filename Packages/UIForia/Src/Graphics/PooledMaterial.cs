using UIForia.Util;
using UnityEngine;

namespace Vertigo {

    internal class PooledMaterial {

        public Material material;
        public ShaderPool pool;
        public bool isPooled;
        
        public PooledMaterial(Material material, ShaderPool pool) {
            this.material = material;
            this.pool = pool;
            this.isPooled = false;
        }

        public PooledMaterial Clone() {
            return pool.GetClonedInstance(material);
        }
        
        public void Release() {
            if (!isPooled) {
                isPooled = true;
                pool.pool.Add(this);
            }
        }
       
    }

}