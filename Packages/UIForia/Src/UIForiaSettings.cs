using UnityEngine;


namespace UIForia {

    public class UIForiaSettings : ScriptableObject {

        public bool loadTemplatesFromStreamingAssets = false;
        public Material svgxMaterial;
        public Material batchedMaterial;
        public Material sdfPathMaterial;
        public Material spriteAtlasMaterial;
        public Material clearClipRegionsMaterial;
        public Material clipCountMaterial;
        public Material clipBlitMaterial;
        public string[] defaultNamespaces;

        public bool usePreCompiledTemplates;

        public void OnEnable() {
            loadTemplatesFromStreamingAssets = loadTemplatesFromStreamingAssets || !UnityEngine.Application.isEditor;
        }

    }

}