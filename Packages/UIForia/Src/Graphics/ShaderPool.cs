using UIForia.Util;
using UnityEngine;

namespace Vertigo {

    internal struct ShaderPool {

        public Material defaultMaterial;
        public LightList<PooledMaterial> pool;

        public ShaderPool(Shader shader) {
            this.pool = new LightList<PooledMaterial>();
            this.defaultMaterial = new Material(shader);
        }

        public PooledMaterial GetClonedInstance(Material material) {
            if (pool.Count > 0) {
                PooledMaterial retn = pool.RemoveLast();
                retn.material.CopyPropertiesFromMaterial(material);
                retn.isPooled = false;
                return retn;
            }
            else {
                PooledMaterial retn = new PooledMaterial(new Material(material), this);
                retn.isPooled = false;
                return retn;
            }
        }

//        public PooledMaterial GetDefaultInstance() {
//            if (pool.Count > 0) {
//                Material retn = pool.RemoveLast();
//                retn.CopyPropertiesFromMaterial(defaultMaterial);
//                return new PooledMaterial(retn, pool);
//            }
//            else {
//                Material retn = new Material(defaultMaterial);
//                return new PooledMaterial(retn, pool);
//            }
//        }

    }

}