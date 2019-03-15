using UIForia.Elements;
using UIForia.Rendering;
using UIForia.Text;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Layout.LayoutTypes {

    public class TextLayoutBox : LayoutBox {

        public TextLayoutBox(UIElement element) : base(element) { }

        public TextInfo2 TextInfo => ((UITextElement) element).textInfo;

        protected override float ComputeContentWidth() {
            return ((UITextElement) element).TextInfo.ComputeWidth();
        }

        protected override float ComputeContentHeight(float width) {
            return ((UITextElement) element).TextInfo.ComputeHeight(width - PaddingHorizontal - BorderHorizontal);
        }

        public override void RunLayout() {
            TextInfo2 textInfo = ((UITextElement) element).TextInfo;
            float topOffset = PaddingTop + BorderTop;
            float leftOffset = PaddingLeft + BorderLeft;

            Size size = textInfo.Layout(new Vector2(leftOffset, topOffset), allocatedWidth - PaddingHorizontal - BorderHorizontal);

            actualWidth = size.width + PaddingHorizontal + BorderHorizontal;

            actualHeight = size.height + PaddingBottom + BorderBottom;
        }

        public override void OnStylePropertyChanged(LightList<StyleProperty> property) {
            base.OnStylePropertyChanged(property);
            for (int i = 0; i < property.Count; i++) {
                if (property[i].propertyId == StylePropertyId.TextFontSize) {
                    RequestContentSizeChangeLayout();
                    break;
                }
            }
        }

        public void OnTextContentUpdated() {
            RequestContentSizeChangeLayout();
        }

    }

}