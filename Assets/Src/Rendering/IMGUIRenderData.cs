using Rendering;
using UnityEngine;

namespace Src.Systems {

    public class IMGUIRenderData : IHierarchical {

        public Rect layoutRect;
        public string textContent;

        public Texture2D borderTexture;
        public Texture2D backgroundTexture;
        public BorderRadius borderRadius;
        public ContentBoxRect borderSize;
        public TextStyle textStyle;
        
        public UIElement element;
        public RenderPrimitiveType primitiveType;

        public IMGUIRenderData(UIElement element) {
            this.element = element;
            this.borderSize = ContentBoxRect.Unset;
            this.primitiveType = RenderPrimitiveType.None;
        }
        
        public int UniqueId => element.id;
        public IHierarchical Element => element;
        public IHierarchical Parent => element.parent;
        
        public void SetText(string text) {
            textContent = text;
        }

        public void SetLocalLayoutRect(Rect layoutRect) {
            this.layoutRect = layoutRect;
        }

        public void SetFontProperties(TextStyle textStyle) {
            this.textStyle = textStyle;
        }

    }

}