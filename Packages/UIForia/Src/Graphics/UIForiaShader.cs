using UIForia.Util;
using UnityEngine;

namespace Vertigo {

    internal struct UIForiaMaterial {

        public Material material;
        public MaterialPropertyBlock block;
        public LightList<Material> pool;

        public UIForiaMaterial(Material material, MaterialPropertyBlock block, LightList<Material> pool) {
            this.material = material;
            this.block = block;
            this.pool = pool;
        }

        public void Release() {
            pool.Add(material);
        }

        public UIForiaMaterial Clone() {
            if (pool.Count > 0) {
                Material retn = pool.RemoveLast();
                retn.CopyPropertiesFromMaterial(material);
                return new UIForiaMaterial(retn, pool);
            }
            else {
                Material retn = new Material(material);
                return new UIForiaMaterial(retn, pool);
            }
        }

    }

    internal struct ShaderPool {

        public Material defaultMaterial;
        public LightList<Material> pool;

        public ShaderPool(Shader shader) {
            this.pool = new LightList<Material>();
            this.defaultMaterial = new Material(shader);
        }

        public UIForiaMaterial GetClonedInstance(Material material) {
            if (pool.Count > 0) {
                Material retn = pool.RemoveLast();
                retn.CopyPropertiesFromMaterial(material);
                return new UIForiaMaterial(retn, pool);
            }
            else {
                Material retn = new Material(material);
                return new UIForiaMaterial(retn, pool);
            }
        }

        public UIForiaMaterial GetDefaultInstance() {
            if (pool.Count > 0) {
                Material retn = pool.RemoveLast();
                retn.CopyPropertiesFromMaterial(defaultMaterial);
                return new UIForiaMaterial(retn, pool);
            }
            else {
                Material retn = new Material(defaultMaterial);
                return new UIForiaMaterial(retn, pool);
            }
        }

    }

}