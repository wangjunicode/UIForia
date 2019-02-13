using UnityEngine;
using UnityEngine.UI;

namespace UIForia.Rendering {

    [AddComponentMenu("UIElement/BorderedImage")]
    public class BorderedImage : Image {

        private Material materialInstance;
        private static Sprite instance;
       
        public Rect border;
        public Rect borderRadii;
        public Color borderColor;
        
        protected override void OnEnable() {
            base.OnEnable();
            this.Init();
        }

        private void Init() {
            if (instance == null) {
                Texture2D tex = new Texture2D(1, 1);
                tex.SetPixel(0, 0, Color.white);
                tex.Apply();
                instance = Sprite.Create(tex, new Rect(0, 0, 1, 1), Vector2.zero);
            }

            if (materialInstance == null) {
                materialInstance = new Material(Shader.Find("UIElement/BorderedImage"));
            }

            this.material = materialInstance;
        }

        public void Update() {
            this.UpdateMaterial();
        }

        public void SetMaterialParameters(Rect border, Rect borderRadii, Color borderColor, Color contentColor) {
            this.border = border;
            this.borderColor = borderColor;
            this.borderRadii = borderRadii;
            this.color = contentColor;
        }
        
        public override Material GetModifiedMaterial(Material baseMaterial) {
            float width = rectTransform.rect.width;
            float height = rectTransform.rect.height;
            Vector4 borderVector = new Vector4(border.x, border.y, border.width, border.height);
            Vector4 borderRadiiVector = new Vector4(borderRadii.x, borderRadii.y, borderRadii.width, borderRadii.height);
            
            baseMaterial.SetVector("_RectVector", new Vector4(0, 0, width, height));
            baseMaterial.SetVector("_BorderWidthVector", borderVector);
            baseMaterial.SetVector("_BorderRadiiVector", borderRadiiVector);
            baseMaterial.SetColor("_ContentColor", color);
            baseMaterial.SetColor("_BorderColor", borderColor);
            return baseMaterial;
        }
       
    }   

}