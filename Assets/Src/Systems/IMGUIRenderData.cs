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

        public void OnBeforeTraverse() {
            s_DrawRect = new Rect(s_DrawRect) {
                x = s_DrawRect.x + layoutRect.x,
                y = s_DrawRect.y + layoutRect.y,
                width = layoutRect.width,
                height = layoutRect.height
            };

            switch (primitiveType) {
                case RenderPrimitiveType.RawImage:
                case RenderPrimitiveType.ProceduralImage:

                    if (borderTexture != null && borderSize.IsDefined()) {
                        if (backgroundTexture != null) {
                            Rect innerRect = new Rect(s_DrawRect);
                            innerRect.x += borderSize.left;
                            innerRect.y += borderSize.top;
                            innerRect.width -= borderSize.right * 2f;
                            innerRect.height -= borderSize.bottom * 2f;
//                            material.SetFloat("_Radius", 0.5f);
//                            material.SetColor("_Color", Color.red);
//                            material.SetFloat("_Width", 100f);
//                            material.SetFloat("_Height", 100f);
                            //Graphics.DrawTexture(new Rect(300, 300, 300f, 300f), backgroundTexture, material);
                        }

//                        // only draws a border!
                        GUI.DrawTexture(
                            s_DrawRect,
                            borderTexture,
                            ScaleMode.ScaleToFit,
                            true,
                            s_DrawRect.width / s_DrawRect.height,
                            Color.white,
                            borderSize, 
                            borderRadius
                        );
                    }
                    else {
                        if (backgroundTexture != null) {
                            GUI.DrawTexture(s_DrawRect, backgroundTexture);
                        }
                    }

                    break;

                case RenderPrimitiveType.Text:
                    GUIStyle style = new GUIStyle();
                    style.fontSize = 12;
                    style.wordWrap = true;
                    GUI.Label(s_DrawRect, textContent, style);
                    break;
            }
        }

        public void OnAfterTraverse() {
            s_DrawRect = new Rect(s_DrawRect) {
                x = s_DrawRect.x - layoutRect.x,
                y = s_DrawRect.y - layoutRect.y
            };
        }

        public IHierarchical Element => element;
        public IHierarchical Parent => element.parent;

        public void SetFontProperties(TextStyle textStyle) {
            this.textStyle = textStyle;
        }

    }

}