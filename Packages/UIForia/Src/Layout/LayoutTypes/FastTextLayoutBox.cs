using UIForia.Elements;
using UIForia.Text;
using UnityEngine;

namespace UIForia.Layout {

    public class FastTextLayoutBox : FastLayoutBox {

        public TextInfo TextInfo => ((UITextElement) element).textInfo;

        protected override float ComputeContentWidth(float blockWidth) {
            return ((UITextElement) element).TextInfo.ComputeWidth();
        }

        protected override void PerformLayout() {
            TextInfo textInfo = ((UITextElement) element).TextInfo;
            float topOffset = paddingBox.top - borderBox.top;
            float leftOffset = paddingBox.left + borderBox.left;

            contentSize = textInfo.Layout(new Vector2(leftOffset, topOffset), size.width - paddingBox.left - paddingBox.right - borderBox.right - borderBox.left);
            
//            actualWidth = size.width + PaddingHorizontal + BorderHorizontal;
//            actualHeight = size.height + resolvedPaddingBottom + resolvedBorderBottom;
        }

        protected override float ComputeContentHeight(float width, float blockWidth, float blockHeight) {
            return ((UITextElement) element).TextInfo.ComputeHeight(width - paddingBox.left - paddingBox.right - borderBox.left - borderBox.right);
        }

//        public override void OnStylePropertyChanged(StructList<StyleProperty> property) {
//            base.OnStylePropertyChanged(property);
//            for (int i = 0; i < property.Count; i++) {
//                if (property[i].propertyId == StylePropertyId.TextFontSize) {
//                    RequestContentSizeChangeLayout();
//                    break;
//                }
//            }
//        }

    }

}