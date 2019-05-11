using UIForia.Util;
using UnityEngine;

namespace Vertigo {

    public class UIForiaShader {

        public string shaderName;
        private StructList<MaterialProperty> properties;
        internal UIForiaShader origin;
        internal Material material;
        internal bool isPooled;
        internal bool isRoot;

        private LightList<UIForiaShader> instances;

        public UIForiaShader(Material material) {
            this.material = new Material(material);
            this.shaderName = material.shader.name;
        }
        
        internal UIForiaShader(UIForiaShader toClone) {
            this.material = new Material(toClone.material);
            this.shaderName = toClone.shaderName;
        }

        private static UIForiaShader defaultShader;
        
        public static UIForiaShader Default {
            get {
                if (defaultShader != null) {
                    return defaultShader;
                }
                Material material = new Material(Shader.Find("Vertigo/VertigoSDF"));
                defaultShader = CreateRootShader(material);
                return defaultShader;
            }
        }

        private static UIForiaShader CreateRootShader(Material material) {
            UIForiaShader retn = new UIForiaShader(material);
            retn.instances = new LightList<UIForiaShader>();
            retn.isRoot = true;
            return retn;
        }

        public UIForiaShader GetInstance() {
            if (instances.Count > 0) {
                return instances.RemoveLast();
            }

            UIForiaShader instance = new UIForiaShader(this);
            instance.origin = this;
            return instance;
        }

        internal void Release() {
            isPooled = false;
            if (isRoot) return;
            origin.instances.Add(this);
        }

        public void SetFloatProperty(int key, float value) {
            throw new System.NotImplementedException();
        }

        public UIForiaShader Clone() {
            if (!isRoot) {
                UIForiaShader shader = origin.GetInstance();
                shader.material.CopyPropertiesFromMaterial(material);
                return shader;
            }
            else {
                UIForiaShader shader = GetInstance();
                shader.material.CopyPropertiesFromMaterial(material);
                return shader;
            }
        }
        
    }

}