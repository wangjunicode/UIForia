using Rendering;
using UnityEditor;
using UnityEngine;

namespace Src.Systems {

    public class IMGUIRenderData : ISkipTreeTraversable {

        public Rect layoutRect;

        public Texture2D borderTexture;
        public Texture2D backgroundTexture;
        public BorderRadius borderRadius;

        public Material material;
        public string textContent;
        public TextStyle textStyle;
        public Color backgroundColor;
        public Vector2 textElementSize;
        public ContentBoxRect borderSize;
        
        public UIElement element;
        public IMGUIRenderData parent;
        public RenderPrimitiveType primitiveType;
        
        public static GUIContent s_GUIContent = new GUIContent();
        public static Rect s_DrawRect;
        
        public IMGUIRenderData(UIElement element, RenderPrimitiveType primitiveType) {
            this.element = element;
            this.primitiveType = primitiveType;
            this.backgroundColor = Color.white;
            this.borderSize = ContentBoxRect.Unset;
            this.material = AssetDatabase.LoadAssetAtPath<Material>("Assets/GUITexture.mat");
        }

        public void SetText(string text) {
            textContent = text;
        }

        public static int _RectShaderPropertyId = Shader.PropertyToID("_Rect");
        public static int _BorderWidthPropertyId = Shader.PropertyToID("_BorderWidths");
        public static int _BorderRadiusPropertyId = Shader.PropertyToID("_BorderRadii");
        public static int _BorderColorPropertyId = Shader.PropertyToID("_BorderColor");
        
        // temp

        public void SetLocalLayoutRect(Rect layoutRect) {
            this.layoutRect = layoutRect;
        }

        public void OnParentChanged(ISkipTreeTraversable newParent) {
            parent = (IMGUIRenderData) newParent;
        }

        public void OnBeforeTraverse() {}

        public void OnAfterTraverse() {}

        public IHierarchical Element => element;
        public IHierarchical Parent => element.parent;

        public void SetFontProperties(TextStyle textStyle) {
            this.textStyle = textStyle;
        }

    }

}