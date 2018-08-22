using UnityEngine;

namespace Src.Systems {

    public class IMGUIRenderData : ISkipTreeTraversable {

        public Rect layoutRect;
        public Texture2D texture;
        public UIElement element;
        public GUIContent content;
        public GUIStyle imguiStyle;
        public Color backgroundColor;
        public IMGUIRenderData parent;
        public RenderPrimitiveType primitiveType;

        public IMGUIRenderData(UIElement element, RenderPrimitiveType primitiveType, GUIStyle imguiStyle) {
            this.element = element;
            this.primitiveType = primitiveType;
            this.imguiStyle = imguiStyle ?? GUIStyle.none;
            this.content = content ?? GUIContent.none;
            this.backgroundColor = Color.white;
        }
        
        public void SetText(string text) {
            if (content == GUIContent.none) {
                content = new GUIContent();
            }

            content.text = text;
            UITextElement textElement = (UITextElement) element;
            textElement.SetDimensions(imguiStyle.CalcSize(content));
        }
        
        // temp
        public static Rect s_DrawRect;      

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
                    GUI.DrawTexture(s_DrawRect, texture, ScaleMode.StretchToFill, false, s_DrawRect.width / s_DrawRect.height, Color.cyan, 5, 20f);
                    break;
                
                case RenderPrimitiveType.Text:
                    GUI.Label(s_DrawRect, content);
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

    }

}